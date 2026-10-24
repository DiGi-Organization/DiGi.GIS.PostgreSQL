using DiGi.Core.Interfaces;
using DiGi.GIS.PostgreSQL.Enums;
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
    /// Provides a base implementation for a PostgreSQL converter specifically designed for building 2D referenced objects.
    /// <para>Rows are addressed at two levels - <c>(county_id, reference)</c> for everything held for a building, <c>unique_id</c> for one stored object within that set - and a building may hold several rows. <see cref="Building2DReferencedObject{TUniqueObject}"/> describes the convention in full; it decides which of the read and remove methods below is the right one for a given job.</para>
    /// </summary>
    /// <typeparam name="TBuilding2DReferencedObject">The type of the building 2D referenced object.</typeparam>
    /// <typeparam name="TUniqueObject">The type of the unique object used for identification, which must implement the <see cref="IUniqueObject"/> interface.</typeparam>
    public abstract class Building2DReferencedObjectPostgreSQLConverter<TBuilding2DReferencedObject, TUniqueObject> : ReferencedObjectPostgreSQLConverter<TBuilding2DReferencedObject, TUniqueObject> where TBuilding2DReferencedObject : Building2DReferencedObject<TUniqueObject> where TUniqueObject : IUniqueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DReferencedObjectPostgreSQLConverter{TBuilding2DReferencedObject, TUniqueObject}" /> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData" /> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public Building2DReferencedObjectPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Asynchronously retrieves the total count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The optional unique identifier of the county to filter the count; if null, the total count across all counties is retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a long integer.</returns>
        public async Task<long> GetCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            string tableName = TableName;
            if (countyId != null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, tableName, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the total count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="countyId">The optional unique identifier of the county to filter the count; if null, the total count across all counties is retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a long integer.</returns>
        public async Task<long> GetCountAsync(int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetCountAsync(npgsqlConnection, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the most recent <c>created_at</c> stamp among the stored rows, optionally within one county partition.
        /// <para>This is the only freshness signal the stored objects carry - the object JSON has no timestamp - so it is what tells a county regenerated by the latest run apart from one still holding the previous run's rows.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The optional unique identifier of the county whose partition is read; if null, the whole table is read.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the latest creation stamp in UTC, or null when the connection is null, the table or partition does not exist, or it holds no rows.</returns>
        public async Task<DateTimeOffset?> GetLatestCreatedAtAsync(NpgsqlConnection? npgsqlConnection, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string tableName = TableName;
            if (countyId != null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            bool exists = await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, tableName, cancellationToken: cancellationToken);
            if (!exists)
            {
                return null;
            }

            await using NpgsqlCommand npgsqlCommand = new($"SELECT MAX(created_at) FROM {tableName};", npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            object? value = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
            if (value is not DateTime dateTime)
            {
                return null;
            }

            // Npgsql hands timestamptz back as a UTC DateTime; the offset is fixed accordingly.
            return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        }

        /// <summary>
        /// Asynchronously retrieves the most recent <c>created_at</c> stamp among the stored rows, optionally within one county partition, on a connection opened for the call.
        /// </summary>
        /// <param name="countyId">The optional unique identifier of the county whose partition is read; if null, the whole table is read.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the latest creation stamp in UTC, or null when no connection could be made, the table or partition does not exist, or it holds no rows.</returns>
        public async Task<DateTimeOffset?> GetLatestCreatedAtAsync(int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetLatestCreatedAtAsync(npgsqlConnection, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated count of records, optionally filtered by a specific county identifier and with the option to update table statistics before estimation.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The optional integer identifier of the county used to filter the count.</param>
        /// <param name="analyze">A boolean value indicating whether an ANALYZE operation should be performed on the table to update statistics for a more accurate estimate.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated record count as a nullable long integer, -1 when the partition exists but has not been analysed, or null if the table does not exist or connection is null.</returns>
        public async Task<long?> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string tableName = TableName;
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
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary keyed by county identifier holding the estimated row count for every county whose partition exists, or null when the connection or the identifiers are null.</returns>
        public async Task<Dictionary<int, long>?> GetEstimatedCountsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, bool analyze = false, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || countyIds is null)
            {
                return null;
            }

            Dictionary<string, int> countyIds_ByTableName = [];
            foreach (int countyId in countyIds)
            {
                countyIds_ByTableName[string.Format("{0}_{1}", TableName, countyId)] = countyId;
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
        /// Asynchronously gets the estimated row count for the specified county identifiers in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> to use for the query.</param>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for.</param>
        /// <param name="analyze">A boolean indicating whether to run a vacuum analyze operation before fetching the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated number of rows as a long, or -1 when an error occurs or any named county has no partition or has never been analysed.</returns>
        public async Task<long> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int> countyIds, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            // Alone among the sibling converters this overload used to walk countyIds with no null check,
            // so a null argument threw NullReferenceException instead of answering -1 like the rest.
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
        /// Asynchronously retrieves an estimated row count, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county to filter the estimate.</param>
        /// <param name="analyze">A boolean value indicating whether to run an analysis operation before fetching the count to ensure higher accuracy.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated row count as a nullable <see cref="System.Int64"/>, -1 when the partition exists but has not been analysed, or null if an error occurs.</returns>
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
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
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
        /// Asynchronously retrieves the estimated row count for the specified collection of county identifiers.
        /// </summary>
        /// <param name="countyIds">A collection of integers representing the unique identifiers of the counties to be counted.</param>
        /// <param name="analyze">A boolean value indicating whether to perform a database analysis operation before retrieving the estimate.</param>
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
        /// Asynchronously retrieves a building 2D referenced object by its unique identifier and an optional county identifier.
        /// </summary>
        /// <param name="id">The long integer unique identifier of the item to retrieve.</param>
        /// <param name="countyId">The optional integer identifier of the county associated with the item.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the retrieved <seeref name="TBuilding2DReferencedObject"/>, or null if no item with the specified identifier was found.</returns>
        public async Task<TBuilding2DReferencedObject?> GetItemByIdAsync(long id, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            return await GetItemsByIdsAsync([id], countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken).ContinueWith(t => t.Result?.FirstOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <seeref name="TBuilding2DReferencedObject"/> using the specified reference and optional county identifier.
        /// <para>A reference can hold several rows, one per stored object, so this returns the most recently stored of them. Use <see cref="GetItemsByReferenceAsync"/> when the whole set is wanted.</para>
        /// </summary>
        /// <param name="reference">The string reference of the item to retrieve.</param>
        /// <param name="countyId">The optional integer identifier for the county.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone if the item is not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matching <seeref name="TBuilding2DReferencedObject"/> if found; otherwise, null.</returns>
        public async Task<TBuilding2DReferencedObject?> GetItemByReferenceAsync(string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return await GetItemsByReferencesAsync([reference], countyId, 1, fallbackByReference, commandTimeout: commandTimeout, cancellationToken: cancellationToken).ContinueWith(t => t.Result?.FirstOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of items of type <typeparamref name="TBuilding2DReferencedObject"/> based on the specified identifiers and county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="ids">A collection of <see cref="System.Int64"/> identifiers for the items to retrieve.</param>
        /// <param name="countyId">The optional integer identifier of the county used to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TBuilding2DReferencedObject}"/> of matching items, or null if the connection or identifiers are null.</returns>
        public async Task<List<TBuilding2DReferencedObject>?> GetItemsByIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<long>? ids, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ids is null)
            {
                return null;
            }

            List<TBuilding2DReferencedObject>? result = [];

            if (!ids.Any())
            {
                return result;
            }

            string commandText = countyId.HasValue ? $@"
                SELECT id, county_id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE county_id = @countyId
                  AND id = ANY(@ids);" : $@"
                SELECT id, county_id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE id = ANY(@ids);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.AddWithValue("ids", ids.ToArray());

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            result = await ReadAsync(npgsqlCommand, cancellationToken);

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of building 2D referenced objects based on the specified identifiers and an optional county identifier.
        /// </summary>
        /// <param name="ids">A collection of long identifiers of the items to retrieve.</param>
        /// <param name="countyId">The optional nullable integer identifier of the county used to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TBuilding2DReferencedObject}"/> of matching items, or null if no items are found or the provided identifiers are null.</returns>
        public async Task<List<TBuilding2DReferencedObject>?> GetItemsByIdsAsync(IEnumerable<long>? ids, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (ids is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetItemsByIdsAsync(npgsqlConnection, ids, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the largest <c>id</c> held in the county partition of the table.
        /// <para>The upper bound for <see cref="GetItemsByIdRangeAsync(Npgsql.NpgsqlConnection?, int, long, long, int, int, System.Threading.CancellationToken)"/> keyset paging: the caller reads it once when the part starts, so rows a concurrent regeneration inserts afterwards are not chased by the walk - they are stamped by the creation path that wrote them.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The identifier of the county partition to read.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the largest identifier of the partition, or null when the connection is null or the partition holds no rows.</returns>
        public async Task<long?> GetMaxIdAsync(NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandQuery = $@"
                SELECT max(id)
                FROM {TableName}
                WHERE county_id = @county_id;";

            await using NpgsqlCommand npgsqlCommand = new(commandQuery, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });

            object? value = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
            if (value is not long id)
            {
                return null;
            }

            return id;
        }

        /// <summary>
        /// Asynchronously retrieves the largest <c>id</c> held in the county partition of the table, on a connection opened for the call.
        /// </summary>
        /// <param name="countyId">The identifier of the county partition to read.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the largest identifier of the partition, or null when the connection could not be created or the partition holds no rows.</returns>
        public async Task<long?> GetMaxIdAsync(int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetMaxIdAsync(npgsqlConnection, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the stored objects of a county partition within an identifier range, ordered by identifier.
        /// <para>The key is <c>id</c> rather than <c>ctid</c> because the caller updates the rows it walks: an update writes a new tuple version elsewhere in the heap, which a <c>ctid</c> walk would read twice or miss, while the identifier never changes on update. The partition's primary key leads with <c>id</c>, so the range reads the index. The upper bound is read once by the caller when the part starts, so rows a concurrent regeneration inserts afterwards are not chased - they are stamped by the creation path that wrote them.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The identifier of the county partition to read.</param>
        /// <param name="idAfter">The exclusive lower bound of the range; zero for the first page.</param>
        /// <param name="idMax">The inclusive upper bound of the range - the partition's largest identifier when the part started.</param>
        /// <param name="limit">The maximum number of rows to return.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the page of stored objects in identifier order, or null when the connection is null.</returns>
        public async Task<List<TBuilding2DReferencedObject>?> GetItemsByIdRangeAsync(NpgsqlConnection? npgsqlConnection, int countyId, long idAfter, long idMax, int limit = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandQuery = $@"
                SELECT id, county_id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE county_id = @county_id
                  AND id > @id_after
                  AND id <= @id_max
                ORDER BY id
                LIMIT @limit;";

            await using NpgsqlCommand npgsqlCommand = new(commandQuery, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("id_after", NpgsqlDbType.Bigint) { Value = idAfter });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("id_max", NpgsqlDbType.Bigint) { Value = idMax });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit });

            return await ReadAsync(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the stored objects of a county partition within an identifier range, ordered by identifier, on a connection opened for the call.
        /// </summary>
        /// <param name="countyId">The identifier of the county partition to read.</param>
        /// <param name="idAfter">The exclusive lower bound of the range; zero for the first page.</param>
        /// <param name="idMax">The inclusive upper bound of the range - the partition's largest identifier when the part started.</param>
        /// <param name="limit">The maximum number of rows to return.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the page of stored objects in identifier order, or null when the connection could not be created.</returns>
        public async Task<List<TBuilding2DReferencedObject>?> GetItemsByIdRangeAsync(int countyId, long idAfter, long idMax, int limit = 1000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetItemsByIdRangeAsync(npgsqlConnection, countyId, idAfter, idMax, limit, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of building 2D referenced objects based on the specified reference and optional filters.
        /// </summary>
        /// <param name="reference">The string reference used to identify the items.</param>
        /// <param name="countyId">The optional integer identifier of the county used to filter the results.</param>
        /// <param name="limit">The optional maximum number of items to retrieve, specified as a long integer.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone if the items are not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <typeparamref name="TBuilding2DReferencedObject"/> objects if matching items are found; otherwise, null.</returns>
        public async Task<List<TBuilding2DReferencedObject>?> GetItemsByReferenceAsync(string reference, int? countyId, long? limit = null, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return await GetItemsByReferencesAsync([reference], countyId, limit, fallbackByReference, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of items that implement <typeparamref name="TBuilding2DReferencedObject" /> based on the provided references and optional filters.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the PostgreSQL database.</param>
        /// <param name="references">A collection of <see cref="System.String" /> representing the references of the items to be retrieved.</param>
        /// <param name="countyId">The optional integer identifier of the county used to filter the results.</param>
        /// <param name="limit">The optional maximum number of items to retrieve as a <see cref="System.Int64" />.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search for any references not found in the initial search. Only applies when <paramref name="countyId"/> is given.</param>
        /// <param name="fallbackCountyIds">The county parts the fallback searches - normally every polygon part of <paramref name="countyId"/>'s county, which is where a row filed under a sibling part sits. The search is then pruned to those partitions; <paramref name="countyId"/> itself is skipped, and a county with no other part needs no fallback at all. When null the fallback matches the reference alone across every partition of the table, which for thousands of references not stored anywhere - a county's first prediction run - read 10 095 references in 29 s and timed out under load.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TBuilding2DReferencedObject}" /> of matching items, or null if the connection or references are null.</returns>
        public async Task<List<TBuilding2DReferencedObject>?> GetItemsByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int? countyId, long? limit = null, bool fallbackByReference = false, IEnumerable<int>? fallbackCountyIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            // Base query with reference filter and optional countyId filter.
            // The ordering is not cosmetic: a reference can hold several rows - one per stored object -
            // so without it a limited read returns whichever row the plan happened to reach first, and
            // the answer changes with a vacuum or the heap ordering. created_at alone does not settle it
            // either, because now() is transaction start and a bulk write stamps every row of the batch
            // identically; id is what actually decides between them.
            string commandText = countyId.HasValue ? $@"
                SELECT id, county_id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE county_id = @countyId
                  AND reference = ANY(@references)
                ORDER BY created_at DESC, id DESC" : $@"
                SELECT id, county_id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE reference = ANY(@references)
                ORDER BY created_at DESC, id DESC";

            // Append LIMIT if provided
            if (limit.HasValue)
            {
                commandText += " LIMIT @limit";
            }

            commandText += ";";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.AddWithValue("references", references_Array);

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            if (limit.HasValue)
            {
                npgsqlCommand.Parameters.AddWithValue("limit", limit.Value);
            }

            List<TBuilding2DReferencedObject>? result = await ReadAsync(npgsqlCommand, cancellationToken);
            if (result is null)
            {
                return null;
            }

            if (fallbackByReference && countyId is not null)
            {
                HashSet<string> foundReferences = [.. result.Where(r => !string.IsNullOrWhiteSpace(r.Reference)).Select(r => r.Reference!)];
                string[] missingReferences = [.. references_Array.Where(r => !foundReferences.Contains(r))];

                if (missingReferences.Length > 0)
                {
                    long? remainingLimit = limit.HasValue ? limit.Value - result.Count : null;
                    if (!limit.HasValue || (remainingLimit.HasValue && remainingLimit.Value > 0))
                    {
                        List<TBuilding2DReferencedObject>? fallbackItems = null;
                        if (fallbackCountyIds is null)
                        {
                            fallbackItems = await GetItemsByReferencesAsync(npgsqlConnection, missingReferences, null, remainingLimit, false, null, commandTimeout, cancellationToken);
                        }
                        else
                        {
                            int[] fallbackCountyIds_Array = [.. fallbackCountyIds.Where(x => x > 0 && x != countyId.Value).Distinct()];
                            if (fallbackCountyIds_Array.Length > 0)
                            {
                                // The same ordering as the base query, for the same reason.
                                string commandText_Fallback = $@"
                                    SELECT id, county_id, unique_id, reference, object, created_at
                                    FROM {TableName}
                                    WHERE county_id = ANY(@countyIds)
                                      AND reference = ANY(@references)
                                    ORDER BY created_at DESC, id DESC{(remainingLimit.HasValue ? " LIMIT @limit" : string.Empty)};";

                                await using NpgsqlCommand npgsqlCommand_Fallback = new(commandText_Fallback, npgsqlConnection);
                                npgsqlCommand_Fallback.CommandTimeout = commandTimeout;
                                npgsqlCommand_Fallback.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = fallbackCountyIds_Array });
                                npgsqlCommand_Fallback.Parameters.AddWithValue("references", missingReferences);
                                if (remainingLimit.HasValue)
                                {
                                    npgsqlCommand_Fallback.Parameters.AddWithValue("limit", remainingLimit.Value);
                                }

                                fallbackItems = await ReadAsync(npgsqlCommand_Fallback, cancellationToken);
                            }
                        }

                        if (fallbackItems is not null && fallbackItems.Count > 0)
                        {
                            result.AddRange(fallbackItems);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of items based on the provided references and county identifier.
        /// </summary>
        /// <param name="references">An optional collection of strings representing the unique references of the items to be retrieved.</param>
        /// <param name="countyId">An optional integer specifying the county identifier to filter the results.</param>
        /// <param name="limit">An optional long value that specifies the maximum number of items to return.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search for any references not found in the initial search. Only applies when <paramref name="countyId"/> is given.</param>
        /// <param name="fallbackCountyIds">The county parts the fallback searches, normally every polygon part of <paramref name="countyId"/>'s county; pruned to those partitions. When null the fallback matches the reference alone across the whole table. See the connection overload.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <typeparamref name="TBuilding2DReferencedObject"/> objects if matches are found; otherwise, null.</returns>
        public async Task<List<TBuilding2DReferencedObject>?> GetItemsByReferencesAsync(IEnumerable<string>? references, int? countyId, long? limit = null, bool fallbackByReference = false, IEnumerable<int>? fallbackCountyIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            return await GetItemsByReferencesAsync(npgsqlConnection, references, countyId, limit, fallbackByReference, fallbackCountyIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves every reference held under the specified county identifier, or across all counties if null.
        /// <para>The whole row is not read - only the reference column - so this stays usable on a county part holding tens of thousands of rows, which reading the objects would not be.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="countyId">The optional identifier of the county row to read; if null, references across all counties are retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the distinct references held, or null when the connection is null.</returns>
        public async Task<HashSet<string>?> GetReferencesAsync(NpgsqlConnection? npgsqlConnection, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = countyId.HasValue ? $@"
                SELECT DISTINCT reference
                FROM {TableName}
                WHERE county_id = @countyId
                  AND reference IS NOT NULL;" : $@"
                SELECT DISTINCT reference
                FROM {TableName}
                WHERE reference IS NOT NULL;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            HashSet<string> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(npgsqlDataReader.GetString(0));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves every reference held under the specified county identifier, or across all counties if null.
        /// <para>The whole row is not read - only the reference column - so this stays usable on a county part holding tens of thousands of rows, which reading the objects would not be.</para>
        /// </summary>
        /// <param name="countyId">The optional identifier of the county row to read; if null, references across all counties are retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the distinct references held, or null when the connection could not be created.</returns>
        public async Task<HashSet<string>?> GetReferencesAsync(int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetReferencesAsync(npgsqlConnection, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the unique identifiers of every row held for the given references under an optional county identifier.
        /// <para>Only the identifier column is read. It is the counterpart of <c>GetItemsByReferencesAsync</c> for a caller that wants to address the rows rather than the objects inside them - reading the objects to reach one column would move every stored object across the connection, which on a table of building models is gigabytes fetched for a value a few characters long.</para>
        /// <para>This is the read half of replacing what a building holds. Take the identifiers first, write the new rows, then delete the identifiers taken here with <c>RemoveByUniqueIdsAsync(uniqueIds, countyId)</c>. Ordered that way round a run interrupted between the write and the delete leaves the building holding both its old and its new object, which is recoverable, rather than holding neither.</para>
        /// <para>A table that does not exist answers an empty set rather than throwing: nothing is stored, so nothing can be superseded, and the write that follows is what creates it.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="references">The references whose rows are to be identified.</param>
        /// <param name="countyId">The optional identifier of the county row holding them; if null, searches across all counties.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone for any references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifiers held for the references, an empty set when the table does not exist, or null when no references were given or the connection is null.</returns>
        public async Task<HashSet<string>?> GetUniqueIdsByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            // Nothing of this type has ever been stored, so nothing can be superseded by the write
            // that follows - and that write is what creates the table. Failing here instead leaves a
            // storage database whose table was dropped to regenerate it from scratch impossible to
            // write to at all: the read refuses, the caller answers 500, and the DDL is never reached.
            // Creating the table here would be the wrong half of the fix. It belongs to the write
            // path, and TableAsync_Building2DReferencedObject carries the [ReferencedObjectIndexes]
            // migration, which can spend minutes building an index across every partition - work a
            // read has no business triggering.
            bool exists = await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName, cancellationToken: cancellationToken);
            if (!exists)
            {
                return [];
            }

            string commandText = countyId.HasValue ? $@"
                SELECT unique_id, reference
                FROM {TableName}
                WHERE county_id = @countyId
                  AND reference = ANY(@references);" : $@"
                SELECT unique_id, reference
                FROM {TableName}
                WHERE reference = ANY(@references);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("references", references_Array);
            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            HashSet<string> result = [];
            HashSet<string> foundReferences = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(npgsqlDataReader.GetString(0));
                if (!npgsqlDataReader.IsDBNull(1))
                {
                    foundReferences.Add(npgsqlDataReader.GetString(1));
                }
            }

            if (fallbackByReference && countyId is not null)
            {
                string[] missingReferences = [.. references_Array.Where(r => !foundReferences.Contains(r))];
                if (missingReferences.Length > 0)
                {
                    HashSet<string>? fallbackUniqueIds = await GetUniqueIdsByReferencesAsync(npgsqlConnection, missingReferences, null, false, commandTimeout, cancellationToken);
                    if (fallbackUniqueIds is not null && fallbackUniqueIds.Count > 0)
                    {
                        foreach (string uniqueId in fallbackUniqueIds)
                        {
                            result.Add(uniqueId);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the unique identifiers of every row held for the given references under an optional county identifier.
        /// <para>Only the identifier column is read. It is the counterpart of <c>GetItemsByReferencesAsync</c> for a caller that wants to address the rows rather than the objects inside them - reading the objects to reach one column would move every stored object across the connection, which on a table of building models is gigabytes fetched for a value a few characters long.</para>
        /// <para>This is the read half of replacing what a building holds. Take the identifiers first, write the new rows, then delete the identifiers taken here with <c>RemoveByUniqueIdsAsync(uniqueIds, countyId)</c>. Ordered that way round a run interrupted between the write and the delete leaves the building holding both its old and its new object, which is recoverable, rather than holding neither.</para>
        /// </summary>
        /// <param name="references">The references whose rows are to be identified.</param>
        /// <param name="countyId">The optional identifier of the county row holding them; if null, searches across all counties.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone for any references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifiers held for the references, an empty set when the table does not exist, or null when no references were given or the connection could not be created.</returns>
        public async Task<HashSet<string>?> GetUniqueIdsByReferencesAsync(IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            return await GetUniqueIdsByReferencesAsync(npgsqlConnection, references, countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Moves every row held for the given references under some other county row into <paramref name="countyId"/>, so that all of a building's data sits under the county part the building actually belongs to.
        /// <para>This repairs data filed under the wrong part of a multi-part county. <c>building_2d</c> is the source of truth for the <c>(county_id, reference)</c> pair; rows in this table that disagree with it are unreachable, because every read here filters on <c>county_id</c> first and so returns nothing for the part the building is now known to belong to.</para>
        /// <para><c>county_id</c> is the <b>partition key</b>, so this is a row movement between partitions rather than an ordinary column update. The destination partition is created first - PostgreSQL cannot move a row into a partition that does not exist - and the identifiers of the rows are preserved, so anything holding an <c>id</c> from before the call still addresses the same record.</para>
        /// <para><b>Nothing is deleted.</b> The table constrains <c>UNIQUE (county_id, unique_id)</c>, so a row cannot move onto a destination that already holds that same stored object, and two rows carrying one <c>unique_id</c> under two different wrong counties cannot both arrive. Such a row is left exactly where it is and its reference is <b>not</b> reported, so a caller that compares the result against what it passed in learns which references still hold something to be resolved by hand - with <see cref="GetItemsByReferenceAsync"/> and <see cref="RemoveByUniqueIdsAsync(IEnumerable{string}?, string, int?, int, CancellationToken)"/> - rather than having it silently discarded here.</para>
        /// <para><b>Cost.</b> The county a stray row ended up under is not known, so the statement cannot be pruned to a partition and reads every partition of the table once. Call it once per county with all of that county's references, never once per reference: the references are sent in batches inside, so one call is not one statement to lose on a timeout.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="references">The references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county row every one of those references should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched, which the index cannot serve.</param>
        /// <param name="batchSize">The number of references sent in one statement. One statement for a whole county is fewer round trips and a single timeout to lose them all in.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references that had at least one row moved - not the number of rows - or null when no references were given or the connection is null.</returns>
        public async Task<HashSet<string>?> RefreshCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int countyId, IEnumerable<int>? countyIds_Source = null, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            // Deduplicating here rather than in the statement keeps the array parameter as small as
            // the caller's data allows; the server would otherwise carry the repeats through the scan.
            HashSet<string> references_Unique = [];
            foreach (string reference in references)
            {
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    references_Unique.Add(reference);
                }
            }

            HashSet<string> result = [];

            if (references_Unique.Count == 0)
            {
                return result;
            }

            // Nothing of this type has ever been stored, so there is nothing to move. Creating the
            // table here instead would be worse than useless: TableAsync_Building2DReferencedObject
            // carries the [ReferencedObjectIndexes] migration and can spend minutes building an index
            // across every partition, which a refresh has no business triggering.
            bool exists = await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName);
            if (!exists)
            {
                return result;
            }

            // A row cannot move into a partition that does not exist - without this the update fails
            // with 'no partition of relation found for row'. It is an idempotent catalog check once
            // the partition is there.
            await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(TableName, countyId);

            // One statement for the whole batch, which makes it one transaction as well: an
            // interrupted run leaves every row either under the county it came from or under
            // countyId, never half-moved.
            //
            // MATERIALIZED pins the expensive part to a single evaluation. The CTE is the only place
            // the table is read without a county_id to prune on, and inlining it would repeat that
            // scan for the update.
            //
            // county_id <> @countyId is the whole of the filtering: rows already filed correctly are
            // not touched, and a reference split across two counties is handled without a separate
            // pass. Excluding references that merely have a row under countyId would skip exactly
            // those split ones.
            //
            // The parts the rows can still be sitting under are written into the statement when the caller
            // knows them, because the index here leads with county_id: without a predicate on it there is
            // nothing to seek on and every partition is read. A batch carrying one reference measured close
            // to a minute that way (DiGi-Organization/DiGi.GIS.PostgreSQL#68). Callers that do not know where
            // a row strayed to still pay for the whole table, which is the cost this parameter avoids.
            int[]? countyIds_Array = countyIds_Source is null ? null : [.. new HashSet<int>(countyIds_Source)];

            string condition_Source = countyIds_Array is null ? string.Empty : "\r\n                      AND t.county_id = ANY(@countyIds)";

            // NOT EXISTS is the collision guard for UNIQUE (county_id, unique_id). It is cheap
            // despite the surrounding scan: county_id is fixed, so it prunes to the destination
            // partition and probes the index that constraint is already backed by.
            //
            // ROW_NUMBER guards the other collision - two rows carrying one unique_id under two
            // different wrong counties both pass NOT EXISTS and would then collide with each other on
            // arrival. One moves, the rest stay. created_at alone does not settle which one, because
            // a bulk write stamps every row of the batch identically; id is what decides.
            //
            // The update addresses the row by its primary key with the partition key present, so it
            // is a pruned index lookup rather than a second scan. ctid would not do - it is unique
            // only within a partition.
            string commandText = $@"
                WITH stray AS MATERIALIZED (
                    SELECT t.id AS id,
                           t.county_id AS county_id,
                           ROW_NUMBER() OVER (PARTITION BY t.unique_id ORDER BY t.created_at DESC, t.id DESC) AS move_rank
                    FROM {TableName} t
                    WHERE t.reference = ANY(@references)
                      AND t.county_id <> @countyId{condition_Source}
                      AND NOT EXISTS (
                              SELECT 1
                              FROM {TableName} t_Target
                              WHERE t_Target.county_id = @countyId
                                AND t_Target.unique_id = t.unique_id)
                )
                UPDATE {TableName} t
                SET county_id = @countyId
                FROM stray s
                WHERE t.id = s.id
                  AND t.county_id = s.county_id
                  AND s.move_rank = 1
                RETURNING t.reference;";

            string[] references_Array = [.. references_Unique];

            // Sent in batches rather than as one statement carrying every reference of the county. One
            // statement is fewer round trips, but it is also one timeout: a county of 157 354 buildings ran
            // for the whole ten minutes and then lost all of it, while the counties either side of it
            // finished (DiGi-Organization/DiGi.GIS.PostgreSQL#68). A batch that fits inside the timeout turns
            // a county that cannot be carried at all into one carried a thousand references at a time.
            int batchSize_Effective = batchSize < 1 ? 1 : batchSize;

            List<string> references_List = [.. references_Unique];

            for (int i = 0; i < references_List.Count; i += batchSize_Effective)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string[] references_Batch = [.. references_List.GetRange(i, Math.Min(batchSize_Effective, references_List.Count - i))];

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId);
                npgsqlCommand.Parameters.AddWithValue("references", references_Batch);

                if (countyIds_Array is not null)
                {
                    npgsqlCommand.Parameters.AddWithValue("countyIds", countyIds_Array);
                }

                // RETURNING yields one row per record moved, and several of them can belong to one
                // building - the set collapses those without the server having to sort for DISTINCT.
                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    result.Add(npgsqlDataReader.GetString(0));
                }
            }

            return result;
        }

        /// <summary>
        /// Moves every row held for the given references under some other county row into <paramref name="countyId"/>, so that all of a building's data sits under the county part the building actually belongs to.
        /// <para>This repairs data filed under the wrong part of a multi-part county. <c>building_2d</c> is the source of truth for the <c>(county_id, reference)</c> pair; rows in this table that disagree with it are unreachable, because every read here filters on <c>county_id</c> first and so returns nothing for the part the building is now known to belong to.</para>
        /// <para><c>county_id</c> is the <b>partition key</b>, so this is a row movement between partitions rather than an ordinary column update. The destination partition is created first - PostgreSQL cannot move a row into a partition that does not exist - and the identifiers of the rows are preserved, so anything holding an <c>id</c> from before the call still addresses the same record.</para>
        /// <para><b>Nothing is deleted.</b> The table constrains <c>UNIQUE (county_id, unique_id)</c>, so a row cannot move onto a destination that already holds that same stored object, and two rows carrying one <c>unique_id</c> under two different wrong counties cannot both arrive. Such a row is left exactly where it is and its reference is <b>not</b> reported, so a caller that compares the result against what it passed in learns which references still hold something to be resolved by hand - with <see cref="GetItemsByReferenceAsync"/> and <see cref="RemoveByUniqueIdsAsync(IEnumerable{string}?, string, int?, int, CancellationToken)"/> - rather than having it silently discarded here.</para>
        /// <para><b>Cost.</b> The county a stray row ended up under is not known, so the statement cannot be pruned to a partition and reads every partition of the table once. Call it once per county with all of that county's references, never once per reference: the references are sent in batches inside, so one call is not one statement to lose on a timeout.</para>
        /// </summary>
        /// <param name="references">The references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county row every one of those references should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched, which the index cannot serve.</param>
        /// <param name="batchSize">The number of references sent in one statement. One statement for a whole county is fewer round trips and a single timeout to lose them all in.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references that had at least one row moved - not the number of rows - or null when no references were given or the connection could not be created.</returns>
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

            return await RefreshCountyIdsAsync(npgsqlConnection, references, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Probes for the rows held for the given references under a county row other than <paramref name="countyId"/>, and classifies each reference that has at least one such row as movable or blocked.
        /// <para>This is the read half of <see cref="RefreshCountyIdsAsync(IEnumerable{string}, int, IEnumerable{int}, int, int, CancellationToken)"/>. It lifts the mover's stray CTE and asks the one question the mover answers only by writing: which of the references would it move, and which would the destination refuse. Because the classification is the mover's own rule, a dry run reported here and the move that follows it agree by construction - a reference is movable iff the move would report it, and blocked iff the move would report nothing for it.</para>
        /// <para>Blocked means the destination part already holds the stored object: <c>UNIQUE (county_id, unique_id)</c> refuses the arrival, and deleting either copy is a decision for a person, so the reference is reported rather than settled. The two sets are disjoint and together hold every reference with at least one stray row.</para>
        /// <para>Nothing is written: not even the destination partition is created, which the move creates and the probe only reads. It can therefore run against a database the move must not touch yet.</para>
        /// <para><b>Cost.</b> The county a stray row ended up under is not known, so the statement cannot be pruned to a partition unless <paramref name="countyIds_Source"/> names the parts it can sit under, and it reads every other partition of the table once. Call it once per county with all of that county's references, never once per reference: the references are sent in batches inside, so one call is not one statement to lose on a timeout.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="references">The references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county row every one of those references should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched, which the index cannot serve.</param>
        /// <param name="batchSize">The number of references sent in one statement. One statement for a whole county is fewer round trips and a single timeout to lose them all in.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result classifies the references with at least one stray row into the ones the move would take and the ones it would refuse, an empty result when the table does not exist, or null when no references were given or the connection is null.</returns>
        public async Task<Building2DReferencedObjectStrayResult?> GetStrayReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int countyId, IEnumerable<int>? countyIds_Source = null, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            // Deduplicating here rather than in the statement keeps the array parameter as small as
            // the caller's data allows; the server would otherwise carry the repeats through the scan.
            HashSet<string> references_Unique = [];
            foreach (string reference in references)
            {
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    references_Unique.Add(reference);
                }
            }

            if (references_Unique.Count == 0)
            {
                return new Building2DReferencedObjectStrayResult(countyId, null, null);
            }

            // Nothing of this type has ever been stored, so there is nothing to report. The probe does
            // not create the table - a read must not trigger the migration - and it does not create the
            // destination partition either, which the move creates and the probe only reads.
            bool exists = await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName);
            if (!exists)
            {
                return new Building2DReferencedObjectStrayResult(countyId, null, null);
            }

            // The parts the rows can still be sitting under are written into the statement when the caller
            // knows them, because the index here leads with county_id: without a predicate on it there is
            // nothing to seek on and every partition is read (DiGi-Organization/DiGi.GIS.PostgreSQL#68).
            int[]? countyIds_Array = countyIds_Source is null ? null : [.. new HashSet<int>(countyIds_Source)];

            string condition_Source = countyIds_Array is null ? string.Empty : "\r\n                      AND t.county_id = ANY(@countyIds)";

            // The stray CTE is the mover's own, minus the two columns the update addresses the row by:
            // the probe classifies, it does not move.
            //
            // The collision test sits on the ranked row rather than in the CTE as in the mover, which is
            // the same filter: the destination either holds a stored object or it does not, for every row
            // carrying it, so the mover's NOT EXISTS keeps or drops the whole set of a unique_id. The
            // top-ranked row of a unique_id passes the mover's CTE iff it passes the test here, and the
            // two statements therefore agree on every reference by construction.
            string commandText = $@"
                WITH stray AS MATERIALIZED (
                    SELECT t.reference AS reference,
                           t.unique_id AS unique_id,
                           ROW_NUMBER() OVER (PARTITION BY t.unique_id ORDER BY t.created_at DESC, t.id DESC) AS move_rank
                    FROM {TableName} t
                    WHERE t.reference = ANY(@references)
                      AND t.county_id <> @countyId{condition_Source}
                ),
                movable AS (
                    SELECT DISTINCT s.reference
                    FROM stray s
                    WHERE s.move_rank = 1
                      AND NOT EXISTS (
                              SELECT 1
                              FROM {TableName} t_Target
                              WHERE t_Target.county_id = @countyId
                                AND t_Target.unique_id = s.unique_id)
                )
                SELECT s.reference AS reference,
                       s.reference IN (SELECT m.reference FROM movable m) AS movable
                FROM (SELECT DISTINCT reference FROM stray) s
                ORDER BY s.reference;";

            // Sent in batches rather than as one statement carrying every reference of the county, for
            // the same reason the mover is: one statement is fewer round trips, but it is also one
            // timeout to lose them all in (DiGi-Organization/DiGi.GIS.PostgreSQL#68).
            int batchSize_Effective = batchSize < 1 ? 1 : batchSize;

            List<string> references_List = [.. references_Unique];

            Building2DReferencedObjectStrayResult result = new(countyId, null, null);

            for (int i = 0; i < references_List.Count; i += batchSize_Effective)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string[] references_Batch = [.. references_List.GetRange(i, Math.Min(batchSize_Effective, references_List.Count - i))];

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId);
                npgsqlCommand.Parameters.AddWithValue("references", references_Batch);

                if (countyIds_Array is not null)
                {
                    npgsqlCommand.Parameters.AddWithValue("countyIds", countyIds_Array);
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    string reference = npgsqlDataReader.GetString(0);

                    // The classifier is the mover's own rule applied before the write, so the two halves
                    // of the pair cannot drift into disagreeing about what the move would do.
                    if (npgsqlDataReader.GetBoolean(1))
                    {
                        result.MovableReferences.Add(reference);
                    }
                    else
                    {
                        result.BlockedReferences.Add(reference);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Probes for the rows held for the given references under a county row other than <paramref name="countyId"/>, and classifies each reference that has at least one such row as movable or blocked.
        /// <para>Read half of <see cref="GetStrayReferencesAsync(NpgsqlConnection, IEnumerable{string}, int, IEnumerable{int}, int, int, CancellationToken)"/>: see that overload for what movable and blocked mean and for the cost of the read.</para>
        /// </summary>
        /// <param name="references">The references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county row every one of those references should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched, which the index cannot serve.</param>
        /// <param name="batchSize">The number of references sent in one statement. One statement for a whole county is fewer round trips and a single timeout to lose them all in.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result classifies the references with at least one stray row into the ones the move would take and the ones it would refuse, an empty result when the table does not exist, or null when no references were given or the connection could not be created.</returns>
        public async Task<Building2DReferencedObjectStrayResult?> GetStrayReferencesAsync(IEnumerable<string>? references, int countyId, IEnumerable<int>? countyIds_Source = null, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
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

            return await GetStrayReferencesAsync(npgsqlConnection, references, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Deletes the rows holding the given references under an optional county identifier.
        /// <para>A reference is unique only per <c>county_id</c>: the same building is held once per county row it was imported under, so a delete has to name the row as well as the reference. Deleting by reference alone would take the building out of every part of the county.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="references">The references to delete.</param>
        /// <param name="countyId">The optional identifier of the county row to delete them from; if null, deletes matching references across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, or null if the connection or references are null.</returns>
        public async Task<HashSet<long>?> RemoveAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
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
                DELETE FROM {TableName}
                WHERE county_id = @countyId
                  AND reference = ANY(@references)
                RETURNING id;" : $@"
                DELETE FROM {TableName}
                WHERE reference = ANY(@references)
                RETURNING id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("references", references_Array);

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            HashSet<long> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(npgsqlDataReader.GetInt64(0));
            }

            return result;
        }

        /// <summary>
        /// Deletes the rows holding the given references under an optional county identifier.
        /// <para>A reference is unique only per <c>county_id</c>: the same building is held once per county row it was imported under, so a delete has to name the row as well as the reference. Deleting by reference alone would take the building out of every part of the county.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="references">The references to delete.</param>
        /// <param name="countyId">The optional identifier of the county row to delete them from; if null, deletes matching references across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, which is how many of the references were really there.</returns>
        public async Task<HashSet<long>?> RemoveAsync(IEnumerable<string>? references, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            return await RemoveAsync(npgsqlConnection, references, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Deletes single stored objects from the set held for one building, naming each of them by its own unique identifier.
        /// <para>A building can hold several rows in this table - one per stored object - so <see cref="RemoveAsync(IEnumerable{string}?, int?, int, CancellationToken)"/>, which takes out everything held for a reference, is too blunt to correct one of them. This is the delete half of updating a single object: read the set with <see cref="GetItemsByReferenceAsync"/>, pick the one to change, remove it here, then write the replacement.</para>
        /// <para><c>county_id</c> and <c>unique_id</c> already identify the row on their own - the table declares <c>UNIQUE (county_id, unique_id)</c>. The reference is required as well and is matched as a guard, so a unique identifier belonging to a different building cannot silently take out that building's object.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="uniqueIds">The unique identifiers of the stored objects to delete.</param>
        /// <param name="reference">The reference of the building the objects belong to.</param>
        /// <param name="countyId">The optional identifier of the county row holding them; if null, deletes matching unique IDs across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, or null if the connection or inputs are null.</returns>
        public async Task<HashSet<long>?> RemoveByUniqueIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? uniqueIds, string reference, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || uniqueIds is null || string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            string[] uniqueIds_Array = [.. uniqueIds];
            if (uniqueIds_Array.Length == 0)
            {
                return [];
            }

            string commandText = countyId.HasValue ? $@"
                DELETE FROM {TableName}
                WHERE county_id = @countyId
                  AND reference = @reference
                  AND unique_id = ANY(@uniqueIds)
                RETURNING id;" : $@"
                DELETE FROM {TableName}
                WHERE reference = @reference
                  AND unique_id = ANY(@uniqueIds)
                RETURNING id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("reference", reference);
            npgsqlCommand.Parameters.AddWithValue("uniqueIds", uniqueIds_Array);

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            HashSet<long> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(npgsqlDataReader.GetInt64(0));
            }

            return result;
        }

        /// <summary>
        /// Deletes single stored objects from the set held for one building, naming each of them by its own unique identifier.
        /// <para>A building can hold several rows in this table - one per stored object - so <see cref="RemoveAsync(IEnumerable{string}?, int?, int, CancellationToken)"/>, which takes out everything held for a reference, is too blunt to correct one of them. This is the delete half of updating a single object: read the set with <see cref="GetItemsByReferenceAsync"/>, pick the one to change, remove it here, then write the replacement.</para>
        /// <para><c>county_id</c> and <c>unique_id</c> already identify the row on their own - the table declares <c>UNIQUE (county_id, unique_id)</c>. The reference is required as well and is matched as a guard, so a unique identifier belonging to a different building cannot silently take out that building's object.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="uniqueIds">The unique identifiers of the stored objects to delete.</param>
        /// <param name="reference">The reference of the building the objects belong to.</param>
        /// <param name="countyId">The optional identifier of the county row holding them; if null, deletes matching unique IDs across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, which is how many of the unique identifiers were really there.</returns>
        public async Task<HashSet<long>?> RemoveByUniqueIdsAsync(IEnumerable<string>? uniqueIds, string reference, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (uniqueIds is null || string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await RemoveByUniqueIdsAsync(npgsqlConnection, uniqueIds, reference, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Deletes single stored objects from one county row, naming each of them by its own unique identifier without stating which building it belongs to.
        /// <para>The unguarded counterpart of <see cref="RemoveByUniqueIdsAsync(NpgsqlConnection?, IEnumerable{string}?, string, int?, int, CancellationToken)"/>. That overload matches the reference as well, so a unique identifier belonging to a different building cannot silently take out that building's object; it is the right one whenever the identifiers came from anywhere other than the rows being deleted, and it costs one statement per building.</para>
        /// <para>This one is for the case where the identifiers were read back from the very rows being replaced, with <see cref="GetUniqueIdsByReferencesAsync(NpgsqlConnection?, IEnumerable{string}?, int?, bool, int, CancellationToken)"/>. They already name exactly those rows, so the guard would only re-check what that read established, and a whole batch of buildings goes out in one statement instead of one each.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="uniqueIds">The unique identifiers of the stored objects to delete.</param>
        /// <param name="countyId">The optional identifier of the county row holding them; if null, deletes matching unique IDs across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, or null if the connection or inputs are null.</returns>
        public async Task<HashSet<long>?> RemoveByUniqueIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? uniqueIds, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || uniqueIds is null)
            {
                return null;
            }

            string[] uniqueIds_Array = [.. uniqueIds];
            if (uniqueIds_Array.Length == 0)
            {
                return [];
            }

            string commandText = countyId.HasValue ? $@"
                DELETE FROM {TableName}
                WHERE county_id = @countyId
                  AND unique_id = ANY(@uniqueIds)
                RETURNING id;" : $@"
                DELETE FROM {TableName}
                WHERE unique_id = ANY(@uniqueIds)
                RETURNING id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("uniqueIds", uniqueIds_Array);
            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            HashSet<long> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(npgsqlDataReader.GetInt64(0));
            }

            return result;
        }

        /// <summary>
        /// Deletes single stored objects from one county row, naming each of them by its own unique identifier without stating which building it belongs to.
        /// <para>The unguarded counterpart of <see cref="RemoveByUniqueIdsAsync(IEnumerable{string}?, string, int?, int, CancellationToken)"/>. That overload matches the reference as well, so a unique identifier belonging to a different building cannot silently take out that building's object; it is the right one whenever the identifiers came from anywhere other than the rows being deleted, and it costs one statement per building.</para>
        /// <para>This one is for the case where the identifiers were read back from the very rows being replaced, with <see cref="GetUniqueIdsByReferencesAsync(IEnumerable{string}?, int?, bool, int, CancellationToken)"/>. They already name exactly those rows, so the guard would only re-check what that read established, and a whole batch of buildings goes out in one statement instead of one each.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="uniqueIds">The unique identifiers of the stored objects to delete.</param>
        /// <param name="countyId">The optional identifier of the county row holding them; if null, deletes matching unique IDs across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, which is how many of the unique identifiers were really there.</returns>
        public async Task<HashSet<long>?> RemoveByUniqueIdsAsync(IEnumerable<string>? uniqueIds, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (uniqueIds is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await RemoveByUniqueIdsAsync(npgsqlConnection, uniqueIds, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the specified collection of building 2D referenced objects.
        /// <para>The upsert targets <c>(county_id, unique_id)</c>, which is the identity of the stored <b>object</b>, not of the building. An object read back from the database keeps its identifier and so replaces its own row; an object built fresh carries a new one and is <b>added</b> alongside whatever the building already holds. That is the intended behaviour - see <see cref="Building2DReferencedObject{TUniqueObject}"/> - so a caller that means to replace a building's data has to remove it first, with <see cref="RemoveAsync(IEnumerable{string}?, int?, int, CancellationToken)"/> for the whole set or <c>RemoveByUniqueIdsAsync</c> for one object.</para>
        /// </summary>
        /// <param name="building2DReferencedObjects">An <see cref="IEnumerable{TBuilding2DReferencedObject}"/> containing the referenced objects to be updated, or <c>null</c>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers written and the rows dropped before the database, or null when the update could not be attempted at all - no connection, or the table could not be created.</returns>
        public async Task<PostgreSQLUpdateResult?> UpdateAsync(IEnumerable<TBuilding2DReferencedObject>? building2DReferencedObjects, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DReferencedObjects is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool succeeded = await npgsqlConnection.TableAsync_Building2DReferencedObject(TableName);
            if (!succeeded)
            {
                return null;
            }

            HashSet<long> result = [];
            List<Rejection> rejections = [];
            List<TBuilding2DReferencedObject> building2DReferencedObjects_List = [.. building2DReferencedObjects.Where(x => x != null)];
            if (building2DReferencedObjects_List.Count == 0)
            {
                return new PostgreSQLUpdateResult(result, rejections);
            }

            IEnumerable<IGrouping<int?, TBuilding2DReferencedObject>> groupings = building2DReferencedObjects_List.GroupBy(x => x.CountyId);

            foreach (IGrouping<int?, TBuilding2DReferencedObject> grouping in groupings)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!grouping.Key.HasValue)
                {
                    // Recorded rather than skipped in silence, so the rejection count stays an exact
                    // account of the rows that never reached the database.
                    foreach (TBuilding2DReferencedObject building2DReferencedObject_Rejected in grouping)
                    {
                        rejections.Add(new Rejection(building2DReferencedObject_Rejected.Reference,
                            UpdateRejectionReason.MissingCounty));
                    }

                    continue;
                }

                int countyId = grouping.Key.Value;
                await npgsqlConnection.TableAsync_Building2DReferencedObject_Partition(TableName, countyId);

                List<TBuilding2DReferencedObject> objectsInGroup = [.. grouping];

                PostgreSQLUpdateResult? result_Group = await UpdateAsync(npgsqlConnection, null, objectsInGroup, commandTimeout, cancellationToken: cancellationToken);
                if (result_Group is not null)
                {
                    result.UnionWith(result_Group.Ids);
                    rejections.AddRange(result_Group.Rejections);
                }
            }

            return new PostgreSQLUpdateResult(result, rejections);
        }

        /// <summary>
        /// Asynchronously upserts the specified referenced objects through an already-open connection, optionally inside a caller-owned transaction.
        /// <para>This is the shared core of the upsert: the <c>INSERT … ON CONFLICT (county_id, unique_id) DO UPDATE</c> statement lives here, and the instance <see cref="UpdateAsync(System.Collections.Generic.IEnumerable{TBuilding2DReferencedObject}, int, System.Threading.CancellationToken)"/> delegates its batch loop to it. The caller ensures the table and the objects' partitions exist; this method performs no DDL, which is what allows it to run inside a transaction that must roll back as one unit.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> to execute the commands on.</param>
        /// <param name="npgsqlTransaction">The transaction the commands join, or null for autocommit.</param>
        /// <param name="building2DReferencedObjects">The referenced objects to be upserted, or <c>null</c>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers written and the rows dropped before the database, or null when the connection or the collection is null.</returns>
        public async Task<PostgreSQLUpdateResult?> UpdateAsync(NpgsqlConnection? npgsqlConnection, NpgsqlTransaction? npgsqlTransaction, IEnumerable<TBuilding2DReferencedObject>? building2DReferencedObjects, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || building2DReferencedObjects is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            List<TBuilding2DReferencedObject> building2DReferencedObjects_List = [.. building2DReferencedObjects.Where(x => x != null)];
            if (building2DReferencedObjects_List.Count == 0)
            {
                return new PostgreSQLUpdateResult([], []);
            }

            HashSet<long> result = [];

            const int batchSize = 1000;
            for (int i = 0; i < building2DReferencedObjects_List.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<TBuilding2DReferencedObject> chunk = building2DReferencedObjects_List.GetRange(i, Math.Min(batchSize, building2DReferencedObjects_List.Count - i));

                await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection, npgsqlTransaction);
                npgsqlBatch.Timeout = commandTimeout;

                foreach (TBuilding2DReferencedObject countyReferencedObject in chunk)
                {
                    NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                        INSERT INTO {TableName} (county_id, unique_id, reference, object)
                        VALUES (@county_id, @unique_id, @reference, @object)
                        ON CONFLICT (county_id, unique_id)
                        DO UPDATE SET
                            object = EXCLUDED.object,
                            reference = EXCLUDED.reference
                        RETURNING id;");

                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyReferencedObject.CountyId });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("unique_id", NpgsqlDbType.Text) { Value = countyReferencedObject.UniqueId });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = countyReferencedObject.Reference ?? (object)DBNull.Value });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb) { Value = (object?)countyReferencedObject.Object?.ToJsonString() ?? DBNull.Value });

                    npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync(cancellationToken);
                int chunkIndex = 0;
                do
                {
                    while (await npgsqlDataReader.ReadAsync(cancellationToken))
                    {
                        long id = npgsqlDataReader.GetInt64(0);
                        result.Add(id);
                        if (chunkIndex < chunk.Count)
                        {
                            chunk[chunkIndex].Id = id;
                        }
                        chunkIndex++;
                    }
                }
                while (await npgsqlDataReader.NextResultAsync(cancellationToken));
            }

            return new PostgreSQLUpdateResult(result, []);
        }

        /// <summary>
        /// Asynchronously replaces one JSONB property of the stored objects of a county partition, addressed by identifier.
        /// <para>The property is one key of the <c>object</c> JSONB, set with <c>jsonb_set</c> with the missing-key flag, so exactly that key changes and the rest of the stored object - including members older code wrote that the current class no longer declares - is untouched. The property name travels as a value parameter, not an identifier, so no whitelist applies. The statement never inserts and never touches <c>county_id</c>, so a row a concurrent run removed between the read and the write answers with no identifier rather than coming back as a fresh row, and the identifiers the statement actually updated are returned so the caller sees which of the ones it asked for are gone. Pairs whose value is null are skipped, so a SQL NULL can never reach <c>jsonb_set</c> and delete the key it names. The caller ensures the table exists - the statement runs no DDL - and joins the caller's transaction when one is given.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open PostgreSQL connection the statement executes on.</param>
        /// <param name="npgsqlTransaction">The transaction the statement joins, or null for autocommit.</param>
        /// <param name="countyId">The identifier of the county partition to update.</param>
        /// <param name="propertyName">The name of the JSONB property to replace, as stored in the object JSON.</param>
        /// <param name="values">The identifier to new property value pairs to update, or <c>null</c>.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the statement. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers the statement updated, or null when the connection or the collection is null.</returns>
        public async Task<HashSet<long>?> UpdateObjectPropertiesAsync(NpgsqlConnection? npgsqlConnection, NpgsqlTransaction? npgsqlTransaction, int countyId, string propertyName, IEnumerable<KeyValuePair<long, JsonNode?>>? values, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || values is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            List<KeyValuePair<long, JsonNode>> values_List = [];
            foreach (KeyValuePair<long, JsonNode?> pair in values)
            {
                if (pair.Value is not null)
                {
                    values_List.Add(new KeyValuePair<long, JsonNode>(pair.Key, pair.Value));
                }
            }

            if (values_List.Count == 0)
            {
                return [];
            }

            HashSet<long> result = [];

            const int batchSize = 1000;
            for (int i = 0; i < values_List.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<KeyValuePair<long, JsonNode>> chunk = values_List.GetRange(i, Math.Min(batchSize, values_List.Count - i));

                await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection, npgsqlTransaction);
                npgsqlBatch.Timeout = commandTimeout;

                foreach (KeyValuePair<long, JsonNode> pair in chunk)
                {
                    NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                        UPDATE {TableName}
                        SET object = jsonb_set(object, ARRAY[@property_name], @value::jsonb, true)
                        WHERE county_id = @county_id
                          AND id = @id
                        RETURNING id;");

                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("property_name", NpgsqlDbType.Text) { Value = propertyName });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Jsonb) { Value = pair.Value.ToJsonString() });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = pair.Key });

                    npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync(cancellationToken);
                do
                {
                    while (await npgsqlDataReader.ReadAsync(cancellationToken))
                    {
                        result.Add(npgsqlDataReader.GetInt64(0));
                    }
                }
                while (await npgsqlDataReader.NextResultAsync(cancellationToken));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously replaces one JSONB property of the stored objects of a county partition, addressed by identifier, on a connection opened for the call.
        /// <para>Each batch commits on its own: the connection the call opens carries no transaction, so a failure between two batches leaves the earlier batches written. For a page that must stand or fall together, use the overload that joins the caller's transaction.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county partition to update.</param>
        /// <param name="propertyName">The name of the JSONB property to replace, as stored in the object JSON.</param>
        /// <param name="values">The identifier to new property value pairs to update, or <c>null</c>.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the statement. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers the statement updated, or null when the collection is null or the connection could not be created.</returns>
        public async Task<HashSet<long>?> UpdateObjectPropertiesAsync(int countyId, string propertyName, IEnumerable<KeyValuePair<long, JsonNode?>>? values, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await UpdateObjectPropertiesAsync(npgsqlConnection, null, countyId, propertyName, values, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the building references that hold more than one record, optionally filtered by county identifier, ordered by count descending.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the query.</param>
        /// <param name="countyId">The optional unique identifier of the county to filter by; if null, searches across all counties.</param>
        /// <param name="limit">The maximum number of duplicate references to return. Defaults to 100.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The cancellation token used to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the list of duplicate references, an empty list if none are found, or null if the connection is null.</returns>
        public async Task<List<Building2DReferenceDuplicate>?> GetBuilding2DReferenceDuplicatesAsync(NpgsqlConnection? npgsqlConnection, int? countyId = null, int limit = 100, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || limit <= 0 || commandTimeout < 0)
            {
                return null;
            }

            string commandText = countyId.HasValue ? $@"
                SELECT reference, COUNT(*) AS count, ARRAY_AGG(DISTINCT county_id ORDER BY county_id) AS county_ids
                FROM {TableName}
                WHERE county_id = @countyId
                  AND reference IS NOT NULL
                GROUP BY reference
                HAVING COUNT(*) > 1
                ORDER BY count DESC, reference ASC
                LIMIT @limit;" : $@"
                SELECT reference, COUNT(*) AS count, ARRAY_AGG(DISTINCT county_id ORDER BY county_id) AS county_ids
                FROM {TableName}
                WHERE reference IS NOT NULL
                GROUP BY reference
                HAVING COUNT(*) > 1
                ORDER BY count DESC, reference ASC
                LIMIT @limit;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit });

            List<Building2DReferenceDuplicate> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                string? reference = npgsqlDataReader.IsDBNull(0) ? null : npgsqlDataReader.GetString(0);
                long count = npgsqlDataReader.IsDBNull(1) ? 0 : npgsqlDataReader.GetInt64(1);
                int[]? countyIds = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetFieldValue<int[]>(2);

                result.Add(new Building2DReferenceDuplicate(reference, count, countyIds));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the building references that hold more than one record, optionally filtered by county identifier, ordered by count descending.
        /// </summary>
        /// <param name="countyId">The optional unique identifier of the county to filter by; if null, searches across all counties.</param>
        /// <param name="limit">The maximum number of duplicate references to return. Defaults to 100.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The cancellation token used to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the list of duplicate references, an empty list if none are found, or null if the connection could not be built.</returns>
        public async Task<List<Building2DReferenceDuplicate>?> GetBuilding2DReferenceDuplicatesAsync(int? countyId = null, int limit = 100, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (limit <= 0 || commandTimeout < 0)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBuilding2DReferenceDuplicatesAsync(npgsqlConnection, countyId, limit, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the count of building references that hold more than one record, optionally filtered by county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the query.</param>
        /// <param name="countyId">The optional unique identifier of the county to filter by; if null, counts across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The cancellation token used to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the total count of duplicate references, or -1 if the connection is null.</returns>
        public async Task<long> GetDuplicatesCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || commandTimeout < 0)
            {
                return -1;
            }

            string commandText = countyId.HasValue ? $@"
                SELECT COUNT(*)
                FROM (
                    SELECT 1
                    FROM {TableName}
                    WHERE county_id = @countyId
                      AND reference IS NOT NULL
                    GROUP BY reference
                    HAVING COUNT(*) > 1
                ) sub;" : $@"
                SELECT COUNT(*)
                FROM (
                    SELECT 1
                    FROM {TableName}
                    WHERE reference IS NOT NULL
                    GROUP BY reference
                    HAVING COUNT(*) > 1
                ) sub;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            object? scalarResult = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
            if (scalarResult is long count)
            {
                return count;
            }

            if (scalarResult is int countInt)
            {
                return countInt;
            }

            return 0;
        }

        /// <summary>
        /// Asynchronously retrieves the count of building references that hold more than one record, optionally filtered by county identifier.
        /// </summary>
        /// <param name="countyId">The optional unique identifier of the county to filter by; if null, counts across all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The cancellation token used to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the total count of duplicate references, or -1 if the connection could not be built.</returns>
        public async Task<long> GetDuplicatesCountAsync(int? countyId = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (commandTimeout < 0)
            {
                return -1;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetDuplicatesCountAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reports, for every polygon part of a multi-part county, the rows of this table held under that part whose reference <c>building_2d</c> does not hold under the same part.
        /// <para>A county code names one <c>administrative_areal_2d</c> row per polygon part, and a row here is filed under one of them. A row mismatches when <c>building_2d</c> holds its reference under a different part, or under none at all. This is the discovery half of the part misfile: it finds where a row sits that <c>building_2d</c> says it does not belong, the opposite direction of the <see cref="RefreshCountyIdsAsync(IEnumerable{string}, int, IEnumerable{int}, int, int, CancellationToken)"/> repair.</para>
        /// <para>The mismatches split per part into <see cref="Building2DReferencedObjectCountyPartMismatchResult.CountHeldElsewhere"/> - <c>building_2d</c> holds the reference under another part, so it has a destination and the repair has somewhere to put it - and <see cref="Building2DReferencedObjectCountyPartMismatchResult.CountOrphan"/> - <c>building_2d</c> holds the reference under no part, so there is no destination and the gap is a missing building row. <see cref="Building2DReferencedObjectCountyPartMismatchResult.Count"/> is the sum, the before and after number of a repair.</para>
        /// <para>Only parts holding at least one mismatched row are returned, so a clean measurement is an empty list; single-part codes are left out entirely because with one part there is nothing to be filed under by mistake. No geometry is involved - a referenced object carries no box - so a non-zero count is a certain misfile, not a suspect like the <c>building_2d</c> bounding-box lower bound.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the queries.</param>
        /// <param name="code">An optional county code to restrict the measurement to. When null every multi-part code is measured.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to each command executed. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one entry per part holding a mismatched row, ordered by code then part identifier, an empty list when no measured part holds a mismatched row, or null when the connection is null or the timeout is negative.</returns>
        public async Task<List<Building2DReferencedObjectCountyPartMismatchResult>?> GetCountyPartMismatchesAsync(NpgsqlConnection? npgsqlConnection, string? code = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || commandTimeout < 0)
            {
                return null;
            }

            // The parts are resolved up front from administrative_areal_2d and their codes carried into the result, rather
            // than joining the two tables in one statement: a join against the table under test hands the planner no
            // partition promise, and the handful of parts a multi-part code owns are all the measurement needs. Single-part
            // codes are excluded - with one part there is nothing to be filed under by mistake - the same rule building_2d
            // applies to its own box measurement.
            string commandText_Parts = $@"
                SELECT id, code
                FROM {Constants.TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                  AND code IS NOT NULL
                  AND code IN (
                      SELECT code
                      FROM {Constants.TableName.AdministrativeAreal2D}
                      WHERE type_id = @typeId AND code IS NOT NULL
                      GROUP BY code
                      HAVING COUNT(*) > 1)
                  AND (@code IS NULL OR code = @code)
                ORDER BY code ASC, id ASC;";

            List<int> countyIds = [];
            List<string> codes = [];

            await using (NpgsqlCommand npgsqlCommand_Parts = new(commandText_Parts, npgsqlConnection))
            {
                npgsqlCommand_Parts.CommandTimeout = commandTimeout;
                npgsqlCommand_Parts.Parameters.AddWithValue("typeId", (short)AdministrativeArealType.County);
                npgsqlCommand_Parts.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Text) { Value = string.IsNullOrWhiteSpace(code) ? DBNull.Value : code });

                await using NpgsqlDataReader npgsqlDataReader_Parts = await npgsqlCommand_Parts.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader_Parts.ReadAsync(cancellationToken))
                {
                    countyIds.Add(npgsqlDataReader_Parts.GetInt32(0));
                    codes.Add(npgsqlDataReader_Parts.GetString(1));
                }
            }

            if (countyIds.Count == 0)
            {
                return [];
            }

            Dictionary<string, List<int>> countyIds_ByCode = [];
            Dictionary<int, string> code_ByCountyId = [];
            for (int i = 0; i < countyIds.Count; i++)
            {
                code_ByCountyId[countyIds[i]] = codes[i];

                if (!countyIds_ByCode.TryGetValue(codes[i], out List<int>? countyIds_Code) || countyIds_Code is null)
                {
                    countyIds_Code = [];
                    countyIds_ByCode[codes[i]] = countyIds_Code;
                }

                countyIds_Code.Add(countyIds[i]);
            }

            // The candidate references under those parts, pruned to the parts in play. A row with no stated reference has
            // nothing for building_2d to hold, so it cannot be a misfile and is left out of the measurement.
            string commandText_References = $@"
                SELECT DISTINCT county_id, reference
                FROM {TableName}
                WHERE county_id = ANY(@countyIds)
                  AND reference IS NOT NULL
                ORDER BY county_id ASC, reference ASC;";

            Dictionary<int, HashSet<string>> references_ByCountyId = [];

            await using (NpgsqlCommand npgsqlCommand_References = new(commandText_References, npgsqlConnection))
            {
                npgsqlCommand_References.CommandTimeout = commandTimeout;
                npgsqlCommand_References.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds.ToArray() });

                await using NpgsqlDataReader npgsqlDataReader_References = await npgsqlCommand_References.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader_References.ReadAsync(cancellationToken))
                {
                    int countyId_Read = npgsqlDataReader_References.GetInt32(0);
                    string reference_Read = npgsqlDataReader_References.GetString(1);

                    if (!references_ByCountyId.TryGetValue(countyId_Read, out HashSet<string>? references_CountyId) || references_CountyId is null)
                    {
                        references_CountyId = [];
                        references_ByCountyId[countyId_Read] = references_CountyId;
                    }

                    references_CountyId.Add(reference_Read);
                }
            }

            if (references_ByCountyId.Count == 0)
            {
                return [];
            }

            // One (part, reference) pair per candidate row, tagged with the part it is held under. The resolver answers the
            // own-part question pruned by that tag, and only for the pairs it does not hold under their own part does it take
            // the unpruned leg to find where building_2d holds them - so that leg reads the mismatched subset alone and costs
            // nothing on a clean estate.
            List<Building2DReference> building2DReferences_Input = [];
            foreach (KeyValuePair<int, HashSet<string>> references_County in references_ByCountyId)
            {
                foreach (string reference in references_County.Value)
                {
                    building2DReferences_Input.Add(new Building2DReference { CountyId = references_County.Key, Reference = reference });
                }
            }

            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(ConnectionData);
            List<Building2DReference>? building2DReferences_Resolved = await building2DPostgreSQLConverter.GetBuilding2DReferencesAsync(building2DReferences_Input, true, commandTimeout, cancellationToken);
            if (building2DReferences_Resolved is null)
            {
                return null;
            }

            // A (part, reference) pair is in the first set exactly when building_2d holds the reference under that part, and a
            // reference is in the second exactly when building_2d holds it under any part. Together they separate the three
            // outcomes of a candidate row: held under its own part (not a misfile), held elsewhere (a misfile with a
            // destination), or held nowhere (an orphan).
            HashSet<string> references_HeldUnderOwn = [.. building2DReferences_Resolved.Where(x => x.CountyId.HasValue && !string.IsNullOrWhiteSpace(x.Reference)).Select(x => $"{x.CountyId!.Value}_{x.Reference}")];
            HashSet<string> references_HeldAnywhere = [.. building2DReferences_Resolved.Where(x => !string.IsNullOrWhiteSpace(x.Reference)).Select(x => x.Reference!)];

            Dictionary<int, List<string>> references_MismatchedByPart = [];
            HashSet<string> references_MismatchedHeldElsewhere = [];
            foreach (int countyId_Part in countyIds)
            {
                if (!references_ByCountyId.TryGetValue(countyId_Part, out HashSet<string>? references_Part) || references_Part is null)
                {
                    continue;
                }

                List<string>? references_Mismatched = null;
                foreach (string reference in references_Part)
                {
                    if (references_HeldUnderOwn.Contains($"{countyId_Part}_{reference}"))
                    {
                        continue;
                    }

                    references_Mismatched ??= [];
                    references_Mismatched.Add(reference);

                    if (references_HeldAnywhere.Contains(reference))
                    {
                        references_MismatchedHeldElsewhere.Add(reference);
                    }
                }

                if (references_Mismatched is not null)
                {
                    references_MismatchedByPart[countyId_Part] = references_Mismatched;
                }
            }

            if (references_MismatchedByPart.Count == 0)
            {
                return [];
            }

            // The classification above is per distinct reference, but the figure is a row count: several rows of one part can
            // share a reference, and each of them needs the repair. Counting the pairs would report one misfile where the
            // table holds ten, so the before and after of a repair would not match the rows actually moved.
            //
            // The pairs are matched as pairs, not over the two sets separately: a reference can be mismatched under one part
            // while correctly held by another, so a cross-product filter would count rows that are not misfiles.
            List<int> pairCountyIds = [];
            List<string> pairReferences = [];
            foreach (KeyValuePair<int, List<string>> references_Mismatched in references_MismatchedByPart)
            {
                foreach (string reference in references_Mismatched.Value)
                {
                    pairCountyIds.Add(references_Mismatched.Key);
                    pairReferences.Add(reference);
                }
            }

            string commandText_Counts = $@"
                SELECT county_id, reference, COUNT(*) AS count
                FROM {TableName}
                WHERE EXISTS (
                    SELECT 1
                    FROM unnest(@pairCountyIds, @pairReferences) AS pair(county_id, reference)
                    WHERE pair.county_id = {TableName}.county_id
                      AND pair.reference = {TableName}.reference)
                GROUP BY county_id, reference;";

            Dictionary<int, long> rows_ByPart = [];
            Dictionary<int, long> rowsHeldElsewhere_ByPart = [];

            await using (NpgsqlCommand npgsqlCommand_Counts = new(commandText_Counts, npgsqlConnection))
            {
                npgsqlCommand_Counts.CommandTimeout = commandTimeout;
                npgsqlCommand_Counts.Parameters.Add(new NpgsqlParameter("pairCountyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = pairCountyIds.ToArray() });
                npgsqlCommand_Counts.Parameters.Add(new NpgsqlParameter("pairReferences", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = pairReferences.ToArray() });

                await using NpgsqlDataReader npgsqlDataReader_Counts = await npgsqlCommand_Counts.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader_Counts.ReadAsync(cancellationToken))
                {
                    int countyId_Count = npgsqlDataReader_Counts.GetInt32(0);
                    string reference_Count = npgsqlDataReader_Counts.GetString(1);
                    long count_Rows = npgsqlDataReader_Counts.GetInt64(2);

                    rows_ByPart[countyId_Count] = rows_ByPart.GetValueOrDefault(countyId_Count) + count_Rows;

                    if (references_MismatchedHeldElsewhere.Contains(reference_Count))
                    {
                        rowsHeldElsewhere_ByPart[countyId_Count] = rowsHeldElsewhere_ByPart.GetValueOrDefault(countyId_Count) + count_Rows;
                    }
                }
            }

            List<Building2DReferencedObjectCountyPartMismatchResult> result = [];
            foreach (int countyId_Part in countyIds)
            {
                if (!references_MismatchedByPart.ContainsKey(countyId_Part))
                {
                    continue;
                }

                long count = rows_ByPart.GetValueOrDefault(countyId_Part);
                if (count == 0)
                {
                    continue;
                }

                long countHeldElsewhere = rowsHeldElsewhere_ByPart.GetValueOrDefault(countyId_Part);
                long countOrphan = count - countHeldElsewhere;

                result.Add(new Building2DReferencedObjectCountyPartMismatchResult(code_ByCountyId[countyId_Part], countyId_Part, countyIds_ByCode[code_ByCountyId[countyId_Part]], count, countHeldElsewhere, countOrphan));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reports, for every polygon part of a multi-part county, the rows of this table held under that part whose reference <c>building_2d</c> does not hold under the same part.
        /// <para>See the <see cref="GetCountyPartMismatchesAsync(NpgsqlConnection, string, int, CancellationToken)"/> overload for what a mismatch is, how it splits into held elsewhere and orphan, and why only parts holding a mismatched row are returned.</para>
        /// </summary>
        /// <param name="code">An optional county code to restrict the measurement to. When null every multi-part code is measured.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to each command executed. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one entry per part holding a mismatched row, an empty list when no measured part holds a mismatched row, or null when the timeout is negative or the connection could not be created.</returns>
        public async Task<List<Building2DReferencedObjectCountyPartMismatchResult>?> GetCountyPartMismatchesAsync(string? code = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (commandTimeout < 0)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetCountyPartMismatchesAsync(npgsqlConnection, code, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Creates a new instance of a building referenced object using the specified identification and metadata.
        /// </summary>
        /// <param name="id">The unique <see cref="System.Int64"/> identifier for the record.</param>
        /// <param name="countyId">The optional <see cref="System.Int32"/> identifier for the county associated with the building.</param>
        /// <param name="uniqueId">The optional <see cref="System.String"/> representing a unique identification code.</param>
        /// <param name="reference">The optional <see cref="System.String"/> used as a reference for the record.</param>
        /// <param name="object">The optional <see cref="JsonObject"/> containing raw data for initialization.</param>
        /// <param name="createdAt">The optional <see cref="DateTime"/> indicating when the record was created.</param>
        /// <returns>A new instance of <seeref name="TBuilding2DReferencedObject"/>.</returns>
        protected abstract TBuilding2DReferencedObject Create(long id, int? countyId, string? uniqueId, string? reference, JsonObject? @object, DateTime? createdAt);

        /// <summary>
        /// Creates an instance of <seeref name="TBuilding2DReferencedObject"/> using the data provided by the <see cref="NpgsqlDataReader"/>.
        /// </summary>
        /// <param name="npgsqlDataReader">The <see cref="NpgsqlDataReader"/> containing the database record to be used for object creation.</param>
        /// <returns>A new instance of <seeref name="TBuilding2DReferencedObject"/> populated with data from the reader.</returns>
        protected override TBuilding2DReferencedObject Create(NpgsqlDataReader npgsqlDataReader)
        {
            return Create(npgsqlDataReader.GetInt64(0), npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetInt32(1), npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2), npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetString(3), JsonNode.Parse(npgsqlDataReader.GetString(4)) as JsonObject, npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetDateTime(5));
        }
    }
}