using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Constants;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides a specialized implementation for converting <see cref="OrtoDatas"/> objects to and from a PostgreSQL database format, incorporating GIS-specific conversion capabilities.
    /// </summary>
    public class OrtoDatasPostgreSQLConverter : PostgreSQLConverter<OrtoDatas>, IGISPostgreSQLConverter<OrtoDatas>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDatasPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public OrtoDatasPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Asynchronously retrieves the count of records from the database, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The optional integer identifier of the county used to filter the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a <see cref="long"/>.</returns>
        public static async Task<long> GetCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            string tableName = TableName.OrtoDatas;
            if (countyId != null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, tableName, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The optional integer identifier for the county; if null, the estimate is calculated across all counties.</param>
        /// <param name="analyze">A value indicating whether to perform an ANALYZE operation on the database table to update statistics before retrieving the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated count as a nullable long integer, -1 when the partition exists but has not been analysed, or null when the table or partition does not exist or connection is null.</returns>
        public static async Task<long?> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string tableName = TableName.OrtoDatas;
            if (countyId != null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.EstimatedCountAsync(npgsqlConnection, tableName, analyze, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the estimated row count for each of the specified county partitions, reading them all in a single catalog query rather than one query per county.
        /// <para>A county is absent from the result when it has no partition, and carries <c>-1</c> when its partition exists but has never been analysed - the same two cases the singular overload reports as <c>null</c> and <c>-1</c>.</para>
        /// <para>Setting <paramref name="analyze"/> costs one <c>VACUUM ANALYZE</c> statement per existing partition. That work cannot be batched, so it grows with the size of <paramref name="countyIds"/>.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> to use for the query.</param>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for.</param>
        /// <param name="analyze">A boolean indicating whether to run VACUUM ANALYZE on each existing partition before reading the estimates.</param>
        /// <param name="batchSize">The maximum number of partition names sent in a single catalog query.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary keyed by county identifier holding the estimated row count for every county whose partition exists, or null when the connection or the identifiers are null.</returns>
        public static async Task<Dictionary<int, long>?> GetEstimatedCountsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, bool analyze = false, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || countyIds is null)
            {
                return null;
            }

            Dictionary<string, int> countyIds_ByTableName = [];
            foreach (int countyId in countyIds)
            {
                countyIds_ByTableName[string.Format("{0}_{1}", TableName.OrtoDatas, countyId)] = countyId;
            }

            Dictionary<string, long>? counts_ByTableName = await DiGi.PostgreSQL.Query.EstimatedCountsAsync(npgsqlConnection, countyIds_ByTableName.Keys, analyze, batchSize, commandTimeout, cancellationToken);
            if (counts_ByTableName is null)
            {
                return null;
            }

            Dictionary<int, long> result = [];
            foreach (KeyValuePair<string, long> keyValuePair in counts_ByTableName)
            {
                if (countyIds_ByTableName.TryGetValue(keyValuePair.Key, out int countyId))
                {
                    result[countyId] = keyValuePair.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the estimated row count for the specified county identifiers in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> to use for the query.</param>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for.</param>
        /// <param name="analyze">A boolean indicating whether to run an analysis operation before fetching the estimated count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total estimated row count as a long, or -1 when an error occurs or any named county has no partition or has never been analysed.</returns>
        public static async Task<long> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int> countyIds, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            if (countyIds is null)
            {
                long? count_Single = await GetEstimatedCountAsync(npgsqlConnection, (int?)null, analyze, commandTimeout, cancellationToken);
                return count_Single ?? -1;
            }

            Dictionary<int, long>? counts = await GetEstimatedCountsAsync(npgsqlConnection, countyIds, analyze, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (counts is null)
            {
                return -1;
            }

            // A county named twice in countyIds is one county, so the sum walks the distinct counties -
            // the same set the dictionary always held.
            HashSet<int> countyIds_Temp = [.. countyIds];

            long result = 0;
            foreach (int countyId in countyIds_Temp)
            {
                // A county that has never been imported has no partition and is absent from the dictionary.
                // An unanalysed partition answers -1. In either case the sum would be a lower bound, not a
                // measurement of the counties named, so the overload answers -1 instead (decided in
                // DiGi-Organization/DiGi.GIS.PostgreSQL#44).
                if (!counts.TryGetValue(countyId, out long count) || count < 0)
                {
                    return -1;
                }

                result += count;
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves existing building 2D references from the database based on the provided collection.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="building2DReferences">A collection of <see cref="Building2DReference"/> objects to check for existence in the database.</param>
        /// <param name="inverted">A boolean value indicating whether to invert the search criteria, returning references that do not exist if set to true.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions for references not matched by county.</param>
        /// <param name="countyId">The county the lookup is confined to. The table is partitioned on that column, so naming it lets the query reach a single partition instead of every one of them. Null searches the whole table, which is what a collection spanning several counties needs.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DReference"/> objects, or null if the connection or input collection is null.</returns>
        public static async Task<List<Building2DReference>?> GetExistingBuilding2DReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<Building2DReference>? building2DReferences, bool inverted = false, bool fallbackByReference = false, int? countyId = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || building2DReferences is null)
            {
                return null;
            }

            List<Building2DReference> building2DReferences_List = [.. building2DReferences];
            if (building2DReferences_List.Count == 0)
            {
                return [];
            }

            // 1. Prepare data for UNNEST
            int[] countyIds = [.. building2DReferences_List.Select(l => l.CountyId ?? 0)];
            string[] references = [.. building2DReferences_List.Select(l => l.Reference ?? string.Empty)];

            // 2. We use a LEFT JOIN between the input list (UNNEST) and the actual table.
            // If u.reference IS NULL, it means the item does NOT exist in the database.
            // The county equality is emitted only when there is one to emit, and as a plain parameterised
            // equality on the partition key rather than an "@countyId IS NULL OR ..." disjunction, because
            // only the former lets the executor prune to a single partition. It belongs in the ON clause:
            // moved to WHERE it would discard the non-matching rows the inverted case exists to report.
            string commandText = $@"
                SELECT input.c, input.r
                FROM UNNEST(@counties, @refs) AS input(c, r)
                LEFT JOIN {TableName.OrtoDatas} u ON (input.c = 0 OR u.county_id = input.c) AND u.reference = input.r{(countyId.HasValue ? " AND u.county_id = @countyId" : string.Empty)}
                WHERE (@inverted = false AND u.reference IS NOT NULL)  -- Item exists
                   OR (@inverted = true AND u.reference IS NULL);     -- Item does not exist";

            List<Building2DReference> results = [];

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.AddWithValue("counties", countyIds);
                npgsqlCommand.Parameters.AddWithValue("refs", references);
                npgsqlCommand.Parameters.AddWithValue("inverted", inverted);
                if (countyId.HasValue)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    results.Add(new Building2DReference { CountyId = npgsqlDataReader.GetInt32(0), Reference = npgsqlDataReader.GetString(1) });
                }
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed for {Count} references", nameof(GetExistingBuilding2DReferencesAsync), building2DReferences_List.Count);
                return null;
            }

            if (fallbackByReference)
            {
                if (!inverted)
                {
                    HashSet<string> foundReferences = [.. results.Where(r => !string.IsNullOrWhiteSpace(r.Reference)).Select(r => r.Reference!)];
                    string[] missingReferences = [.. references.Where(r => !string.IsNullOrWhiteSpace(r) && !foundReferences.Contains(r)).Distinct()];

                    if (missingReferences.Length > 0)
                    {
                        const string fallbackCommandText = $@"
                            SELECT u.county_id, u.reference
                            FROM UNNEST(@missingRefs) AS input(r)
                            INNER JOIN {TableName.OrtoDatas} u ON u.reference = input.r;";

                        try
                        {
                            await using NpgsqlCommand fallbackCommand = new(fallbackCommandText, npgsqlConnection);
                            fallbackCommand.CommandTimeout = commandTimeout;
                            fallbackCommand.Parameters.AddWithValue("missingRefs", missingReferences);

                            await using NpgsqlDataReader fallbackReader = await fallbackCommand.ExecuteReaderAsync(cancellationToken);
                            while (await fallbackReader.ReadAsync(cancellationToken))
                            {
                                results.Add(new Building2DReference
                                {
                                    CountyId = fallbackReader.GetInt32(0),
                                    Reference = fallbackReader.GetString(1)
                                });
                            }
                        }
                        catch (NpgsqlException npgsqlException)
                        {
                            Serilog.Modify.Log(npgsqlException, "{Method} fallback failed for {Count} references", nameof(GetExistingBuilding2DReferencesAsync), missingReferences.Length);
                            return null;
                        }
                    }
                }
                else
                {
                    string[] candidateReferences = [.. results.Where(r => !string.IsNullOrWhiteSpace(r.Reference)).Select(r => r.Reference!).Distinct()];

                    if (candidateReferences.Length > 0)
                    {
                        const string checkExistCommandText = $@"
                            SELECT DISTINCT u.reference
                            FROM UNNEST(@candidateRefs) AS input(r)
                            INNER JOIN {TableName.OrtoDatas} u ON u.reference = input.r;";

                        HashSet<string> existingGlobally = [];

                        try
                        {
                            await using NpgsqlCommand checkCommand = new(checkExistCommandText, npgsqlConnection);
                            checkCommand.CommandTimeout = commandTimeout;
                            checkCommand.Parameters.AddWithValue("candidateRefs", candidateReferences);

                            await using NpgsqlDataReader checkReader = await checkCommand.ExecuteReaderAsync(cancellationToken);
                            while (await checkReader.ReadAsync(cancellationToken))
                            {
                                existingGlobally.Add(checkReader.GetString(0));
                            }
                        }
                        catch (NpgsqlException npgsqlException)
                        {
                            Serilog.Modify.Log(npgsqlException, "{Method} inverted fallback failed for {Count} references", nameof(GetExistingBuilding2DReferencesAsync), candidateReferences.Length);
                            return null;
                        }

                        if (existingGlobally.Count > 0)
                        {
                            results.RemoveAll(r => !string.IsNullOrWhiteSpace(r.Reference) && existingGlobally.Contains(r.Reference));
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatas" /> based on the specified references and optional county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the database.</param>
        /// <param name="references">A collection of strings representing the references to search for.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the results.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatas" /> objects, or null if the connection is null or no matching data is found.</returns>
        public static async Task<List<OrtoDatas>?> GetOrtoDatasByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            string[] references_Array = [.. references];
            if (references_Array.Length == 0)
            {
                return [];
            }

            string commandText = countyId.HasValue ? $@"
                SELECT id, county_id, reference, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                FROM {TableName.OrtoDatas}
                WHERE county_id = @countyId
                  AND reference = ANY(@references);" : $@"
                SELECT id, county_id, reference, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                FROM {TableName.OrtoDatas}
                WHERE reference = ANY(@references);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });
            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            List<OrtoDatas>? result = await ReadAsync_OrtoDatas(npgsqlCommand, cancellationToken);
            if (result is null)
            {
                return null;
            }

            if (fallbackByReference && countyId is not null)
            {
                HashSet<string> foundReferences = [.. result.Where(o => !string.IsNullOrWhiteSpace(o.Reference)).Select(o => o.Reference!)];
                string[] missingReferences = [.. references_Array.Where(r => !string.IsNullOrWhiteSpace(r) && !foundReferences.Contains(r)).Distinct()];

                if (missingReferences.Length > 0)
                {
                    List<OrtoDatas>? fallbackItems = await GetOrtoDatasByReferencesAsync(npgsqlConnection, missingReferences, null, false, commandTimeout, cancellationToken);
                    if (fallbackItems is not null && fallbackItems.Count > 0)
                    {
                        result.AddRange(fallbackItems);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously clears all data and restarts the identity sequence.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the operation succeeded; otherwise, false.</returns>
        public async Task<bool> ClearAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool result_1 = await DiGi.PostgreSQL.Modify.ClearAsync(npgsqlConnection, TableName.OrtoDatas, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            bool result_2 = await DiGi.PostgreSQL.Modify.ClearAsync(npgsqlConnection, TableName.OrtoDatas_Building2DReference_Update, commandTimeout: commandTimeout, cancellationToken: cancellationToken);

            return result_1 || result_2;
        }

        /// <summary>
        /// Asynchronously checks for the existence of a collection of references, optionally filtered by a county identifier.
        /// </summary>
        /// <param name="references">An <see cref="IEnumerable{T}"/> of strings representing the references to be checked.</param>
        /// <param name="countyId">The optional integer identifier for the county; if null, the search is not filtered by county.</param>
        /// <param name="inverted">A boolean value indicating whether to return the set of references that do not exist (true) or those that do exist (false).</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback check without county filtering for references not matched by county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of strings containing the filtered references, or null if the operation fails or no results are found.</returns>
        public async Task<HashSet<string>?> ContainsByReferencesAsync(IEnumerable<string> references, int? countyId, bool inverted = false, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            string[] referenceArray = [.. references.Distinct()];
            if (referenceArray.Length == 0)
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            HashSet<string> result = [];

            string commandText = countyId.HasValue ? $@"
                SELECT input_ref
                FROM UNNEST(@refs) AS input_ref
                LEFT JOIN {TableName.OrtoDatas} u ON u.reference = input_ref
                    AND u.county_id = @county_id
                WHERE
                    (@inverted = false AND u.reference IS NOT NULL)
                    OR
                    (@inverted = true AND u.reference IS NULL);" : $@"
                SELECT input_ref
                FROM UNNEST(@refs) AS input_ref
                LEFT JOIN {TableName.OrtoDatas} u ON u.reference = input_ref
                WHERE
                    (@inverted = false AND u.reference IS NOT NULL)
                    OR
                    (@inverted = true AND u.reference IS NULL);";

            try
            {
                await npgsqlConnection.OpenAsync(cancellationToken);

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("refs", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = referenceArray });
                if (countyId.HasValue)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId.Value });
                }
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("inverted", NpgsqlDbType.Boolean) { Value = inverted });

                await using NpgsqlDataReader reader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(reader.GetString(0));
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in {nameof(ContainsByReferencesAsync)}: {ex.Message}");
                throw;
            }

            if (fallbackByReference && countyId is not null)
            {
                if (!inverted)
                {
                    string[] missingReferences = [.. referenceArray.Where(r => !result.Contains(r))];
                    if (missingReferences.Length > 0)
                    {
                        const string fallbackCommandText = $@"
                            SELECT DISTINCT u.reference
                            FROM UNNEST(@missingRefs) AS input(r)
                            INNER JOIN {TableName.OrtoDatas} u ON u.reference = input.r;";

                        try
                        {
                            await using NpgsqlCommand fallbackCommand = new(fallbackCommandText, npgsqlConnection);
                            fallbackCommand.CommandTimeout = commandTimeout;
                            fallbackCommand.Parameters.Add(new NpgsqlParameter("missingRefs", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = missingReferences });

                            await using NpgsqlDataReader fallbackReader = await fallbackCommand.ExecuteReaderAsync(cancellationToken);
                            while (await fallbackReader.ReadAsync(cancellationToken))
                            {
                                result.Add(fallbackReader.GetString(0));
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine($"Database error in {nameof(ContainsByReferencesAsync)} fallback: {ex.Message}");
                            throw;
                        }
                    }
                }
                else
                {
                    string[] candidateReferences = [.. result];
                    if (candidateReferences.Length > 0)
                    {
                        const string checkExistCommandText = $@"
                            SELECT DISTINCT u.reference
                            FROM UNNEST(@candidateRefs) AS input(r)
                            INNER JOIN {TableName.OrtoDatas} u ON u.reference = input.r;";

                        HashSet<string> existingGlobally = [];

                        try
                        {
                            await using NpgsqlCommand checkCommand = new(checkExistCommandText, npgsqlConnection);
                            checkCommand.CommandTimeout = commandTimeout;
                            checkCommand.Parameters.Add(new NpgsqlParameter("candidateRefs", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = candidateReferences });

                            await using NpgsqlDataReader checkReader = await checkCommand.ExecuteReaderAsync(cancellationToken);
                            while (await checkReader.ReadAsync(cancellationToken))
                            {
                                existingGlobally.Add(checkReader.GetString(0));
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            Console.WriteLine($"Database error in {nameof(ContainsByReferencesAsync)} inverted fallback: {ex.Message}");
                            throw;
                        }

                        if (existingGlobally.Count > 0)
                        {
                            result.RemoveWhere(existingGlobally.Contains);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county to filter the count; if null, the count is retrieved for all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total row count as a long.</returns>
        public async Task<long> GetCountAsync(int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string tableName = TableName.OrtoDatas;
            if (countyId != null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, tableName, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated row count, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="countyId">The optional <see cref="System.Int32"/> identifier of the county to filter the estimate; if null, the estimate is calculated for all counties.</param>
        /// <param name="analyze">A <see cref="System.Boolean"/> value indicating whether to run an analysis operation before fetching the count to improve accuracy.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated number of rows as a nullable <see cref="System.Int64"/>, -1 when the partition exists but has not been analysed, or null if an error occurs or the target does not exist.</returns>
        public async Task<long?> GetEstimatedCountAsync(int? countyId, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetEstimatedCountAsync(npgsqlConnection, countyId, analyze, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the estimated row count for each of the specified county partitions on a single connection, reading them all in one catalog query rather than one query per county.
        /// <para>A county is absent from the result when it has no partition, and carries <c>-1</c> when its partition exists but has never been analysed.</para>
        /// </summary>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for.</param>
        /// <param name="analyze">A boolean indicating whether to run VACUUM ANALYZE on each existing partition before reading the estimates.</param>
        /// <param name="batchSize">The maximum number of partition names sent in a single catalog query.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary keyed by county identifier holding the estimated row count for every county whose partition exists, or null when no connection could be opened or the identifiers are null.</returns>
        public async Task<Dictionary<int, long>?> GetEstimatedCountsAsync(IEnumerable<int>? countyIds, bool analyze = false, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetEstimatedCountsAsync(npgsqlConnection, countyIds, analyze, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated row count for the specified collection of county identifiers.
        /// </summary>
        /// <param name="countyIds">A collection of integers representing the IDs of the counties to estimate counts for.</param>
        /// <param name="analyze">A boolean value indicating whether to perform a database analysis before fetching the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated total row count as a long, or -1 when an error occurs or any named county has no partition or has never been analysed.</returns>
        public async Task<long> GetEstimatedCountAsync(IEnumerable<int> countyIds, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetEstimatedCountAsync(npgsqlConnection, countyIds, analyze, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves existing building 2D references based on the provided collection and inversion criteria.
        /// </summary>
        /// <param name="building2DReferences">An <see cref="IEnumerable{Building2DReference}"/> of building 2D references to check for existence.</param>
        /// <param name="inverted">A boolean value indicating whether to invert the result; if set to <see langword="true"/>, retrieves references that do not exist.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions for references not matched by county.</param>
        /// <param name="countyId">The county the lookup is confined to. The table is partitioned on that column, so naming it lets the query reach a single partition instead of every one of them. Null searches the whole table, which is what a collection spanning several counties needs.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{Building2DReference}"/> of matching references, or null if the input collection is null.</returns>
        public async Task<List<Building2DReference>?> GetExistingBuilding2DReferencesAsync(IEnumerable<Building2DReference>? building2DReferences, bool inverted = false, bool fallbackByReference = false, int? countyId = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (building2DReferences == null)
            {
                return null;
            }

            if (!building2DReferences.Any())
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetExistingBuilding2DReferencesAsync(npgsqlConnection, building2DReferences, inverted, fallbackByReference, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves and claims a batch of building 2D references from the update queue.
        /// <para>Rows are atomically claimed by updating <c>claimed_at</c> to the current timestamp rather than deleting them immediately. If the claim is not acknowledged within <paramref name="claimTimeoutMinutes"/> minutes, the rows automatically become available for subsequent claims.</para>
        /// <para>Never-claimed rows are handed out before rows whose claim expired, and each group in the order it was queued. Ordering on the queuing time alone put an expired claim back at the head of the queue, ahead of everything that had never been attempted, so a run failing on the rows it reached first could re-attempt those forever without ever seeing the rest. The ordering matches the index the table carries for it.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="count">The maximum number of <see cref="Building2DReference"/> objects to retrieve.</param>
        /// <param name="claimTimeoutMinutes">The duration in minutes before an unacknowledged claim expires and returns to the queue. Defaults to 30.</param>
        /// <param name="maxAttempts">The maximum number of claim attempts before a reference is retired as a poison row. Defaults to 5.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of claimed <see cref="Building2DReference"/> objects, empty when the queue holds nothing claimable, or null when the queue could not be reached at all.</returns>
        public static async Task<List<Building2DReference>?> GetNextBuilding2DReferencesAsync(
            NpgsqlConnection? npgsqlConnection,
            int count = 100,
            int claimTimeoutMinutes = 30,
            int maxAttempts = 5,
            int commandTimeout = 60,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || count <= 0 || maxAttempts <= 0)
            {
                return null;
            }

            // The DDL rather than a mere existence check, because a queue table created before claiming
            // existed has no claimed_at column and answering "the table is there" is not enough to claim
            // from it. Every statement in it is conditional - CREATE TABLE IF NOT EXISTS, ADD COLUMN IF
            // NOT EXISTS, CREATE INDEX IF NOT EXISTS - so running it on every claim costs a catalog lookup
            // and is what the enqueue path already does.
            if (!await Create.TableAsync_Building2DReference(npgsqlConnection, TableName.OrtoDatas_Building2DReference_Update, commandTimeout, cancellationToken))
            {
                return null;
            }

            string retireCommandText = $@"
                DELETE FROM {TableName.OrtoDatas_Building2DReference_Update}
                WHERE COALESCE(attempts, 0) >= @maxAttempts
                RETURNING id, county_id, reference;";

            try
            {
                await using NpgsqlCommand retireCommand = new(retireCommandText, npgsqlConnection);
                retireCommand.CommandTimeout = commandTimeout;
                retireCommand.Parameters.Add(new NpgsqlParameter("maxAttempts", NpgsqlDbType.Integer) { Value = maxAttempts });

                await using NpgsqlDataReader retireReader = await retireCommand.ExecuteReaderAsync(cancellationToken);
                while (await retireReader.ReadAsync(cancellationToken))
                {
                    long retiredId = retireReader.GetInt64(0);
                    int? retiredCountyId = retireReader.IsDBNull(1) ? null : (int?)retireReader.GetInt32(1);
                    string? retiredReference = retireReader.IsDBNull(2) ? null : retireReader.GetString(2);

                    Serilog.Modify.Log(
                        Serilog.Enums.LogEventLevel.Error,
                        "Retired poison queue entry {Id} (County: {CountyId}, Reference: {Reference}) after reaching {MaxAttempts} attempts",
                        retiredId,
                        retiredCountyId?.ToString() ?? "Unknown",
                        retiredReference ?? "Unknown",
                        maxAttempts);
                }
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed while retiring poison references", nameof(GetNextBuilding2DReferencesAsync));
                return null;
            }

            string commandText = $@"
                UPDATE {TableName.OrtoDatas_Building2DReference_Update}
                SET claimed_at = now(),
                    attempts = COALESCE(attempts, 0) + 1
                WHERE id IN (
                    SELECT id FROM {TableName.OrtoDatas_Building2DReference_Update}
                    WHERE (claimed_at IS NULL OR claimed_at < now() - (@claimTimeoutMinutes * interval '1 minute'))
                      AND COALESCE(attempts, 0) < @maxAttempts
                    ORDER BY claimed_at ASC NULLS FIRST, created_at ASC
                    FOR UPDATE SKIP LOCKED
                    LIMIT @count
                )
                RETURNING id, county_id, reference, subdivision_id;";

            List<Building2DReference> result = [];

            try
            {
                await using NpgsqlCommand command = new(commandText, npgsqlConnection);
                command.CommandTimeout = commandTimeout;
                command.Parameters.Add(new NpgsqlParameter("count", NpgsqlDbType.Integer) { Value = count });
                command.Parameters.Add(new NpgsqlParameter("claimTimeoutMinutes", NpgsqlDbType.Integer) { Value = claimTimeoutMinutes });
                command.Parameters.Add(new NpgsqlParameter("maxAttempts", NpgsqlDbType.Integer) { Value = maxAttempts });

                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    Building2DReference building2DReference = new()
                    {
                        Id = reader.GetInt64(0),
                        CountyId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        Reference = reader.IsDBNull(2) ? null : reader.GetString(2),
                        SubdivisionId = reader.IsDBNull(3) ? null : reader.GetInt32(3)
                    };

                    result.Add(building2DReference);
                }
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed while claiming {Count} references", nameof(GetNextBuilding2DReferencesAsync), count);
                return null;
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves and claims a batch of building 2D references from the update queue.
        /// <para>Rows are atomically claimed by updating <c>claimed_at</c> to the current timestamp rather than deleting them immediately. If the claim is not acknowledged within <paramref name="claimTimeoutMinutes"/> minutes, the rows automatically become available for subsequent claims.</para>
        /// </summary>
        /// <param name="count">The maximum number of <see cref="Building2DReference"/> objects to retrieve.</param>
        /// <param name="claimTimeoutMinutes">The duration in minutes before an unacknowledged claim expires and returns to the queue. Defaults to 30.</param>
        /// <param name="maxAttempts">The maximum number of claim attempts before a reference is retired as a poison row. Defaults to 5.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of claimed <see cref="Building2DReference"/> objects, or null if the table does not exist or an error occurs.</returns>
        public async Task<List<Building2DReference>?> GetNextBuilding2DReferencesAsync(
            int count = 100,
            int claimTimeoutMinutes = 30,
            int maxAttempts = 5,
            int commandTimeout = 60,
            CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetNextBuilding2DReferencesAsync(npgsqlConnection, count, claimTimeoutMinutes, maxAttempts, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously acknowledges and deletes completed building 2D references from the update queue.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="ids">The collection of queue entry identifiers to acknowledge and remove from the queue.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of deleted queue entries, or -1 if an error occurs.</returns>
        public static async Task<long> AcknowledgeBuilding2DReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<long>? ids, int commandTimeout = 60, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ids is null)
            {
                return -1;
            }

            long[] ids_Array = [.. ids];
            if (ids_Array.Length == 0)
            {
                return 0;
            }

            // The DDL rather than a mere existence check, for the same reason the claim runs it: a queue
            // table predating the claim column has to be brought up to the current shape by whichever path
            // reaches it first. Every statement in it is conditional, so this is a catalog lookup.
            if (!await Create.TableAsync_Building2DReference(npgsqlConnection, TableName.OrtoDatas_Building2DReference_Update, commandTimeout, cancellationToken))
            {
                return -1;
            }

            string commandText = $@"
                DELETE FROM {TableName.OrtoDatas_Building2DReference_Update}
                WHERE id = ANY(@ids);";

            try
            {
                await using NpgsqlCommand command = new(commandText, npgsqlConnection);
                command.CommandTimeout = commandTimeout;
                command.Parameters.Add(new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = ids_Array });

                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed while acknowledging {Count} references", nameof(AcknowledgeBuilding2DReferencesAsync), ids_Array.Length);
                return -1;
            }
        }

        /// <summary>
        /// Asynchronously acknowledges and deletes completed building 2D references from the update queue.
        /// </summary>
        /// <param name="ids">The collection of queue entry identifiers to acknowledge and remove from the queue.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of deleted queue entries, or -1 if an error occurs.</returns>
        public async Task<long> AcknowledgeBuilding2DReferencesAsync(IEnumerable<long>? ids, int commandTimeout = 60, CancellationToken cancellationToken = default)
        {
            if (ids is null)
            {
                return -1;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await AcknowledgeBuilding2DReferencesAsync(npgsqlConnection, ids, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves orthodata based on a specified reference and an optional county identifier.
        /// </summary>
        /// <param name="reference">The string reference used to identify the orthodata.</param>
        /// <param name="countyId">The optional integer identifier of the county.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering if not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="OrtoDatas"/> object if found; otherwise, null.</returns>
        public async Task<OrtoDatas?> GetOrtoDatasByReferenceAsync(string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return await GetOrtoDatasByReferencesAsync([reference], countyId, fallbackByReference, commandTimeout, cancellationToken).ContinueWith(t => t.Result?.FirstOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatas"/> based on the specified references and county identifier.
        /// </summary>
        /// <param name="references">An optional collection of strings representing the references to filter by.</param>
        /// <param name="countyId">An optional integer representing the unique identifier of the county.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatas"/> objects, or null if no matching data is found.</returns>
        public async Task<List<OrtoDatas>?> GetOrtoDatasByReferencesAsync(IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetOrtoDatasByReferencesAsync(npgsqlConnection, references, countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatas"/> based on the specified building 2D references.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="building2DReferences">A collection of <see cref="Building2DReference"/> objects to search for.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions for references not matched by county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatas"/> objects, or null if the connection is null or no matching data is found.</returns>
        public static async Task<List<OrtoDatas>?> GetOrtoDatasByBuilding2DReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || building2DReferences is null)
            {
                return null;
            }

            List<Building2DReference> building2DReferences_List = [.. building2DReferences];
            if (building2DReferences_List.Count == 0)
            {
                return [];
            }

            int[] countyIds = [.. building2DReferences_List.Select(l => l.CountyId ?? 0)];
            string[] references = [.. building2DReferences_List.Select(l => l.Reference ?? string.Empty)];

            const string commandText = $@"
                SELECT u.id, u.county_id, u.reference, u.min_x, u.min_y, u.max_x, u.max_y, u.subdivision_id, u.object, u.created_at
                FROM UNNEST(@counties, @refs) AS input(c, r)
                INNER JOIN {TableName.OrtoDatas} u ON (input.c = 0 OR u.county_id = input.c) AND u.reference = input.r;";

            List<OrtoDatas>? result;

            await using (NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection))
            {
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.AddWithValue("counties", countyIds);
                npgsqlCommand.Parameters.AddWithValue("refs", references);

                result = await ReadAsync_OrtoDatas(npgsqlCommand, cancellationToken);
            }

            if (result is null)
            {
                return null;
            }

            if (fallbackByReference)
            {
                HashSet<string> foundReferences = [.. result.Where(o => !string.IsNullOrWhiteSpace(o.Reference)).Select(o => o.Reference!)];
                string[] missingReferences = [.. references.Where(r => !string.IsNullOrWhiteSpace(r) && !foundReferences.Contains(r)).Distinct()];

                if (missingReferences.Length > 0)
                {
                    List<OrtoDatas>? fallbackItems = await GetOrtoDatasByReferencesAsync(npgsqlConnection, missingReferences, null, false, commandTimeout, cancellationToken);
                    if (fallbackItems is not null && fallbackItems.Count > 0)
                    {
                        result.AddRange(fallbackItems);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatas"/> based on the specified building 2D references.
        /// </summary>
        /// <param name="building2DReferences">A collection of <see cref="Building2DReference"/> objects to search for.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions for references not matched by county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatas"/> objects, or null if no matching data is found.</returns>
        public async Task<List<OrtoDatas>?> GetOrtoDatasByBuilding2DReferencesAsync(IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DReferences is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetOrtoDatasByBuilding2DReferencesAsync(npgsqlConnection, building2DReferences, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves orthodata based on a specified building 2D reference.
        /// </summary>
        /// <param name="building2DReference">The <see cref="Building2DReference"/> used to identify the orthodata.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions if not matched by county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="OrtoDatas"/> object if found; otherwise, null.</returns>
        public async Task<OrtoDatas?> GetOrtoDatasByBuilding2DReferenceAsync(Building2DReference? building2DReference, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DReference is null || string.IsNullOrWhiteSpace(building2DReference.Reference))
            {
                return null;
            }

            List<OrtoDatas>? results = await GetOrtoDatasByBuilding2DReferencesAsync([building2DReference], fallbackByReference, commandTimeout, cancellationToken);
            return results?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatasReference"/> objects for a specified county, with optional filtering by subdivision identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatasReference"/> objects, or null if the connection is null.</returns>
        public static async Task<List<OrtoDatasReference>?> GetOrtoDatasReferencesByCountyIdAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            int[]? subdivisionIds_Array = subdivisionIds?.ToArray();
            bool hasSubdivisionIds = subdivisionIds_Array != null && subdivisionIds_Array.Length > 0;

            string commandText = $@"
                SELECT id, county_id, reference, min_x, min_y, max_x, max_y, subdivision_id, created_at
                FROM {TableName.OrtoDatas}
                WHERE county_id = @county_id{(hasSubdivisionIds ? " AND (subdivision_id = ANY(@subdivision_ids) OR subdivision_id IS NULL)" : "")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
            if (hasSubdivisionIds)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivision_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = subdivisionIds_Array });
            }

            return await ReadAsync_OrtoDatasReference(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatasReference"/> objects for a specified county, with optional filtering by subdivision identifiers.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatasReference"/> objects, or null if connection fails.</returns>
        public async Task<List<OrtoDatasReference>?> GetOrtoDatasReferencesByCountyIdAsync(int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetOrtoDatasReferencesByCountyIdAsync(npgsqlConnection, countyId, subdivisionIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatasReference"/> objects based on the specified references and optional county identifier, omitting the binary JSON payload.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="references">A collection of strings representing the references to search for.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the results.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatasReference"/> objects, or null if the connection is null or input references are null.</returns>
        public static async Task<List<OrtoDatasReference>?> GetOrtoDatasReferencesByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            string[] references_Array = [.. references];
            if (references_Array.Length == 0)
            {
                return [];
            }

            string commandText = countyId.HasValue ? $@"
                SELECT id, county_id, reference, min_x, min_y, max_x, max_y, subdivision_id, created_at
                FROM {TableName.OrtoDatas}
                WHERE county_id = @countyId
                  AND reference = ANY(@references);" : $@"
                SELECT id, county_id, reference, min_x, min_y, max_x, max_y, subdivision_id, created_at
                FROM {TableName.OrtoDatas}
                WHERE reference = ANY(@references);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });
            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            List<OrtoDatasReference>? result = await ReadAsync_OrtoDatasReference(npgsqlCommand, cancellationToken);
            if (result is null)
            {
                return null;
            }

            if (fallbackByReference && countyId is not null)
            {
                HashSet<string> foundReferences = [.. result.Where(o => !string.IsNullOrWhiteSpace(o.Reference)).Select(o => o.Reference!)];
                string[] missingReferences = [.. references_Array.Where(r => !string.IsNullOrWhiteSpace(r) && !foundReferences.Contains(r)).Distinct()];

                if (missingReferences.Length > 0)
                {
                    List<OrtoDatasReference>? fallbackItems = await GetOrtoDatasReferencesByReferencesAsync(npgsqlConnection, missingReferences, null, false, commandTimeout, cancellationToken);
                    if (fallbackItems is not null && fallbackItems.Count > 0)
                    {
                        result.AddRange(fallbackItems);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatasReference"/> objects based on the specified references and optional county identifier.
        /// </summary>
        /// <param name="references">An optional collection of strings representing the references to filter by.</param>
        /// <param name="countyId">An optional integer representing the unique identifier of the county.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatasReference"/> objects, or null if references are null or connection fails.</returns>
        public async Task<List<OrtoDatasReference>?> GetOrtoDatasReferencesByReferencesAsync(IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetOrtoDatasReferencesByReferencesAsync(npgsqlConnection, references, countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an <see cref="OrtoDatasReference"/> object based on the specified reference and optional county identifier.
        /// </summary>
        /// <param name="reference">The reference string to search for.</param>
        /// <param name="countyId">An optional integer representing the unique identifier of the county.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering if not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="OrtoDatasReference"/> if found; otherwise, null.</returns>
        public async Task<OrtoDatasReference?> GetOrtoDatasReferenceByReferenceAsync(string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return await GetOrtoDatasReferencesByReferencesAsync([reference], countyId, fallbackByReference, commandTimeout, cancellationToken).ContinueWith(t => t.Result?.FirstOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatasReference"/> objects based on the specified building 2D references.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="building2DReferences">A collection of <see cref="Building2DReference"/> objects to search for.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions for references not matched by county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatasReference"/> objects, or null if the connection is null or input references are null.</returns>
        public static async Task<List<OrtoDatasReference>?> GetOrtoDatasReferencesByBuilding2DReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || building2DReferences is null)
            {
                return null;
            }

            List<Building2DReference> building2DReferences_List = [.. building2DReferences];
            if (building2DReferences_List.Count == 0)
            {
                return [];
            }

            int[] countyIds = [.. building2DReferences_List.Select(l => l.CountyId ?? 0)];
            string[] references = [.. building2DReferences_List.Select(l => l.Reference ?? string.Empty)];

            const string commandText = $@"
                SELECT u.id, u.county_id, u.reference, u.min_x, u.min_y, u.max_x, u.max_y, u.subdivision_id, u.created_at
                FROM UNNEST(@counties, @refs) AS input(c, r)
                INNER JOIN {TableName.OrtoDatas} u ON (input.c = 0 OR u.county_id = input.c) AND u.reference = input.r;";

            List<OrtoDatasReference>? result;

            await using (NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection))
            {
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.AddWithValue("counties", countyIds);
                npgsqlCommand.Parameters.AddWithValue("refs", references);

                result = await ReadAsync_OrtoDatasReference(npgsqlCommand, cancellationToken);
            }

            if (result is null)
            {
                return null;
            }

            if (fallbackByReference)
            {
                HashSet<string> foundReferences = [.. result.Where(o => !string.IsNullOrWhiteSpace(o.Reference)).Select(o => o.Reference!)];
                string[] missingReferences = [.. references.Where(r => !string.IsNullOrWhiteSpace(r) && !foundReferences.Contains(r)).Distinct()];

                if (missingReferences.Length > 0)
                {
                    List<OrtoDatasReference>? fallbackItems = await GetOrtoDatasReferencesByReferencesAsync(npgsqlConnection, missingReferences, null, false, commandTimeout, cancellationToken);
                    if (fallbackItems is not null && fallbackItems.Count > 0)
                    {
                        result.AddRange(fallbackItems);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="OrtoDatasReference"/> objects based on the specified building 2D references.
        /// </summary>
        /// <param name="building2DReferences">A collection of <see cref="Building2DReference"/> objects to search for.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions for references not matched by county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="OrtoDatasReference"/> objects, or null if no matching data is found.</returns>
        public async Task<List<OrtoDatasReference>?> GetOrtoDatasReferencesByBuilding2DReferencesAsync(IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DReferences is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetOrtoDatasReferencesByBuilding2DReferencesAsync(npgsqlConnection, building2DReferences, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an <see cref="OrtoDatasReference"/> based on a specified building 2D reference.
        /// </summary>
        /// <param name="building2DReference">The <see cref="Building2DReference"/> used to identify the orthodata reference.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone across all partitions if not matched by county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="OrtoDatasReference"/> object if found; otherwise, null.</returns>
        public async Task<OrtoDatasReference?> GetOrtoDatasReferenceByBuilding2DReferenceAsync(Building2DReference? building2DReference, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DReference is null || string.IsNullOrWhiteSpace(building2DReference.Reference))
            {
                return null;
            }

            List<OrtoDatasReference>? results = await GetOrtoDatasReferencesByBuilding2DReferencesAsync([building2DReference], fallbackByReference, commandTimeout, cancellationToken);
            return results?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves an <see cref="OrtoDatasReference"/> object by its unique ID and optional county identifier.
        /// </summary>
        /// <param name="id">The unique ID of the orthodata record.</param>
        /// <param name="countyId">The optional county identifier used to narrow the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="OrtoDatasReference"/> if found; otherwise, null.</returns>
        public async Task<OrtoDatasReference?> GetOrtoDatasReferenceByIdAsync(long id, int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string commandText = $@"
                SELECT id, county_id, reference, min_x, min_y, max_x, max_y, subdivision_id, created_at
                FROM {TableName.OrtoDatas}
                WHERE id = @id{(countyId is null ? "" : " AND county_id = @countyId")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("id", id);
            if (countyId is not null)
            {
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId.Value);
            }

            List<OrtoDatasReference>? results = await ReadAsync_OrtoDatasReference(npgsqlCommand, cancellationToken);
            return results?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously updates the data based on the provided orthodata and tolerance.
        /// <para>An entry that names no county row is resolved by geometry against every county its bounding box overlaps. When the county is known but which of its polygon parts is not, use <see cref="UpdateAsync(IEnumerable{OrtoDatas}, IEnumerable{int}, double, int, CancellationToken)"/> and hand over the parts - it narrows the field before the geometry runs.</para>
        /// </summary>
        /// <param name="ortoDatas">A nullable enumerable collection of <see cref="OrtoDatas"/> to be processed for the update.</param>
        /// <param name="tolerance">A double-precision floating-point number representing the distance tolerance used during the update process. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers written and the rows dropped before the database, or null when the update could not be attempted at all - no connection, or the table could not be created.</returns>
        public async Task<PostgreSQLUpdateResult?> UpdateAsync(IEnumerable<OrtoDatas>? ortoDatas, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            return await UpdateAsync(ortoDatas, (IEnumerable<int>?)null, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the data based on the provided orthodata and tolerance, resolving the county of an entry that names none from the given candidate rows.
        /// <para>County assignment runs in three tiers, in descending reliability:</para>
        /// <para>1. the entry's own county identifier - already names the row, nothing to infer.</para>
        /// <para>2. <paramref name="countyIds"/> - a county code names one row per polygon part, so the caller can state the county without stating the part. A single candidate resolves outright; several only narrow the field and are handed to tier 3.</para>
        /// <para>3. geometry - <see cref="Query.CountyId(IDictionary{int, Geometry.Planar.Interfaces.IPolygonal2D}, Geometry.Planar.Interfaces.IPolygonal2D, double)"/> decides which candidate the entry lies in, is nearest to, or overlaps most. With no candidates the field is every county its bounding box overlaps, which is both slower and wider than narrowing first.</para>
        /// </summary>
        /// <param name="ortoDatas">A nullable enumerable collection of <see cref="OrtoDatas"/> to be processed for the update.</param>
        /// <param name="countyIds">The candidate county rows an entry with no county of its own may be filed under. Null or empty searches every county overlapping the entry instead.</param>
        /// <param name="tolerance">A double-precision floating-point number representing the distance tolerance used during the update process. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers written and the rows dropped before the database, or null when the update could not be attempted at all - no connection, or the table could not be created.</returns>
        public async Task<PostgreSQLUpdateResult?> UpdateAsync(IEnumerable<OrtoDatas>? ortoDatas, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (ortoDatas is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool succeded = await Create.TableAsync_OrtoDatas(npgsqlConnection, commandTimeout, cancellationToken);
            if (!succeded)
            {
                return null;
            }

            HashSet<long> ids = [];
            List<Rejection> rejections = [];

            if (!ortoDatas.Any())
            {
                return new PostgreSQLUpdateResult(ids, rejections);
            }

            List<int> countyIds_Candidate = countyIds is null ? [] : [.. new HashSet<int>(countyIds)];

            // Derived once for the whole batch: a part's polygon is the same for every entry tested against
            // it, and deriving one deserializes a county-sized geometry. Left null while there is nothing to
            // choose between, so a single candidate costs no polygon at all.
            Dictionary<int, Geometry.Planar.Interfaces.IPolygonal2D>? polygonal2Ds_ByCountyId = null;
            if (countyIds_Candidate.Count > 1)
            {
                polygonal2Ds_ByCountyId = (await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(npgsqlConnection, countyIds_Candidate, commandTimeout, cancellationToken)).Polygonal2DsByCountyId();
            }

            Dictionary<int, List<OrtoDatas>> dictionary_OrtoDatas = [];

            foreach (OrtoDatas ortoDatas_Temp in ortoDatas)
            {
                if (ortoDatas_Temp is null)
                {
                    // Recorded rather than skipped in silence, so the rejection count stays an exact
                    // account of the rows that never reached the database.
                    rejections.Add(new Rejection(null, Enums.UpdateRejectionReason.Undefined));
                    continue;
                }

                int? countyId = ortoDatas_Temp.CountyId;

                if (countyId is null || !countyId.HasValue)
                {
                    if (countyIds_Candidate.Count == 1)
                    {
                        countyId = countyIds_Candidate[0];
                    }
                    else
                    {
                        BoundingBox2D? boundingBox2D = ortoDatas_Temp.BoundingBox2D;
                        if (boundingBox2D is null)
                        {
                            // No county was named and there is no geometry to infer one from, so resolution
                            // never even starts. A defect in the posted payload, unlike the tiers below.
                            rejections.Add(new Rejection(ortoDatas_Temp.Reference, Enums.UpdateRejectionReason.MissingGeometry));
                            continue;
                        }

                        Geometry.Planar.Interfaces.IPolygonal2D? polygonal2D = (Polygon2D?)boundingBox2D;

                        if (polygonal2Ds_ByCountyId is not null)
                        {
                            countyId = polygonal2Ds_ByCountyId.CountyId(polygonal2D, tolerance);
                        }
                        else
                        {
                            // The bounding box path answers a different candidate set per entry, so there is
                            // nothing to cache across them.
                            List<AdministrativeAreal2D>? administrativeAreal2Ds = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, Enums.AdministrativeArealType.County, tolerance, commandTimeout, cancellationToken);
                            if (administrativeAreal2Ds is not null && administrativeAreal2Ds.Count != 0)
                            {
                                // A single overlapping county is taken as it stands: the bounding box search
                                // already proved the overlap from the stored extents, so a row whose geometry
                                // cannot be deserialized must not cost the entry its only candidate.
                                countyId = administrativeAreal2Ds.Count == 1 ? administrativeAreal2Ds[0].Id : administrativeAreal2Ds.CountyId(polygonal2D, tolerance);
                            }
                        }
                    }
                }

                if (countyId is null || !countyId.HasValue)
                {
                    // Resolution ran and named no part. The row is dropped, and naming it here is the only
                    // place that can - nothing downstream knows which rows never made the batch.
                    rejections.Add(new Rejection(ortoDatas_Temp.Reference, Enums.UpdateRejectionReason.CountyUnresolved));
                    continue;
                }

                if (!dictionary_OrtoDatas.TryGetValue(countyId.Value, out List<OrtoDatas>? ortoDatas_CountyId) || ortoDatas_CountyId is null)
                {
                    ortoDatas_CountyId = [];
                    dictionary_OrtoDatas[countyId.Value] = ortoDatas_CountyId;
                }

                ortoDatas_CountyId.Add(ortoDatas_Temp);
            }

            await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection);
            npgsqlBatch.Timeout = commandTimeout;

            foreach (KeyValuePair<int, List<OrtoDatas>> keyValuePair in dictionary_OrtoDatas)
            {
                int countyId = keyValuePair.Key;

                succeded = await Create.TableAsync_OrtoDatas_Partition(npgsqlConnection, countyId, commandTimeout, cancellationToken);
                if (!succeded)
                {
                    // A whole county's worth of rows disappears here, and it is the one drop that is never
                    // the caller's doing.
                    foreach (OrtoDatas ortoDatas_Rejected in keyValuePair.Value)
                    {
                        rejections.Add(new Rejection(ortoDatas_Rejected.Reference, Enums.UpdateRejectionReason.PartitionUnavailable));
                    }

                    continue;
                }

                foreach (OrtoDatas ortoDatas_Temp in keyValuePair.Value)
                {
                    // SQL with full update on conflict (excluding ID)
                    NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                    INSERT INTO {TableName.OrtoDatas} (county_id, reference, min_x, min_y, max_x, max_y, subdivision_id, object)
                    VALUES (@county_id, @reference, @min_x, @min_y, @max_x, @max_y, @subdivision_id, @object)
                    ON CONFLICT (reference, county_id)
                    DO UPDATE SET
                        min_x = EXCLUDED.min_x,
                        min_y = EXCLUDED.min_y,
                        max_x = EXCLUDED.max_x,
                        max_y = EXCLUDED.max_y,
                        subdivision_id = COALESCE(EXCLUDED.subdivision_id, {TableName.OrtoDatas}.subdivision_id),
                        object = EXCLUDED.object
                    RETURNING id;");

                    BoundingBox2D? boundingBox2D = ortoDatas_Temp.BoundingBox2D;

                    // Adding parameters with explicit NpgsqlDbType
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = ortoDatas_Temp.Reference });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Min.X });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Min.Y });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Max.X });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Max.Y });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("subdivision_id", NpgsqlDbType.Integer) { Value = (object?)ortoDatas_Temp.SubdivisionId ?? DBNull.Value });

                    // Handling potential null for JSONB object
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb)
                    {
                        Value = (object?)ortoDatas_Temp.Object?.ToJsonString() ?? DBNull.Value
                    });

                    npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
                }
            }

            // Every row was dropped, so there is nothing to execute. Npgsql 10.0.2 answers a zero-command
            // batch with an empty reader rather than throwing, which is precisely why the all-dropped case
            // used to be indistinguishable from an unreachable database: both returned an empty id set. The
            // rejections tell them apart now, and the round trip buys nothing.
            if (npgsqlBatch.BatchCommands.Count == 0)
            {
                return new PostgreSQLUpdateResult(ids, rejections);
            }

            // Execute batch and collect IDs
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync(cancellationToken);

            do
            {
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    // The RETURNING id works for both INSERT and UPDATE cases
                    long id = npgsqlDataReader.GetInt64(0);
                    ids.Add(id);
                }
            }
            while (await npgsqlDataReader.NextResultAsync(cancellationToken));

            return new PostgreSQLUpdateResult(ids, rejections);
        }

        /// <summary>
        /// Asynchronously enqueues the specified collection of building 2D references for an orthophoto download.
        /// <para>The rows land in <see cref="TableName.OrtoDatas_Building2DReference_Update"/>, the queue drained by the download task, so this schedules work rather than storing any orthophoto data of its own.</para>
        /// <para>A reference carrying no county is dropped rather than filed under county 0: the queue's county column is the address the download writes its result back to, and 0 is not a county.</para>
        /// </summary>
        /// <param name="building2DReferences">An <see cref="IEnumerable{Building2DReference}"/> containing the building 2D references to update.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{Building2DReference}"/> of the updated references, or null if the operation failed or the input collection was null.</returns>
        public async Task<List<Building2DReference>?> UpdateBuilding2DReferencesAsync(IEnumerable<Building2DReference> building2DReferences, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (building2DReferences is null)
            {
                return null;
            }

            // A reference with no county has nowhere to be filed, and county 0 is not a partition anyone
            // reads back. Dropped here rather than written as a row nothing can act on.
            List<Building2DReference> building2DReferences_List = [.. building2DReferences.Where(x => x is not null && x.CountyId.HasValue && !string.IsNullOrWhiteSpace(x.Reference))];
            if (building2DReferences_List.Count == 0)
            {
                return [];
            }

            // Ensure we have a valid connection
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            // Check if table exists before proceeding
            bool created = await Create.TableAsync_Building2DReference(npgsqlConnection, TableName.OrtoDatas_Building2DReference_Update, commandTimeout, cancellationToken);
            if (!created)
            {
                return null;
            }

            // ON CONFLICT (county_id, reference) DO NOTHING ensures we don't add duplicates.
            // RETURNING * will only return rows that were actually inserted.
            string sql = $@"
                INSERT INTO {TableName.OrtoDatas_Building2DReference_Update} (county_id, reference, subdivision_id)
                SELECT * FROM UNNEST(@counties, @refs, @subs)
                ON CONFLICT (county_id, reference) DO NOTHING
                RETURNING id, county_id, reference, subdivision_id;";

            List<Building2DReference> result = [];

            try
            {
                await using NpgsqlCommand command = new(sql, npgsqlConnection);
                command.CommandTimeout = commandTimeout;

                // Preparing arrays for PostgreSQL UNNEST to avoid multiple INSERT calls (optimization)
                int[] countyIds = [.. building2DReferences_List.Select(x => x.CountyId!.Value)];
                string[] references = [.. building2DReferences_List.Select(x => x.Reference!)];
                int?[] subdivisionIds = [.. building2DReferences_List.Select(x => x.SubdivisionId)];

                command.Parameters.AddWithValue("counties", countyIds);
                command.Parameters.AddWithValue("refs", references);
                command.Parameters.AddWithValue("subs", subdivisionIds);

                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    Building2DReference building2DReference = new()
                    {
                        Id = reader.GetInt64(reader.GetOrdinal("id")),
                        CountyId = reader.IsDBNull(reader.GetOrdinal("county_id")) ? null : reader.GetInt32(reader.GetOrdinal("county_id")),
                        Reference = reader.IsDBNull(reader.GetOrdinal("reference")) ? null : reader.GetString(reader.GetOrdinal("reference")),
                        SubdivisionId = reader.IsDBNull(reader.GetOrdinal("subdivision_id")) ? null : reader.GetInt32(reader.GetOrdinal("subdivision_id"))
                    };

                    result.Add(building2DReference);
                }
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed while enqueuing {Count} references", nameof(UpdateBuilding2DReferencesAsync), building2DReferences_List.Count);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Asynchronously pushes the subdivision identifier each reference carries onto the matching stored row.
        /// <para>An entry whose <see cref="Building2DReference.SubdivisionId"/> is null is left out of the write entirely, and the statement guards the same case again with a <c>COALESCE</c>. A null there means the building's subdivision has not been resolved yet, not that it has none, so writing it through would clear a subdivision resolved by an earlier run - the defect issue #23 fixed on <c>building_2d</c> and issue #31 on this table.</para>
        /// <para>Naming <paramref name="countyId"/> confines the statement to a single partition. Without it the join is planned against the whole partitioned table, because a county carried per row in an unnested array is not something the planner can prune on.</para>
        /// </summary>
        /// <param name="building2DReferences">An <see cref="IEnumerable{Building2DReference}"/> containing the building 2D references to be updated, or null.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search across partitions by reference alone for references whose county identifier was mismatched.</param>
        /// <param name="countyId">The county the write is confined to, or null to let each entry name its own.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{Building2DReference}"/> of the rows actually written, or null if the input collection was null or no connection could be built.</returns>
        public async Task<List<Building2DReference>?> UpdateSubdivisionIdsAsync(IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int? countyId = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (building2DReferences == null)
            {
                return null;
            }

            // Only an entry that names a subdivision can do anything here. Dropping the rest is what keeps a
            // building whose subdivision has not been resolved from clearing a stored one, and it is also what
            // keeps the statement small - most of building_2d carries no subdivision link yet.
            List<Building2DReference> building2DReferences_List = [.. building2DReferences.Where(x => x is not null && x.SubdivisionId.HasValue)];
            if (building2DReferences_List.Count == 0)
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            // Separated into the entries that name their county and the ones that do not
            List<Building2DReference> withCounty = [.. building2DReferences_List.Where(x => x.CountyId.HasValue)];
            List<Building2DReference> withoutCounty = [.. building2DReferences_List.Where(x => !x.CountyId.HasValue)];

            // An entry with no county has to have one read back before the update can be addressed
            if (withoutCounty.Count > 0)
            {
                Dictionary<string, Building2DReference> building2DReferences_ByReference = [];
                foreach (Building2DReference building2DReference_WithoutCounty in withoutCounty)
                {
                    if (string.IsNullOrWhiteSpace(building2DReference_WithoutCounty.Reference))
                    {
                        continue;
                    }

                    building2DReferences_ByReference[building2DReference_WithoutCounty.Reference] = building2DReference_WithoutCounty;
                }

                if (building2DReferences_ByReference.Count != 0)
                {
                    string[] missingReferences = [.. building2DReferences_ByReference.Keys];

                    string findSql = $@"
                        SELECT reference, county_id
                        FROM {TableName.OrtoDatas}
                        WHERE reference = ANY(@refs){(countyId.HasValue ? " AND county_id = @countyId" : string.Empty)}";

                    await using NpgsqlCommand npgsqlCommand = new(findSql, npgsqlConnection);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.AddWithValue("refs", missingReferences);
                    if (countyId.HasValue)
                    {
                        npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
                    }

                    await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                    while (await npgsqlDataReader.ReadAsync(cancellationToken))
                    {
                        // Taken out of the dictionary as it is matched, so a reference held by more than one
                        // polygon part of a county resolves once rather than being queued for every part.
                        if (!building2DReferences_ByReference.Remove(npgsqlDataReader.GetString(0), out Building2DReference? building2DReference_Match) || building2DReference_Match is null)
                        {
                            continue;
                        }

                        // Copied rather than assigned into: the caller handed these over to be read, and a
                        // county filled in here would otherwise change what the caller sees afterwards.
                        withCounty.Add(new Building2DReference(building2DReference_Match) { CountyId = npgsqlDataReader.GetInt32(1) });
                    }

                    await npgsqlDataReader.CloseAsync();
                }
            }

            if (withCounty.Count == 0)
            {
                return [];
            }

            // COALESCE guards the case the filter above has already removed. Both are kept: the filter spares
            // the database a write that could only ever be a no-op or a loss, and the COALESCE protects the row
            // from any future caller that does not filter.
            string updateSql = $@"
                UPDATE {TableName.OrtoDatas} u
                SET subdivision_id = COALESCE(data.new_sub_id, u.subdivision_id)
                FROM (
                    SELECT * FROM UNNEST(@counties, @refs, @subs) AS t(c_id, r_text, new_sub_id)
                ) AS data
                WHERE u.county_id = data.c_id{(countyId.HasValue ? " AND u.county_id = @countyId" : string.Empty)}
                  AND u.reference = data.r_text
                RETURNING u.id, u.county_id, u.reference, u.subdivision_id;";

            List<Building2DReference> result = [];

            try
            {
                await using NpgsqlCommand command = new(updateSql, npgsqlConnection);
                command.CommandTimeout = commandTimeout;

                command.Parameters.AddWithValue("counties", withCounty.Select(x => x.CountyId ?? 0).ToArray());
                command.Parameters.AddWithValue("refs", withCounty.Select(x => x.Reference ?? string.Empty).ToArray());
                command.Parameters.AddWithValue("subs", withCounty.Select(x => x.SubdivisionId).ToArray());
                if (countyId.HasValue)
                {
                    command.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
                }

                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(new Building2DReference
                    {
                        Id = reader.GetInt64(0),
                        CountyId = reader.GetInt32(1),
                        Reference = reader.GetString(2),
                        SubdivisionId = reader.IsDBNull(3) ? null : (int?)reader.GetInt32(3)
                    });
                }
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed while writing {Count} subdivision identifiers", nameof(UpdateSubdivisionIdsAsync), withCounty.Count);
                throw;
            }

            if (fallbackByReference)
            {
                HashSet<string> updatedRefs = [.. result.Where(r => !string.IsNullOrWhiteSpace(r.Reference)).Select(r => r.Reference!)];

                Dictionary<string, Building2DReference> building2DReferences_Remaining = [];
                foreach (Building2DReference building2DReference_WithCounty in withCounty)
                {
                    if (string.IsNullOrWhiteSpace(building2DReference_WithCounty.Reference) || updatedRefs.Contains(building2DReference_WithCounty.Reference))
                    {
                        continue;
                    }

                    building2DReferences_Remaining[building2DReference_WithCounty.Reference] = building2DReference_WithCounty;
                }

                if (building2DReferences_Remaining.Count > 0)
                {
                    string[] remainingRefs = [.. building2DReferences_Remaining.Keys];

                    // No county filter: this searches the partitions the county-addressed statement above has
                    // already failed to match, which is the whole point of the fallback.
                    string fallbackFindSql = $@"
                        SELECT reference, county_id
                        FROM {TableName.OrtoDatas}
                        WHERE reference = ANY(@refs)";

                    List<Building2DReference> fallbackResolved = [];

                    await using (NpgsqlCommand fallbackFindCmd = new(fallbackFindSql, npgsqlConnection))
                    {
                        fallbackFindCmd.CommandTimeout = commandTimeout;
                        fallbackFindCmd.Parameters.AddWithValue("refs", remainingRefs);

                        await using NpgsqlDataReader fallbackFindReader = await fallbackFindCmd.ExecuteReaderAsync(cancellationToken);
                        while (await fallbackFindReader.ReadAsync(cancellationToken))
                        {
                            if (!building2DReferences_Remaining.TryGetValue(fallbackFindReader.GetString(0), out Building2DReference? building2DReference_Match) || building2DReference_Match is null)
                            {
                                continue;
                            }

                            fallbackResolved.Add(new Building2DReference
                            {
                                Reference = building2DReference_Match.Reference,
                                CountyId = fallbackFindReader.GetInt32(1),
                                SubdivisionId = building2DReference_Match.SubdivisionId
                            });
                        }
                    }

                    if (fallbackResolved.Count > 0)
                    {
                        // Deliberately the unconfined form of the statement: the counties the fallback found are
                        // by definition not the one the caller named.
                        string fallbackUpdateSql = $@"
                            UPDATE {TableName.OrtoDatas} u
                            SET subdivision_id = COALESCE(data.new_sub_id, u.subdivision_id)
                            FROM (
                                SELECT * FROM UNNEST(@counties, @refs, @subs) AS t(c_id, r_text, new_sub_id)
                            ) AS data
                            WHERE u.county_id = data.c_id
                              AND u.reference = data.r_text
                            RETURNING u.id, u.county_id, u.reference, u.subdivision_id;";

                        await using NpgsqlCommand fallbackUpdateCmd = new(fallbackUpdateSql, npgsqlConnection);
                        fallbackUpdateCmd.CommandTimeout = commandTimeout;

                        fallbackUpdateCmd.Parameters.AddWithValue("counties", fallbackResolved.Select(x => x.CountyId ?? 0).ToArray());
                        fallbackUpdateCmd.Parameters.AddWithValue("refs", fallbackResolved.Select(x => x.Reference ?? string.Empty).ToArray());
                        fallbackUpdateCmd.Parameters.AddWithValue("subs", fallbackResolved.Select(x => x.SubdivisionId).ToArray());

                        await using NpgsqlDataReader fallbackUpdateReader = await fallbackUpdateCmd.ExecuteReaderAsync(cancellationToken);
                        while (await fallbackUpdateReader.ReadAsync(cancellationToken))
                        {
                            result.Add(new Building2DReference
                            {
                                Id = fallbackUpdateReader.GetInt64(0),
                                CountyId = fallbackUpdateReader.GetInt32(1),
                                Reference = fallbackUpdateReader.GetString(2),
                                SubdivisionId = fallbackUpdateReader.IsDBNull(3) ? null : (int?)fallbackUpdateReader.GetInt32(3)
                            });
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously summarises what each of the named county partitions holds: how many rows, how many name a subdivision, how many distinct subdivisions they are spread across, and when they were written.
        /// <para>One aggregate per partition rather than a row per building, so it costs the same whether a county holds a thousand rows or a hundred thousand. It is the cheap way to ask the question <see cref="Query.SubdivisionLinksAsync(OrtoDatasPostgreSQLConverter?, Building2DPostgreSQLConverter?, int, int, int, CancellationToken)"/> answers exactly, and the figure to record either side of a refresh.</para>
        /// <para>Naming no county summarises every partition. Counties holding no row are absent from the result rather than present with a zero.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The identifiers of the county partitions to summarise. Null summarises every one.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one <see cref="OrtoDatasCountyResult"/> per county holding rows, or null when the connection is null or nothing has ever been stored.</returns>
        public static async Task<List<OrtoDatasCountyResult>?> GetSummariesByCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Answered before the aggregate is sent. A store nothing has ever been written to has no table at
            // all, and running the query against it raises an undefined relation - which reaches a caller as a
            // server fault rather than as the plain fact that there is nothing stored yet.
            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName.OrtoDatas))
            {
                return null;
            }

            int[]? countyIds_Array = countyIds is null ? null : [.. countyIds];

            string commandText = $@"
                SELECT
                    county_id,
                    COUNT(*),
                    COUNT(*) FILTER (WHERE subdivision_id IS NOT NULL),
                    COUNT(DISTINCT subdivision_id),
                    MIN(created_at), MAX(created_at)
                FROM {TableName.OrtoDatas}
                {WhereCountyIds(countyIds_Array)}
                GROUP BY county_id
                ORDER BY county_id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddCountyIdsParameter(npgsqlCommand, countyIds_Array);

            List<OrtoDatasCountyResult> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(new OrtoDatasCountyResult(
                    npgsqlDataReader.GetInt32(0),
                    npgsqlDataReader.GetInt64(1),
                    npgsqlDataReader.GetInt64(2),
                    npgsqlDataReader.GetInt64(3),
                    npgsqlDataReader.IsDBNull(4) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(4),
                    npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(5)));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously summarises what each of the named county partitions holds.
        /// </summary>
        /// <param name="countyIds">The identifiers of the county partitions to summarise. Null summarises every one.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one <see cref="OrtoDatasCountyResult"/> per county holding rows, or null when no connection could be built or nothing has ever been stored.</returns>
        public async Task<List<OrtoDatasCountyResult>?> GetSummariesByCountyIdsAsync(IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetSummariesByCountyIdsAsync(npgsqlConnection, countyIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the subdivision each of a county's stored rows is filed under, keyed by reference.
        /// <para>Deliberately projects two columns and never <c>object</c>: that column holds the orthophoto imagery for every year the row carries, so the ordinary reads cost megabytes a row. This is what makes a whole-county comparison affordable at all.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyId">The identifier of the county partition to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result maps each reference to the subdivision it is filed under, which is null when it is filed under none, or null overall when the connection is null or nothing has ever been stored.</returns>
        public static async Task<Dictionary<string, int?>?> GetSubdivisionIdsByCountyIdAsync(NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName.OrtoDatas))
            {
                return null;
            }

            const string commandText = $@"
                SELECT reference, subdivision_id
                FROM {TableName.OrtoDatas}
                WHERE county_id = @countyId;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });

            Dictionary<string, int?> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (npgsqlDataReader.IsDBNull(0))
                {
                    continue;
                }

                // The unique index on (county_id, reference) makes the key unique within a partition, so the
                // indexer cannot silently drop a row here.
                result[npgsqlDataReader.GetString(0)] = npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetInt32(1);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads the subdivision each of a county's stored rows is filed under, keyed by reference.
        /// </summary>
        /// <param name="countyId">The identifier of the county partition to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result maps each reference to the subdivision it is filed under, or null when no connection could be built or nothing has ever been stored.</returns>
        public async Task<Dictionary<string, int?>?> GetSubdivisionIdsByCountyIdAsync(int countyId, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetSubdivisionIdsByCountyIdAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously draws references at random from the orthophoto rows of the named county parts, without reading the imagery.
        /// <para>Only the <c>reference</c> column is projected: the <c>object</c> column is TOASTed out of line, so the draw touches the small heap tuples and never detoasts a photo. Whether a drawn row actually holds a photo (<c>Values</c> non-empty) is a per-candidate question answered afterwards by <see cref="GetYearsByReferenceAsync(NpgsqlConnection, string, int?, bool, int, CancellationToken)"/> - putting <c>jsonb_array_length(object-&gt;'Values')</c> here would detoast every row of the part.</para>
        /// <para>This is the storage-side half of the random unverified-building draw (<c>Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync</c>); the building and year-built sides live in the main database and are matched against these references there.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The <c>building_2d</c> part ids to draw from.</param>
        /// <param name="count">How many references to draw. A value of 0 or less draws none.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the drawn references, fewer than <paramref name="count"/> when the parts hold fewer rows, or null when the connection or the parts are missing or the table has never been created.</returns>
        public static async Task<List<string>?> GetRandomReferencesByCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, int count, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || countyIds is null)
            {
                return null;
            }

            int[] countyIds_Array = [.. countyIds.Distinct()];
            if (countyIds_Array.Length == 0)
            {
                return null;
            }

            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName.OrtoDatas))
            {
                return null;
            }

            List<string> result = [];
            if (count <= 0)
            {
                return result;
            }

            string commandText = $@"
                SELECT reference
                FROM {TableName.OrtoDatas}
                WHERE county_id = ANY(@countyIds)
                ORDER BY random()
                LIMIT @count;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("count", NpgsqlDbType.Integer) { Value = count });

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (!npgsqlDataReader.IsDBNull(0))
                {
                    result.Add(npgsqlDataReader.GetString(0));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously draws references at random from the orthophoto rows of the named county parts, without reading the imagery.
        /// </summary>
        /// <param name="countyIds">The <c>building_2d</c> part ids to draw from.</param>
        /// <param name="count">How many references to draw. A value of 0 or less draws none.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the drawn references, or null when no connection could be built, the parts are missing or the table has never been created.</returns>
        public async Task<List<string>?> GetRandomReferencesByCountyIdsAsync(IEnumerable<int>? countyIds, int count, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetRandomReferencesByCountyIdsAsync(npgsqlConnection, countyIds, count, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the years that hold a photo for one building - the years that have a card, not every year of the <c>[min, max]</c> range.
        /// <para>Projects the <c>DateTime</c> of the stored <c>Values</c> elements and never reads the <c>Bytes</c>, so listing a building's years stays cheap no matter how much imagery it carries.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="reference">The reference of the building to read the years of.</param>
        /// <param name="countyId">The optional county identifier the read is pruned to.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to re-run the read by reference alone when nothing is held under the named county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the years that hold a photo, empty when the building holds none, or null when the connection or the reference is null.</returns>
        public static async Task<List<short>?> GetYearsByReferenceAsync(NpgsqlConnection? npgsqlConnection, string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            string commandText = countyId.HasValue ? $@"
                SELECT DISTINCT EXTRACT(YEAR FROM (v->>'DateTime')::timestamp)::smallint
                FROM {TableName.OrtoDatas} o, jsonb_array_elements(o.object->'Values') v
                WHERE o.reference = @reference
                  AND o.county_id = @countyId
                  AND (v->>'DateTime') IS NOT NULL
                ORDER BY 1;" : $@"
                SELECT DISTINCT EXTRACT(YEAR FROM (v->>'DateTime')::timestamp)::smallint
                FROM {TableName.OrtoDatas} o, jsonb_array_elements(o.object->'Values') v
                WHERE o.reference = @reference
                  AND (v->>'DateTime') IS NOT NULL
                ORDER BY 1;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = reference });
            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            List<short> result = [];

            // The reader is closed before the fallback re-runs: a second command on the same connection while
            // the first reader is still open is NpgsqlOperationInProgressException (DiGi.GIS.PostgreSQL#91).
            await using (NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken))
            {
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    result.Add(npgsqlDataReader.GetInt16(0));
                }
            }

            if (result.Count > 0 || !fallbackByReference || countyId is null)
            {
                return result;
            }

            return await GetYearsByReferenceAsync(npgsqlConnection, reference, null, false, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the years that hold a photo for one building - the years that have a card, not every year of the <c>[min, max]</c> range.
        /// </summary>
        /// <param name="reference">The reference of the building to read the years of.</param>
        /// <param name="countyId">The optional county identifier the read is pruned to.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to re-run the read by reference alone when nothing is held under the named county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the years that hold a photo, empty when the building holds none, or null when no connection could be built.</returns>
        public async Task<List<short>?> GetYearsByReferenceAsync(string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetYearsByReferenceAsync(npgsqlConnection, reference, countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the bytes of the photo one building holds for one exact year, or null when that year has no photo.
        /// <para>Matches the stored <c>DateTime</c> of the <c>Values</c> elements against <c>make_date(@year, 1, 1)</c> - the shape the producer stores - so a year the building does not hold answers <c>null</c> instead of a neighbour's image. The stored <c>Bytes</c> member is a JSON array of numbers - the form the DiGi serializer writes for a <c>byte[]</c> - and is decoded with <see cref="Convert.ToSystem_Bytes(JsonNode)"/>. This is the exact-year counterpart of <see cref="GetYearsByReferenceAsync(NpgsqlConnection, string, int?, bool, int, System.Threading.CancellationToken)"/>.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="reference">The reference of the building to read the photo of.</param>
        /// <param name="countyId">The optional county identifier the read is pruned to.</param>
        /// <param name="year">The exact year the photo is requested for.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to re-run the read by reference alone when nothing is held under the named county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the photo bytes of the exact year, or null when the building holds no photo of that year or the connection is null.</returns>
        public static async Task<byte[]?> GetBytesByReferenceAsync(NpgsqlConnection? npgsqlConnection, string reference, int? countyId, short year, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            // v->'Bytes' (jsonb, not ->>): the stored member is a JSON array of numbers, which is what the DiGi
            // serializer writes for a byte[]; the text form is parsed back below. Reading it with ->> as base64
            // threw on every row (DiGi.GIS.PostgreSQL#90).
            string commandText = countyId.HasValue ? $@"
                SELECT v->'Bytes'
                FROM {TableName.OrtoDatas} o, jsonb_array_elements(o.object->'Values') v
                WHERE o.reference = @reference
                  AND o.county_id = @countyId
                  AND (v->>'DateTime')::timestamp = make_date(@year, 1, 1)
                LIMIT 1;" : $@"
                SELECT v->'Bytes'
                FROM {TableName.OrtoDatas} o, jsonb_array_elements(o.object->'Values') v
                WHERE o.reference = @reference
                  AND (v->>'DateTime')::timestamp = make_date(@year, 1, 1)
                LIMIT 1;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = reference });
            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("year", NpgsqlDbType.Smallint) { Value = year });

            // The reader is closed before the fallback re-runs: a second command on the same connection while
            // the first reader is still open is NpgsqlOperationInProgressException (DiGi.GIS.PostgreSQL#91).
            await using (NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken))
            {
                if (await npgsqlDataReader.ReadAsync(cancellationToken) && !npgsqlDataReader.IsDBNull(0))
                {
                    byte[]? bytes = JsonNode.Parse(npgsqlDataReader.GetString(0)).ToSystem_Bytes();
                    if (bytes is not null)
                    {
                        return bytes;
                    }
                }
            }

            if (!fallbackByReference || countyId is null)
            {
                return null;
            }

            return await GetBytesByReferenceAsync(npgsqlConnection, reference, null, year, false, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the bytes of the photo one building holds for one exact year, or null when that year has no photo.
        /// </summary>
        /// <param name="reference">The reference of the building to read the photo of.</param>
        /// <param name="countyId">The optional county identifier the read is pruned to.</param>
        /// <param name="year">The exact year the photo is requested for.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to re-run the read by reference alone when nothing is held under the named county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the photo bytes of the exact year, or null when the building holds no photo of that year or no connection could be built.</returns>
        public async Task<byte[]?> GetBytesByReferenceAsync(string reference, int? countyId, short year, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBytesByReferenceAsync(npgsqlConnection, reference, countyId, year, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reports what each of the named counties still has waiting in the download queue.
        /// <para>Reads the queue without claiming anything from it, unlike <see cref="GetNextBuilding2DReferencesAsync(NpgsqlConnection?, int, int, int, int, CancellationToken)"/>, which claims the rows it returns. It is the only way to see what a refresh queued.</para>
        /// <para>Naming no county reports every one. Counties with nothing waiting are absent from the result rather than present with a zero.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The identifiers of the counties to report on. Null reports every one.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one <see cref="OrtoDatasQueueResult"/> per county with entries waiting, or null when the connection is null or the queue has never been created.</returns>
        public static async Task<List<OrtoDatasQueueResult>?> GetQueueSummariesByCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // The queue table is created by the first write rather than up front, so its absence means no
            // refresh has ever run - a fact, not a fault.
            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName.OrtoDatas_Building2DReference_Update))
            {
                return null;
            }

            int[]? countyIds_Array = countyIds is null ? null : [.. countyIds];

            string commandText = $@"
                SELECT
                    county_id,
                    COUNT(*),
                    COUNT(*) FILTER (WHERE subdivision_id IS NOT NULL),
                    COUNT(*) FILTER (WHERE COALESCE(attempts, 0) > 0),
                    MIN(created_at), MAX(created_at)
                FROM {TableName.OrtoDatas_Building2DReference_Update}
                {WhereCountyIds(countyIds_Array)}
                GROUP BY county_id
                ORDER BY county_id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddCountyIdsParameter(npgsqlCommand, countyIds_Array);

            List<OrtoDatasQueueResult> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(new OrtoDatasQueueResult(
                    npgsqlDataReader.GetInt32(0),
                    npgsqlDataReader.GetInt64(1),
                    npgsqlDataReader.GetInt64(2),
                    npgsqlDataReader.GetInt64(3),
                    npgsqlDataReader.IsDBNull(4) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(4),
                    npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(5)));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reports what each of the named counties still has waiting in the download queue.
        /// </summary>
        /// <param name="countyIds">The identifiers of the counties to report on. Null reports every one.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one <see cref="OrtoDatasQueueResult"/> per county with entries waiting, or null when no connection could be built or the queue has never been created.</returns>
        public async Task<List<OrtoDatasQueueResult>?> GetQueueSummariesByCountyIdsAsync(IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetQueueSummariesByCountyIdsAsync(npgsqlConnection, countyIds, commandTimeout, cancellationToken);
        }

        private static void AddCountyIdsParameter(NpgsqlCommand npgsqlCommand, int[]? countyIds)
        {
            if (countyIds is null)
            {
                return;
            }

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds });
        }

        private static string WhereCountyIds(int[]? countyIds)
        {
            if (countyIds is null)
            {
                return string.Empty;
            }

            return "WHERE county_id = ANY(@countyIds)";
        }
        
        private static OrtoDatas Create_OrtoDatas(NpgsqlDataReader npgsqlDataReader)
        {
            return new OrtoDatas
            {
                Id = npgsqlDataReader.GetInt64(0),
                CountyId = npgsqlDataReader.IsDBNull(1) ? null : (int?)npgsqlDataReader.GetInt32(1),
                Reference = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2),
                BoundingBox2D = new BoundingBox2D(
                        new Point2D(npgsqlDataReader.IsDBNull(3) ? double.NaN : npgsqlDataReader.GetDouble(3),
                                    npgsqlDataReader.IsDBNull(4) ? double.NaN : npgsqlDataReader.GetDouble(4)),
                        new Point2D(npgsqlDataReader.IsDBNull(5) ? double.NaN : npgsqlDataReader.GetDouble(5),
                                    npgsqlDataReader.IsDBNull(6) ? double.NaN : npgsqlDataReader.GetDouble(6))),
                SubdivisionId = npgsqlDataReader.IsDBNull(7) ? null : (int?)npgsqlDataReader.GetInt32(7),
                Object = npgsqlDataReader.IsDBNull(8) ? null : JsonNode.Parse(npgsqlDataReader.GetString(8)) as JsonObject,
                CreatedAt = npgsqlDataReader.IsDBNull(9) ? null : (DateTime?)npgsqlDataReader.GetDateTime(9)
            };
        }

        private static async Task<List<OrtoDatas>> ReadAsync_OrtoDatas(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<OrtoDatas> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_OrtoDatas(npgsqlDataReader));
            }

            return result;
        }

        private static async Task<List<OrtoDatas>?> ReadAsync_OrtoDatas(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_OrtoDatas(npgsqlDataReader, cancellationToken);
        }

        private static OrtoDatasReference Create_OrtoDatasReference(NpgsqlDataReader npgsqlDataReader)
        {
            return new OrtoDatasReference
            {
                Id = npgsqlDataReader.GetInt64(0),
                CountyId = npgsqlDataReader.IsDBNull(1) ? null : (int?)npgsqlDataReader.GetInt32(1),
                Reference = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2),
                BoundingBox2D = new BoundingBox2D(
                        new Point2D(npgsqlDataReader.IsDBNull(3) ? double.NaN : npgsqlDataReader.GetDouble(3),
                                    npgsqlDataReader.IsDBNull(4) ? double.NaN : npgsqlDataReader.GetDouble(4)),
                        new Point2D(npgsqlDataReader.IsDBNull(5) ? double.NaN : npgsqlDataReader.GetDouble(5),
                                    npgsqlDataReader.IsDBNull(6) ? double.NaN : npgsqlDataReader.GetDouble(6))),
                SubdivisionId = npgsqlDataReader.IsDBNull(7) ? null : (int?)npgsqlDataReader.GetInt32(7),
                CreatedAt = npgsqlDataReader.IsDBNull(8) ? null : (DateTime?)npgsqlDataReader.GetDateTime(8)
            };
        }

        private static async Task<List<OrtoDatasReference>> ReadAsync_OrtoDatasReference(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<OrtoDatasReference> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_OrtoDatasReference(npgsqlDataReader));
            }

            return result;
        }

        private static async Task<List<OrtoDatasReference>?> ReadAsync_OrtoDatasReference(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_OrtoDatasReference(npgsqlDataReader, cancellationToken);
        }

        /// <summary>
        /// Asynchronously moves the rows held for the given building references onto <paramref name="countyId"/>, so that they stay under the same county polygon part as the building itself.
        /// <para>Called after a building has been re-filed under the part its footprint lies in. A row left behind under the part the building came from is unreachable: every read here filters on <c>county_id</c> first. Nothing is deleted - a row the destination will not take is left where it is and its reference is not reported.</para>
        /// <para>The move, its collision guards and what it costs are described on <see cref="Modify.RefreshCountyIdsAsync(NpgsqlConnection, string, IEnumerable{string}, int, IEnumerable{int}, int, int, CancellationToken)"/>.</para>
        /// </summary>
        /// <param name="references">The building references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county polygon part the rows should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched.</param>
        /// <param name="batchSize">The number of references sent in one statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references that had at least one row moved, or null when no references were given or the connection could not be created.</returns>
        public async Task<HashSet<string>?> RefreshCountyIdsAsync(IEnumerable<string>? references, int countyId, IEnumerable<int>? countyIds_Source = null, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await npgsqlConnection.RefreshCountyIdsAsync(TableName.OrtoDatas, references, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken);
        }
    }
}