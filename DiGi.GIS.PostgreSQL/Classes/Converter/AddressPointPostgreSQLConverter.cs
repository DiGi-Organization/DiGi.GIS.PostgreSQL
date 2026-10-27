using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and searches PRG address points in the <see cref="Constants.TableName.AddressPoint"/> table: one row per PRG local identifier, filed under the county part the point lies in, holding the <see cref="AddressPointMatch"/> that assigned it to a <c>building_2d</c> footprint.
    /// <para>The <c>object</c> column carries the whole <see cref="AddressPointMatch"/>, so every read returns the address point together with its match type, building reference, guid and distance. The table lives on the Main database, beside <c>building_2d</c>.</para>
    /// <para>Search runs on <c>search_text</c>, normalised by <see cref="Query.SearchText(string?)"/> on both sides and indexed with a GIN trigram index (<see cref="Create.TableAsync_AddressPoint(NpgsqlConnection?, int, CancellationToken)"/>).</para>
    /// </summary>
    public class AddressPointPostgreSQLConverter : PostgreSQLConverter<AddressPointMatch>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressPointPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public AddressPointPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with address points.
        /// </summary>
        public static string TableName => Constants.TableName.AddressPoint;

        /// <summary>
        /// Asynchronously deletes the address points with the given PRG local identifiers, whatever county part they are filed under.
        /// <para>A table that does not exist yet deletes nothing.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="referenceIds">The PRG local identifiers of the points to delete.</param>
        /// <param name="batchSize">The number of identifiers sent per command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of deleted rows, or null when the connection or the identifiers are null.</returns>
        public static async Task<long?> DeleteByReferenceIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? referenceIds, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || referenceIds is null)
            {
                return null;
            }

            List<string> referenceIds_Valid = [.. referenceIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()];
            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                DELETE FROM {TableName}
                WHERE reference_id = ANY(@reference_ids);";

            long result = 0;
            try
            {
                for (int i = 0; i < referenceIds_Valid.Count; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("reference_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = referenceIds_Valid.Skip(i).Take(batchSize).ToArray() });

                    result += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return 0;
            }

            return result;
        }

        /// <summary>
        /// Asynchronously counts the address points by how each was matched to a building footprint, for one county part or for the whole table.
        /// <para>One <c>GROUP BY match_type</c> pass, so the total and the unmatched share come from a single read. A whole-table count scans every row: raise <paramref name="commandTimeout"/> for a large table. A table that does not exist yet counts zero everywhere.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The county part id to count under, or null for the whole table. A county with several parts is counted once per part (<c>Coding - GIS Administrative Data.md</c>).</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the counts, or null when the connection is null.</returns>
        public static async Task<AddressPointCountResult?> GetCountResultAsync(NpgsqlConnection? npgsqlConnection, int? countyId, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string whereCountyId = countyId is null ? string.Empty : "WHERE county_id = @county_id";

            string commandText = $@"
                SELECT match_type, count(*)
                FROM {TableName}
                {whereCountyId}
                GROUP BY match_type;";

            long inside = 0;
            long nearest = 0;
            long unmatched = 0;
            long total = 0;
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                if (countyId is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    long count = npgsqlDataReader.GetInt64(1);
                    total += count;

                    switch ((AddressPointMatchType)npgsqlDataReader.GetInt16(0))
                    {
                        case AddressPointMatchType.Inside:
                            inside += count;
                            break;

                        case AddressPointMatchType.Nearest:
                            nearest += count;
                            break;

                        case AddressPointMatchType.Unmatched:
                            unmatched += count;
                            break;
                    }
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return new AddressPointCountResult(countyId, 0, 0, 0, 0);
            }

            return new AddressPointCountResult(countyId, inside, nearest, unmatched, total);
        }

        /// <summary>
        /// Asynchronously searches address points by a free-text street or locality name, tolerating typos, missing diacritics and partial names, optionally narrowed by house number and county part.
        /// <para>Both <paramref name="text"/> and the stored <c>search_text</c> are reduced by <see cref="Query.SearchText(string?)"/>. A row qualifies when its trigram similarity reaches the <c>pg_trgm.similarity_threshold</c> (the <c>%</c> operator, 0.3 by default, raised to 0.4 for this statement alone by <c>SET LOCAL</c> inside a short transaction on <paramref name="npgsqlConnection"/> - nothing persists beyond it, so a pooled connection never inherits it; measured on production, the statement went from about 1.5 s to 0.76 s for a common street, at full typo and partial-name recall) or when it contains the normalised text (<c>LIKE '%text%'</c>); both are served by the GIN trigram index. The <c>LIKE</c> branch is what serves short queries, whose trigram similarity is inherently low. The normalised text holds only letters, digits and single spaces, so it needs no <c>LIKE</c> escaping. Reference: DiGi.GIS.PostgreSQL#112.</para>
        /// <para>With a <paramref name="houseNumber"/>, only exact matches and prefix matches qualify, compared case-insensitively: <c>12</c> matches <c>12</c>, <c>12A</c> and <c>12/3</c>, but not <c>120</c> - the character after the prefix must not be a digit.</para>
        /// <para>Order: similarity descending, then an exact house number before a prefix match, then the house number. <paramref name="countyId"/> filters on one county part; a county with several parts is searched without it, or once per part (<c>Coding - GIS Administrative Data.md</c>). A table that does not exist yet answers empty.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="text">The street or locality name to search for, as typed.</param>
        /// <param name="houseNumber">The house number to narrow the result to, or null for any.</param>
        /// <param name="countyId">The county part id to narrow the result to, or null for every county.</param>
        /// <param name="limit">The maximum number of results, clamped to 1-1000.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matches in rank order, or null when the connection is null or <paramref name="text"/> normalises to nothing.</returns>
        public static async Task<List<AddressPointMatch>?> GetAddressPointMatchesByAddressAsync(NpgsqlConnection? npgsqlConnection, string? text, string? houseNumber, int? countyId, int limit = 20, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || Query.SearchText(text) is not string searchText)
            {
                return null;
            }

            string? houseNumber_Lower = string.IsNullOrWhiteSpace(houseNumber) ? null : houseNumber.Trim().ToLower(CultureInfo.InvariantCulture);

            // Optional filters are rendered present or absent rather than as "@x IS NULL OR ...", so the planner sees a plain predicate.
            string whereCountyId = countyId is null ? string.Empty : "AND county_id = @county_id";
            string whereHouseNumber = houseNumber_Lower is null ? string.Empty : @"
                  AND (lower(house_number) = @house_number
                       OR (left(lower(house_number), length(@house_number)) = @house_number
                           AND substr(lower(house_number), length(@house_number) + 1, 1) !~ '[0-9]'))";
            string orderHouseNumber = houseNumber_Lower is null ? string.Empty : "(lower(house_number) = @house_number) DESC,";

            string commandText = $@"
                SELECT object
                FROM {TableName}
                WHERE (search_text % @search_text OR search_text LIKE @search_pattern)
                  {whereCountyId}{whereHouseNumber}
                ORDER BY similarity(search_text, @search_text) DESC, {orderHouseNumber} house_number, reference_id
                LIMIT @limit;";

            List<AddressPointMatch> result = [];
            try
            {
                // DiGi.GIS.PostgreSQL#112: the default similarity threshold of 0.3 made the % operator match 209 182
                // candidates of an 8.6 M row table - about 1.5 s for a common street. 0.4 measured 0.76 s with the
                // typo and the partial-name recall kept (0.4783 >= 0.4; 0.5 would have lost the Warszawa rows).
                // SET LOCAL scopes the raise to this statement's own transaction, so no pooled connection inherits it.
                await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

                await using (NpgsqlCommand thresholdCommand = new("SET LOCAL pg_trgm.similarity_threshold = '0.4';", npgsqlConnection, npgsqlTransaction))
                {
                    thresholdCommand.CommandTimeout = commandTimeout;
                    await thresholdCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection, npgsqlTransaction);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("search_text", NpgsqlDbType.Text) { Value = searchText });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("search_pattern", NpgsqlDbType.Text) { Value = $"%{searchText}%" });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = Math.Clamp(limit, 1, 1000) });
                if (countyId is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId.Value });
                }

                if (houseNumber_Lower is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("house_number", NpgsqlDbType.Text) { Value = houseNumber_Lower });
                }

                await ReadAsync(npgsqlCommand, result, cancellationToken);

                await npgsqlTransaction.CommitAsync(cancellationToken);
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads the address points matched to the given buildings: the reverse of the address search.
        /// <para>The <c>building_reference</c> column holds the <c>building_2d</c> reference the point was matched to. A building may hold any number of points; unmatched points are never returned. A table that does not exist yet answers empty.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="references">The <c>building_2d</c> references of the buildings.</param>
        /// <param name="batchSize">The number of references sent per command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matches, or null when the connection or the references are null.</returns>
        public static async Task<List<AddressPointMatch>?> GetAddressPointMatchesByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            List<string> references_Valid = [.. references.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()];
            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                SELECT object
                FROM {TableName}
                WHERE building_reference = ANY(@references)
                ORDER BY building_reference, house_number, reference_id;";

            List<AddressPointMatch> result = [];
            try
            {
                for (int i = 0; i < references_Valid.Count; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Valid.Skip(i).Take(batchSize).ToArray() });

                    await ReadAsync(npgsqlCommand, result, cancellationToken);
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }

        /// <summary>
        /// Asynchronously stores address point matches under one county part, replacing the row of each PRG local identifier unless the stored version is newer.
        /// <para>The conflict target is <c>reference_id</c> alone, so a point re-imported under a different county part moves there. A stored row is replaced when it has no version or the incoming version is the same or newer; an older incoming version is ignored, so re-running an old import cannot downgrade the table. Within one call the newest version of a duplicated identifier wins, because one statement cannot update the same row twice.</para>
        /// <para>Matches without an address point, a PRG local identifier, a location or any street or locality name are skipped. Rows are written <paramref name="batchSize"/> at a time, one statement per batch carrying one array per column. The table is created when missing.</para>
        /// <para><paramref name="countyId"/> is written as given. The caller resolves the county part each point lies in first: a county code maps to one row per polygon part (<c>Coding - GIS Administrative Data.md</c>).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The resolved county part the points lie in.</param>
        /// <param name="addressPointMatches">The matches to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when the connection or the matches are null or the table could not be created.</returns>
        public static async Task<long?> UpdateAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<AddressPointMatch?>? addressPointMatches, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || addressPointMatches is null)
            {
                return null;
            }

            if (!await Create.TableAsync_AddressPoint(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            Dictionary<string, (AddressPointMatch AddressPointMatch, string SearchText)> dictionary = [];
            foreach (AddressPointMatch? addressPointMatch in addressPointMatches)
            {
                if (addressPointMatch?.AddressPoint is not AddressPoint addressPoint || string.IsNullOrWhiteSpace(addressPoint.ReferenceId) || addressPoint.Location is null || addressPoint.SearchText() is not string searchText)
                {
                    continue;
                }

                if (dictionary.TryGetValue(addressPoint.ReferenceId, out (AddressPointMatch AddressPointMatch, string SearchText) tuple) && (addressPoint.Version is null || (tuple.AddressPointMatch.AddressPoint?.Version is DateTimeOffset version && addressPoint.Version < version)))
                {
                    continue;
                }

                dictionary[addressPoint.ReferenceId] = (addressPointMatch, searchText);
            }

            List<(AddressPointMatch AddressPointMatch, string SearchText)> tuples = [.. dictionary.Values];
            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                INSERT INTO {TableName} (reference_id, version, county_id, building_reference, match_type, distance, street, ulic_id, house_number, locality, simc_id, postal_code, x, y, search_text, object)
                SELECT t.reference_id, t.version, @county_id, t.building_reference, t.match_type, t.distance, t.street, t.ulic_id, t.house_number, t.locality, t.simc_id, t.postal_code, t.x, t.y, t.search_text, t.object
                FROM unnest(@reference_ids, @versions, @building_references, @match_types, @distances, @streets, @ulic_ids, @house_numbers, @localities, @simc_ids, @postal_codes, @xs, @ys, @search_texts, @objects)
                    AS t(reference_id, version, building_reference, match_type, distance, street, ulic_id, house_number, locality, simc_id, postal_code, x, y, search_text, object)
                ON CONFLICT (reference_id) DO UPDATE SET
                    version = EXCLUDED.version,
                    county_id = EXCLUDED.county_id,
                    building_reference = EXCLUDED.building_reference,
                    match_type = EXCLUDED.match_type,
                    distance = EXCLUDED.distance,
                    street = EXCLUDED.street,
                    ulic_id = EXCLUDED.ulic_id,
                    house_number = EXCLUDED.house_number,
                    locality = EXCLUDED.locality,
                    simc_id = EXCLUDED.simc_id,
                    postal_code = EXCLUDED.postal_code,
                    x = EXCLUDED.x,
                    y = EXCLUDED.y,
                    search_text = EXCLUDED.search_text,
                    object = EXCLUDED.object,
                    created_at = now()
                WHERE {TableName}.version IS NULL OR EXCLUDED.version >= {TableName}.version;";

            long result = 0;
            for (int i = 0; i < tuples.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<(AddressPointMatch AddressPointMatch, string SearchText)> tuples_Batch = tuples.GetRange(i, Math.Min(batchSize, tuples.Count - i));
                int count = tuples_Batch.Count;

                string[] referenceIds = new string[count];
                DateTime?[] versions = new DateTime?[count];
                string?[] buildingReferences = new string?[count];
                short[] matchTypes = new short[count];
                double?[] distances = new double?[count];
                string?[] streets = new string?[count];
                string?[] ulicIds = new string?[count];
                string?[] houseNumbers = new string?[count];
                string?[] localities = new string?[count];
                string?[] simcIds = new string?[count];
                string?[] postalCodes = new string?[count];
                double[] xs = new double[count];
                double[] ys = new double[count];
                string[] searchTexts = new string[count];
                string[] objects = new string[count];

                for (int j = 0; j < count; j++)
                {
                    (AddressPointMatch addressPointMatch, string searchText) = tuples_Batch[j];
                    AddressPoint addressPoint = addressPointMatch.AddressPoint!;

                    referenceIds[j] = addressPoint.ReferenceId!;
                    versions[j] = addressPoint.Version?.UtcDateTime;
                    buildingReferences[j] = addressPointMatch.Reference;
                    matchTypes[j] = (short)addressPointMatch.AddressPointMatchType;
                    distances[j] = addressPointMatch.Distance;
                    streets[j] = addressPoint.StreetName;
                    ulicIds[j] = addressPoint.ULIC;
                    houseNumbers[j] = addressPoint.HouseNumber;
                    localities[j] = addressPoint.LocalityName;
                    simcIds[j] = addressPoint.SIMC;
                    postalCodes[j] = addressPoint.PostalCode;
                    xs[j] = addressPoint.Location!.X;
                    ys[j] = addressPoint.Location!.Y;
                    searchTexts[j] = searchText;
                    objects[j] = addressPointMatch.ToJsonObject()?.ToJsonString() ?? "{}";
                }

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("reference_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = referenceIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("versions", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz) { Value = versions });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("building_references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = buildingReferences });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("match_types", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = matchTypes });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("distances", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = distances });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("streets", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = streets });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("ulic_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = ulicIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("house_numbers", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = houseNumbers });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("localities", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = localities });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("simc_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = simcIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("postal_codes", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = postalCodes });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("xs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = xs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("ys", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = ys });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("search_texts", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = searchTexts });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("objects", NpgsqlDbType.Array | NpgsqlDbType.Jsonb) { Value = objects });

                result += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes the address points with the given PRG local identifiers, automatically managing the connection.
        /// </summary>
        /// <param name="referenceIds">The PRG local identifiers of the points to delete.</param>
        /// <param name="batchSize">The number of identifiers sent per command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of deleted rows, or null when nothing could be deleted.</returns>
        public async Task<long?> DeleteByReferenceIdsAsync(IEnumerable<string>? referenceIds, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await DeleteByReferenceIdsAsync(npgsqlConnection, referenceIds, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the address points by match type for one county part or for the whole table, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The county part id to count under, or null for the whole table.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the counts, or null when nothing could be counted.</returns>
        public async Task<AddressPointCountResult?> GetCountResultAsync(int? countyId, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetCountResultAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously searches address points by a free-text street or locality name, automatically managing the connection.
        /// </summary>
        /// <param name="text">The street or locality name to search for, as typed.</param>
        /// <param name="houseNumber">The house number to narrow the result to, or null for any.</param>
        /// <param name="countyId">The county part id to narrow the result to, or null for every county.</param>
        /// <param name="limit">The maximum number of results, clamped to 1-1000.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matches in rank order, or null when nothing could be searched.</returns>
        public async Task<List<AddressPointMatch>?> GetAddressPointMatchesByAddressAsync(string? text, string? houseNumber, int? countyId, int limit = 20, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetAddressPointMatchesByAddressAsync(npgsqlConnection, text, houseNumber, countyId, limit, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the address points matched to the given buildings, automatically managing the connection.
        /// </summary>
        /// <param name="references">The <c>building_2d</c> references of the buildings.</param>
        /// <param name="batchSize">The number of references sent per command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matches, or null when nothing could be read.</returns>
        public async Task<List<AddressPointMatch>?> GetAddressPointMatchesByReferencesAsync(IEnumerable<string>? references, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetAddressPointMatchesByReferencesAsync(npgsqlConnection, references, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores address point matches under one county part, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The resolved county part the points lie in.</param>
        /// <param name="addressPointMatches">The matches to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when nothing could be stored.</returns>
        public async Task<long?> UpdateAsync(int countyId, IEnumerable<AddressPointMatch?>? addressPointMatches, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, countyId, addressPointMatches, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Reads the <c>object</c> column of every row of a command into address point matches.
        /// </summary>
        /// <param name="npgsqlCommand">The command whose first column is the <c>object</c> JSON.</param>
        /// <param name="addressPointMatches">The list the matches are appended to.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task ReadAsync(NpgsqlCommand npgsqlCommand, List<AddressPointMatch> addressPointMatches, CancellationToken cancellationToken)
        {
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (JsonNode.Parse(npgsqlDataReader.GetString(0)) is JsonObject jsonObject)
                {
                    addressPointMatches.Add(new AddressPointMatch(jsonObject));
                }
            }
        }
    }
}
