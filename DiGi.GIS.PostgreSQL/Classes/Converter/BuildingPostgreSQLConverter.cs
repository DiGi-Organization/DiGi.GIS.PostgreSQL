using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.PostgreSQL.Constants;
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
    /// Provides a specialized implementation for converting <see cref="Building"/> objects to and from a PostgreSQL database format, incorporating GIS-specific conversion capabilities.
    /// </summary>
    public class BuildingPostgreSQLConverter : PostgreSQLConverter<Building>, IGISPostgreSQLConverter<Building>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public BuildingPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Asynchronously retrieves the <see cref="Building"/> record with the latest creation timestamp using the specified connection.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the latest <see cref="Building"/> object, or null if none is found or the connection is null.</returns>
        public static async Task<Building?> GetBuildingByLatestCreatedAtAsync(NpgsqlConnection? npgsqlConnection, int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = countyId.HasValue ? $@"
                SELECT id, county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object, created_at
                FROM {TableName.Building}
                WHERE county_id = @countyId
                ORDER BY created_at DESC
                LIMIT 1;" : $@"
                SELECT id, county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object, created_at
                FROM {TableName.Building}
                ORDER BY created_at DESC
                LIMIT 1;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            List<Building>? buildings = await ReadAsync_Building(npgsqlCommand, cancellationToken);
            if (buildings is null || buildings.Count == 0)
            {
                return null;
            }

            return buildings[0];
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building"/> records whose bounding box overlaps the specified 2D bounding box, optionally filtered by a county identifier.
        /// <para>Only the X and Y extents participate in the filter; records stored without a bounding box are excluded.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building"/> objects, or null if the connection or the bounding box are null.</returns>
        public static async Task<List<Building>?> GetBuildingsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return null;
            }

            // PostgreSQL orders NaN above every other float value, so records stored without a bounding box
            // (written as NaN by UpdateAsync) have to be excluded explicitly before the overlap test.
            string commandText = countyId.HasValue ? $@"
                SELECT id, county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object, created_at
                FROM {TableName.Building}
                WHERE county_id = @countyId
                  AND min_x <> 'NaN'::float8
                  AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@minX, @minY), point(@maxX, @maxY));" : $@"
                SELECT id, county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object, created_at
                FROM {TableName.Building}
                WHERE min_x <> 'NaN'::float8
                  AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@minX, @minY), point(@maxX, @maxY));";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("minX", NpgsqlDbType.Double) { Value = boundingBox2D.Min.X });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("minY", NpgsqlDbType.Double) { Value = boundingBox2D.Min.Y });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("maxX", NpgsqlDbType.Double) { Value = boundingBox2D.Max.X });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("maxY", NpgsqlDbType.Double) { Value = boundingBox2D.Max.Y });

            return await ReadAsync_Building(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building"/> records based on the specified references and optional county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="references">A collection of strings representing the references to search for.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the results.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building"/> objects, or null if the connection or references are null.</returns>
        public static async Task<List<Building>?> GetBuildingsByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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
                SELECT id, county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object, created_at
                FROM {TableName.Building}
                WHERE county_id = @countyId
                  AND reference = ANY(@references);" : $@"
                SELECT id, county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object, created_at
                FROM {TableName.Building}
                WHERE reference = ANY(@references);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });
            if (countyId.HasValue)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
            }

            List<Building>? result = await ReadAsync_Building(npgsqlCommand, cancellationToken);
            if (result is null)
            {
                return null;
            }

            if (fallbackByReference && countyId is not null)
            {
                HashSet<string> foundReferences = [.. result.Where(b => !string.IsNullOrWhiteSpace(b.Reference)).Select(b => b.Reference!)];
                string[] missingReferences = [.. references_Array.Where(r => !string.IsNullOrWhiteSpace(r) && !foundReferences.Contains(r)).Distinct()];

                if (missingReferences.Length > 0)
                {
                    List<Building>? fallbackItems = await GetBuildingsByReferencesAsync(npgsqlConnection, missingReferences, null, false, commandTimeout, cancellationToken);
                    if (fallbackItems is not null && fallbackItems.Count > 0)
                    {
                        result.AddRange(fallbackItems);
                    }
                }
            }

            return result;
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

            string tableName = TableName.Building;
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

            string tableName = TableName.Building;
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
                countyIds_ByTableName[string.Format("{0}_{1}", TableName.Building, countyId)] = countyId;
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
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> to use for the query.</param>
        /// <param name="countyIds">A collection of integers representing the county identifiers to estimate counts for.</param>
        /// <param name="analyze">A boolean indicating whether to run an analysis operation before fetching the estimated count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
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
        /// Asynchronously clears all data from the building table.
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

            bool result = await DiGi.PostgreSQL.Modify.ClearAsync(npgsqlConnection, TableName.Building, commandTimeout: commandTimeout, cancellationToken: cancellationToken);

            return result;
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
                LEFT JOIN {TableName.Building} u ON u.reference = input_ref
                    AND u.county_id = @county_id
                WHERE
                    (@inverted = false AND u.reference IS NOT NULL)
                    OR
                    (@inverted = true AND u.reference IS NULL);" : $@"
                SELECT input_ref
                FROM UNNEST(@refs) AS input_ref
                LEFT JOIN {TableName.Building} u ON u.reference = input_ref
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
                            INNER JOIN {TableName.Building} u ON u.reference = input.r;";

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
                            INNER JOIN {TableName.Building} u ON u.reference = input.r;";

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
        /// Asynchronously retrieves the <see cref="Building"/> record with the latest creation timestamp.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county to filter the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the latest <see cref="Building"/> object, or null if none is found or the connection fails.</returns>
        public async Task<Building?> GetBuildingByLatestCreatedAtAsync(int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBuildingByLatestCreatedAtAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the single most relevant <see cref="Building"/> for the specified reference, falling back to a spatial search around the supplied point when the reference cannot be resolved.
        /// <para>Candidates are ranked ascending by level of detail and then by year, with nulls treated as the lowest rank; ties between candidates of equal rank are broken by the surface geometry closest to <paramref name="point3D"/>.</para>
        /// <para>The spatial fallback ignores the reference entirely and is limited in X and Y by <paramref name="maxDistance"/>; the resulting candidate distance itself is not capped.</para>
        /// </summary>
        /// <param name="reference">The string reference of the building to search for.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the results.</param>
        /// <param name="point3D">The optional <see cref="Point3D"/> used to break ties and to locate the building when the reference cannot be resolved.</param>
        /// <param name="maxDistance">The distance used to inflate <paramref name="point3D"/> in X and Y into the bounding box of the spatial fallback search.</param>
        /// <param name="tolerance">The tolerance used for the closest point calculation.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search without county filtering if the reference is not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the most relevant <see cref="Building"/>, or null if none could be resolved.</returns>
        public async Task<Building?> GetBuildingByReferenceAsync(
            string reference,
            int? countyId,
            Point3D? point3D,
            double maxDistance = 1,
            double tolerance = Core.Constants.Tolerance.MacroDistance,
            bool fallbackByReference = false,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            List<Building>? buildings = await GetBuildingsByReferencesAsync(npgsqlConnection, [reference], countyId, fallbackByReference, commandTimeout, cancellationToken);

            Building? result = Query.Building(buildings, point3D, tolerance);
            if (result is not null)
            {
                return result;
            }

            if (point3D is null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = new(
                new Point2D(point3D.X - maxDistance, point3D.Y - maxDistance),
                new Point2D(point3D.X + maxDistance, point3D.Y + maxDistance));

            buildings = await GetBuildingsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, countyId, commandTimeout, cancellationToken);

            return Query.Building(buildings, point3D, tolerance);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building"/> records based on the specified reference and optional county identifier.
        /// </summary>
        /// <param name="reference">The string reference of the buildings to search for.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the results.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering if not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building"/> objects, or null if the operation fails.</returns>
        public async Task<List<Building>?> GetBuildingsByReferenceAsync(string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return await GetBuildingsByReferencesAsync([reference], countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building"/> records based on the specified references and optional county identifier.
        /// </summary>
        /// <param name="references">A collection of strings representing the references to search for.</param>
        /// <param name="countyId">The optional integer identifier of the county to filter the results.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building"/> objects, or null if the operation fails.</returns>
        public async Task<List<Building>?> GetBuildingsByReferencesAsync(IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            return await GetBuildingsByReferencesAsync(npgsqlConnection, references, countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county to filter the count; if null, the count is retrieved for all counties.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total row count as a long.</returns>
        public async Task<long> GetCountAsync(int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string tableName = TableName.Building;
            if (countyId != null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, tableName, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated row count, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county to filter the estimate.</param>
        /// <param name="analyze">A boolean value indicating whether to run an analysis operation before fetching the count to ensure higher accuracy.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated row count as a nullable long, -1 when the partition exists but has not been analysed, or null if an error occurs or the target does not exist.</returns>
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
        /// Asynchronously updates a collection of <see cref="Building"/> records in the PostgreSQL database.
        /// <para>A building that names no county row is resolved by geometry against every county its bounding box overlaps. When the county is known but which of its polygon parts is not, use <see cref="UpdateAsync(IEnumerable{Building}, IEnumerable{int}, AdministrativeAreal2DPostgreSQLConverter, double, int, CancellationToken)"/> and hand over the parts - it narrows the field before the geometry runs.</para>
        /// </summary>
        /// <param name="buildings">The collection of <see cref="Building"/> records to be updated or inserted.</param>
        /// <param name="tolerance">The tolerance to use for spatial classification if county ID is missing.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the identifiers written and the rows dropped before the database, or null when the update could not be attempted at all - no connection, or the table could not be created.</returns>
        public async Task<PostgreSQLUpdateResult?> UpdateAsync(IEnumerable<Building>? buildings, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            return await UpdateAsync(buildings, (IEnumerable<int>?)null, null, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates a collection of <see cref="Building"/> records in the PostgreSQL database, resolving the county of a building that names none from the given candidate rows.
        /// <para>County assignment runs in three tiers, in descending reliability:</para>
        /// <para>1. the building's own county identifier - already names the row, nothing to infer.</para>
        /// <para>2. <paramref name="countyIds"/> - a county code names one row per polygon part, so the caller can state the county without stating the part. A single candidate resolves outright; several only narrow the field and are handed to tier 3.</para>
        /// <para>3. geometry - <see cref="Query.CountyId(IDictionary{int, Geometry.Planar.Interfaces.IPolygonal2D}, Geometry.Planar.Interfaces.IPolygonal2D, double)"/> decides which candidate the building lies in, is nearest to, or overlaps most. With no candidates the field is every county its bounding box overlaps, which is both slower and wider than narrowing first.</para>
        /// <para>The building is tested as the rectangle of its <see cref="Building.BoundingBox3D"/> in X and Y: a <see cref="CityGML.Classes.Building"/> carries no footprint, and county parts lie kilometres apart, so the rectangle and a true footprint can only disagree for a building sitting on a part boundary.</para>
        /// <para>Tier 3 reads <c>administrative_areal_2d</c>, which lives in the main database, while <c>building</c> lives in the storage database - so the county polygons are read through <paramref name="administrativeAreal2DPostgreSQLConverter"/>, never through this converter's own connection, and only once a building actually reaches tier 3.</para>
        /// </summary>
        /// <param name="buildings">The collection of <see cref="Building"/> records to be updated or inserted.</param>
        /// <param name="countyIds">The candidate county rows a building with no county of its own may be filed under. Null or empty searches every county overlapping the building instead.</param>
        /// <param name="administrativeAreal2DPostgreSQLConverter">The converter reading the county polygons for tier 3, connected to the main database. Null falls back to this converter's own connection, which answers only where both tables share one database.</param>
        /// <param name="tolerance">The tolerance to use for spatial classification if county ID is missing.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the identifiers written and the rows dropped before the database, or null when the update could not be attempted at all - no connection, or the table could not be created.</returns>
        public async Task<PostgreSQLUpdateResult?> UpdateAsync(IEnumerable<Building>? buildings, IEnumerable<int>? countyIds, AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (buildings is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool succeeded = await Create.TableAsync_Building(npgsqlConnection, commandTimeout, cancellationToken);
            if (!succeeded)
            {
                return null;
            }

            HashSet<long> ids = [];
            List<Rejection> rejections = [];

            if (!buildings.Any())
            {
                return new PostgreSQLUpdateResult(ids, rejections);
            }

            List<int> countyIds_Candidate = countyIds is null ? [] : [.. new HashSet<int>(countyIds)];

            // Derived once for the whole batch: a part's polygon is the same for every building tested
            // against it, and deriving one deserializes a county-sized geometry. Read on the first building that
            // reaches the geometry tier, so a single candidate, or a batch whose parts the caller already
            // resolved, costs no polygon and no trip to the main database at all.
            Dictionary<int, Geometry.Planar.Interfaces.IPolygonal2D>? polygonal2Ds_ByCountyId = null;
            bool polygonal2Ds_Read = false;

            Dictionary<int, List<Building>> dictionary_Buildings = [];

            foreach (Building building_Temp in buildings)
            {
                if (building_Temp is null)
                {
                    // Recorded rather than skipped in silence, so the rejection count stays an exact
                    // account of the rows that never reached the database.
                    rejections.Add(new Rejection(null, Enums.UpdateRejectionReason.Undefined));
                    continue;
                }

                int? countyId = building_Temp.CountyId;

                if (countyId is null || !countyId.HasValue)
                {
                    if (countyIds_Candidate.Count == 1)
                    {
                        countyId = countyIds_Candidate[0];
                    }
                    else
                    {
                        BoundingBox3D? boundingBox3D = building_Temp.BoundingBox3D;
                        if (boundingBox3D is null)
                        {
                            // No county was named and there is no geometry to infer one from, so resolution
                            // never even starts. A defect in the posted payload, unlike the tiers below.
                            rejections.Add(new Rejection(building_Temp.Reference, Enums.UpdateRejectionReason.MissingGeometry));
                            continue;
                        }

                        BoundingBox2D boundingBox2D = new(new Point2D(boundingBox3D.MinX, boundingBox3D.MinY), new Point2D(boundingBox3D.MaxX, boundingBox3D.MaxY));

                        Geometry.Planar.Interfaces.IPolygonal2D? polygonal2D = (Polygon2D?)boundingBox2D;

                        if (countyIds_Candidate.Count > 1 && !polygonal2Ds_Read)
                        {
                            polygonal2Ds_Read = true;

                            List<AdministrativeAreal2D>? administrativeAreal2Ds_Candidate = administrativeAreal2DPostgreSQLConverter is null
                                ? await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(npgsqlConnection, countyIds_Candidate, commandTimeout, cancellationToken)
                                : await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(countyIds_Candidate, commandTimeout, cancellationToken);

                            polygonal2Ds_ByCountyId = administrativeAreal2Ds_Candidate.Polygonal2DsByCountyId();
                        }

                        if (polygonal2Ds_ByCountyId is not null)
                        {
                            countyId = polygonal2Ds_ByCountyId.CountyId(polygonal2D, tolerance);
                        }
                        else
                        {
                            // The bounding box path answers a different candidate set per building, so there
                            // is nothing to cache across them.
                            List<AdministrativeAreal2D>? administrativeAreal2Ds = administrativeAreal2DPostgreSQLConverter is null
                                ? await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, Enums.AdministrativeArealType.County, tolerance, commandTimeout, cancellationToken)
                                : await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(boundingBox2D, Enums.AdministrativeArealType.County, tolerance, commandTimeout, cancellationToken);
                            if (administrativeAreal2Ds is not null && administrativeAreal2Ds.Count != 0)
                            {
                                // A single overlapping county is taken as it stands: the bounding box search
                                // already proved the overlap from the stored extents, so a row whose geometry
                                // cannot be deserialized must not cost the building its only candidate.
                                countyId = administrativeAreal2Ds.Count == 1 ? administrativeAreal2Ds[0].Id : administrativeAreal2Ds.CountyId(polygonal2D, tolerance);
                            }
                        }
                    }
                }

                if (countyId is null || !countyId.HasValue)
                {
                    // Resolution ran and named no part. The row is dropped, and naming it here is the only
                    // place that can - nothing downstream knows which rows never made the batch.
                    rejections.Add(new Rejection(building_Temp.Reference, Enums.UpdateRejectionReason.CountyUnresolved));
                    continue;
                }

                if (!dictionary_Buildings.TryGetValue(countyId.Value, out List<Building>? buildings_CountyId) || buildings_CountyId is null)
                {
                    buildings_CountyId = [];
                    dictionary_Buildings[countyId.Value] = buildings_CountyId;
                }

                buildings_CountyId.Add(building_Temp);
            }

            await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection);
            npgsqlBatch.Timeout = commandTimeout;

            // Filled alongside the batch commands rather than from dictionary_Buildings afterwards. The
            // identifiers come back in batch order, and a county whose partition could not be created
            // contributes no commands - reading the dictionary would shift every identifier after it onto
            // the wrong building.
            List<Building> buildings_Ordered = [];

            foreach (KeyValuePair<int, List<Building>> keyValuePair in dictionary_Buildings)
            {
                int countyId = keyValuePair.Key;

                succeeded = await Create.TableAsync_Building_Partition(npgsqlConnection, countyId, commandTimeout, cancellationToken);
                if (!succeeded)
                {
                    // A whole county's worth of rows disappears here, and it is the one drop that is never
                    // the caller's doing.
                    foreach (Building building_Rejected in keyValuePair.Value)
                    {
                        rejections.Add(new Rejection(building_Rejected.Reference, Enums.UpdateRejectionReason.PartitionUnavailable));
                    }

                    continue;
                }

                foreach (Building building_Temp in keyValuePair.Value)
                {
                    NpgsqlBatchCommand npgsqlBatchCommand;
                    if (building_Temp.Id > 0)
                    {
                        npgsqlBatchCommand = new($@"
                        INSERT INTO {TableName.Building} (id, county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object)
                        OVERRIDING SYSTEM VALUE
                        VALUES (@id, @county_id, @reference, @lod, @year, @min_x, @min_y, @min_z, @max_x, @max_y, @max_z, @object)
                        ON CONFLICT (id, county_id)
                        DO UPDATE SET
                            reference = EXCLUDED.reference,
                            lod = EXCLUDED.lod,
                            year = EXCLUDED.year,
                            min_x = EXCLUDED.min_x,
                            min_y = EXCLUDED.min_y,
                            min_z = EXCLUDED.min_z,
                            max_x = EXCLUDED.max_x,
                            max_y = EXCLUDED.max_y,
                            max_z = EXCLUDED.max_z,
                            object = EXCLUDED.object
                        RETURNING id;");

                        npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = building_Temp.Id });
                    }
                    else
                    {
                        npgsqlBatchCommand = new($@"
                        INSERT INTO {TableName.Building} (county_id, reference, lod, year, min_x, min_y, min_z, max_x, max_y, max_z, object)
                        VALUES (@county_id, @reference, @lod, @year, @min_x, @min_y, @min_z, @max_x, @max_y, @max_z, @object)
                        ON CONFLICT (county_id, reference, lod, year)
                        DO UPDATE SET
                            min_x = EXCLUDED.min_x,
                            min_y = EXCLUDED.min_y,
                            min_z = EXCLUDED.min_z,
                            max_x = EXCLUDED.max_x,
                            max_y = EXCLUDED.max_y,
                            max_z = EXCLUDED.max_z,
                            object = EXCLUDED.object
                        RETURNING id;");
                    }

                    BoundingBox3D? boundingBox3D = building_Temp.BoundingBox3D;

                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = building_Temp.Reference ?? (object)DBNull.Value });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("lod", NpgsqlDbType.Smallint) { Value = building_Temp.LOD is null ? DBNull.Value : (short)building_Temp.LOD.Value });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("year", NpgsqlDbType.Smallint) { Value = building_Temp.Year is null ? DBNull.Value : building_Temp.Year.Value });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox3D is null ? double.NaN : boundingBox3D.MinX });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox3D is null ? double.NaN : boundingBox3D.MinY });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_z", NpgsqlDbType.Double) { Value = boundingBox3D is null ? double.NaN : boundingBox3D.MinZ });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox3D is null ? double.NaN : boundingBox3D.MaxX });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox3D is null ? double.NaN : boundingBox3D.MaxY });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_z", NpgsqlDbType.Double) { Value = boundingBox3D is null ? double.NaN : boundingBox3D.MaxZ });

                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb)
                    {
                        Value = (object?)building_Temp.Object?.ToJsonString() ?? DBNull.Value
                    });

                    npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
                    buildings_Ordered.Add(building_Temp);
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

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync(cancellationToken);

            int buildingIndex = 0;

            do
            {
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    long id = npgsqlDataReader.GetInt64(0);
                    ids.Add(id);

                    if (buildingIndex < buildings_Ordered.Count)
                    {
                        buildings_Ordered[buildingIndex].Id = id;
                        buildingIndex++;
                    }
                }
            }
            while (await npgsqlDataReader.NextResultAsync(cancellationToken));

            return new PostgreSQLUpdateResult(ids, rejections);
        }

        private static Building Create_Building(NpgsqlDataReader npgsqlDataReader)
        {
            return new Building
            {
                Id = npgsqlDataReader.GetInt64(0),
                CountyId = npgsqlDataReader.IsDBNull(1) ? null : (int?)npgsqlDataReader.GetInt32(1),
                Reference = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2),
                LOD = npgsqlDataReader.IsDBNull(3) ? null : (CityGML.Enums.LOD?)npgsqlDataReader.GetInt16(3),
                Year = npgsqlDataReader.IsDBNull(4) ? null : (short?)npgsqlDataReader.GetInt16(4),
                BoundingBox3D = npgsqlDataReader.IsDBNull(5) || double.IsNaN(npgsqlDataReader.GetDouble(5)) ? null : new BoundingBox3D(
                    new Point3D(npgsqlDataReader.GetDouble(5), npgsqlDataReader.GetDouble(6), npgsqlDataReader.GetDouble(7)),
                    new Point3D(npgsqlDataReader.GetDouble(8), npgsqlDataReader.GetDouble(9), npgsqlDataReader.GetDouble(10))),
                Object = npgsqlDataReader.IsDBNull(11) ? null : JsonNode.Parse(npgsqlDataReader.GetString(11)) as JsonObject,
                CreatedAt = npgsqlDataReader.IsDBNull(12) ? null : (DateTime?)npgsqlDataReader.GetDateTime(12)
            };
        }

        private static async Task<List<Building>> ReadAsync_Building(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<Building> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_Building(npgsqlDataReader));
            }

            return result;
        }

        private static async Task<List<Building>?> ReadAsync_Building(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_Building(npgsqlDataReader, cancellationToken);
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

            return await npgsqlConnection.RefreshCountyIdsAsync(TableName.Building, references, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken);
        }
    }
}