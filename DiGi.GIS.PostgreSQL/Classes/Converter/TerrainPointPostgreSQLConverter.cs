using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.PointCloud.Spatial.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides functionality for converting, querying, and managing <see cref="TerrainPoint"/> entities and <see cref="PointCloud3D"/> point cloud data within a PostgreSQL database.
    /// </summary>
    public class TerrainPointPostgreSQLConverter : PostgreSQLConverter<TerrainPoint>, IGISPostgreSQLConverter<TerrainPoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainPointPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public TerrainPointPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Asynchronously clears all records from the terrain point table or a specific county partition.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The optional integer identifier of the county partition to clear. If null, the entire table is cleared.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the operation succeeded; otherwise, false.</returns>
        public static async Task<bool> ClearAsync(NpgsqlConnection? npgsqlConnection, int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            return await DiGi.PostgreSQL.Modify.ClearAsync(npgsqlConnection, TableName(countyId), commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously clears all records from the terrain point table or a specific county partition, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county partition to clear. If null, the entire table is cleared.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the operation succeeded; otherwise, false.</returns>
        public async Task<bool> ClearAsync(int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await ClearAsync(npgsqlConnection, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the exact count of records from the database, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The optional integer identifier of the county partition used to filter the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a <see cref="long"/>, or -1 when the table or partition does not exist.</returns>
        public static async Task<long> GetCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, TableName(countyId), commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the exact count of records from the database, optionally filtered by a specific county identifier, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county partition used to filter the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a <see cref="long"/>, or -1 when the table or partition does not exist.</returns>
        public async Task<long> GetCountAsync(int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
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
        /// Asynchronously retrieves an estimated count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The optional integer identifier for the county; if null, the estimate is read from the partitioned parent.</param>
        /// <param name="analyze">A value indicating whether to perform an ANALYZE operation on the database table to update statistics before retrieving the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated count as a nullable <see cref="long"/>, -1 when the partition exists but has not been analysed, or null when the table or partition does not exist or connection is null.</returns>
        public static async Task<long?> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId = null, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            return await DiGi.PostgreSQL.Query.EstimatedCountAsync(npgsqlConnection, TableName(countyId), analyze, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated count of records, optionally filtered by a specific county identifier, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The optional integer identifier for the county; if null, the estimate is read from the partitioned parent.</param>
        /// <param name="analyze">A value indicating whether to perform an ANALYZE operation on the database table to update statistics before retrieving the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated count as a nullable <see cref="long"/>, -1 when the partition exists but has not been analysed, or null when the table or partition does not exist or connection could not be built.</returns>
        public async Task<long?> GetEstimatedCountAsync(int? countyId = null, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
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
        /// Asynchronously retrieves the estimated row count for the specified county identifiers in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> to use for the query.</param>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for. When null, the estimate is read from the partitioned parent instead.</param>
        /// <param name="analyze">A boolean indicating whether to run an analysis operation before fetching the estimated count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total estimated row count as a <see cref="long"/>. The result is -1 when an error occurs or any named county has no partition or has never been analysed.</returns>
        public static async Task<long> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            if (countyIds is null)
            {
                long? count_Parent = await GetEstimatedCountAsync(npgsqlConnection, (int?)null, analyze, commandTimeout, cancellationToken);
                return count_Parent ?? -1;
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
        /// Asynchronously retrieves the estimated row count for each of the specified county partitions, reading them all in a single catalog query rather than one query per county.
        /// <para>A county is absent from the result when it has no partition, and carries <c>-1</c> when its partition exists but has never been analysed - the same two cases the singular overload reports as <c>null</c> and <c>-1</c>.</para>
        /// <para>Unlike the summing overload this one has no partitioned-parent fallback: a null <paramref name="countyIds"/> yields null rather than the parent estimate, because there is no county identifier to key the parent figure under.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> to use for the query.</param>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for.</param>
        /// <param name="analyze">A boolean indicating whether to run VACUUM ANALYZE on each existing partition before reading the estimates.</param>
        /// <param name="batchSize">The maximum number of partition names sent in a single catalog query.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
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
                countyIds_ByTableName[TableName(countyId)] = countyId;
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
        /// Asynchronously retrieves the estimated row count for the specified county identifiers in the PostgreSQL database, automatically managing the connection.
        /// </summary>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for. When null, the estimate is read from the partitioned parent instead.</param>
        /// <param name="analyze">A boolean indicating whether to run an analysis operation before fetching the estimated count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total estimated row count as a <see cref="long"/>. The result is -1 when an error occurs or any named county has no partition or has never been analysed.</returns>
        public async Task<long> GetEstimatedCountAsync(IEnumerable<int>? countyIds, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
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
        /// Asynchronously retrieves the estimated row count for each of the specified county partitions on a single connection, reading them all in one catalog query rather than one query per county.
        /// <para>A county is absent from the result when it has no partition, and carries <c>-1</c> when its partition exists but has never been analysed.</para>
        /// </summary>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for.</param>
        /// <param name="analyze">A boolean indicating whether to run VACUUM ANALYZE on each existing partition before reading the estimates.</param>
        /// <param name="batchSize">The maximum number of partition names sent in a single catalog query.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
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
        /// Asynchronously retrieves the number of points stored for each of the specified county partitions.
        /// <para>Counties holding no point are absent from the result rather than present with a zero, because a county with no partition and a county with an empty one are the same thing to this query.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The identifiers of the county partitions to count. When null every county is counted.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result maps each county identifier to the number of points stored for it, or null when the connection is null or the terrain point table does not exist.</returns>
        public static async Task<Dictionary<int, long>?> GetCountsByCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Answered before the aggregate is sent. A store nothing has ever been written to has no table at
            // all, and running the query against it raises an undefined relation - which reaches a caller as a
            // server fault rather than as the plain fact that there is nothing stored yet. GetCountAsync has
            // always answered that case with -1 for the same reason.
            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, Constants.TableName.TerrainPoint))
            {
                return null;
            }

            int[]? countyIds_Array = countyIds is null ? null : [.. countyIds];

            string commandText = $@"
                SELECT county_id, COUNT(*)
                FROM {Constants.TableName.TerrainPoint}
                {WhereCountyIds(countyIds_Array)}
                GROUP BY county_id
                ORDER BY county_id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddCountyIdsParameter(npgsqlCommand, countyIds_Array);

            Dictionary<int, long> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result[npgsqlDataReader.GetInt32(0)] = npgsqlDataReader.GetInt64(1);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the number of points stored for each of the specified county partitions, automatically managing the connection.
        /// </summary>
        /// <param name="countyIds">The identifiers of the county partitions to count. When null every county is counted.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result maps each county identifier to the number of points stored for it, or null when no connection could be opened.</returns>
        public async Task<Dictionary<int, long>?> GetCountsByCountyIdsAsync(IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetCountsByCountyIdsAsync(npgsqlConnection, countyIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously summarises what each of the specified county partitions holds: how many points, over what extent, at what elevations, filed under how many subdivisions, and when they were written.
        /// <para>This is the lasting account of a sampling run. The run keeps its own tallies in memory and discards them when it ends, so a run that reported failure leaves nothing else to read.</para>
        /// <para>Counties holding no point are absent from the result rather than present with a zero.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The identifiers of the county partitions to summarise. When null every county is summarised.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries one <see cref="TerrainPointCountyResult"/> per county holding points, ordered by county identifier, or null when the connection is null or the terrain point table does not exist.</returns>
        public static async Task<List<TerrainPointCountyResult>?> GetSummariesByCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Answered before the aggregate is sent. A store nothing has ever been written to has no table at
            // all, and running the query against it raises an undefined relation - which reaches a caller as a
            // server fault rather than as the plain fact that there is nothing stored yet. GetCountAsync has
            // always answered that case with -1 for the same reason.
            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, Constants.TableName.TerrainPoint))
            {
                return null;
            }

            int[]? countyIds_Array = countyIds is null ? null : [.. countyIds];

            // COUNT(DISTINCT subdivision_id) is the expensive column of the six aggregates, and the one that
            // catches a county whose points were all filed under a single subdivision. It is what the generous
            // command timeout is for.
            string commandText = $@"
                SELECT
                    county_id,
                    COUNT(*),
                    MIN(x), MAX(x), MIN(y), MAX(y), MIN(z), MAX(z),
                    COUNT(*) FILTER (WHERE z = 0),
                    COUNT(DISTINCT subdivision_id),
                    COUNT(*) FILTER (WHERE subdivision_id IS NULL),
                    MIN(created_at), MAX(created_at)
                FROM {Constants.TableName.TerrainPoint}
                {WhereCountyIds(countyIds_Array)}
                GROUP BY county_id
                ORDER BY county_id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddCountyIdsParameter(npgsqlCommand, countyIds_Array);

            List<TerrainPointCountyResult> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(new TerrainPointCountyResult(
                    npgsqlDataReader.GetInt32(0),
                    npgsqlDataReader.GetInt64(1),
                    npgsqlDataReader.GetDouble(2),
                    npgsqlDataReader.GetDouble(3),
                    npgsqlDataReader.GetDouble(4),
                    npgsqlDataReader.GetDouble(5),
                    npgsqlDataReader.GetDouble(6),
                    npgsqlDataReader.GetDouble(7),
                    npgsqlDataReader.GetInt64(8),
                    npgsqlDataReader.GetInt64(9),
                    npgsqlDataReader.GetInt64(10),
                    npgsqlDataReader.IsDBNull(11) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(11),
                    npgsqlDataReader.IsDBNull(12) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(12)));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously summarises what each of the specified county partitions holds, automatically managing the connection.
        /// </summary>
        /// <param name="countyIds">The identifiers of the county partitions to summarise. When null every county is summarised.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries one <see cref="TerrainPointCountyResult"/> per county holding points, ordered by county identifier, or null when no connection could be opened.</returns>
        public async Task<List<TerrainPointCountyResult>?> GetSummariesByCountyIdsAsync(IEnumerable<int>? countyIds, int commandTimeout = 600, CancellationToken cancellationToken = default)
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
        /// Asynchronously writes a <see cref="PointCloud3D"/> to the database for a specific county, creating the table and the county partition first.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyId">The integer identifier of the county partition.</param>
        /// <param name="pointCloud3D">The <see cref="PointCloud3D"/> containing the point coordinates to write.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision.</param>
        /// <param name="binaryInsert">Opts into a PostgreSQL binary COPY, which is much faster for large clouds but cannot skip points the table already holds - a repeated import, or two source tiles overlapping, fails on the primary key. Left at its default the points are sent as a single array-valued INSERT that leaves existing points as they are, so the write can be repeated safely. Set it true only for data known to carry no point already stored.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries the number of points stored and any rejections, or null when the connection is null or the table could not be created.</returns>
        public static async Task<TerrainPointUpdateResult?> UpdateAsync(NpgsqlConnection? npgsqlConnection, int countyId, PointCloud3D? pointCloud3D, int? subdivisionId = null, bool binaryInsert = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            if (!await Create.TableAsync_TerrainPoint(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            if (pointCloud3D is null || pointCloud3D.Count == 0)
            {
                return new TerrainPointUpdateResult(0, null);
            }

            if (!await Create.TableAsync_TerrainPoint_Partition(npgsqlConnection, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken))
            {
                return new TerrainPointUpdateResult(0, [new Rejection(countyId.ToString(), UpdateRejectionReason.PartitionUnavailable)]);
            }

            long count;
            if (binaryInsert)
            {
                DateTime createdAt = DateTime.UtcNow;

                await using NpgsqlBinaryImporter npgsqlBinaryImporter = await npgsqlConnection.BeginBinaryImportAsync(CopyCommandText(countyId), cancellationToken);

                int count_Points = pointCloud3D.Count;
                for (int i = 0; i < count_Points; i++)
                {
                    if (!pointCloud3D.TryGetPoint(i, out double x, out double y, out double z))
                    {
                        continue;
                    }

                    await WriteRowAsync(npgsqlBinaryImporter, countyId, subdivisionId, x, y, z, createdAt, cancellationToken);
                }

                count = (long)await npgsqlBinaryImporter.CompleteAsync(cancellationToken);
            }
            else
            {
                double[]? x = pointCloud3D.GetX();
                double[]? y = pointCloud3D.GetY();
                double[]? z = pointCloud3D.GetZ();

                if (x is null || y is null || z is null)
                {
                    return new TerrainPointUpdateResult(0, null);
                }

                int[] countyIds = new int[x.Length];
                int?[] subdivisionIds = new int?[x.Length];
                DateTime[] createdAts = new DateTime[x.Length];

                DateTime createdAt = DateTime.UtcNow;
                for (int i = 0; i < x.Length; i++)
                {
                    countyIds[i] = countyId;
                    subdivisionIds[i] = subdivisionId;
                    createdAts[i] = createdAt;
                }

                count = await InsertArraysAsync(npgsqlConnection, countyIds, subdivisionIds, x, y, z, createdAts, commandTimeout, cancellationToken);
            }

            return new TerrainPointUpdateResult(count, null);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="PointCloud3D"/> to the database for a specific county, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county partition.</param>
        /// <param name="pointCloud3D">The <see cref="PointCloud3D"/> containing the point coordinates to write.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision.</param>
        /// <param name="binaryInsert">Opts into a PostgreSQL binary COPY, which is much faster for large clouds but cannot skip points the table already holds - a repeated import, or two source tiles overlapping, fails on the primary key. Left at its default the points are sent as a single array-valued INSERT that leaves existing points as they are, so the write can be repeated safely. Set it true only for data known to carry no point already stored.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries the number of points stored and any rejections, or null when no connection could be opened.</returns>
        public async Task<TerrainPointUpdateResult?> UpdateAsync(int countyId, PointCloud3D? pointCloud3D, int? subdivisionId = null, bool binaryInsert = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, countyId, pointCloud3D, subdivisionId, binaryInsert, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a collection of <see cref="TerrainPoint"/> entities to the database, creating the table and every county partition first.
        /// <para>Points carrying no county or no geometry are recorded in <see cref="TerrainPointUpdateResult.Rejections"/> one by one; a county whose partition cannot be created contributes a single rejection naming the county, because a terrain batch runs to millions of points and one rejection each would cost more than the batch itself.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="terrainPoints">The collection of <see cref="TerrainPoint"/> objects to write.</param>
        /// <param name="binaryInsert">Opts into a PostgreSQL binary COPY, one per county, which is much faster for large collections but cannot skip points the table already holds - a repeated import, or two source tiles overlapping, fails on the primary key. Left at its default the points are sent as a single array-valued INSERT that leaves existing points as they are, so the write can be repeated safely. Set it true only for data known to carry no point already stored.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries the number of points stored and any rejections, or null when the connection is null or the table could not be created.</returns>
        public static async Task<TerrainPointUpdateResult?> UpdateAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<TerrainPoint>? terrainPoints, bool binaryInsert = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            if (!await Create.TableAsync_TerrainPoint(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            List<Rejection> rejections = [];

            if (terrainPoints is null)
            {
                return new TerrainPointUpdateResult(0, rejections);
            }

            // Materialised once. The source is walked twice below - to sort the points into counties and
            // then to write them - and an IEnumerable that recomputes itself would do the work twice too.
            List<TerrainPoint> terrainPoints_Accepted = [];
            HashSet<int> countyIds_Required = [];

            foreach (TerrainPoint terrainPoint in terrainPoints)
            {
                if (terrainPoint is null)
                {
                    rejections.Add(new Rejection(null, UpdateRejectionReason.Undefined));
                    continue;
                }

                if (!terrainPoint.CountyId.HasValue)
                {
                    rejections.Add(new Rejection(null, UpdateRejectionReason.CountyUnresolved));
                    continue;
                }

                if (terrainPoint.Point3D is null)
                {
                    rejections.Add(new Rejection(terrainPoint.CountyId.Value.ToString(), UpdateRejectionReason.MissingGeometry));
                    continue;
                }

                terrainPoints_Accepted.Add(terrainPoint);
                countyIds_Required.Add(terrainPoint.CountyId.Value);
            }

            // The partitions are created before the transaction opens. Partition creation takes an exclusive
            // lock on the parent, and holding that for the length of a multi-million-point import would
            // block every reader of every county until the import finished.
            HashSet<int> countyIds_Unavailable = [];
            foreach (int countyId in countyIds_Required)
            {
                if (!await Create.TableAsync_TerrainPoint_Partition(npgsqlConnection, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken))
                {
                    countyIds_Unavailable.Add(countyId);
                    rejections.Add(new Rejection(countyId.ToString(), UpdateRejectionReason.PartitionUnavailable));
                }
            }

            if (countyIds_Unavailable.Count != 0)
            {
                terrainPoints_Accepted = [.. terrainPoints_Accepted.Where(x => !countyIds_Unavailable.Contains(x.CountyId!.Value))];
            }

            if (terrainPoints_Accepted.Count == 0)
            {
                return new TerrainPointUpdateResult(0, rejections);
            }

            long count = 0;
            if (binaryInsert)
            {
                DateTime createdAt_Default = DateTime.UtcNow;

                // One COPY per county: a binary import names a single table, and the points are streamed
                // into that county's partition rather than through the partitioned parent.
                foreach (IGrouping<int, TerrainPoint> grouping in terrainPoints_Accepted.GroupBy(x => x.CountyId!.Value))
                {
                    await using NpgsqlBinaryImporter npgsqlBinaryImporter = await npgsqlConnection.BeginBinaryImportAsync(CopyCommandText(grouping.Key), cancellationToken);

                    foreach (TerrainPoint terrainPoint in grouping)
                    {
                        Point3D point3D = terrainPoint.Point3D!;

                        await WriteRowAsync(
                            npgsqlBinaryImporter,
                            grouping.Key,
                            terrainPoint.SubdivisionId,
                            point3D.X,
                            point3D.Y,
                            point3D.Z,
                            terrainPoint.CreatedAt ?? createdAt_Default,
                            cancellationToken);
                    }

                    count += (long)await npgsqlBinaryImporter.CompleteAsync(cancellationToken);
                }
            }
            else
            {
                int count_Points = terrainPoints_Accepted.Count;

                int[] countyIds = new int[count_Points];
                int?[] subdivisionIds = new int?[count_Points];
                double[] x = new double[count_Points];
                double[] y = new double[count_Points];
                double[] z = new double[count_Points];
                DateTime[] createdAts = new DateTime[count_Points];

                DateTime createdAt_Default = DateTime.UtcNow;
                for (int i = 0; i < count_Points; i++)
                {
                    TerrainPoint terrainPoint = terrainPoints_Accepted[i];
                    Point3D point3D = terrainPoint.Point3D!;

                    countyIds[i] = terrainPoint.CountyId!.Value;
                    subdivisionIds[i] = terrainPoint.SubdivisionId;
                    x[i] = point3D.X;
                    y[i] = point3D.Y;
                    z[i] = point3D.Z;
                    createdAts[i] = terrainPoint.CreatedAt ?? createdAt_Default;
                }

                count = await InsertArraysAsync(npgsqlConnection, countyIds, subdivisionIds, x, y, z, createdAts, commandTimeout, cancellationToken);
            }

            return new TerrainPointUpdateResult(count, rejections);
        }

        /// <summary>
        /// Asynchronously writes a collection of <see cref="TerrainPoint"/> entities to the database, automatically managing the connection.
        /// </summary>
        /// <param name="terrainPoints">The collection of <see cref="TerrainPoint"/> objects to write.</param>
        /// <param name="binaryInsert">Opts into a PostgreSQL binary COPY, one per county, which is much faster for large collections but cannot skip points the table already holds - a repeated import, or two source tiles overlapping, fails on the primary key. Left at its default the points are sent as a single array-valued INSERT that leaves existing points as they are, so the write can be repeated safely. Set it true only for data known to carry no point already stored.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries the number of points stored and any rejections, or null when no connection could be opened.</returns>
        public async Task<TerrainPointUpdateResult?> UpdateAsync(IEnumerable<TerrainPoint>? terrainPoints, bool binaryInsert = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, terrainPoints, binaryInsert, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes zero-elevation sentinel points from the terrain point table.
        /// <para>The elevation service returns <c>0</c> for coordinates outside terrain coverage or over water bodies. This method removes rows where <c>z = 0</c> across the specified counties or the whole table.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The identifiers of the county partitions to delete zero-elevation points from, or null to clean every county partition.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries the number of zero-elevation points deleted, or null when the connection is null or the terrain point table does not exist.</returns>
        public static async Task<long?> DeleteZeroElevationsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, Constants.TableName.TerrainPoint))
            {
                return null;
            }

            int[]? countyIds_Array = countyIds is null ? null : [.. countyIds];

            string whereClause = countyIds_Array is null
                ? "WHERE z = 0"
                : "WHERE z = 0 AND county_id = ANY(@countyIds)";

            string commandText = $@"
                DELETE FROM {Constants.TableName.TerrainPoint}
                {whereClause};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddCountyIdsParameter(npgsqlCommand, countyIds_Array);

            int rowsAffected = await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected;
        }

        /// <summary>
        /// Asynchronously deletes zero-elevation sentinel points from the terrain point table, automatically managing the connection.
        /// </summary>
        /// <param name="countyIds">The identifiers of the county partitions to delete zero-elevation points from, or null to clean every county partition.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries the number of zero-elevation points deleted, or null when no connection could be opened.</returns>
        public async Task<long?> DeleteZeroElevationsAsync(IEnumerable<int>? countyIds = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await DeleteZeroElevationsAsync(npgsqlConnection, countyIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> of the terrain points lying within a distance of the specified plan coordinate.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="point2D">The <see cref="Point2D"/> coordinate to search around. This value can be null.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The half-width, in model units, of the square searched around the coordinate. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>, which is a millimetre - well below the spacing of an elevation grid, so a caller wanting the points near a coordinate has to pass a distance of its own.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the matching <see cref="PointCloud3D"/>, or null if no points are found within the distance or if the provided point or county identifiers are null.</returns>
        public static async Task<PointCloud3D?> GetPointCloud3DByPoint2DAsync(NpgsqlConnection? npgsqlConnection, Point2D? point2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || point2D is null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = new(point2D, point2D);
            return await GetPointCloud3DByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyIds, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> of the terrain points lying within a distance of the specified plan coordinate, automatically managing the connection.
        /// </summary>
        /// <param name="point2D">The <see cref="Point2D"/> coordinate to search around. This value can be null.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The half-width, in model units, of the square searched around the coordinate. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>, which is a millimetre - well below the spacing of an elevation grid, so a caller wanting the points near a coordinate has to pass a distance of its own.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the matching <see cref="PointCloud3D"/>, or null if no points are found within the distance or if the provided point or county identifiers are null.</returns>
        public async Task<PointCloud3D?> GetPointCloud3DByPoint2DAsync(Point2D? point2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (point2D is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetPointCloud3DByPoint2DAsync(npgsqlConnection, point2D, countyIds, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> within the specified 2D bounding box for a specific county partition.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyId">The integer identifier of the county partition to query.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision to filter by.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the <see cref="PointCloud3D"/> within the bounding box, or null if no points match.</returns>
        public static async Task<PointCloud3D?> GetPointCloud3DByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, int countyId, int? subdivisionId = null, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT x, y, z
                FROM {Constants.TableName.TerrainPoint}
                WHERE county_id = @countyId
                  AND point(x, y) <@ box(point(@minX, @minY), point(@maxX, @maxY))
                  AND (@subdivisionId IS NULL OR subdivision_id = @subdivisionId);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddBoundingBox2DParameters(npgsqlCommand, boundingBox2D, tolerance);
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivisionId", NpgsqlDbType.Integer) { Value = (object?)subdivisionId ?? DBNull.Value });

            return await ReadAsync_PointCloud3D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> within the specified 2D bounding box for a specific county partition, automatically managing the connection.
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyId">The integer identifier of the county partition to query.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision to filter by.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the <see cref="PointCloud3D"/> within the bounding box, or null if no points match.</returns>
        public async Task<PointCloud3D?> GetPointCloud3DByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, int countyId, int? subdivisionId = null, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (boundingBox2D is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetPointCloud3DByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyId, subdivisionId, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> within the specified 2D bounding box across the given counties.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the combined <see cref="PointCloud3D"/> across the given county partitions, or null if no county was given or no points match.</returns>
        public static async Task<PointCloud3D?> GetPointCloud3DByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null || countyIds is null)
            {
                return null;
            }

            List<PointCloud3D?> pointCloud3Ds = [];
            foreach (int countyId in countyIds)
            {
                pointCloud3Ds.Add(await GetPointCloud3DByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyId, null, tolerance, commandTimeout, cancellationToken));
            }

            // Concatenation is plain geometry, and the factory does it in one block copy per county.
            // Gathering the axes here through GetX/GetY/GetZ copied each county three times over.
            return Geometry.PointCloud.Spatial.Create.PointCloud3D(pointCloud3Ds);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> within the specified 2D bounding box across the given counties, automatically managing the connection.
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the combined <see cref="PointCloud3D"/> across the given county partitions, or null if no county was given or no points match.</returns>
        public async Task<PointCloud3D?> GetPointCloud3DByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (boundingBox2D is null || countyIds is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetPointCloud3DByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyIds, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> of the terrain points lying inside the specified circle, for a specific county partition.
        /// <para>The circle is evaluated by the database, not by filtering a cloud that was fetched as a square: the points outside it are never read, never transferred and never allocated. PostgreSQL's point operator class supports containment in a circle on the same GiST index the bounding box queries use, so this is a narrower filter rather than a slower one.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area.</param>
        /// <param name="countyId">The integer identifier of the county partition to query.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision to filter by.</param>
        /// <param name="tolerance">The distance the search radius is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the <see cref="PointCloud3D"/> inside the circle, or null if the circle is null or degenerate, or no points match.</returns>
        public static async Task<PointCloud3D?> GetPointCloud3DByCircle2DAsync(NpgsqlConnection? npgsqlConnection, Circle2D? circle2D, int countyId, int? subdivisionId = null, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || circle2D is null || !IsQueryable(circle2D))
            {
                return null;
            }

            string commandText = $@"
                SELECT x, y, z
                FROM {Constants.TableName.TerrainPoint}
                WHERE county_id = @countyId
                  AND point(x, y) <@ circle(point(@centerX, @centerY), @radius)
                  AND (@subdivisionId IS NULL OR subdivision_id = @subdivisionId);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddCircle2DParameters(npgsqlCommand, circle2D, tolerance);
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivisionId", NpgsqlDbType.Integer) { Value = (object?)subdivisionId ?? DBNull.Value });

            return await ReadAsync_PointCloud3D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> of the terrain points lying inside the specified circle, for a specific county partition, automatically managing the connection.
        /// </summary>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area.</param>
        /// <param name="countyId">The integer identifier of the county partition to query.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision to filter by.</param>
        /// <param name="tolerance">The distance the search radius is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the <see cref="PointCloud3D"/> inside the circle, or null if the circle is null or degenerate, or no points match.</returns>
        public async Task<PointCloud3D?> GetPointCloud3DByCircle2DAsync(Circle2D? circle2D, int countyId, int? subdivisionId = null, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (circle2D is null || !IsQueryable(circle2D))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetPointCloud3DByCircle2DAsync(npgsqlConnection, circle2D, countyId, subdivisionId, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> of the terrain points lying inside the specified circle, across the given counties.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The distance the search radius is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the combined <see cref="PointCloud3D"/> across the given county partitions, or null if the circle is null or degenerate, no county was given, or no points match.</returns>
        public static async Task<PointCloud3D?> GetPointCloud3DByCircle2DAsync(NpgsqlConnection? npgsqlConnection, Circle2D? circle2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || circle2D is null || countyIds is null || !IsQueryable(circle2D))
            {
                return null;
            }

            List<PointCloud3D?> pointCloud3Ds = [];
            foreach (int countyId in countyIds)
            {
                pointCloud3Ds.Add(await GetPointCloud3DByCircle2DAsync(npgsqlConnection, circle2D, countyId, null, tolerance, commandTimeout, cancellationToken));
            }

            return Geometry.PointCloud.Spatial.Create.PointCloud3D(pointCloud3Ds);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PointCloud3D"/> of the terrain points lying inside the specified circle, across the given counties, automatically managing the connection.
        /// </summary>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The distance the search radius is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task containing the combined <see cref="PointCloud3D"/> across the given county partitions, or null if the circle is null or degenerate, no county was given, or no points match.</returns>
        public async Task<PointCloud3D?> GetPointCloud3DByCircle2DAsync(Circle2D? circle2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (circle2D is null || countyIds is null || !IsQueryable(circle2D))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetPointCloud3DByCircle2DAsync(npgsqlConnection, circle2D, countyIds, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="TerrainPoint"/> records within the specified 2D bounding box for a specific county partition.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyId">The integer identifier of the county partition to query.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision to filter by.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="TerrainPoint"/> objects, or null if no records match.</returns>
        public static async Task<List<TerrainPoint>?> GetTerrainPointsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, int countyId, int? subdivisionId = null, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT county_id, subdivision_id, x, y, z, created_at
                FROM {Constants.TableName.TerrainPoint}
                WHERE county_id = @countyId
                  AND point(x, y) <@ box(point(@minX, @minY), point(@maxX, @maxY))
                  AND (@subdivisionId IS NULL OR subdivision_id = @subdivisionId);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddBoundingBox2DParameters(npgsqlCommand, boundingBox2D, tolerance);
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivisionId", NpgsqlDbType.Integer) { Value = (object?)subdivisionId ?? DBNull.Value });

            return await ReadAsync_TerrainPoint(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="TerrainPoint"/> records within the specified 2D bounding box for a specific county partition, automatically managing the connection.
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyId">The integer identifier of the county partition to query.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision to filter by.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="TerrainPoint"/> objects, or null if no records match.</returns>
        public async Task<List<TerrainPoint>?> GetTerrainPointsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, int countyId, int? subdivisionId = null, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (boundingBox2D is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetTerrainPointsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyId, subdivisionId, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="TerrainPoint"/> records within the specified 2D bounding box across the given counties.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all <see cref="TerrainPoint"/> objects across the given counties, or null if no county was given or no records match.</returns>
        public static async Task<List<TerrainPoint>?> GetTerrainPointsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null || countyIds is null)
            {
                return null;
            }

            List<TerrainPoint> result = [];

            foreach (int countyId in countyIds)
            {
                List<TerrainPoint>? terrainPoints = await GetTerrainPointsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyId, null, tolerance, commandTimeout, cancellationToken);
                if (terrainPoints is not null && terrainPoints.Count != 0)
                {
                    result.AddRange(terrainPoints);
                }
            }

            if (result.Count == 0)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Compares the nodes of a lattice lying on a county's land against the points this table holds for its partition.
        /// <para>Measured against the single partition named. For a county held in several pieces pass every part to the <see cref="GetCoverageByCountyIdAsync(NpgsqlConnection, IEnumerable{int}, Dictionary{int, PolygonalFace2D}, BoundingBox2D, double, Point2D, double, int, long, int, int, CancellationToken)"/> overload instead - a piece holds only the territory filed under it, so one partition alone reads as short of its siblings.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyId">The identifier of the county partition to measure.</param>
        /// <param name="polygonalFace2Ds_ById">The outlines of the county's subdivisions, keyed by subdivision identifier.</param>
        /// <param name="boundingBox2D_Limit">An area to confine the measurement to, or null for the whole county.</param>
        /// <param name="gridSize">The lattice spacing.</param>
        /// <param name="origin">The point the lattice is anchored at.</param>
        /// <param name="tolerance">The distance a stored point may lie from a node and still be counted as that node.</param>
        /// <param name="limit">The largest number of missing coordinates to carry back. The counts themselves are never capped, only the list.</param>
        /// <param name="maximumNodeCount">The largest number of lattice nodes the request may generate, checked before a single node is built. Values of zero or less do not cap it.</param>
        /// <param name="tileSize">The number of lattice steps along a tile edge. Matches the sampling task so that the two walk the same tiles.</param>
        /// <param name="commandTimeout">The timeout in seconds for each read of what a tile already holds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller to cancel the asynchronous operation.</param>
        /// <returns>The coverage, or <see langword="null"/> when the input is unusable or the area and the lattice together exceed <paramref name="maximumNodeCount"/>.</returns>
        public static async Task<TerrainPointCoverageResult?> GetCoverageByCountyIdAsync(
            NpgsqlConnection? npgsqlConnection,
            int countyId,
            Dictionary<int, PolygonalFace2D>? polygonalFace2Ds_ById,
            BoundingBox2D? boundingBox2D_Limit,
            double gridSize,
            Point2D? origin,
            double tolerance,
            int limit,
            long maximumNodeCount,
            int tileSize = 128,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            return await GetCoverageByCountyIdAsync(npgsqlConnection, [countyId], polygonalFace2Ds_ById, boundingBox2D_Limit, gridSize, origin, tolerance, limit, maximumNodeCount, tileSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Compares the nodes of a lattice lying on a county's land against the points this table holds for it.
        /// <para>Walked in tiles rather than in one pass, the same way the sampling task walks a county: a county at a fine spacing is millions of nodes, and holding all of them and all of their stored counterparts at once is what a tile exists to avoid. The tiles are cut from the shared lattice in index space, so a node belongs to exactly one of them.</para>
        /// <para>The stored read reaches every partition named, so a county held in several pieces is measured against what all of its parts hold together - whichever partition a point is filed under, the node it answers for counts as stored. The subdivisions are passed in rather than read here, because they live in a different database to the points.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="countyIds">The identifiers of the county partitions to measure - every part sharing a code for a multi-part county. The result names the lowest of them.</param>
        /// <param name="polygonalFace2Ds_ById">The outlines of the county's subdivisions, keyed by subdivision identifier.</param>
        /// <param name="boundingBox2D_Limit">An area to confine the measurement to, or null for the whole county.</param>
        /// <param name="gridSize">The lattice spacing.</param>
        /// <param name="origin">The point the lattice is anchored at.</param>
        /// <param name="tolerance">The distance a stored point may lie from a node and still be counted as that node.</param>
        /// <param name="limit">The largest number of missing coordinates to carry back. The counts themselves are never capped, only the list.</param>
        /// <param name="maximumNodeCount">The largest number of lattice nodes the request may generate, checked before a single node is built. Values of zero or less do not cap it.</param>
        /// <param name="tileSize">The number of lattice steps along a tile edge. Matches the sampling task so that the two walk the same tiles.</param>
        /// <param name="commandTimeout">The timeout in seconds for each read of what a tile already holds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller to cancel the asynchronous operation.</param>
        /// <returns>The coverage, or <see langword="null"/> when the input is unusable or the area and the lattice together exceed <paramref name="maximumNodeCount"/>.</returns>
        public static async Task<TerrainPointCoverageResult?> GetCoverageByCountyIdAsync(
            NpgsqlConnection? npgsqlConnection,
            IEnumerable<int>? countyIds,
            Dictionary<int, PolygonalFace2D>? polygonalFace2Ds_ById,
            BoundingBox2D? boundingBox2D_Limit,
            double gridSize,
            Point2D? origin,
            double tolerance,
            int limit,
            long maximumNodeCount,
            int tileSize = 128,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || countyIds is null || polygonalFace2Ds_ById is null || origin is null || gridSize <= 0 || tileSize <= 0)
            {
                return null;
            }

            List<int> countyIds_Sorted = [.. countyIds];
            countyIds_Sorted.Sort();

            if (countyIds_Sorted.Count == 0)
            {
                return null;
            }

            int countyId = countyIds_Sorted[0];

            static int FloorDivide(int value, int divisor)
            {
                int quotient = value / divisor;

                return value % divisor != 0 && value < 0 ? quotient - 1 : quotient;
            }

            List<BoundingBox2D> boundingBox2Ds_Subdivision = [];
            foreach (PolygonalFace2D polygonalFace2D in polygonalFace2Ds_ById.Values)
            {
                if (polygonalFace2D.GetBoundingBox() is BoundingBox2D boundingBox2D_Subdivision)
                {
                    boundingBox2Ds_Subdivision.Add(boundingBox2D_Subdivision);
                }
            }

            if (boundingBox2Ds_Subdivision.Count == 0)
            {
                return new TerrainPointCoverageResult(countyId, gridSize, origin.X, origin.Y, 0, 0, 0, 0, null);
            }

            BoundingBox2D boundingBox2D_County = new(boundingBox2Ds_Subdivision);

            double x_Min = boundingBox2D_County.Min.X;
            double x_Max = boundingBox2D_County.Max.X;
            double y_Min = boundingBox2D_County.Min.Y;
            double y_Max = boundingBox2D_County.Max.Y;

            if (boundingBox2D_Limit is not null)
            {
                x_Min = Math.Max(x_Min, boundingBox2D_Limit.Min.X);
                x_Max = Math.Min(x_Max, boundingBox2D_Limit.Max.X);
                y_Min = Math.Max(y_Min, boundingBox2D_Limit.Min.Y);
                y_Max = Math.Min(y_Max, boundingBox2D_Limit.Max.Y);

                if (x_Max < x_Min || y_Max < y_Min)
                {
                    return new TerrainPointCoverageResult(countyId, gridSize, origin.X, origin.Y, 0, 0, 0, 0, null);
                }
            }

            double index_X_Min_Double = Math.Ceiling((x_Min - origin.X - tolerance) / gridSize);
            double index_X_Max_Double = Math.Floor((x_Max - origin.X + tolerance) / gridSize);
            double index_Y_Min_Double = Math.Ceiling((y_Min - origin.Y - tolerance) / gridSize);
            double index_Y_Max_Double = Math.Floor((y_Max - origin.Y + tolerance) / gridSize);

            if (double.IsNaN(index_X_Min_Double) || double.IsNaN(index_X_Max_Double) || double.IsNaN(index_Y_Min_Double) || double.IsNaN(index_Y_Max_Double))
            {
                return new TerrainPointCoverageResult(countyId, gridSize, origin.X, origin.Y, 0, 0, 0, 0, null);
            }

            if (index_X_Max_Double < index_X_Min_Double || index_Y_Max_Double < index_Y_Min_Double)
            {
                return new TerrainPointCoverageResult(countyId, gridSize, origin.X, origin.Y, 0, 0, 0, 0, null);
            }

            // Checked in double and before a single node is built. The whole rectangle is the ceiling rather than
            // the land inside it, because knowing how much of it is land is itself the work being guarded against.
            if (maximumNodeCount > 0 && (index_X_Max_Double - index_X_Min_Double + 1) * (index_Y_Max_Double - index_Y_Min_Double + 1) > maximumNodeCount)
            {
                return null;
            }

            if (index_X_Min_Double < int.MinValue || index_X_Max_Double > int.MaxValue || index_Y_Min_Double < int.MinValue || index_Y_Max_Double > int.MaxValue)
            {
                return null;
            }

            int index_X_Min = System.Convert.ToInt32(index_X_Min_Double);
            int index_X_Max = System.Convert.ToInt32(index_X_Max_Double);
            int index_Y_Min = System.Convert.ToInt32(index_Y_Min_Double);
            int index_Y_Max = System.Convert.ToInt32(index_Y_Max_Double);

            long expectedCount = 0;
            long storedCount = 0;
            long missingCount = 0;
            long offGridCount = 0;
            List<Point2D> point2Ds_Missing = [];

            int block_X_Min = FloorDivide(index_X_Min, tileSize);
            int block_X_Max = FloorDivide(index_X_Max, tileSize);
            int block_Y_Min = FloorDivide(index_Y_Min, tileSize);
            int block_Y_Max = FloorDivide(index_Y_Max, tileSize);

            for (int block_X = block_X_Min; block_X <= block_X_Max; block_X++)
            {
                for (int block_Y = block_Y_Min; block_Y <= block_Y_Max; block_Y++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int index_X_Low = Math.Max(index_X_Min, block_X * tileSize);
                    int index_X_High = Math.Min(index_X_Max, ((block_X + 1) * tileSize) - 1);
                    int index_Y_Low = Math.Max(index_Y_Min, block_Y * tileSize);
                    int index_Y_High = Math.Min(index_Y_Max, ((block_Y + 1) * tileSize) - 1);

                    if (index_X_High < index_X_Low || index_Y_High < index_Y_Low)
                    {
                        continue;
                    }

                    BoundingBox2D boundingBox2D_Tile = new(
                        new Point2D(origin.X + (index_X_Low * gridSize), origin.Y + (index_Y_Low * gridSize)),
                        new Point2D(origin.X + (index_X_High * gridSize), origin.Y + (index_Y_High * gridSize)));

                    bool intersects = false;
                    foreach (BoundingBox2D boundingBox2D_Subdivision in boundingBox2Ds_Subdivision)
                    {
                        if (boundingBox2D_Tile.InRange(boundingBox2D_Subdivision, tolerance))
                        {
                            intersects = true;
                            break;
                        }
                    }

                    if (!intersects)
                    {
                        continue;
                    }

                    List<Point2D>? point2Ds_Tile = boundingBox2D_Tile.Point2Ds(origin, gridSize, gridSize, tolerance);
                    if (point2Ds_Tile is null || point2Ds_Tile.Count == 0)
                    {
                        continue;
                    }

                    int?[]? subdivisionIds = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds_Tile, tolerance);
                    if (subdivisionIds is null)
                    {
                        continue;
                    }

                    HashSet<(int, int)> indexes_Stored = [];
                    PointCloud3D? pointCloud3D = await GetPointCloud3DByBoundingBox2DAsync(npgsqlConnection, boundingBox2D_Tile, countyIds_Sorted, tolerance, commandTimeout, cancellationToken);
                    if (pointCloud3D is not null)
                    {
                        for (int i = 0; i < pointCloud3D.Count; i++)
                        {
                            if (!pointCloud3D.TryGetPoint(i, out double x, out double y, out double _))
                            {
                                continue;
                            }

                            if (new Point2D(x, y).TryGetGridIndex(origin, gridSize, gridSize, out int index_X_Stored, out int index_Y_Stored, tolerance))
                            {
                                indexes_Stored.Add((index_X_Stored, index_Y_Stored));
                            }
                            else
                            {
                                offGridCount++;
                            }
                        }
                    }

                    for (int i = 0; i < point2Ds_Tile.Count; i++)
                    {
                        // A node in no subdivision is outside the county's land. A rectangle laid over an irregular
                        // outline always holds some, and a run was never going to sample them.
                        if (subdivisionIds[i] is not int)
                        {
                            continue;
                        }

                        expectedCount++;

                        Point2D point2D = point2Ds_Tile[i];
                        if (point2D.TryGetGridIndex(origin, gridSize, gridSize, out int index_X_Point, out int index_Y_Point, tolerance) && indexes_Stored.Contains((index_X_Point, index_Y_Point)))
                        {
                            storedCount++;
                            continue;
                        }

                        missingCount++;

                        if (point2Ds_Missing.Count < limit)
                        {
                            point2Ds_Missing.Add(point2D);
                        }
                    }
                }
            }

            return new TerrainPointCoverageResult(countyId, gridSize, origin.X, origin.Y, expectedCount, storedCount, missingCount, offGridCount, point2Ds_Missing);
        }

        /// <summary>
        /// Compares the nodes of a lattice lying on a county's land against the points this table holds for it, automatically managing the connection.
        /// <para>Walked in tiles rather than in one pass, the same way the sampling task walks a county: a county at a fine spacing is millions of nodes, and holding all of them and all of their stored counterparts at once is what a tile exists to avoid. The tiles are cut from the shared lattice in index space, so a node belongs to exactly one of them.</para>
        /// <para>The membership test, the node generation and the lattice all come from the helpers the sampling task itself uses. Deriving them again elsewhere would let the two drift, and a coverage that disagrees with the run it measures reports holes where nothing was ever going to be sampled. This sits on the converter rather than in the Web API for that reason: the endpoint that reports a missing node and the task that goes back for it have to mean the same thing by it.</para>
        /// <para>The subdivision outlines are passed in rather than read here, because they live in a different database to the points.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county partition to measure.</param>
        /// <param name="polygonalFace2Ds_ById">The outlines of the county's subdivisions, keyed by subdivision identifier.</param>
        /// <param name="boundingBox2D_Limit">An area to confine the measurement to, or null for the whole county.</param>
        /// <param name="gridSize">The lattice spacing.</param>
        /// <param name="origin">The point the lattice is anchored at.</param>
        /// <param name="tolerance">The distance a stored point may lie from a node and still be counted as that node.</param>
        /// <param name="limit">The largest number of missing coordinates to carry back. The counts themselves are never capped, only the list.</param>
        /// <param name="maximumNodeCount">The largest number of lattice nodes the request may generate, checked before a single node is built. Values of zero or less do not cap it.</param>
        /// <param name="tileSize">The number of lattice steps along a tile edge. Matches the sampling task so that the two walk the same tiles.</param>
        /// <param name="commandTimeout">The timeout in seconds for each read of what a tile already holds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller to cancel the asynchronous operation.</param>
        /// <returns>The coverage, or <see langword="null"/> when the input is unusable or the area and the lattice together exceed <paramref name="maximumNodeCount"/>.</returns>
        public async Task<TerrainPointCoverageResult?> GetCoverageByCountyIdAsync(
            int countyId,
            Dictionary<int, PolygonalFace2D>? polygonalFace2Ds_ById,
            BoundingBox2D? boundingBox2D_Limit,
            double gridSize,
            Point2D? origin,
            double tolerance,
            int limit,
            long maximumNodeCount,
            int tileSize = 128,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (polygonalFace2Ds_ById is null || origin is null || gridSize <= 0 || tileSize <= 0)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetCoverageByCountyIdAsync(npgsqlConnection, countyId, polygonalFace2Ds_ById, boundingBox2D_Limit, gridSize, origin, tolerance, limit, maximumNodeCount, tileSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Compares the nodes of a lattice lying on a county's land against the points this table holds for it, automatically managing the connection.
        /// <para>The stored read reaches every partition named, so a county held in several pieces is measured against what all of its parts hold together - whichever partition a point is filed under, the node it answers for counts as stored. Pass every part sharing a code; the single-partition overload reads as short of the siblings' territory. The subdivision outlines are passed in rather than read here, because they live in a different database to the points.</para>
        /// </summary>
        /// <param name="countyIds">The identifiers of the county partitions to measure - every part sharing a code for a multi-part county. The result names the lowest of them.</param>
        /// <param name="polygonalFace2Ds_ById">The outlines of the county's subdivisions, keyed by subdivision identifier.</param>
        /// <param name="boundingBox2D_Limit">An area to confine the measurement to, or null for the whole county.</param>
        /// <param name="gridSize">The lattice spacing.</param>
        /// <param name="origin">The point the lattice is anchored at.</param>
        /// <param name="tolerance">The distance a stored point may lie from a node and still be counted as that node.</param>
        /// <param name="limit">The largest number of missing coordinates to carry back. The counts themselves are never capped, only the list.</param>
        /// <param name="maximumNodeCount">The largest number of lattice nodes the request may generate, checked before a single node is built. Values of zero or less do not cap it.</param>
        /// <param name="tileSize">The number of lattice steps along a tile edge. Matches the sampling task so that the two walk the same tiles.</param>
        /// <param name="commandTimeout">The timeout in seconds for each read of what a tile already holds.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller to cancel the asynchronous operation.</param>
        /// <returns>The coverage, or <see langword="null"/> when the input is unusable or the area and the lattice together exceed <paramref name="maximumNodeCount"/>.</returns>
        public async Task<TerrainPointCoverageResult?> GetCoverageByCountyIdAsync(
            IEnumerable<int>? countyIds,
            Dictionary<int, PolygonalFace2D>? polygonalFace2Ds_ById,
            BoundingBox2D? boundingBox2D_Limit,
            double gridSize,
            Point2D? origin,
            double tolerance,
            int limit,
            long maximumNodeCount,
            int tileSize = 128,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (polygonalFace2Ds_ById is null || origin is null || gridSize <= 0 || tileSize <= 0)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetCoverageByCountyIdAsync(npgsqlConnection, countyIds, polygonalFace2Ds_ById, boundingBox2D_Limit, gridSize, origin, tolerance, limit, maximumNodeCount, tileSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="TerrainPoint"/> records within the specified 2D bounding box across the given counties, automatically managing the connection.
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyIds">The identifiers of the county partitions to search. The terrain table is partitioned by county and this database holds no administrative geometry to derive them from, so they are the caller's to supply.</param>
        /// <param name="tolerance">The distance the search bounding box is expanded by. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all <see cref="TerrainPoint"/> objects across the given counties, or null if no county was given or no records match.</returns>
        public async Task<List<TerrainPoint>?> GetTerrainPointsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, IEnumerable<int>? countyIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (boundingBox2D is null || countyIds is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetTerrainPointsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyIds, tolerance, commandTimeout, cancellationToken);
        }

        private static string TableName(int? countyId)
        {
            if (countyId.HasValue)
            {
                return $"{Constants.TableName.TerrainPoint}_{countyId.Value}";
            }

            return Constants.TableName.TerrainPoint;
        }

        private static string CopyCommandText(int countyId)
        {
            return $@"
                COPY {TableName(countyId)} (county_id, subdivision_id, x, y, z, created_at)
                FROM STDIN (FORMAT BINARY)";
        }

        private static async Task WriteRowAsync(NpgsqlBinaryImporter npgsqlBinaryImporter, int countyId, int? subdivisionId, double x, double y, double z, DateTime createdAt, CancellationToken cancellationToken)
        {
            await npgsqlBinaryImporter.StartRowAsync(cancellationToken);
            await npgsqlBinaryImporter.WriteAsync(countyId, NpgsqlDbType.Integer, cancellationToken);

            if (subdivisionId.HasValue)
            {
                await npgsqlBinaryImporter.WriteAsync(subdivisionId.Value, NpgsqlDbType.Integer, cancellationToken);
            }
            else
            {
                await npgsqlBinaryImporter.WriteNullAsync(cancellationToken);
            }

            await npgsqlBinaryImporter.WriteAsync(x, NpgsqlDbType.Double, cancellationToken);
            await npgsqlBinaryImporter.WriteAsync(y, NpgsqlDbType.Double, cancellationToken);
            await npgsqlBinaryImporter.WriteAsync(z, NpgsqlDbType.Double, cancellationToken);
            await npgsqlBinaryImporter.WriteAsync(createdAt, NpgsqlDbType.TimestampTz, cancellationToken);
        }

        private static async Task<long> InsertArraysAsync(NpgsqlConnection npgsqlConnection, int[] countyIds, int?[] subdivisionIds, double[] x, double[] y, double[] z, DateTime[] createdAts, int commandTimeout, CancellationToken cancellationToken)
        {
            // One statement carrying six arrays, rather than one statement per point. A cloud of a million
            // points would otherwise become a million commands with five parameters each, all built and
            // held in memory before any of them reaches the server.
            string commandText = $@"
                INSERT INTO {Constants.TableName.TerrainPoint} (county_id, subdivision_id, x, y, z, created_at)
                SELECT t.county_id, t.subdivision_id, t.x, t.y, t.z, t.created_at
                FROM unnest(@countyIds, @subdivisionIds, @x, @y, @z, @createdAts)
                    AS t(county_id, subdivision_id, x, y, z, created_at)
                ON CONFLICT (county_id, x, y) DO NOTHING;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivisionIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = subdivisionIds });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("x", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = x });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("y", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = y });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("z", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = z });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("createdAts", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz) { Value = createdAts });

            return await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Renders the county filter of an aggregate over the partitioned parent, or nothing at all when every county is wanted.
        /// <para>Written as a clause that is present or absent rather than as one parameter that may be null. A predicate of the form <c>@countyIds IS NULL OR county_id = ANY(@countyIds)</c> reads the same but cannot be resolved when the plan is made, so PostgreSQL keeps every one of the several hundred partitions in the plan even when a single county was asked for.</para>
        /// </summary>
        /// <param name="countyIds">The county identifiers to filter by, or null for every county.</param>
        /// <returns>The WHERE clause, or an empty string.</returns>
        private static string WhereCountyIds(int[]? countyIds)
        {
            if (countyIds is null)
            {
                return string.Empty;
            }

            return "WHERE county_id = ANY(@countyIds)";
        }

        /// <summary>
        /// Adds the county filter parameter that <see cref="WhereCountyIds(int[])"/> refers to, when there is a clause to refer to it.
        /// </summary>
        /// <param name="npgsqlCommand">The command to add the parameter to.</param>
        /// <param name="countyIds">The county identifiers to filter by, or null for every county.</param>
        private static void AddCountyIdsParameter(NpgsqlCommand npgsqlCommand, int[]? countyIds)
        {
            if (countyIds is null)
            {
                return;
            }

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds });
        }

        private static void AddBoundingBox2DParameters(NpgsqlCommand npgsqlCommand, BoundingBox2D boundingBox2D, double tolerance)
        {
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("minX", NpgsqlDbType.Double) { Value = boundingBox2D.Min.X - tolerance });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("minY", NpgsqlDbType.Double) { Value = boundingBox2D.Min.Y - tolerance });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("maxX", NpgsqlDbType.Double) { Value = boundingBox2D.Max.X + tolerance });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("maxY", NpgsqlDbType.Double) { Value = boundingBox2D.Max.Y + tolerance });
        }

        private static bool IsQueryable(Circle2D circle2D)
        {
            if (circle2D.Center is null)
            {
                return false;
            }

            double radius = circle2D.Radius;

            return !double.IsNaN(radius) && !double.IsInfinity(radius) && radius > 0;
        }

        private static void AddCircle2DParameters(NpgsqlCommand npgsqlCommand, Circle2D circle2D, double tolerance)
        {
            // Center hands back a fresh Point2D on every read, so it is taken once rather than three times.
            Point2D? center = circle2D.Center;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("centerX", NpgsqlDbType.Double) { Value = center!.X });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("centerY", NpgsqlDbType.Double) { Value = center.Y });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("radius", NpgsqlDbType.Double) { Value = circle2D.Radius + tolerance });
        }

        private static TerrainPoint Create_TerrainPoint(NpgsqlDataReader npgsqlDataReader)
        {
            return new TerrainPoint
            {
                CountyId = npgsqlDataReader.IsDBNull(0) ? null : npgsqlDataReader.GetInt32(0),
                SubdivisionId = npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetInt32(1),
                Point3D = new Point3D(npgsqlDataReader.GetDouble(2), npgsqlDataReader.GetDouble(3), npgsqlDataReader.GetDouble(4)),
                CreatedAt = npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetDateTime(5)
            };
        }

        private static async Task<List<TerrainPoint>?> ReadAsync_TerrainPoint(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken)
        {
            List<TerrainPoint> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_TerrainPoint(npgsqlDataReader));
            }

            if (result.Count == 0)
            {
                return null;
            }

            return result;
        }

        private static async Task<PointCloud3D?> ReadAsync_PointCloud3D(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken)
        {
            List<double> xValues = [];
            List<double> yValues = [];
            List<double> zValues = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                xValues.Add(npgsqlDataReader.GetDouble(0));
                yValues.Add(npgsqlDataReader.GetDouble(1));
                zValues.Add(npgsqlDataReader.GetDouble(2));
            }

            if (xValues.Count == 0)
            {
                return null;
            }

            return new PointCloud3D([.. xValues], [.. yValues], [.. zValues]);
        }
    }
}
