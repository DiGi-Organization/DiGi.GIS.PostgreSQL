using DiGi.GIS.Interfaces;
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
    /// Provides a converter for YearBuiltData objects when interacting with a PostgreSQL database.
    /// </summary>
    public class YearBuiltDataPostgreSQLConverter : Building2DReferencedObjectPostgreSQLConverter<YearBuiltData, IYearBuiltData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YearBuiltDataPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to establish a connection to the PostgreSQL database.</param>
        public YearBuiltDataPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the table in the PostgreSQL database.
        /// </summary>
        public override string TableName => Constants.TableName.YearBuiltData;

        /// <summary>
        /// Creates an instance of <see cref="YearBuiltData"/> based on the provided database values.
        /// </summary>
        /// <param name="id">The unique identifier of the record.</param>
        /// <param name="countyId">The county identifier associated with the record.</param>
        /// <param name="uniqueId">The unique external identifier for the record.</param>
        /// <param name="reference">The reference string for the record.</param>
        /// <param name="object">The JSON object containing the data attributes.</param>
        /// <param name="createdAt">The timestamp when the record was created.</param>
        /// <returns>A new <see cref="YearBuiltData"/> instance.</returns>
        protected override YearBuiltData Create(long id, int? countyId, string? uniqueId, string? reference, JsonObject? @object, DateTime? createdAt)
        {
            return new YearBuiltData()
            {
                Id = id,
                CountyId = countyId,
                UniqueId = uniqueId,
                Reference = reference,
                Object = @object,
                CreatedAt = createdAt
            };
        }

        /// <summary>
        /// Asynchronously replaces the user-provided year built of one building - every stored object of the building keeps exactly the given user entry, and a building that holds no row gets one - committed as a single transaction.
        /// <para>The county part the write lands under is resolved through <c>building_2d</c> first: the caller's county identifier may name a sibling polygon part, and the <c>building_2d</c> row's part is the one every referenced-object table is filed under. A reference <c>building_2d</c> does not hold answers <c>null</c>.</para>
        /// <para>Each row read back keeps its own <c>unique_id</c>, so the upsert replaces the row it came from; a building with no rows gets one fresh object and therefore one new row. Predicted and other entries are left untouched - the object's entries are keyed by source, so setting the user entry replaces exactly the previous user entry.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county row the caller names; the building's own part is resolved through <c>building_2d</c>.</param>
        /// <param name="reference">The reference of the building to write the year for.</param>
        /// <param name="userYearBuilt">The user-provided year built entry to store, relation and provenance included.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> when the write committed, <c>false</c> when it failed and rolled back, and <c>null</c> when the building is unknown or the write could not be attempted.</returns>
        public async Task<bool?> UpdateUserYearBuiltAsync(int countyId, string reference, GIS.Classes.UserYearBuilt userYearBuilt, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference) || userYearBuilt is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // The part the building is filed under is the key every referenced-object table uses; writing under the
            // caller's identifier verbatim would file a second row under a sibling part.
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(ConnectionData);
            Building2DReference? building2DReference = await building2DPostgreSQLConverter.GetBuilding2DReferenceByReferenceAsync(reference, countyId, fallbackByReference: true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (building2DReference is null || building2DReference.CountyId is null)
            {
                return null;
            }

            int countyId_Part = building2DReference.CountyId.Value;

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            if (!await npgsqlConnection.TableAsync_Building2DReferencedObject(TableName, commandTimeout, cancellationToken: cancellationToken)
             || !await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(TableName, countyId_Part, commandTimeout, cancellationToken: cancellationToken))
            {
                return false;
            }

            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Locks the building's rows under its part before the read, so a concurrent removal of a user entry or of a
                // prediction run waits for this write instead of rewriting the object from a copy read before it.
                await LockAsync(npgsqlConnection, [countyId_Part], [reference], commandTimeout, cancellationToken);

                List<YearBuiltData>? yearBuiltDatas = await GetItemsByReferencesAsync(npgsqlConnection, [reference], countyId_Part, fallbackByReference: true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);

                List<YearBuiltData> yearBuiltDatas_ToWrite = [];
                if (yearBuiltDatas is not null && yearBuiltDatas.Count > 0)
                {
                    foreach (YearBuiltData yearBuiltData in yearBuiltDatas)
                    {
                        if (yearBuiltData.ToDiGi() is not GIS.Classes.YearBuiltData yearBuiltData_GIS)
                        {
                            continue;
                        }

                        // Keyed by source: replaces exactly the previous user entry, leaves the others untouched.
                        yearBuiltData_GIS.SetUserYearBuilt(userYearBuilt);

                        if (yearBuiltData_GIS.ToPostgreSQL(countyId_Part) is YearBuiltData yearBuiltData_PostgreSQL)
                        {
                            yearBuiltDatas_ToWrite.Add(yearBuiltData_PostgreSQL);
                        }
                    }

                    // Every row the building held was unreadable - committing success over them would read as a clean
                    // write while dropping their entries, so the failure stands and the prior state is left intact.
                    if (yearBuiltDatas_ToWrite.Count == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    // Random mode's usual case: the building holds no row at all. A fresh object mints a new
                    // unique_id, so the upsert adds a row rather than replacing one.
                    GIS.Classes.YearBuiltData yearBuiltData_New = new(reference);
                    yearBuiltData_New.SetUserYearBuilt(userYearBuilt);

                    if (yearBuiltData_New.ToPostgreSQL(countyId_Part) is not YearBuiltData yearBuiltData_PostgreSQL)
                    {
                        return false;
                    }

                    yearBuiltDatas_ToWrite.Add(yearBuiltData_PostgreSQL);
                }

                PostgreSQLUpdateResult? result = await UpdateAsync(npgsqlConnection, npgsqlTransaction, yearBuiltDatas_ToWrite, commandTimeout, cancellationToken: cancellationToken);
                if (result is null)
                {
                    return false;
                }

                await npgsqlTransaction.CommitAsync(cancellationToken);
                return true;
            }
            catch (Exception exception)
            {
                Serilog.Modify.Log(exception, "{Type} user year built write for reference {Reference} of county part {CountyId} failed and rolled back", nameof(YearBuiltDataPostgreSQLConverter), reference, countyId_Part);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously keeps, of the named references, the buildings of the named county parts that hold no user-provided year built yet.
        /// <para>The main-database half of the random unverified-building draw (<c>Query.RandomBuilding2DReferenceWithoutUserYearBuiltAsync</c>): the references arrive from the orthophoto store, which lives in the other database, and are matched here against <c>building_2d</c> and anti-joined against <c>year_built_data</c>. A reference with no <c>building_2d</c> row under the parts drops out.</para>
        /// <para>Eligibility is strict on the user entries: a building holding a <c>PredictedYearBuilt</c> entry stays eligible - predictions never affect it - while a <c>UserYearBuilt</c> of any relation excludes it, because a bound is still a verification.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The <c>building_2d</c> part ids the references are filed under.</param>
        /// <param name="references">The references to keep or drop.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the eligible buildings in the order the references were given, or null when the connection, the parts or the references are missing or either table has never been created.</returns>
        public static async Task<List<Building2DReference>?> GetBuilding2DReferencesWithoutUserYearBuiltAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, IEnumerable<string>? references, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || countyIds is null || references is null)
            {
                return null;
            }

            int[] countyIds_Array = [.. countyIds.Distinct()];
            string[] references_Array = [.. references.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()];
            if (countyIds_Array.Length == 0 || references_Array.Length == 0)
            {
                return null;
            }

            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, Constants.TableName.Building2D, cancellationToken: cancellationToken)
             || !await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, Constants.TableName.YearBuiltData, cancellationToken: cancellationToken))
            {
                return null;
            }

            string? userType = Core.Query.FullTypeName(typeof(DiGi.GIS.Classes.UserYearBuilt));
            if (string.IsNullOrWhiteSpace(userType))
            {
                return null;
            }

            // array_position keeps the caller's (random) order, so the first row is the first drawn survivor.
            string commandText = $@"
                SELECT b.id, b.county_id, b.reference, b.subdivision_id
                FROM {Constants.TableName.Building2D} b
                WHERE b.county_id = ANY(@countyIds)
                  AND b.reference = ANY(@references)
                  AND NOT EXISTS (
                      SELECT 1
                      FROM {Constants.TableName.YearBuiltData} y
                      WHERE y.county_id = b.county_id
                        AND y.reference = b.reference
                        AND EXISTS (
                            SELECT 1
                            FROM jsonb_array_elements(y.object->'YearBuilts') AS entry(value)
                            WHERE entry->>'_type' = @userType))
                ORDER BY array_position(@references, b.reference);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("userType", NpgsqlDbType.Text) { Value = userType });

            List<Building2DReference> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(new Building2DReference
                {
                    Id = npgsqlDataReader.GetInt64(0),
                    CountyId = npgsqlDataReader.GetInt32(1),
                    Reference = npgsqlDataReader.GetString(2),
                    SubdivisionId = npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetInt32(3)
                });
            }

            return result;
        }

        /// <summary>
        /// Asynchronously keeps, of the named references, the buildings of the named county parts that hold no user-provided year built yet.
        /// </summary>
        /// <param name="countyIds">The <c>building_2d</c> part ids the references are filed under.</param>
        /// <param name="references">The references to keep or drop.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the eligible buildings in the order the references were given, or null when no connection could be built, the parts or the references are missing or either table has never been created.</returns>
        public async Task<List<Building2DReference>?> GetBuilding2DReferencesWithoutUserYearBuiltAsync(IEnumerable<int>? countyIds, IEnumerable<string>? references, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBuilding2DReferencesWithoutUserYearBuiltAsync(npgsqlConnection, countyIds, references, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes stored year built data objects of explicit county parts, in one transaction.
        /// <para>The scope is always a set of county parts - a reference is unique only per <c>county_id</c>, and the inherited <c>RemoveAsync</c> with no county deletes across every county. <paramref name="references"/> narrows it to those buildings; leaving it out is accepted only together with <paramref name="emptyOnly"/>, so the widest call this method makes deletes the objects of a county that hold no entry at all.</para>
        /// <para>An object is empty when its <c>YearBuilts</c> is missing, not an array, or an empty array. The scoped rows are selected <c>FOR UPDATE</c>; when more match than <paramref name="limit"/> the transaction is rolled back and nothing is deleted - a delete cut short at the limit would leave the caller unable to tell which rows went. A dry run selects and counts the same rows and rolls back.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> the transaction runs on.</param>
        /// <param name="countyIds">The county parts to delete from. Normally every polygon part of one county.</param>
        /// <param name="references">The references of the buildings whose objects are deleted, or null for every building of the parts (only with <paramref name="emptyOnly"/>).</param>
        /// <param name="emptyOnly">A value indicating whether only objects holding no entry are deleted.</param>
        /// <param name="dryRun">A value indicating whether the rows are only counted.</param>
        /// <param name="limit">The largest number of rows the call may delete.</param>
        /// <param name="batchSize">The number of rows deleted per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result describes what matched and what was deleted, or is null when the connection is null, no county part is named, <paramref name="limit"/> is not positive, or <paramref name="references"/> is null without <paramref name="emptyOnly"/>.</returns>
        public async Task<YearBuiltDataRemoveResult?> RemoveItemsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, IEnumerable<string>? references, bool emptyOnly, bool dryRun, int limit, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            int[] countyIds_Array = countyIds is null ? [] : [.. countyIds.Distinct()];
            if (npgsqlConnection is null || countyIds_Array.Length == 0 || limit <= 0 || batchSize <= 0 || (references is null && !emptyOnly))
            {
                return null;
            }

            string[]? references_Array = references is null ? null : [.. references.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()];
            if (references_Array is not null && references_Array.Length == 0)
            {
                return new YearBuiltDataRemoveResult(dryRun, limit, 0, 0, null);
            }

            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName, cancellationToken: cancellationToken))
            {
                return new YearBuiltDataRemoveResult(dryRun, limit, 0, 0, references_Array);
            }

            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

            // jsonb_array_length raises on a non-array and SQL does not promise to evaluate an OR left to right, so
            // the array test guards it through CASE. A missing object or member is empty too.
            string commandText = $@"
                SELECT id, reference
                FROM {TableName}
                WHERE county_id = ANY(@countyIds)
                  {(references_Array is null ? string.Empty : "AND reference = ANY(@references)")}
                  {(emptyOnly ? "AND CASE WHEN jsonb_typeof(object->'YearBuilts') = 'array' THEN jsonb_array_length(object->'YearBuilts') = 0 ELSE TRUE END" : string.Empty)}
                ORDER BY id
                FOR UPDATE;";

            List<long> ids = [];
            HashSet<string> references_Matched = [];

            await using (NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection, npgsqlTransaction))
            {
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
                if (references_Array is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    ids.Add(npgsqlDataReader.GetInt64(0));
                    references_Matched.Add(npgsqlDataReader.GetString(1));
                }
            }

            List<string>? references_Unmatched = references_Array?.Where(x => !references_Matched.Contains(x)).ToList();

            if (dryRun || ids.Count > limit)
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
                return new YearBuiltDataRemoveResult(dryRun, limit, ids.Count, 0, references_Unmatched);
            }

            int removed = 0;
            for (int i = 0; i < ids.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                long[] ids_Batch = [.. ids.GetRange(i, Math.Min(batchSize, ids.Count - i))];

                await using NpgsqlCommand npgsqlCommand = new($@"
                    DELETE FROM {TableName}
                    WHERE county_id = ANY(@countyIds)
                      AND id = ANY(@ids);", npgsqlConnection, npgsqlTransaction);

                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = ids_Batch });

                removed += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await npgsqlTransaction.CommitAsync(cancellationToken);

            return new YearBuiltDataRemoveResult(dryRun, limit, ids.Count, removed, references_Unmatched);
        }

        /// <summary>
        /// Asynchronously deletes stored year built data objects of explicit county parts, in one transaction.
        /// <para>See <see cref="RemoveItemsAsync(NpgsqlConnection?, IEnumerable{int}?, IEnumerable{string}?, bool, bool, int, int, int, CancellationToken)"/>; this overload opens its own connection.</para>
        /// </summary>
        /// <param name="countyIds">The county parts to delete from. Normally every polygon part of one county.</param>
        /// <param name="references">The references of the buildings whose objects are deleted, or null for every building of the parts (only with <paramref name="emptyOnly"/>).</param>
        /// <param name="emptyOnly">A value indicating whether only objects holding no entry are deleted.</param>
        /// <param name="dryRun">A value indicating whether the rows are only counted.</param>
        /// <param name="limit">The largest number of rows the call may delete.</param>
        /// <param name="batchSize">The number of rows deleted per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result describes what matched and what was deleted, or is null when no connection could be built or the arguments are refused.</returns>
        public async Task<YearBuiltDataRemoveResult?> RemoveItemsAsync(IEnumerable<int>? countyIds, IEnumerable<string>? references, bool emptyOnly, bool dryRun, int limit, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await RemoveItemsAsync(npgsqlConnection, countyIds, references, emptyOnly, dryRun, limit, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously removes one prediction run - the predicted year built entries stamped <paramref name="dateTime"/> - from the stored year built data objects of explicit county parts, in one transaction.
        /// <para>Only objects carrying a predicted entry are read, <c>FOR UPDATE</c> and in pages of <paramref name="batchSize"/>; the entry is matched by <see cref="DateTime.Ticks"/>, the key it is stored under, so the kind of the value plays no part. Each changed object is rewritten under the county part it was read from and keeps its <c>unique_id</c>, so it replaces its own row. Other stamps and the user entry are untouched, and an object left with no entry is kept and reported - deleting it is <see cref="RemoveItemsAsync(IEnumerable{int}?, IEnumerable{string}?, bool, bool, int, int, int, CancellationToken)"/>'s job.</para>
        /// <para>When more objects carry the stamp than <paramref name="limit"/>, or on a dry run, the transaction is rolled back and nothing is written.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> the transaction runs on.</param>
        /// <param name="countyIds">The county parts to remove the run from. Normally every polygon part of one county.</param>
        /// <param name="dateTime">The stamp of the run to remove.</param>
        /// <param name="references">The references of the buildings to remove the run from, or null for every building of the parts.</param>
        /// <param name="dryRun">A value indicating whether the entries are only counted.</param>
        /// <param name="limit">The largest number of objects the call may rewrite.</param>
        /// <param name="batchSize">The number of rows read per page.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result describes what matched and what was removed, or is null when the connection is null, no county part is named, <paramref name="limit"/> is not positive, or the rewrite failed (the transaction is then rolled back).</returns>
        public async Task<PredictedYearBuiltRemoveResult?> RemovePredictedYearBuiltsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, DateTime dateTime, IEnumerable<string>? references, bool dryRun, int limit, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            int[] countyIds_Array = countyIds is null ? [] : [.. countyIds.Distinct()];
            if (npgsqlConnection is null || countyIds_Array.Length == 0 || limit <= 0 || batchSize <= 0)
            {
                return null;
            }

            string[]? references_Array = references is null ? null : [.. references.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()];
            if ((references_Array is not null && references_Array.Length == 0) || !await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName, cancellationToken: cancellationToken))
            {
                return new PredictedYearBuiltRemoveResult(dryRun, limit, dateTime.Ticks, 0, 0, null, null);
            }

            string? predictedType = Core.Query.FullTypeName(typeof(GIS.Classes.PredictedYearBuilt));
            if (string.IsNullOrWhiteSpace(predictedType))
            {
                return null;
            }

            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

            // Only rows carrying a predicted entry at all are read; which of them carries this stamp is decided in
            // memory on the ticks, because the stored text of a date depends on the kind it was written with.
            string commandText = $@"
                SELECT id, county_id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE county_id = ANY(@countyIds)
                  {(references_Array is null ? string.Empty : "AND reference = ANY(@references)")}
                  AND id > @id
                  AND jsonb_typeof(object->'YearBuilts') = 'array'
                  AND EXISTS (
                      SELECT 1
                      FROM jsonb_array_elements(object->'YearBuilts') AS entry(value)
                      WHERE entry->>'_type' = @predictedType)
                ORDER BY id
                LIMIT @batchSize
                FOR UPDATE;";

            int matched = 0;
            List<YearBuiltData> yearBuiltDatas_ToWrite = [];
            HashSet<string> references_Matched = [];
            HashSet<string> references_Emptied = [];

            long id_Last = long.MinValue;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<YearBuiltData>? yearBuiltDatas;
                await using (NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection, npgsqlTransaction))
                {
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = id_Last });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("predictedType", NpgsqlDbType.Text) { Value = predictedType });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("batchSize", NpgsqlDbType.Integer) { Value = batchSize });
                    if (references_Array is not null)
                    {
                        npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });
                    }

                    yearBuiltDatas = await ReadAsync(npgsqlCommand, cancellationToken);
                }

                if (yearBuiltDatas is null || yearBuiltDatas.Count == 0)
                {
                    break;
                }

                foreach (YearBuiltData yearBuiltData in yearBuiltDatas)
                {
                    id_Last = Math.Max(id_Last, yearBuiltData.Id);

                    if (yearBuiltData.ToDiGi() is not GIS.Classes.YearBuiltData yearBuiltData_GIS)
                    {
                        continue;
                    }

                    GIS.Classes.PredictedYearBuilt? predictedYearBuilt = yearBuiltData_GIS.GetPredictedYearBuilts()?.FirstOrDefault(x => x.DateTime.Ticks == dateTime.Ticks);
                    if (predictedYearBuilt is null || !yearBuiltData_GIS.Remove(predictedYearBuilt.Source))
                    {
                        continue;
                    }

                    matched++;

                    // Past the limit the call will roll back, so only the count is kept.
                    if (matched > limit)
                    {
                        continue;
                    }

                    string? reference = yearBuiltData.Reference ?? yearBuiltData_GIS.Reference;
                    if (reference is not null)
                    {
                        references_Matched.Add(reference);
                        if (yearBuiltData_GIS.YearBuilts is null || !yearBuiltData_GIS.YearBuilts.Any())
                        {
                            references_Emptied.Add(reference);
                        }
                    }

                    if (yearBuiltData_GIS.ToPostgreSQL(yearBuiltData.CountyId) is YearBuiltData yearBuiltData_PostgreSQL)
                    {
                        yearBuiltDatas_ToWrite.Add(yearBuiltData_PostgreSQL);
                    }
                }
            }

            if (dryRun || matched > limit)
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
                return new PredictedYearBuiltRemoveResult(dryRun, limit, dateTime.Ticks, matched, 0, matched > limit ? null : references_Matched, matched > limit ? null : references_Emptied);
            }

            // An object that could not be converted back cannot be rewritten; committing the rest would remove the run
            // only partly while reporting it gone.
            if (yearBuiltDatas_ToWrite.Count != matched)
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
                return null;
            }

            PostgreSQLUpdateResult? postgreSQLUpdateResult = await UpdateAsync(npgsqlConnection, npgsqlTransaction, yearBuiltDatas_ToWrite, commandTimeout, cancellationToken);
            if (postgreSQLUpdateResult is null)
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
                return null;
            }

            await npgsqlTransaction.CommitAsync(cancellationToken);

            return new PredictedYearBuiltRemoveResult(dryRun, limit, dateTime.Ticks, matched, matched, references_Matched, references_Emptied);
        }

        /// <summary>
        /// Asynchronously removes one prediction run from the stored year built data objects of explicit county parts, in one transaction.
        /// <para>See <see cref="RemovePredictedYearBuiltsAsync(NpgsqlConnection?, IEnumerable{int}?, DateTime, IEnumerable{string}?, bool, int, int, int, CancellationToken)"/>; this overload opens its own connection.</para>
        /// </summary>
        /// <param name="countyIds">The county parts to remove the run from. Normally every polygon part of one county.</param>
        /// <param name="dateTime">The stamp of the run to remove.</param>
        /// <param name="references">The references of the buildings to remove the run from, or null for every building of the parts.</param>
        /// <param name="dryRun">A value indicating whether the entries are only counted.</param>
        /// <param name="limit">The largest number of objects the call may rewrite.</param>
        /// <param name="batchSize">The number of rows read per page.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result describes what matched and what was removed, or is null when no connection could be built, the arguments are refused or the rewrite failed.</returns>
        public async Task<PredictedYearBuiltRemoveResult?> RemovePredictedYearBuiltsAsync(IEnumerable<int>? countyIds, DateTime dateTime, IEnumerable<string>? references, bool dryRun, int limit, int batchSize = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await RemovePredictedYearBuiltsAsync(npgsqlConnection, countyIds, dateTime, references, dryRun, limit, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously withdraws the user-provided year built entry of the named buildings of explicit county parts, in one transaction.
        /// <para>Every object of a building holds the same user entry (<see cref="UpdateUserYearBuiltAsync"/> writes it to each), so the entry is removed from all of them. With <paramref name="userName"/> set, a building is withdrawn only when every user entry it holds was recorded by that user (compared without regard to case); a building holding someone else's is left whole and reported - this is the signed-in visitor's own-entry path. With <paramref name="userName"/> null any entry is withdrawn - the moderation path.</para>
        /// <para>An object left with no entry is kept. The rows are read <c>FOR UPDATE</c>, the same lock <see cref="UpdateUserYearBuiltAsync"/> takes, so a write and a withdrawal of one building never interleave. A dry run classifies and rolls back.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> the transaction runs on.</param>
        /// <param name="countyIds">The county parts the buildings are stored under. Normally every polygon part of one county.</param>
        /// <param name="references">The references of the buildings to withdraw the user entry of.</param>
        /// <param name="userName">The user whose entries may be withdrawn, or null to withdraw any entry.</param>
        /// <param name="dryRun">A value indicating whether the buildings are only classified.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result classifies every requested reference, or is null when the connection, the parts or the references are missing, or the rewrite failed (the transaction is then rolled back).</returns>
        public async Task<UserYearBuiltRemoveResult?> RemoveUserYearBuiltsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, IEnumerable<string>? references, string? userName, bool dryRun, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            int[] countyIds_Array = countyIds is null ? [] : [.. countyIds.Distinct()];
            if (npgsqlConnection is null || countyIds_Array.Length == 0 || references is null)
            {
                return null;
            }

            string[] references_Array = [.. references.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()];
            if (references_Array.Length == 0 || !await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName, cancellationToken: cancellationToken))
            {
                return new UserYearBuiltRemoveResult(dryRun, null, null, references_Array);
            }

            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

            List<YearBuiltData>? yearBuiltDatas;
            await using (NpgsqlCommand npgsqlCommand = new($@"
                SELECT id, county_id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE county_id = ANY(@countyIds)
                  AND reference = ANY(@references)
                ORDER BY id
                FOR UPDATE;", npgsqlConnection, npgsqlTransaction))
            {
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });

                yearBuiltDatas = await ReadAsync(npgsqlCommand, cancellationToken);
            }

            if (yearBuiltDatas is null)
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
                return null;
            }

            // reference => the objects holding a user entry, already stripped of it, ready to be written back
            Dictionary<string, List<YearBuiltData>> yearBuiltDatas_ByReference = [];
            HashSet<string> references_NotOwned = [];

            foreach (YearBuiltData yearBuiltData in yearBuiltDatas)
            {
                if (yearBuiltData.Reference is not string reference || yearBuiltData.ToDiGi() is not GIS.Classes.YearBuiltData yearBuiltData_GIS)
                {
                    continue;
                }

                GIS.Classes.UserYearBuilt? userYearBuilt = yearBuiltData_GIS.GetUserYearBuilt();
                if (userYearBuilt is null)
                {
                    continue;
                }

                if (userName is not null && !string.Equals(userYearBuilt.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    references_NotOwned.Add(reference);
                    continue;
                }

                if (!yearBuiltData_GIS.RemoveUserYearBuilt() || yearBuiltData_GIS.ToPostgreSQL(yearBuiltData.CountyId) is not YearBuiltData yearBuiltData_PostgreSQL)
                {
                    await npgsqlTransaction.RollbackAsync(cancellationToken);
                    return null;
                }

                if (!yearBuiltDatas_ByReference.TryGetValue(reference, out List<YearBuiltData>? yearBuiltDatas_Reference))
                {
                    yearBuiltDatas_Reference = [];
                    yearBuiltDatas_ByReference[reference] = yearBuiltDatas_Reference;
                }

                yearBuiltDatas_Reference.Add(yearBuiltData_PostgreSQL);
            }

            List<string> references_Removed = [];
            List<YearBuiltData> yearBuiltDatas_ToWrite = [];
            foreach (KeyValuePair<string, List<YearBuiltData>> keyValuePair in yearBuiltDatas_ByReference)
            {
                // A building is decided as a whole: one entry of someone else keeps all of its objects as they are.
                if (references_NotOwned.Contains(keyValuePair.Key))
                {
                    continue;
                }

                references_Removed.Add(keyValuePair.Key);
                yearBuiltDatas_ToWrite.AddRange(keyValuePair.Value);
            }

            List<string> references_NotFound = [.. references_Array.Where(x => !references_NotOwned.Contains(x) && !yearBuiltDatas_ByReference.ContainsKey(x))];

            if (dryRun || yearBuiltDatas_ToWrite.Count == 0)
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
                return new UserYearBuiltRemoveResult(dryRun, references_Removed, references_NotOwned, references_NotFound);
            }

            PostgreSQLUpdateResult? postgreSQLUpdateResult = await UpdateAsync(npgsqlConnection, npgsqlTransaction, yearBuiltDatas_ToWrite, commandTimeout, cancellationToken);
            if (postgreSQLUpdateResult is null)
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
                return null;
            }

            await npgsqlTransaction.CommitAsync(cancellationToken);

            return new UserYearBuiltRemoveResult(dryRun, references_Removed, references_NotOwned, references_NotFound);
        }

        /// <summary>
        /// Asynchronously withdraws the user-provided year built entry of the named buildings of explicit county parts, in one transaction.
        /// <para>See <see cref="RemoveUserYearBuiltsAsync(NpgsqlConnection?, IEnumerable{int}?, IEnumerable{string}?, string?, bool, int, CancellationToken)"/>; this overload opens its own connection.</para>
        /// </summary>
        /// <param name="countyIds">The county parts the buildings are stored under. Normally every polygon part of one county.</param>
        /// <param name="references">The references of the buildings to withdraw the user entry of.</param>
        /// <param name="userName">The user whose entries may be withdrawn, or null to withdraw any entry.</param>
        /// <param name="dryRun">A value indicating whether the buildings are only classified.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result classifies every requested reference, or is null when no connection could be built, the arguments are missing or the rewrite failed.</returns>
        public async Task<UserYearBuiltRemoveResult?> RemoveUserYearBuiltsAsync(IEnumerable<int>? countyIds, IEnumerable<string>? references, string? userName, bool dryRun, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await RemoveUserYearBuiltsAsync(npgsqlConnection, countyIds, references, userName, dryRun, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously lists the prediction runs stored under the given county parts: one entry per part, stamp and model identifier, with the number of objects carrying it.
        /// <para>The stored text of each stamp is turned back into <see cref="DateTime.Ticks"/> by deserializing it exactly as an entry is, so the reported value is the key the entry is stored under and can be handed to <see cref="RemovePredictedYearBuiltsAsync(IEnumerable{int}?, DateTime, IEnumerable{string}?, bool, int, int, int, CancellationToken)"/> as it is.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> the query runs on.</param>
        /// <param name="countyIds">The county parts to list, or null for every county. The estate-wide listing reads every stored object.</param>
        /// <param name="commandTimeout">The timeout in seconds for the query. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result lists the runs ordered by part, stamp and model - empty when there are none or the table has never been created - or is null when the connection is null.</returns>
        public async Task<List<PredictedYearBuiltRunResult>?> GetPredictedYearBuiltRunsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            int[]? countyIds_Array = countyIds is null ? null : [.. countyIds.Distinct()];
            if ((countyIds_Array is not null && countyIds_Array.Length == 0) || !await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName, cancellationToken: cancellationToken))
            {
                return [];
            }

            string? predictedType = Core.Query.FullTypeName(typeof(GIS.Classes.PredictedYearBuilt));
            if (string.IsNullOrWhiteSpace(predictedType))
            {
                return null;
            }

            string commandText = $@"
                SELECT y.county_id, entry->'DateTime' AS date_time, entry->>'ModelId' AS model_id, COUNT(DISTINCT y.id)::int AS count
                FROM {TableName} y
                CROSS JOIN LATERAL jsonb_array_elements(
                    CASE WHEN jsonb_typeof(y.object->'YearBuilts') = 'array' THEN y.object->'YearBuilts' ELSE '[]'::jsonb END) AS entry(value)
                WHERE entry->>'_type' = @predictedType
                  {(countyIds_Array is null ? string.Empty : "AND y.county_id = ANY(@countyIds)")}
                GROUP BY y.county_id, entry->'DateTime', entry->>'ModelId';";

            // (county, ticks, model) => count; distinct stored texts of one stamp (different kinds) fold into one entry
            Dictionary<Tuple<int, long, string?>, int> counts = [];

            await using (NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection))
            {
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("predictedType", NpgsqlDbType.Text) { Value = predictedType });
                if (countyIds_Array is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    int countyId = npgsqlDataReader.GetInt32(0);
                    string? dateTime_Json = npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetString(1);
                    string? modelId = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2);
                    int count = npgsqlDataReader.GetInt32(3);

                    if (dateTime_Json is null)
                    {
                        continue;
                    }

                    // Only the stamp is carried: a JsonValue built from a CLR int does not convert to the short Year the way a parsed one does.
                    JsonObject jsonObject = new()
                    {
                        ["_type"] = predictedType,
                        ["DateTime"] = JsonNode.Parse(dateTime_Json),
                    };

                    if (Core.Create.SerializableObject<GIS.Classes.PredictedYearBuilt>(jsonObject) is not GIS.Classes.PredictedYearBuilt predictedYearBuilt)
                    {
                        continue;
                    }

                    Tuple<int, long, string?> key = new(countyId, predictedYearBuilt.DateTime.Ticks, modelId);
                    counts[key] = counts.TryGetValue(key, out int count_Existing) ? count_Existing + count : count;
                }
            }

            return [.. counts
                .OrderBy(x => x.Key.Item1)
                .ThenBy(x => x.Key.Item2)
                .ThenBy(x => x.Key.Item3, StringComparer.Ordinal)
                .Select(x => new PredictedYearBuiltRunResult(x.Key.Item1, x.Key.Item2, x.Key.Item3, x.Value))];
        }

        /// <summary>
        /// Asynchronously lists the prediction runs stored under the given county parts.
        /// <para>See <see cref="GetPredictedYearBuiltRunsAsync(NpgsqlConnection?, IEnumerable{int}?, int, CancellationToken)"/>; this overload opens its own connection.</para>
        /// </summary>
        /// <param name="countyIds">The county parts to list, or null for every county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the query. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result lists the runs, or is null when no connection could be built.</returns>
        public async Task<List<PredictedYearBuiltRunResult>?> GetPredictedYearBuiltRunsAsync(IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetPredictedYearBuiltRunsAsync(npgsqlConnection, countyIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Locks the stored rows of the named buildings under the named parts for the rest of the open transaction.
        /// </summary>
        /// <param name="npgsqlConnection">The open connection whose transaction takes the locks.</param>
        /// <param name="countyIds">The county parts the rows are stored under.</param>
        /// <param name="references">The references of the buildings to lock.</param>
        /// <param name="commandTimeout">The timeout in seconds for the statement. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LockAsync(NpgsqlConnection npgsqlConnection, int[] countyIds, string[] references, int commandTimeout, CancellationToken cancellationToken)
        {
            await using NpgsqlCommand npgsqlCommand = new($@"
                SELECT id
                FROM {TableName}
                WHERE county_id = ANY(@countyIds)
                  AND reference = ANY(@references)
                FOR UPDATE;", npgsqlConnection);

            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references });

            await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}