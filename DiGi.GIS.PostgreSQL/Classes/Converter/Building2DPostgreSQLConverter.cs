using DiGi.Core;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
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
    /// Provides functionality for converting and managing <see cref="Building2D"/> entities within a PostgreSQL database, implementing the <see cref="IGISPostgreSQLConverter{T}"/> interface.
    /// <para><b><c>county_id</c> is a polygon part, not a county.</b> It points at an <c>administrative_areal_2d</c> row, and a county whose territory is disconnected has one such row per part - so the buildings of a single county can be spread over several <c>county_id</c> values, and querying one part never returns the whole county. See <see cref="AdministrativeAreal2DPostgreSQLConverter"/> for the storage model.</para>
    /// <para><b><c>reference</c> assures row uniqueness.</b> Each building reference uniquely identifies a <see cref="Building2D"/> record in the database. <c>county_id</c> can be used to scope queries to a specific partition for performance optimization (partition pruning), but is not required for uniqueness. Lookups keyed on reference alone or using fallback search by reference are safe and reliable.</para>
    /// </summary>
    public class Building2DPostgreSQLConverter : PostgreSQLConverter<Building2D>, IGISPostgreSQLConverter<Building2D>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings for the PostgreSQL database, or <see langword="null"/>.</param>
        public Building2DPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Asynchronously counts the number of 2D buildings for a specified county, with optional filtering by subdivision identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the PostgreSQL database.</param>
        /// <param name="countyId">The integer identifier of the county (Partition Key).</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a long integer, or -1 if the connection is null.</returns>
        public static async Task<long> CountAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection == null)
            {
                return -1;
            }

            // Check early if cancellation was already requested
            cancellationToken.ThrowIfCancellationRequested();

            bool hasSubdivisionIds = subdivisionIds != null && subdivisionIds.Any();

            // Build the base query using the mandatory county_id parameter
            string commandText = $@"
                SELECT COUNT(*)
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId";

            // Dynamically append the subdivision_id filter if the collection has elements
            if (hasSubdivisionIds)
            {
                commandText += " AND (subdivision_id = ANY(@subdivisionIds) OR subdivision_id IS NULL)";
            }

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });

            if (hasSubdivisionIds)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivisionIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                {
                    Value = subdivisionIds!.ToArray()
                });
            }

            object? executeResult = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);

            if (executeResult != null && executeResult != DBNull.Value)
            {
                return System.Convert.ToInt64(executeResult);
            }

            return 0;
        }

        /// <summary>
        /// Asynchronously retrieves a list of 2D building references for a specified county, with optional filtering by subdivision identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the PostgreSQL database.</param>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DReference" /> objects, or null if no references are found or an error occurs.</returns>
        public static async Task<List<Building2DReference>?> GetBuilding2DReferencesByCountyIdAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            int[]? subdivisionIds_Array = subdivisionIds?.ToArray();
            bool hasSubdivisionIds = subdivisionIds_Array != null && subdivisionIds_Array.Length > 0;

            string commandText = $@"
                SELECT id, county_id, reference, subdivision_id
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @county_id{(hasSubdivisionIds ? " AND (subdivision_id = ANY(@subdivision_ids) OR subdivision_id IS NULL)" : "")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
            if (hasSubdivisionIds)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivision_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = subdivisionIds_Array });
            }

            return await ReadAsync_Building2DReference(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the bounding-box centres of the buildings for a specified county, with optional filtering by subdivision identifiers.
        /// <para>Reads only the bounding-box columns (<c>min_x</c>, <c>min_y</c>, <c>max_x</c>, <c>max_y</c>) plus <c>reference</c> and <c>county_id</c> - the JSONB <c>object</c> column is never touched. Rows whose bounding box is NULL or NaN are skipped, exactly as <see cref="GetPoint2DsByReferencesAsync(NpgsqlConnection?, IEnumerable{string}?, int?, bool, int, CancellationToken)"/> does. Buildings filed under the county without a subdivision (<c>subdivision_id</c> NULL) are included whenever a subdivision filter is active, keeping the result set in parity with <see cref="GetBuilding2DReferencesByCountyIdAsync(NpgsqlConnection?, int, IEnumerable{int}?, int, CancellationToken)"/>.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the PostgreSQL database.</param>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DCentroid" /> objects, or null if the connection is null.</returns>
        public static async Task<List<Building2DCentroid>?> GetBuilding2DCentroidsByCountyIdAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            int[]? subdivisionIds_Array = subdivisionIds?.ToArray();
            bool hasSubdivisionIds = subdivisionIds_Array is not null && subdivisionIds_Array.Length > 0;

            string commandText = $@"
                SELECT min_x, min_y, max_x, max_y, reference, county_id
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @county_id{(hasSubdivisionIds ? " AND (subdivision_id = ANY(@subdivision_ids) OR subdivision_id IS NULL)" : "")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
            if (hasSubdivisionIds)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivision_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = subdivisionIds_Array });
            }

            List<Building2DCentroid> building2DCentroids = [];
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                Point2D? point2D = CentroidFromBoundingBox(
                    npgsqlDataReader.IsDBNull(0) ? null : npgsqlDataReader.GetDouble(0),
                    npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetDouble(1),
                    npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetDouble(2),
                    npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetDouble(3));
                if (point2D is null)
                {
                    continue;
                }

                building2DCentroids.Add(new Building2DCentroid
                {
                    Reference = npgsqlDataReader.IsDBNull(4) ? null : npgsqlDataReader.GetString(4),
                    CountyId = npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetInt32(5),
                    X = point2D.X,
                    Y = point2D.Y,
                });
            }

            return building2DCentroids;
        }

        /// <summary>
        /// Asynchronously retrieves the bounding-box centres of the buildings for a specified county. See <see cref="GetBuilding2DCentroidsByCountyIdAsync(NpgsqlConnection?, int, IEnumerable{int}?, int, CancellationToken)"/>.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DCentroid" /> objects, or null if the connection could not be established.</returns>
        public async Task<List<Building2DCentroid>?> GetBuilding2DCentroidsByCountyIdAsync(int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBuilding2DCentroidsByCountyIdAsync(npgsqlConnection, countyId, subdivisionIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the bounding-box centres of the buildings whose centre lies inside the given polygon, each keyed by its reference and county partition.
        /// <para>The geometry path of <see cref="GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync"/>, taken for a <see cref="AdministrativeArealType.Subdivision"/>: the county partitions come from <see cref="AdministrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(NpgsqlConnection?, BoundingBox2D?, double, int, CancellationToken)"/> (every part reaching the polygon's box, never one row's <c>county_id</c> - issue #64), the GiST box index narrows them to the rows overlapping that box, and <see cref="IsInside(PolygonalFace2D?, BoundingBox2D?, Point2D?, double)"/> keeps the ones whose centre the polygon contains. Nothing is filtered by <c>subdivision_id</c>. Only the bounding-box columns are read - the JSONB <c>object</c> column is never touched.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the PostgreSQL database.</param>
        /// <param name="polygonalFace2D">The boundary the building centres are kept inside.</param>
        /// <param name="tolerance">The distance tolerance of the box overlap and the containment test.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DCentroid" /> objects, empty when no partition reaches the polygon, or null if the connection or the polygon is null.</returns>
        public static async Task<List<Building2DCentroid>?> GetBuilding2DCentroidsByPolygonalFace2DAsync(NpgsqlConnection? npgsqlConnection, PolygonalFace2D? polygonalFace2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || polygonalFace2D is null)
            {
                return null;
            }

            BoundingBox2D? boundingBox2D = polygonalFace2D.GetBoundingBox();
            if (boundingBox2D is null)
            {
                return null;
            }

            HashSet<int>? countyIds = await AdministrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, tolerance, commandTimeout, cancellationToken);
            if (countyIds is null || countyIds.Count == 0)
            {
                return [];
            }

            const string commandText = $@"
                SELECT min_x, min_y, max_x, max_y, reference, county_id
                FROM {Constants.TableName.Building2D}
                WHERE county_id = ANY(@county_ids)
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@sMinX, @sMinY), point(@sMaxX, @sMaxY));";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddBoundingBox2DParameters(npgsqlCommand, countyIds, boundingBox2D, tolerance);

            List<Building2DCentroid> building2DCentroids = [];
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                Point2D? point2D = CentroidFromBoundingBox(
                    npgsqlDataReader.IsDBNull(0) ? null : npgsqlDataReader.GetDouble(0),
                    npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetDouble(1),
                    npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetDouble(2),
                    npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetDouble(3));
                if (!IsInside(polygonalFace2D, boundingBox2D, point2D, tolerance))
                {
                    continue;
                }

                building2DCentroids.Add(new Building2DCentroid
                {
                    Reference = npgsqlDataReader.IsDBNull(4) ? null : npgsqlDataReader.GetString(4),
                    CountyId = npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetInt32(5),
                    X = point2D!.X,
                    Y = point2D.Y,
                });
            }

            return building2DCentroids;
        }

        /// <summary>
        /// Asynchronously retrieves the references of the buildings whose bounding-box centre lies inside the given polygon.
        /// <para>The geometry path of <see cref="GetBuilding2DReferencesByAdministrativeAreal2DIdsAsync"/>, taken for a <see cref="AdministrativeArealType.Subdivision"/>; the resolution is that of <see cref="GetBuilding2DCentroidsByPolygonalFace2DAsync(NpgsqlConnection?, PolygonalFace2D?, double, int, CancellationToken)"/>, only the projection differs. Nothing is filtered by <c>subdivision_id</c> - the value each row carries is returned as stored, and for a nested subdivision layer it names a container other than the polygon asked for.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the PostgreSQL database.</param>
        /// <param name="polygonalFace2D">The boundary the building centres are kept inside.</param>
        /// <param name="tolerance">The distance tolerance of the box overlap and the containment test.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DReference" /> objects, empty when no partition reaches the polygon, or null if the connection or the polygon is null.</returns>
        public static async Task<List<Building2DReference>?> GetBuilding2DReferencesByPolygonalFace2DAsync(NpgsqlConnection? npgsqlConnection, PolygonalFace2D? polygonalFace2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || polygonalFace2D is null)
            {
                return null;
            }

            BoundingBox2D? boundingBox2D = polygonalFace2D.GetBoundingBox();
            if (boundingBox2D is null)
            {
                return null;
            }

            HashSet<int>? countyIds = await AdministrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, tolerance, commandTimeout, cancellationToken);
            if (countyIds is null || countyIds.Count == 0)
            {
                return [];
            }

            const string commandText = $@"
                SELECT id, county_id, reference, subdivision_id, min_x, min_y, max_x, max_y
                FROM {Constants.TableName.Building2D}
                WHERE county_id = ANY(@county_ids)
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@sMinX, @sMinY), point(@sMaxX, @sMaxY));";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            AddBoundingBox2DParameters(npgsqlCommand, countyIds, boundingBox2D, tolerance);

            List<Building2DReference> building2DReferences = [];
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                Point2D? point2D = CentroidFromBoundingBox(
                    npgsqlDataReader.IsDBNull(4) ? null : npgsqlDataReader.GetDouble(4),
                    npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetDouble(5),
                    npgsqlDataReader.IsDBNull(6) ? null : npgsqlDataReader.GetDouble(6),
                    npgsqlDataReader.IsDBNull(7) ? null : npgsqlDataReader.GetDouble(7));
                if (!IsInside(polygonalFace2D, boundingBox2D, point2D, tolerance))
                {
                    continue;
                }

                building2DReferences.Add(Create_Building2DReference(npgsqlDataReader));
            }

            return building2DReferences;
        }

        /// <summary>
        /// Decides whether a building centre lies inside an area polygon: a cheap bounding-box rejection first, then the polygon's own <see cref="PolygonalFace2D.Inside(Point2D?, double)"/>.
        /// <para>The single containment rule of the geometry path, shared by <see cref="GetBuilding2DCentroidsByPolygonalFace2DAsync(NpgsqlConnection?, PolygonalFace2D?, double, int, CancellationToken)"/> and <see cref="GetBuilding2DReferencesByPolygonalFace2DAsync(NpgsqlConnection?, PolygonalFace2D?, double, int, CancellationToken)"/>. A centre on the boundary, within the tolerance, is not inside - a building sitting on the line between two districts belongs to neither by this rule, which is the same answer for both. The bounding box is passed in rather than recomputed because the polygon's is the same for every row.</para>
        /// </summary>
        /// <param name="polygonalFace2D">The area polygon; null contains nothing.</param>
        /// <param name="boundingBox2D">The polygon's bounding box; null skips the rejection step.</param>
        /// <param name="point2D">The building centre; null is never inside.</param>
        /// <param name="tolerance">The distance tolerance of the containment test.</param>
        /// <returns>True when the centre lies strictly inside the polygon; otherwise, false.</returns>
        public static bool IsInside(PolygonalFace2D? polygonalFace2D, BoundingBox2D? boundingBox2D, Point2D? point2D, double tolerance = Core.Constants.Tolerance.MacroDistance)
        {
            if (polygonalFace2D is null || point2D is null)
            {
                return false;
            }

            if (boundingBox2D is not null && !boundingBox2D.InRange(point2D, tolerance))
            {
                return false;
            }

            return polygonalFace2D.Inside(point2D, tolerance);
        }

        /// <summary>
        /// Asynchronously retrieves a list of 2D buildings for a specified county, with optional filtering by subdivision identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the PostgreSQL database.</param>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D" /> objects, or null if no buildings are found or the connection is null.</returns>
        public static async Task<List<Building2D>?> GetBuilding2DsByCountyIdAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            int[]? subdivisionIds_Array = subdivisionIds?.ToArray();
            bool hasSubdivisionIds = subdivisionIds_Array != null && subdivisionIds_Array.Length > 0;

            string commandText = $@"
                SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @county_id{(hasSubdivisionIds ? " AND (subdivision_id = ANY(@subdivision_ids) OR subdivision_id IS NULL)" : "")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
            if (hasSubdivisionIds)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivision_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = subdivisionIds_Array });
            }

            return await ReadAsync_Building2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the total count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The optional integer identifier of the county used to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other methods as part of cooperating cancellation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a long integer.</returns>
        public static async Task<long> GetCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            string tableName = Constants.TableName.Building2D;
            if (countyId != null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, tableName, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated row count from the database, optionally filtered by a specific county identifier and with an optional statistics update.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The optional integer identifier of the county for which the count is estimated; if null, the estimate may be calculated across all counties.</param>
        /// <param name="analyze">A boolean value indicating whether to perform an ANALYZE operation on the table before retrieving the count to ensure statistics are current.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated row count as a nullable long integer, -1 when the partition exists but has not been analysed, or null when the table or partition does not exist or connection is null.</returns>
        public static async Task<long?> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, int? countyId, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string tableName = Constants.TableName.Building2D;
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
        public static async Task<Dictionary<int, long>?> GetEstimatedCountsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, bool analyze = false, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || countyIds is null)
            {
                return null;
            }

            Dictionary<string, int> countyIds_ByTableName = [];
            foreach (int countyId in countyIds)
            {
                countyIds_ByTableName[string.Format("{0}_{1}", Constants.TableName.Building2D, countyId)] = countyId;
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
        /// Asynchronously gets the estimated row count across specified counties in a PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> to use for the query.</param>
        /// <param name="countyIds">A collection of integers representing the county identifiers.</param>
        /// <param name="analyze">A boolean indicating whether to run an analysis before fetching the estimated count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated total row count as a long, or -1 when an error occurs or any named county has no partition or has never been analysed.</returns>
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
        /// Computes the centre of a bounding box: <c>((min_x + max_x) / 2, (min_y + max_y) / 2</c>.
        /// <para>The single implementation of the centroid arithmetic shared by <see cref="GetPoint2DsByReferencesAsync(NpgsqlConnection?, IEnumerable{string}?, int?, bool, int, CancellationToken)"/> and <see cref="GetBuilding2DCentroidsByCountyIdAsync(NpgsqlConnection?, int, IEnumerable{int}?, int, CancellationToken)"/>. A component that is missing or NaN means the row holds no usable bounding box, so no centre is returned for it.</para>
        /// </summary>
        /// <param name="minX">The minimum X of the bounding box, or null when the column is NULL.</param>
        /// <param name="minY">The minimum Y of the bounding box, or null when the column is NULL.</param>
        /// <param name="maxX">The maximum X of the bounding box, or null when the column is NULL.</param>
        /// <param name="maxY">The maximum Y of the bounding box, or null when the column is NULL.</param>
        /// <returns>The centre of the box, or null when any component is missing or NaN.</returns>
        public static Point2D? CentroidFromBoundingBox(double? minX, double? minY, double? maxX, double? maxY)
        {
            if (minX is null || minY is null || maxX is null || maxY is null
                || double.IsNaN(minX.Value) || double.IsNaN(minY.Value) || double.IsNaN(maxX.Value) || double.IsNaN(maxY.Value))
            {
                return null;
            }

            return new Point2D((minX.Value + maxX.Value) / 2, (minY.Value + maxY.Value) / 2);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Point2D"/> objects associated with the specified references and an optional county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to connect to the database.</param>
        /// <param name="references">An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the references to query.</param>
        /// <param name="countyId">An optional <see cref="int"/> representing the county identifier for filtering results.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone for any references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{T}"/> of <see cref="Point2D"/> objects if matches are found; otherwise, null.</returns>
        public static async Task<List<Point2D>?> GetPoint2DsByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            string commandText = $@"
                    SELECT min_x, min_y, max_x, max_y, reference
                    FROM {Constants.TableName.Building2D}
                    WHERE reference = ANY(@references){(countyId is null ? "" : " AND county_id = @countyId")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("references", references_Array);
            if (countyId is not null)
            {
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId.Value);
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            List<Point2D> result = [];
            HashSet<string> foundReferences = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                Point2D? point2D = CentroidFromBoundingBox(
                    npgsqlDataReader.IsDBNull(0) ? null : npgsqlDataReader.GetDouble(0),
                    npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetDouble(1),
                    npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetDouble(2),
                    npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetDouble(3));
                if (point2D is null)
                {
                    continue;
                }

                if (!npgsqlDataReader.IsDBNull(4))
                {
                    foundReferences.Add(npgsqlDataReader.GetString(4));
                }

                result.Add(point2D);
            }

            if (fallbackByReference && countyId is not null)
            {
                string[] missingReferences = [.. references_Array.Where(r => !foundReferences.Contains(r))];
                if (missingReferences.Length > 0)
                {
                    List<Point2D>? fallbackPoints = await GetPoint2DsByReferencesAsync(npgsqlConnection, missingReferences, null, false, commandTimeout, cancellationToken);
                    if (fallbackPoints is not null && fallbackPoints.Count > 0)
                    {
                        result.AddRange(fallbackPoints);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves full building data based on a collection of references using optimized partition-pruned batching.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="building2DReferences">The collection of <see cref="Building2DReference"/> objects used to identify and retrieve the corresponding buildings from the database.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform an additional fallback query matching by reference only (without county identifier condition) for any references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects matching the provided references, an empty list if none match, or null if the input collection was null or the connection is null.</returns>
        public static async Task<List<Building2D>?> GetBuilding2DsByBuilding2DReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            const int batchSize = 1000;
            List<Building2D> result = [];
            HashSet<long> existingIds = [];

            // 1. Group references with a known county ID for partition pruning
            IEnumerable<IGrouping<int, Building2DReference>> countyGroupings = building2DReferences_List
                .Where(r => r?.CountyId != null && !string.IsNullOrWhiteSpace(r.Reference))
                .GroupBy(r => r.CountyId!.Value);

            foreach (IGrouping<int, Building2DReference> countyGroup in countyGroupings)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int countyId = countyGroup.Key;
                string[] uniqueRefsInCounty = [.. countyGroup.Select(r => r.Reference!).Distinct()];

                for (int i = 0; i < uniqueRefsInCounty.Length; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string[] chunk = uniqueRefsInCounty.Skip(i).Take(batchSize).ToArray();

                    const string commandText = $@"
                        SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                        FROM {Constants.TableName.Building2D}
                        WHERE county_id = @countyId AND reference = ANY(@references);";

                    await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = chunk });

                    List<Building2D>? chunkResults = await ReadAsync_Building2D(npgsqlCommand, cancellationToken);
                    if (chunkResults != null)
                    {
                        foreach (Building2D item in chunkResults)
                        {
                            if (existingIds.Add(item.Id))
                            {
                                result.Add(item);
                            }
                        }
                    }
                }
            }

            // 2. References with no CountyId specified
            string[] unassignedRefs = [.. building2DReferences_List
                .Where(r => r?.CountyId == null && !string.IsNullOrWhiteSpace(r?.Reference))
                .Select(r => r!.Reference!)
                .Distinct()];

            if (unassignedRefs.Length > 0)
            {
                for (int i = 0; i < unassignedRefs.Length; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string[] chunk = unassignedRefs.Skip(i).Take(batchSize).ToArray();

                    const string commandText = $@"
                        SELECT DISTINCT ON (reference)
                               id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                        FROM {Constants.TableName.Building2D}
                        WHERE reference = ANY(@references)
                        ORDER BY reference, id ASC;";

                    await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = chunk });

                    List<Building2D>? chunkResults = await ReadAsync_Building2D(npgsqlCommand, cancellationToken);
                    if (chunkResults != null)
                    {
                        foreach (Building2D item in chunkResults)
                        {
                            if (existingIds.Add(item.Id))
                            {
                                result.Add(item);
                            }
                        }
                    }
                }
            }

            // 3. Fallback for missing references if requested
            if (fallbackByReference)
            {
                HashSet<string> foundCountyReferencePairs = [.. result.Where(b => !string.IsNullOrWhiteSpace(b.Reference) && b.CountyId.HasValue).Select(b => $"{b.CountyId!.Value}_{b.Reference}")];
                HashSet<string> foundReferences = [.. result.Where(b => !string.IsNullOrWhiteSpace(b.Reference)).Select(b => b.Reference!)];

                HashSet<string> missingReferences = [];
                foreach (Building2DReference building2DReference in building2DReferences_List)
                {
                    if (string.IsNullOrWhiteSpace(building2DReference?.Reference))
                    {
                        continue;
                    }

                    bool found = building2DReference.CountyId.HasValue
                        ? foundCountyReferencePairs.Contains($"{building2DReference.CountyId.Value}_{building2DReference.Reference}")
                        : foundReferences.Contains(building2DReference.Reference);

                    if (!found)
                    {
                        missingReferences.Add(building2DReference.Reference);
                    }
                }

                if (missingReferences.Count > 0)
                {
                    string[] missingRefs_Array = [.. missingReferences];
                    for (int i = 0; i < missingRefs_Array.Length; i += batchSize)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string[] chunk = missingRefs_Array.Skip(i).Take(batchSize).ToArray();

                        const string fallbackCommandText = $@"
                            SELECT DISTINCT ON (reference)
                                   id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                            FROM {Constants.TableName.Building2D}
                            WHERE reference = ANY(@missingRefs)
                            ORDER BY reference, id ASC;";

                        await using NpgsqlCommand fallbackCommand = new(fallbackCommandText, npgsqlConnection);
                        fallbackCommand.CommandTimeout = commandTimeout;
                        fallbackCommand.Parameters.Add(new NpgsqlParameter("missingRefs", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = chunk });

                        List<Building2D>? fallbackResults = await ReadAsync_Building2D(fallbackCommand, cancellationToken);
                        if (fallbackResults != null)
                        {
                            foreach (Building2D item in fallbackResults)
                            {
                                if (existingIds.Add(item.Id))
                                {
                                    result.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Asynchronously clears all data from the specified table and restarts its identity sequence.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result is a <see cref="bool"/> indicating true if the operation succeeded; otherwise, false.</returns>
        public async Task<bool> ClearAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await DiGi.PostgreSQL.Modify.ClearAsync(npgsqlConnection, Constants.TableName.Building2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the number of 2D buildings for a specified county, with optional filtering by subdivision identifiers.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county (Partition Key).</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a long integer, or -1 if the connection is null.</returns>
        public async Task<long> CountAsync(int countyId, IEnumerable<int>? subdivisionIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await CountAsync(npgsqlConnection, countyId, subdivisionIds, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the number of 2D buildings for a specified administrative areal 2D identifiers.
        /// </summary>
        /// <param name="administrativeAreal2DIds">A collection of integers representing the administrative areal 2D identifiers to filter by.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>Number of buidlings in the specified administrative areas.</returns>
        public async Task<long> CountAsync(IEnumerable<int> administrativeAreal2DIds, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, administrativeAreal2DIds, cancellationToken: cancellationToken);
            if (administrativeAreal2DReferences is null)
            {
                return -1;
            }

            administrativeAreal2DReferences.Filter(x => x?.AdministrativeArealType == AdministrativeArealType.Subdivision, out administrativeAreal2DReferences, out List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Out);

            administrativeAreal2DReferences ??= [];

            if (administrativeAreal2DReferences_Out is not null && administrativeAreal2DReferences_Out.Count != 0)
            {
                foreach (AdministrativeAreal2DReference administrativeAreal2DReference_Out in administrativeAreal2DReferences_Out)
                {
                    List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Temp = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, AdministrativeArealType.Subdivision, administrativeAreal2DReference_Out.Id, false, cancellationToken: cancellationToken);
                    if (administrativeAreal2DReferences_Temp is not null)
                    {
                        administrativeAreal2DReferences.AddRange(administrativeAreal2DReferences_Temp);
                    }
                }
            }

            if (administrativeAreal2DReferences is null || administrativeAreal2DReferences.Count == 0)
            {
                return 0;
            }

            long result = 0;
            while (administrativeAreal2DReferences is not null && administrativeAreal2DReferences.Count > 0)
            {
                int? countyId = administrativeAreal2DReferences[0]?.CountyId;

                administrativeAreal2DReferences.Filter(x => x?.CountyId == countyId, out List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_CountyId, out administrativeAreal2DReferences);

                if (administrativeAreal2DReferences_CountyId is null || administrativeAreal2DReferences_CountyId.Count == 0)
                {
                    break;
                }

                if (countyId is null || !countyId.HasValue)
                {
                    continue;
                }

                long count = await CountAsync(npgsqlConnection, countyId.Value, administrativeAreal2DReferences_CountyId.ConvertAll(x => x.Id).Distinct(), commandTimeout, cancellationToken);
                if (count > 0)
                {
                    result += count;
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously creates the table in the database.
        /// </summary>
        /// <param name="commandTimeout">The time interval, in seconds, to wait for the command to complete before timing out. The default value is 30 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the table was successfully created.</returns>
        public async Task<bool> CreateTableAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool result = await Create.TableAsync_Building2D(npgsqlConnection, commandTimeout, cancellationToken);
            if (result)
            {
                await DiGi.PostgreSQL.Modify.Analyze(npgsqlConnection, Constants.TableName.Building2D, commandTimeout);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="Building2D"/> instance by its unique identifier and an optional county identifier.
        /// </summary>
        /// <param name="id">The long unique identifier of the building to retrieve.</param>
        /// <param name="countyId">The optional integer identifier of the county used to scope the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Building2D"/> instance if found; otherwise, null.</returns>
        public async Task<Building2D?> GetBuilding2DByIdAsync(long id, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string commandText = $@"
                    SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                    FROM {Constants.TableName.Building2D}
                    WHERE id = @id{(countyId is null ? "" : " AND county_id = @countyId")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("id", id);
            if (countyId is not null)
            {
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId.Value);
            }

            List<Building2D>? results = await ReadAsync_Building2D(npgsqlCommand, cancellationToken);

            return results?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="Building2D"/> located at or near the specified 2D point within a given tolerance.
        /// <para>The subdivisions covering the point are tried first, then every county polygon part the point can be filed under - not only the part whose polygon covers it. A county whose territory is disconnected is stored as one row per part and its buildings can sit under a sibling part, so resolving the covering part alone answers nothing at all in those counties.</para>
        /// </summary>
        /// <param name="point2D">The <see cref="Point2D"/> coordinate to search for. This value can be null.</param>
        /// <param name="tolerance">The <see cref="double"/> distance tolerance used to determine if a building is associated with the given point. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Building2D"/> found at the specified location, or null if no building is found within the tolerance or if the provided point is null.</returns>
        public async Task<Building2D?> GetBuilding2DByPoint2DAsync(Point2D? point2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            BoundingBox2D boundingBox2D = new(point2D, point2D);

            // 1. First attempt: search within matching subdivisions
            List<AdministrativeAreal2D>? administrativeAreal2Ds = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, AdministrativeArealType.Subdivision, tolerance, commandTimeout, cancellationToken);

            Building2D? building2D = await FindBuilding2DBySubdivisionsAsync(administrativeAreal2Ds);
            if (building2D is not null)
            {
                return building2D;
            }

            // 2. Fallback attempt: read every county partition the point can be filed under. Not the part
            // whose polygon covers the point - a county whose territory is disconnected is stored as one row
            // per polygon part, and the buildings can sit under a sibling part, in which case the covering
            // part reads back as no building at all. See
            // https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/64.
            HashSet<int>? countyIds = await AdministrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, tolerance, commandTimeout, cancellationToken);

            if (countyIds is null || countyIds.Count == 0)
            {
                return null;
            }

            foreach (int countyId in countyIds.OrderBy(x => x))
            {
                building2D = await FindBuilding2DAsync(countyId, null);
                if (building2D is not null)
                {
                    return building2D;
                }
            }

            return null;

            async Task<Building2D?> FindBuilding2DBySubdivisionsAsync(List<AdministrativeAreal2D>? administrativeAreal2Ds_Temp)
            {
                if (administrativeAreal2Ds_Temp is null || administrativeAreal2Ds_Temp.Count == 0)
                {
                    return null;
                }

                foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds_Temp)
                {
                    if (administrativeAreal2D is null)
                    {
                        continue;
                    }

                    int? countyId_Parent = administrativeAreal2D.AdministrativeArealType == AdministrativeArealType.County
                        ? administrativeAreal2D.Id
                        : administrativeAreal2D.CountyId;

                    if (countyId_Parent is null)
                    {
                        continue;
                    }

                    int? subdivisionId = administrativeAreal2D.AdministrativeArealType == AdministrativeArealType.Subdivision
                        ? administrativeAreal2D.Id
                        : null;

                    Building2D? building2D_Temp = await FindBuilding2DAsync(countyId_Parent.Value, subdivisionId);
                    if (building2D_Temp is not null)
                    {
                        return building2D_Temp;
                    }
                }

                return null;
            }

            async Task<Building2D?> FindBuilding2DAsync(int countyId, int? subdivisionId)
            {
                // Represent the point as a tolerance-sized search box so the GiST index on
                // box(point(min_x, min_y), point(max_x, max_y)) can serve the '&&' overlap operator.
                double searchMinX = point2D.X - tolerance;
                double searchMinY = point2D.Y - tolerance;
                double searchMaxX = point2D.X + tolerance;
                double searchMaxY = point2D.Y + tolerance;

                string commandText = $@"
                    SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                    FROM {Constants.TableName.Building2D}
                    WHERE county_id = @countyId
                        {(subdivisionId.HasValue ? "AND (subdivision_id = @subdivisionId OR subdivision_id IS NULL)" : "")}
                        AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY))
                    ORDER BY id ASC;";

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
                if (subdivisionId.HasValue)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivisionId", NpgsqlDbType.Integer) { Value = subdivisionId.Value });
                }
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });

                List<Building2D>? results = await ReadAsync_Building2D(npgsqlCommand, cancellationToken);
                if (results is null || results.Count == 0)
                {
                    return null;
                }

                foreach (Building2D building2D_Candidate in results)
                {
                    if (building2D_Candidate.ToDiGi()?.PolygonalFace2D is PolygonalFace2D polygonalFace2D && polygonalFace2D.InRange(point2D, tolerance))
                    {
                        return building2D_Candidate;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="Building2D"/> instance based on the specified reference string and an optional county identifier.
        /// </summary>
        /// <param name="reference">The <see cref="string"/> reference used to identify the building.</param>
        /// <param name="countyId">The optional nullable integer identifier for the county.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone if the building is not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Building2D"/> instance if found; otherwise, null.</returns>
        public async Task<Building2D?> GetBuilding2DByReferenceAsync(string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string commandText = $@"
                    SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                    FROM {Constants.TableName.Building2D}
                    WHERE reference = @reference{(countyId is null ? "" : " AND county_id = @countyId")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("reference", reference);
            if (countyId is not null)
            {
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId.Value);
            }

            List<Building2D>? results = await ReadAsync_Building2D(npgsqlCommand, cancellationToken);

            Building2D? building2D = results?.FirstOrDefault();
            if (building2D is null && countyId is not null && fallbackByReference)
            {
                return await GetBuilding2DByReferenceAsync(reference, null, false, commandTimeout, cancellationToken);
            }

            return building2D;
        }

        /// <summary>
        /// Asynchronously retrieves a building 2D reference by its unique identifier and an optional county identifier.
        /// </summary>
        /// <param name="id">The long integer representing the unique identifier of the building.</param>
        /// <param name="countyId">An optional integer representing the county identifier used to filter the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Building2DReference"/> if a match is found; otherwise, null.</returns>
        public async Task<Building2DReference?> GetBuilding2DReferenceByIdAsync(long id, int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string commandText = $@"
                    SELECT id, county_id, reference, subdivision_id
                    FROM {Constants.TableName.Building2D}
                    WHERE id = @id{(countyId is null ? "" : " AND county_id = @countyId")};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("id", id);
            if (countyId is not null)
            {
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId.Value);
            }

            List<Building2DReference>? results = await ReadAsync_Building2DReference(npgsqlCommand, cancellationToken);

            return results?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves a building 2D reference based on the specified reference string and an optional county identifier.
        /// </summary>
        /// <param name="reference">The unique reference string of the building to retrieve.</param>
        /// <param name="countyId">The optional integer identifier of the county used to filter the search.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone if the reference is not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="T:Building2DReference" /> if a match is found; otherwise, null.</returns>
        public async Task<Building2DReference?> GetBuilding2DReferenceByReferenceAsync(string reference, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            string commandText = $@"
                    SELECT id, county_id, reference, subdivision_id
                    FROM {Constants.TableName.Building2D}
                    WHERE reference = @reference{(countyId is null ? "" : " AND county_id = @countyId")}
                    ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("reference", reference);
            if (countyId is not null)
            {
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId.Value);
            }

            List<Building2DReference>? results = await ReadAsync_Building2DReference(npgsqlCommand, cancellationToken);

            Building2DReference? building2DReference = results?.FirstOrDefault();
            if (building2DReference is null && countyId is not null && fallbackByReference)
            {
                return await GetBuilding2DReferenceByReferenceAsync(reference, null, false, commandTimeout, cancellationToken);
            }

            return building2DReference;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building2D"/> objects for a specified county identifier, with optional filtering by subdivision and excluded references.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county used to filter the search.</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision.</param>
        /// <param name="excludedReferences">Optional collection of references to be excluded from the result.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects if matches are found; otherwise, null.</returns>
        public async Task<List<Building2D>?> GetBuilding2DsByCountyIdAsync(int countyId, int? subdivisionId = null, IEnumerable<string>? excludedReferences = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string commandText = $@"
                SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId";

            bool hasSubdivisionId = subdivisionId.HasValue;
            if (hasSubdivisionId)
            {
                commandText += " AND subdivision_id = @subdivisionId";
            }

            bool hasExcluded = excludedReferences != null && excludedReferences.Any();

            if (hasExcluded)
            {
                commandText += " AND NOT (reference = ANY(@excluded))";
            }

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("countyId", countyId);

            if (hasSubdivisionId)
            {
                npgsqlCommand.Parameters.AddWithValue("subdivisionId", subdivisionId!.Value);
            }

            if (hasExcluded)
            {
                string[] excludedArray = [.. excludedReferences!];
                npgsqlCommand.Parameters.AddWithValue("excluded", excludedArray);
            }

            return await ReadAsync_Building2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building2D"/> objects for a specified county identifier, with optional filtering by subdivision identifiers.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county used to filter the search.</param>
        /// <param name="subdivisionIds">An optional collection of integers representing the subdivision identifiers to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects if matches are found; otherwise, null.</returns>
        public async Task<List<Building2D>?> GetBuilding2DsByCountyIdAsync(int countyId, IEnumerable<int>? subdivisionIds, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBuilding2DsByCountyIdAsync(npgsqlConnection, countyId, subdivisionIds, commandTimeout, cancellationToken);
        }


        /// <summary>
        /// Retrieves full Building2DReference data from building_2d table based on input references.
        /// Performs batch processing using partition-pruned chunked queries.
        /// </summary>
        /// <param name="building2DReferences">Collection of partial references (must have Reference, optional CountyId).</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform an additional fallback query matching by reference only (without county identifier condition) for any references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A list of populated Building2DReference objects found in the database.</returns>
        public async Task<List<Building2DReference>?> GetBuilding2DReferencesAsync(IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DReferences == null)
            {
                return null;
            }

            List<Building2DReference> building2DReferences_List = [.. building2DReferences];
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

            const int batchSize = 1000;
            List<Building2DReference> result = [];
            HashSet<long> existingIds = [];

            // 1. Group references with a known county ID for partition pruning
            IEnumerable<IGrouping<int, Building2DReference>> countyGroupings = building2DReferences_List
                .Where(r => r?.CountyId != null && !string.IsNullOrWhiteSpace(r.Reference))
                .GroupBy(r => r.CountyId!.Value);

            foreach (IGrouping<int, Building2DReference> countyGroup in countyGroupings)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int countyId = countyGroup.Key;
                string[] uniqueRefsInCounty = [.. countyGroup.Select(r => r.Reference!).Distinct()];

                for (int i = 0; i < uniqueRefsInCounty.Length; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string[] chunk = uniqueRefsInCounty.Skip(i).Take(batchSize).ToArray();

                    const string commandText = $@"
                        SELECT id, county_id, reference, subdivision_id
                        FROM {Constants.TableName.Building2D}
                        WHERE county_id = @countyId AND reference = ANY(@references);";

                    await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = chunk });

                    List<Building2DReference>? chunkResults = await ReadAsync_Building2DReference(npgsqlCommand, cancellationToken);
                    if (chunkResults != null)
                    {
                        foreach (Building2DReference item in chunkResults)
                        {
                            if (existingIds.Add(item.Id))
                            {
                                result.Add(item);
                            }
                        }
                    }
                }
            }

            // 2. References with no CountyId specified
            string[] unassignedRefs = [.. building2DReferences_List
                .Where(r => r?.CountyId == null && !string.IsNullOrWhiteSpace(r?.Reference))
                .Select(r => r!.Reference!)
                .Distinct()];

            if (unassignedRefs.Length > 0)
            {
                for (int i = 0; i < unassignedRefs.Length; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string[] chunk = unassignedRefs.Skip(i).Take(batchSize).ToArray();

                    const string commandText = $@"
                        SELECT DISTINCT ON (reference)
                               id, county_id, reference, subdivision_id
                        FROM {Constants.TableName.Building2D}
                        WHERE reference = ANY(@references)
                        ORDER BY reference, id ASC;";

                    await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = chunk });

                    List<Building2DReference>? chunkResults = await ReadAsync_Building2DReference(npgsqlCommand, cancellationToken);
                    if (chunkResults != null)
                    {
                        foreach (Building2DReference item in chunkResults)
                        {
                            if (existingIds.Add(item.Id))
                            {
                                result.Add(item);
                            }
                        }
                    }
                }
            }

            // 3. Fallback for missing references if requested
            if (fallbackByReference)
            {
                HashSet<string> foundCountyReferencePairs = [.. result.Where(b => !string.IsNullOrWhiteSpace(b.Reference) && b.CountyId.HasValue).Select(b => $"{b.CountyId!.Value}_{b.Reference}")];
                HashSet<string> foundReferences = [.. result.Where(b => !string.IsNullOrWhiteSpace(b.Reference)).Select(b => b.Reference!)];

                HashSet<string> missingReferences = [];
                foreach (Building2DReference building2DReference in building2DReferences_List)
                {
                    if (string.IsNullOrWhiteSpace(building2DReference?.Reference))
                    {
                        continue;
                    }

                    bool found = building2DReference.CountyId.HasValue
                        ? foundCountyReferencePairs.Contains($"{building2DReference.CountyId.Value}_{building2DReference.Reference}")
                        : foundReferences.Contains(building2DReference.Reference);

                    if (!found)
                    {
                        missingReferences.Add(building2DReference.Reference);
                    }
                }

                if (missingReferences.Count > 0)
                {
                    string[] missingRefs_Array = [.. missingReferences];
                    for (int i = 0; i < missingRefs_Array.Length; i += batchSize)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string[] chunk = missingRefs_Array.Skip(i).Take(batchSize).ToArray();

                        const string fallbackCommandText = $@"
                            SELECT DISTINCT ON (reference)
                                   id, county_id, reference, subdivision_id
                            FROM {Constants.TableName.Building2D}
                            WHERE reference = ANY(@missingRefs)
                            ORDER BY reference, id ASC;";

                        await using NpgsqlCommand fallbackCommand = new(fallbackCommandText, npgsqlConnection);
                        fallbackCommand.CommandTimeout = commandTimeout;
                        fallbackCommand.Parameters.Add(new NpgsqlParameter("missingRefs", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = chunk });

                        List<Building2DReference>? fallbackResults = await ReadAsync_Building2DReference(fallbackCommand, cancellationToken);
                        if (fallbackResults != null)
                        {
                            foreach (Building2DReference item in fallbackResults)
                            {
                                if (existingIds.Add(item.Id))
                                {
                                    result.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of building 2D references associated with the specified administrative areal 2D identifiers.
        /// <para>An identifier above the subdivision level resolves through <b>Subdivision children</b>, not geometry: it is expanded to its <see cref="AdministrativeArealType.Subdivision"/> descendants and the buildings are then fetched per <c>county_id</c> plus <c>subdivision_id</c>. An identifier with no subdivisions therefore yields an empty list, which does <b>not</b> mean the area holds no buildings - compare with <c>GetBuilding2DReferencesByCountyIdAsync</c> before concluding anything about coverage.</para>
        /// <para>A <see cref="AdministrativeArealType.Subdivision"/> identifier resolves by <b>geometry</b> instead - the buildings whose bounding-box centre lies inside its polygon, see <see cref="GetBuilding2DReferencesByPolygonalFace2DAsync(NpgsqlConnection?, PolygonalFace2D?, double, int, CancellationToken)"/>. The subdivision layer is nested (a city, its districts and their neighbourhoods are all subdivisions of one municipality) and <c>subdivision_id</c> names a single container per building, chosen by the lowest identifier among equal overlaps - so membership can never answer which buildings a district holds. See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/75.</para>
        /// </summary>
        /// <param name="administrativeAreal2DIds">A collection of integers representing the administrative areal 2D identifiers to filter by.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DReference"/> objects, or null if no references are found or an error occurs.</returns>
        public async Task<List<Building2DReference>?> GetBuilding2DReferencesByAdministrativeAreal2DIdsAsync(IEnumerable<int> administrativeAreal2DIds, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, administrativeAreal2DIds, cancellationToken: cancellationToken);
            if (administrativeAreal2DReferences is null)
            {
                return null;
            }

            Dictionary<int, List<int>>? subdivisionIdsByCountyId = await ResolveSubdivisionIdsByCountyIdAsync(npgsqlConnection, administrativeAreal2DReferences, cancellationToken);
            if (subdivisionIdsByCountyId is null)
            {
                return null;
            }

            Dictionary<long, Building2DReference> dictionary = [];
            foreach (KeyValuePair<int, List<int>> pair in subdivisionIdsByCountyId)
            {
                List<Building2DReference>? building2DReferences = await GetBuilding2DReferencesByCountyIdAsync(npgsqlConnection, pair.Key, pair.Value, commandTimeout, cancellationToken);
                if (building2DReferences is not null)
                {
                    foreach (Building2DReference building2DReference in building2DReferences)
                    {
                        dictionary[building2DReference.Id] = building2DReference;
                    }
                }
            }

            List<PolygonalFace2D>? polygonalFace2Ds = await ResolveSubdivisionPolygonalFace2DsAsync(npgsqlConnection, administrativeAreal2DReferences, cancellationToken);
            if (polygonalFace2Ds is null)
            {
                return null;
            }

            foreach (PolygonalFace2D polygonalFace2D in polygonalFace2Ds)
            {
                List<Building2DReference>? building2DReferences = await GetBuilding2DReferencesByPolygonalFace2DAsync(npgsqlConnection, polygonalFace2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (building2DReferences is not null)
                {
                    foreach (Building2DReference building2DReference in building2DReferences)
                    {
                        dictionary[building2DReference.Id] = building2DReference;
                    }
                }
            }

            return [.. dictionary.Values];
        }

        /// <summary>
        /// Asynchronously retrieves the bounding-box centres of the buildings associated with the specified administrative areal 2D identifiers, each keyed by its reference and county partition.
        /// <para>Resolution is identical to <see cref="GetBuilding2DReferencesByAdministrativeAreal2DIdsAsync"/>: an identifier above the subdivision level goes through <b>Subdivision children</b>, not geometry, so one with no subdivisions yields an empty list, which does <b>not</b> mean the area holds no buildings; a <see cref="AdministrativeArealType.Subdivision"/> identifier goes through its <b>polygon</b> instead, because the subdivision layer is nested and <c>subdivision_id</c> membership cannot name the buildings of a district (issue #75). Only the bounding-box columns are read on either path - the JSONB <c>object</c> column is never touched - which makes this the fast path for 2D dot rendering of a whole area.</para>
        /// </summary>
        /// <param name="administrativeAreal2DIds">A collection of integers representing the administrative areal 2D identifiers to filter by.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DCentroid"/> objects, or null if the connection or the area lookup fails.</returns>
        public async Task<List<Building2DCentroid>?> GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync(IEnumerable<int> administrativeAreal2DIds, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, administrativeAreal2DIds, cancellationToken: cancellationToken);
            if (administrativeAreal2DReferences is null)
            {
                return null;
            }

            Dictionary<int, List<int>>? subdivisionIdsByCountyId = await ResolveSubdivisionIdsByCountyIdAsync(npgsqlConnection, administrativeAreal2DReferences, cancellationToken);
            if (subdivisionIdsByCountyId is null)
            {
                return null;
            }

            // Keyed on county part + reference: a reference is unique only within its part, and the two paths can
            // both reach the same row when a subdivision and its parent are requested together.
            Dictionary<string, Building2DCentroid> dictionary = [];
            foreach (KeyValuePair<int, List<int>> pair in subdivisionIdsByCountyId)
            {
                List<Building2DCentroid>? building2DCentroids = await GetBuilding2DCentroidsByCountyIdAsync(npgsqlConnection, pair.Key, pair.Value, commandTimeout, cancellationToken);
                if (building2DCentroids is not null)
                {
                    foreach (Building2DCentroid building2DCentroid in building2DCentroids)
                    {
                        dictionary[$"{building2DCentroid.CountyId}_{building2DCentroid.Reference}"] = building2DCentroid;
                    }
                }
            }

            List<PolygonalFace2D>? polygonalFace2Ds = await ResolveSubdivisionPolygonalFace2DsAsync(npgsqlConnection, administrativeAreal2DReferences, cancellationToken);
            if (polygonalFace2Ds is null)
            {
                return null;
            }

            foreach (PolygonalFace2D polygonalFace2D in polygonalFace2Ds)
            {
                List<Building2DCentroid>? building2DCentroids = await GetBuilding2DCentroidsByPolygonalFace2DAsync(npgsqlConnection, polygonalFace2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (building2DCentroids is not null)
                {
                    foreach (Building2DCentroid building2DCentroid in building2DCentroids)
                    {
                        dictionary[$"{building2DCentroid.CountyId}_{building2DCentroid.Reference}"] = building2DCentroid;
                    }
                }
            }

            return [.. dictionary.Values];
        }

        /// <summary>
        /// Resolves the administrative areal 2D references above the subdivision level to the <see cref="AdministrativeArealType.Subdivision"/> children that hold their buildings, grouped by the county partition that owns them.
        /// <para>A reference that is itself a subdivision is <b>not</b> resolved here - its buildings come from its polygon, see <see cref="ResolveSubdivisionPolygonalFace2DsAsync"/>. Any other reference is expanded to its subdivision descendants; one with no subdivisions contributes nothing, which does <b>not</b> mean the area holds no buildings.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="administrativeAreal2DReferences">The administrative areal 2D references to resolve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary of county identifier to distinct subdivision identifiers, an empty dictionary when no subdivision resolves, or null when the connection is missing.</returns>
        private static async Task<Dictionary<int, List<int>>?> ResolveSubdivisionIdsByCountyIdAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<AdministrativeAreal2DReference?> administrativeAreal2DReferences, CancellationToken cancellationToken)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            List<AdministrativeAreal2DReference> administrativeAreal2DReferences_Subdivision = [];
            foreach (AdministrativeAreal2DReference? administrativeAreal2DReference in administrativeAreal2DReferences)
            {
                if (administrativeAreal2DReference is null || administrativeAreal2DReference.AdministrativeArealType == AdministrativeArealType.Subdivision)
                {
                    continue;
                }

                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Temp = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, AdministrativeArealType.Subdivision, administrativeAreal2DReference.Id, false, cancellationToken: cancellationToken);
                if (administrativeAreal2DReferences_Temp is not null)
                {
                    administrativeAreal2DReferences_Subdivision.AddRange(administrativeAreal2DReferences_Temp);
                }
            }

            Dictionary<int, List<int>> subdivisionIdsByCountyId = [];
            foreach (AdministrativeAreal2DReference? administrativeAreal2DReference in administrativeAreal2DReferences_Subdivision)
            {
                if (administrativeAreal2DReference is null)
                {
                    continue;
                }

                int? countyId = administrativeAreal2DReference.CountyId;
                if (countyId is null)
                {
                    continue;
                }

                if (!subdivisionIdsByCountyId.TryGetValue(countyId.Value, out List<int>? subdivisionIds))
                {
                    subdivisionIds = [];
                    subdivisionIdsByCountyId[countyId.Value] = subdivisionIds;
                }

                if (!subdivisionIds.Contains(administrativeAreal2DReference.Id))
                {
                    subdivisionIds.Add(administrativeAreal2DReference.Id);
                }
            }

            return subdivisionIdsByCountyId;
        }

        /// <summary>
        /// Resolves the <see cref="AdministrativeArealType.Subdivision"/> references among the given ones to their polygons, the boundary their buildings are read by.
        /// <para>The subdivision layer is nested - a city, its districts and their neighbourhoods are all subdivisions of the one municipality - and <c>subdivision_id</c> files each building under a single one of its containers (the smallest one, see <c>GetSubdivisionIdAsync</c>). Membership therefore cannot answer which buildings a district holds; its polygon can. References above the subdivision level are skipped - they resolve through <see cref="ResolveSubdivisionIdsByCountyIdAsync"/>.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="administrativeAreal2DReferences">The administrative areal 2D references to resolve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the polygons of the subdivision references, an empty list when there is none, or null when the connection is missing or the polygon lookup fails.</returns>
        private static async Task<List<PolygonalFace2D>?> ResolveSubdivisionPolygonalFace2DsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<AdministrativeAreal2DReference?> administrativeAreal2DReferences, CancellationToken cancellationToken)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            List<int> ids = [];
            foreach (AdministrativeAreal2DReference? administrativeAreal2DReference in administrativeAreal2DReferences)
            {
                if (administrativeAreal2DReference is not null && administrativeAreal2DReference.AdministrativeArealType == AdministrativeArealType.Subdivision && !ids.Contains(administrativeAreal2DReference.Id))
                {
                    ids.Add(administrativeAreal2DReference.Id);
                }
            }

            if (ids.Count == 0)
            {
                return [];
            }

            List<AdministrativeAreal2D>? administrativeAreal2Ds = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(npgsqlConnection, ids, cancellationToken: cancellationToken);
            if (administrativeAreal2Ds is null)
            {
                return null;
            }

            List<PolygonalFace2D> result = [];
            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
            {
                if (administrativeAreal2D?.ToDiGi()?.PolygonalFace2D is PolygonalFace2D polygonalFace2D)
                {
                    result.Add(polygonalFace2D);
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves all Building2DReferences for a specific county, with an optional exclusion list.
        /// Optimized for partitioned tables using the partition key (county_id).
        /// </summary>
        /// <param name="countyId">The ID of the county (Partition Key).</param>
        /// <param name="subdivisionId">The ID of the subdivision.</param>
        /// <param name="excludedReferences">Optional collection of references to be excluded from the result.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of Building2DReference objects, or null if connection fails.</returns>
        public async Task<List<Building2DReference>?> GetBuilding2DReferencesByCountyIdAsync(int countyId, int? subdivisionId = null, IEnumerable<string>? excludedReferences = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            // 1. Check early if cancellation was already requested to avoid unnecessary allocations
            cancellationToken.ThrowIfCancellationRequested();

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            // 2. Critical: Pass the token to the connection opening process
            await npgsqlConnection.OpenAsync(cancellationToken);

            string commandText = $@"
                SELECT id, county_id, reference, subdivision_id
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId";

            bool hasSubdivisionId = subdivisionId.HasValue;
            if (hasSubdivisionId)
            {
                commandText += " AND subdivision_id = @subdivisionId";
            }

            bool hasExcluded = excludedReferences != null && excludedReferences.Any();

            if (hasExcluded)
            {
                commandText += " AND NOT (reference = ANY(@excluded))";
            }

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("countyId", countyId);

            if (hasSubdivisionId)
            {
                npgsqlCommand.Parameters.AddWithValue("subdivisionId", subdivisionId!.Value);
            }

            if (hasExcluded)
            {
                // Using explicit typing and collection expression for performance
                string[] excludedArray = [.. excludedReferences!];
                npgsqlCommand.Parameters.AddWithValue("excluded", excludedArray);
            }

            // 3. Reading loop with token is correct
            return await ReadAsync_Building2DReference(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the buildings of a <see cref="AdministrativeArealType.Subdivision"/> by geometry - those whose bounding-box centre lies inside the subdivision's polygon.
        /// <para>The subdivision layer is nested - a city, its districts and their neighbourhoods are all subdivisions of one municipality - so a building is returned for <b>every</b> container that holds it, not just the one it is filed under in <c>subdivision_id</c>. This is the geometry path of <see cref="GetBuilding2DReferencesByCountyIdAsync(int, int?, IEnumerable{string}?, int, CancellationToken)"/> and nothing is filtered by that column; the read itself is <see cref="GetBuilding2DReferencesByPolygonalFace2DAsync"/>.</para>
        /// </summary>
        /// <param name="subdivisionId">The identifier of the subdivision to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the buildings inside the subdivision, empty when the identifier is missing, is not a <see cref="AdministrativeArealType.Subdivision"/>, or holds no building, or null when the connection could not be created.</returns>
        public async Task<List<Building2DReference>?> GetBuilding2DReferencesBySubdivisionIdAsync(int subdivisionId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            // 1. Check early if cancellation was already requested to avoid unnecessary allocations
            cancellationToken.ThrowIfCancellationRequested();

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            // 2. Critical: Pass the token to the connection opening process
            await npgsqlConnection.OpenAsync(cancellationToken);

            PolygonalFace2D? polygonalFace2D = await GetSubdivisionPolygonalFace2DAsync(npgsqlConnection, subdivisionId, cancellationToken);
            if (polygonalFace2D is null)
            {
                return [];
            }

            return await GetBuilding2DReferencesByPolygonalFace2DAsync(npgsqlConnection, polygonalFace2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Resolves a <see cref="AdministrativeArealType.Subdivision"/> identifier to the polygon its buildings are read by.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="subdivisionId">The identifier to resolve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the subdivision's polygon, or null when the identifier is missing, is not a <see cref="AdministrativeArealType.Subdivision"/>, or carries no polygon.</returns>
        private static async Task<PolygonalFace2D?> GetSubdivisionPolygonalFace2DAsync(NpgsqlConnection npgsqlConnection, int subdivisionId, CancellationToken cancellationToken)
        {
            List<AdministrativeAreal2D>? administrativeAreal2Ds = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(npgsqlConnection, [subdivisionId], cancellationToken: cancellationToken);
            if (administrativeAreal2Ds is null || administrativeAreal2Ds.Count == 0)
            {
                return null;
            }

            if (administrativeAreal2Ds[0]?.ToDiGi() is not AdministrativeSubdivision administrativeSubdivision)
            {
                return null;
            }

            return administrativeSubdivision.PolygonalFace2D;
        }

        /// <summary>
        /// Asynchronously retrieves a keyset-paginated list of Building2DReference objects for a specified county.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county (Partition Key).</param>
        /// <param name="subdivisionId">The optional integer identifier of the subdivision.</param>
        /// <param name="lastReference">The last reference string from the previous page, used as the pagination cursor.</param>
        /// <param name="pageSize">The maximum number of references to return in a single page.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2DReference"/> objects, or null if connection fails.</returns>
        public async Task<List<Building2DReference>?> GetBuilding2DReferencesByCountyIdAsync(int countyId, int? subdivisionId, string? lastReference, int pageSize, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            // Initialize the base query with the mandatory county_id parameter
            string commandText = $@"
                SELECT id, county_id, reference, subdivision_id
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId";

            // Dynamically append the subdivision_id filter if it was provided
            bool hasSubdivisionId = subdivisionId.HasValue;
            if (hasSubdivisionId)
            {
                commandText += " AND subdivision_id = @subdivisionId";
            }

            // Dynamically append the pagination cursor if it was provided
            bool hasCursor = !string.IsNullOrWhiteSpace(lastReference);
            if (hasCursor)
            {
                commandText += " AND reference > @lastReference";
            }

            // Append sorting and limiting to finalize the keyset pagination query
            commandText += @"
                ORDER BY reference
                LIMIT @pageSize;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("countyId", countyId);
            npgsqlCommand.Parameters.AddWithValue("pageSize", pageSize);

            // Add the optional parameter values to the command if they are part of the query
            if (hasSubdivisionId)
            {
                npgsqlCommand.Parameters.AddWithValue("subdivisionId", subdivisionId!.Value);
            }

            if (hasCursor)
            {
                npgsqlCommand.Parameters.AddWithValue("lastReference", lastReference!);
            }

            List<Building2DReference>? building2DReferences = await ReadAsync_Building2DReference(npgsqlCommand, cancellationToken);

            return building2DReferences;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building2D"/> objects located within the specified bounding box, applying a distance tolerance.
        /// <para>The partitions to read come from <see cref="AdministrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(NpgsqlConnection, BoundingBox2D, double, int, CancellationToken)"/>, so every polygon part of a county reaching the box is read, whichever part its buildings happen to be filed under. Nothing is filtered by subdivision: the bounding box alone decides what comes back.</para>
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the spatial area to search for buildings; may be null.</param>
        /// <param name="tolerance">The double value representing the distance tolerance used during the spatial query.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="List{Building2D}"/> of buildings found within the specified area.</returns>
        public async Task<List<Building2D>?> GetBuilding2DsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            // 1. Get the county partitions to read. Not the county_id the subdivisions in the box carry:
            // that column names one polygon part of a multi-part county, and the buildings can be filed
            // under a sibling part, in which case pruning to it answers nothing at all - silently, because
            // an empty result is indistinguishable from an area holding no buildings. See
            // https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/64.
            HashSet<int>? countyIds = await AdministrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, tolerance, commandTimeout, cancellationToken);

            if (countyIds is null || countyIds.Count == 0)
            {
                return [];
            }

            Dictionary<string, Building2D> dictionary = [];

            // 2. Prepare pre-calculated boundaries for the 'box' constructor
            double searchMinX = boundingBox2D.Min.X - tolerance;
            double searchMaxX = boundingBox2D.Max.X + tolerance;
            double searchMinY = boundingBox2D.Min.Y - tolerance;
            double searchMaxY = boundingBox2D.Max.Y + tolerance;

            // 3. Optimized Query:
            // - county_id = ANY(@county_ids) triggers Partition Pruning
            // - box && box triggers GiST index scan on those partitions
            // There is deliberately no subdivision_id condition. It assisted no index - the GiST box index
            // is what drives this query - and it dropped every building whose stored subdivision_id named a
            // subdivision the box had not returned.
            const string commandText = $@"
                SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                FROM {Constants.TableName.Building2D}
                WHERE county_id = ANY(@county_ids)
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@sMinX, @sMinY), point(@sMaxX, @sMaxY));";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds.ToArray() });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMaxY", NpgsqlDbType.Double) { Value = searchMaxY });

            List<Building2D>? building2Ds = await ReadAsync_Building2D(npgsqlCommand, cancellationToken);

            if (building2Ds is not null)
            {
                foreach (Building2D building in building2Ds)
                {
                    // Defensive de-duplication keyed on county + reference (unique per row, so duplicates are not expected)
                    string key = $"{building.CountyId}_{building.Reference}";
                    dictionary.TryAdd(key, building);
                }
            }

            return [.. dictionary.Values];
        }

        /// <summary>
        /// Asynchronously retrieves full building data based on a collection of references using optimized partition-pruned batching.
        /// </summary>
        /// <param name="building2DReferences">The collection of <see cref="Building2DReference"/> objects used to identify and retrieve the corresponding buildings from the database.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform an additional fallback query matching by reference only (without county identifier condition) for any references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects matching the provided references, an empty list if none match, or null if the input collection was null or the database connection could not be established.</returns>
        public async Task<List<Building2D>?> GetBuilding2DsByBuilding2DReferencesAsync(IEnumerable<Building2DReference>? building2DReferences, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DReferences == null)
            {
                return null;
            }

            List<Building2DReference> building2DReferences_List = [.. building2DReferences];
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

            return await GetBuilding2DsByBuilding2DReferencesAsync(npgsqlConnection, building2DReferences_List, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building2D"/> objects whose bounding box lies within or intersects the specified circular area (the radius expanded by the tolerance).
        /// </summary>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area; can be null.</param>
        /// <param name="tolerance">The double value representing the distance tolerance for the spatial query, defaulting to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{Building2D}"/> of buildings found within the specified area, an empty list if none match, or null if the input is invalid or the connection could not be established.</returns>
        public async Task<List<Building2D>?> GetBuilding2DsByCircle2DAsync(Circle2D? circle2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (circle2D?.Center is null)
            {
                return null;
            }

            // Delegate the coarse spatial search to the bounding-box query: the circle's bounding box
            // expanded by the tolerance is exactly the effective search region (radius + tolerance).
            // This reuses the partition pruning and the GiST-indexed 'box && box' filter.
            List<Building2D>? building2Ds = await GetBuilding2DsByBoundingBox2DAsync(circle2D.GetBoundingBox(), tolerance, commandTimeout, cancellationToken);
            if (building2Ds is null || building2Ds.Count == 0)
            {
                return building2Ds;
            }

            // Narrow the bounding-box superset to the true circular area (radius + tolerance)
            // using the exact rectangle-vs-circle test on each building's bounding box.
            List<Building2D> result = [];
            foreach (Building2D building2D in building2Ds)
            {
                if (circle2D.InRange(building2D.BoundingBox2D, tolerance))
                {
                    result.Add(building2D);
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building2DReference"/> objects for the buildings whose bounding box lies within or intersects the specified circular area (the radius expanded by the tolerance).
        /// </summary>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area; can be null.</param>
        /// <param name="tolerance">The double value representing the distance tolerance for the spatial query, defaulting to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{Building2DReference}"/> of references to the buildings found within the specified area, an empty list if none match, or null if the input is invalid or the connection could not be established.</returns>
        public async Task<List<Building2DReference>?> GetBuilding2DReferencesByCircle2DAsync(Circle2D? circle2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (circle2D?.Center is null)
            {
                return null;
            }

            // Delegate the coarse spatial search to the bounding-box query: the circle's bounding box
            // expanded by the tolerance is exactly the effective search region (radius + tolerance).
            // This reuses the partition pruning and the GiST-indexed 'box && box' filter.
            List<Building2D>? building2Ds = await GetBuilding2DsByBoundingBox2DAsync(circle2D.GetBoundingBox(), tolerance, commandTimeout, cancellationToken);
            if (building2Ds is null)
            {
                return null;
            }

            if (building2Ds.Count == 0)
            {
                return [];
            }

            // Narrow the bounding-box superset to the true circular area (radius + tolerance)
            // using the exact rectangle-vs-circle test on each building's bounding box.
            List<Building2DReference> result = [];
            foreach (Building2D building2D in building2Ds)
            {
                if (circle2D.InRange(building2D.BoundingBox2D, tolerance))
                {
                    result.Add(new Building2DReference(building2D));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the count of records, optionally filtered by a specific county identifier.
        /// </summary>
        /// <param name="countyId">The optional integer identifier of the county to filter the results; if null, the total count is retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total row count as a long.</returns>
        public async Task<long> GetCountAsync(int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string tableName = Constants.TableName.Building2D;
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
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated row count as a nullable <see cref="System.Int64"/>, -1 when the partition exists but has not been analysed, or null if an error occurs or the target does not exist.</returns>
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
        /// Asynchronously retrieves the estimated row count for the specified collection of county identifiers.
        /// </summary>
        /// <param name="countyIds">A collection of integers representing the IDs of the counties to be counted.</param>
        /// <param name="analyze">A boolean value indicating whether to run a database analysis before fetching the estimate.</param>
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
        /// Asynchronously retrieves a list of <see cref="Point2D"/> objects associated with the specified references and optional county identifier.
        /// </summary>
        /// <param name="references">An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the reference identifiers for the points to retrieve.</param>
        /// <param name="countyId">An optional <see cref="int"/> representing the unique identifier of the county used to filter the search.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback search by reference alone for any references not found in the initial search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{T}"/> of <see cref="Point2D"/> objects if matches are found, an empty list if none match, or null if the input collection was null or the connection could not be established.</returns>
        public async Task<List<Point2D>?> GetPoint2DsByReferencesAsync(IEnumerable<string>? references, int? countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            List<string> references_List = [.. references];
            if (references_List.Count == 0)
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetPoint2DsByReferencesAsync(npgsqlConnection, references_List, countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously refreshes the 2D building data in the PostgreSQL database - today, the <c>subdivision_id</c> of each building, derived from its outline (the smallest subdivision containing it).
        /// <para>The pick is made in memory by a <see cref="SubdivisionIdSolver"/> built once per county over the subdivisions whose boxes meet the county's; a building the layer cannot place - outside it, or covered only by an <see cref="GIS.Classes.AdministrativeDivision"/> row - goes through <c>GetSubdivisionIdAsync</c> against the database, the path every building took before (26 buildings a second in Warsaw, where each one re-read the 412 KB city outline; <see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/79">DiGi.GIS.PostgreSQL#79</see>). The layers loaded and the buildings that fell back are logged.</para>
        /// <para>Walks the table one county polygon part at a time, each part in identifier order in batches, each batch under <c>FOR UPDATE SKIP LOCKED</c>. A part is one partition, so a batch is bounded by that partition's own index no matter how many parts are in scope, and the part's subdivision layer is loaded once for the whole part rather than being dropped and rebuilt as its buildings interleave with the others'. By default only buildings with no <c>subdivision_id</c> are visited; <see cref="PostgreSQLBuilding2DRefreshOptions.OverrideExistingSubdivisionIds"/> re-derives every one. The walk can be limited to county polygon parts with <see cref="PostgreSQLBuilding2DRefreshOptions.CountyIds"/>, or to the parts whose subdivision layer nests with <see cref="PostgreSQLBuilding2DRefreshOptions.NestedSubdivisionsOnly"/> - the counties where the previous lowest-identifier tie-break produced an arbitrary value (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>), which is nearly all of them: a village and its named parts nest as a city and its districts do. The parts visited and the rows written per part are logged.</para>
        /// </summary>
        /// <param name="postgreSQLBuilding2DRefreshOptions">The options to configure the refresh process for PostgreSQL 2D buildings. Can be null to use default settings.</param>
        /// <param name="progress">The progress reporter used to report the current progress as a long value representing the count of updated buildings. Can be null if no progress reporting is required.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result carries a <see cref="PostgreSQLBuilding2DRefreshResult"/> with details of the operation, or <see langword="null"/> if the database connection could not be established.</returns>
        public async Task<PostgreSQLBuilding2DRefreshResult?> RefreshAsync(PostgreSQLBuilding2DRefreshOptions? postgreSQLBuilding2DRefreshOptions = default, IProgress<long>? progress = default, int commandTimeout = 60, CancellationToken cancellationToken = default)
        {
            postgreSQLBuilding2DRefreshOptions ??= new PostgreSQLBuilding2DRefreshOptions();

            int batchSize = postgreSQLBuilding2DRefreshOptions.BatchSize < 1 ? 1 : postgreSQLBuilding2DRefreshOptions.BatchSize;
            double tolerance = postgreSQLBuilding2DRefreshOptions.Tolerance;
            bool overrideExistingSubdivisionIds = postgreSQLBuilding2DRefreshOptions.OverrideExistingSubdivisionIds;
            long lastProcessedId = postgreSQLBuilding2DRefreshOptions.StartId;

            long readCount = 0;
            long updatedCount = 0;
            long failedBatchCount = 0;
            bool cancelled = false;

            // The scope is a set of county polygon parts, resolved once before the first batch. Named parts
            // and the nested-layer parts intersect when both are asked for; either alone is the scope; neither
            // means the whole table. An empty scope is an answer - nothing to refresh - not the whole table.
            HashSet<int>? countyIds = postgreSQLBuilding2DRefreshOptions.CountyIds is null ? null : [.. postgreSQLBuilding2DRefreshOptions.CountyIds];
            if (postgreSQLBuilding2DRefreshOptions.NestedSubdivisionsOnly)
            {
                HashSet<int>? countyIds_Nested;

                await using (NpgsqlConnection? npgsqlConnection_Scope = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData))
                {
                    if (npgsqlConnection_Scope is null)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type} refresh failed: unable to create database connection", nameof(Building2DPostgreSQLConverter));
                        return null;
                    }

                    await npgsqlConnection_Scope.OpenAsync(cancellationToken);

                    countyIds_Nested = await AdministrativeAreal2DPostgreSQLConverter.GetCountyIdsWithNestedSubdivisionsAsync(npgsqlConnection_Scope, tolerance, commandTimeout, cancellationToken);
                }

                if (countyIds_Nested is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type} refresh failed: the counties with a nested subdivision layer could not be resolved", nameof(Building2DPostgreSQLConverter));
                    return null;
                }

                if (countyIds is null)
                {
                    countyIds = countyIds_Nested;
                }
                else
                {
                    countyIds.IntersectWith(countyIds_Nested);
                }
            }

            // The walk is per part: each part is one partition with its own identifier index, so a batch is
            // bounded by that partition no matter how many parts are in scope, and the part's subdivision layer
            // is loaded once for the whole part instead of being dropped and rebuilt as its buildings interleave
            // with the others' in a global identifier walk.
            List<int> countyIds_Parts;

            if (countyIds is null)
            {
                // The whole table: exactly the parts that hold rows, so a part with no building costs nothing,
                // and the visit order is stable from run to run.
                await using NpgsqlConnection? npgsqlConnection_Parts = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
                if (npgsqlConnection_Parts is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type} refresh failed: unable to create database connection", nameof(Building2DPostgreSQLConverter));
                    return null;
                }

                await npgsqlConnection_Parts.OpenAsync(cancellationToken);

                string commandText_Parts = $@"SELECT DISTINCT county_id FROM {Constants.TableName.Building2D} ORDER BY county_id";
                await using NpgsqlCommand npgsqlCommand_Parts = new(commandText_Parts, npgsqlConnection_Parts);
                npgsqlCommand_Parts.CommandTimeout = commandTimeout;

                countyIds_Parts = [];
                await using NpgsqlDataReader npgsqlDataReader_Parts = await npgsqlCommand_Parts.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader_Parts.ReadAsync(cancellationToken))
                {
                    countyIds_Parts.Add(npgsqlDataReader_Parts.GetInt32(0));
                }
            }
            else
            {
                countyIds_Parts = [.. countyIds.OrderBy(x => x)];
            }

            Serilog.Modify.Log(
                "{Type} refresh started: {PartsTotal} parts, batch size {BatchSize}, start ID {StartId}, override existing subdivision IDs {OverrideExistingSubdivisionIds}, tolerance {Tolerance}",
                nameof(Building2DPostgreSQLConverter), countyIds_Parts.Count, batchSize, lastProcessedId, overrideExistingSubdivisionIds, tolerance);

            Dictionary<int, long> updatedCounts_ByCountyId = [];
            long fallbackCount = 0;
            int partIndex = 0;

            foreach (int countyId in countyIds_Parts)
            {
                if (cancelled)
                {
                    break;
                }

                partIndex++;

                // The anchor is per part: a part skips its rows at or below StartId and visits the rest, so a
                // part that ends below the anchor is simply done. This is what a cancelled run restarts with -
                // the parts it already finished redo nothing below their rows, and the part it stopped in
                // resumes at the anchor.
                long lastId = postgreSQLBuilding2DRefreshOptions.StartId;
                long partReadCount = 0;
                long partUpdatedCount = 0;
                long partFallbackCount = 0;

                // One subdivision layer for the whole part, built on the part's first batch and dropped when the
                // part ends: the layer loads once per part instead of being kept warm while its buildings
                // interleave with the others', and nothing has to be evicted.
                SubdivisionIdSolver? subdivisionIdSolver = null;

                while (!cancellationToken.IsCancellationRequested && !cancelled)
                {
                    await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
                    if (npgsqlConnection is null)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type} refresh failed: unable to create database connection", nameof(Building2DPostgreSQLConverter));
                        return null;
                    }

                    await npgsqlConnection.OpenAsync(cancellationToken);

                    // Open transaction to maintain FOR UPDATE SKIP LOCKED locks during the batch
                    await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

                    // The part is fixed, so county_id prunes the batch to one partition and id > @lastId walks
                    // that partition's own index. We combine the ID anchor with the optional NULL check.
                    // This ensures we always move forward in the part, regardless of update success.
                    string filterClause = overrideExistingSubdivisionIds
                        ? "county_id = @countyId AND id > @lastId"
                        : "county_id = @countyId AND id > @lastId AND subdivision_id IS NULL";

                    string commandText_Select = $@"
                        SELECT id, county_id, object
                        FROM {Constants.TableName.Building2D}
                        WHERE {filterClause}
                        ORDER BY id ASC
                        FOR UPDATE SKIP LOCKED
                        LIMIT @batchSize";

                    List<(long Id, int CountyId, string Json)> records = [];

                    try
                    {
                        await using (NpgsqlCommand npgsqlCommand = new(commandText_Select, npgsqlConnection, npgsqlTransaction))
                        {
                            npgsqlCommand.CommandTimeout = commandTimeout;
                            npgsqlCommand.Parameters.AddWithValue("batchSize", batchSize);
                            npgsqlCommand.Parameters.AddWithValue("lastId", lastId);
                            npgsqlCommand.Parameters.AddWithValue("countyId", countyId);

                            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess, cancellationToken);
                            while (await npgsqlDataReader.ReadAsync(cancellationToken))
                            {
                                records.Add((npgsqlDataReader.GetInt64(0), npgsqlDataReader.GetInt32(1), npgsqlDataReader.GetString(2)));
                            }
                        }

                        // If the query returns no rows past the anchor, the part is done - the next part,
                        // not the end of the run.
                        if (records.Count == 0)
                        {
                            break;
                        }

                        readCount += records.Count;
                        partReadCount += records.Count;

                        if (subdivisionIdSolver is null)
                        {
                            subdivisionIdSolver = await CreateSubdivisionIdSolverAsync(npgsqlConnection, countyId, tolerance, commandTimeout, cancellationToken);
                        }

                        List<(long Id, int CountyId, int SubdivisionId)> updates = [];

                        foreach ((long Id, int CountyId, string Json) in records)
                        {
                            if (cancellationToken.IsCancellationRequested) break;

                            GIS.Classes.Building2D? building = Core.Convert.ToDiGi<GIS.Classes.Building2D>(Json)?.FirstOrDefault();
                            if (building is null)
                            {
                                continue;
                            }

                            int? subdivisionId;

                            subdivisionIdSolver.Input = building;
                            if (subdivisionIdSolver.Solve())
                            {
                                subdivisionId = subdivisionIdSolver.Output;
                            }
                            else
                            {
                                partFallbackCount++;
                                fallbackCount++;
                                subdivisionId = await GetSubdivisionIdAsync(npgsqlConnection, building, tolerance);
                            }

                            if (subdivisionId.HasValue)
                            {
                                updates.Add((Id, CountyId, subdivisionId.Value));
                            }
                        }

                        // Execute batch update for successfully resolved records
                        if (updates.Count > 0 && !cancellationToken.IsCancellationRequested)
                        {
                            await ExecuteUpdateBatchAsync(npgsqlConnection, npgsqlTransaction, updates, commandTimeout, cancellationToken);
                            updatedCount += updates.Count;
                            partUpdatedCount += updates.Count;
                            progress?.Report(updatedCount);

                            updatedCounts_ByCountyId.TryGetValue(countyId, out long updatedCount_County);
                            updatedCounts_ByCountyId[countyId] = updatedCount_County + updates.Count;
                        }

                        // Commit releases the locks and confirms the batch processing
                        await npgsqlTransaction.CommitAsync(cancellationToken);

                        // Update anchor after successful batch transaction
                        lastId = records[^1].Id;
                        lastProcessedId = records[^1].Id;
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        cancelled = true;
                    }
                    catch (Exception exception)
                    {
                        failedBatchCount++;
                        Serilog.Modify.Log(exception, "{Type} refresh batch failed around part {CountyId} last ID {LastId}", nameof(Building2DPostgreSQLConverter), countyId, lastId);

                        if (records.Count > 0)
                        {
                            lastId = records[^1].Id;
                            lastProcessedId = records[^1].Id;
                        }
                        else
                        {
                            // A batch that failed before reading a single row is stepped over, as before.
                            cancelled = true;
                        }
                    }
                }

                if (cancelled)
                {
                    break;
                }

                Serilog.Modify.Log(
                    "{Type} refresh part {PartIndex}/{PartsTotal} {CountyId} done: {PartReadCount} read, {PartUpdatedCount} written, {PartFallbackCount} fallback",
                    nameof(Building2DPostgreSQLConverter), partIndex, countyIds_Parts.Count, countyId, partReadCount, partUpdatedCount, partFallbackCount);
            }

            cancelled = cancelled || cancellationToken.IsCancellationRequested;

            Serilog.Modify.Log(
                cancelled || failedBatchCount != 0 ? Serilog.Enums.LogEventLevel.Warning : Serilog.Enums.LogEventLevel.Information,
                "{Type} refresh finished{Cancelled}: {ReadCount} records read, {UpdatedCount} subdivision IDs written, {FallbackCount} resolved against the database, {FailedBatchCount} batches stepped over, last ID {LastId}, written per county {UpdatedCountsByCountyId}",
                nameof(Building2DPostgreSQLConverter), cancelled ? " after being cancelled" : string.Empty, readCount, updatedCount, fallbackCount, failedBatchCount, lastProcessedId, string.Join(", ", updatedCounts_ByCountyId.OrderBy(x => x.Key).Select(x => $"{x.Key}: {x.Value}")));

            return new PostgreSQLBuilding2DRefreshResult(readCount, updatedCount, failedBatchCount, lastProcessedId, cancelled);

            // The layer is every subdivision whose box meets the county part's own box - the neighbours' border
            // units included, since the database path never asked which county a subdivision belongs to and a
            // building on the border may well sit in one of them. A county row without a box gives an empty layer,
            // and every building of that county then takes the database path.
            async static Task<SubdivisionIdSolver> CreateSubdivisionIdSolverAsync(NpgsqlConnection npgsqlConnection, int countyId, double tolerance, int commandTimeout, CancellationToken cancellationToken)
            {
                AdministrativeAreal2D? administrativeAreal2D_County = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DByIdAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);

                List<AdministrativeAreal2D>? administrativeAreal2Ds = null;
                if (administrativeAreal2D_County?.BoundingBox2D is BoundingBox2D boundingBox2D_County)
                {
                    administrativeAreal2Ds = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D_County, [AdministrativeArealType.Subdivision], tolerance, commandTimeout, cancellationToken);
                }

                SubdivisionIdSolver subdivisionIdSolver = new(administrativeAreal2Ds, tolerance);

                Serilog.Modify.Log("{Type} refresh loaded the subdivision layer of county {CountyId}: {Count} subdivisions", nameof(Building2DPostgreSQLConverter), countyId, subdivisionIdSolver.Count);

                return subdivisionIdSolver;
            }

            async static Task ExecuteUpdateBatchAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, List<(long Id, int CountyId, int SubdivisionId)> updates, int commandTimeout, CancellationToken cancellationToken)
            {
                await using NpgsqlBatch npgsqlBatch = new(connection, transaction);
                npgsqlBatch.Timeout = commandTimeout;

                foreach ((long Id, int CountyId, int SubdivisionId) in updates)
                {
                    NpgsqlBatchCommand npgsqlBatchCommand = new($"UPDATE {Constants.TableName.Building2D} SET subdivision_id = @subdivision_id WHERE id = @id AND county_id = @county_id");
                    npgsqlBatchCommand.Parameters.AddWithValue("subdivision_id", SubdivisionId);
                    npgsqlBatchCommand.Parameters.AddWithValue("id", Id);
                    npgsqlBatchCommand.Parameters.AddWithValue("county_id", CountyId);
                    npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
                }
                await npgsqlBatch.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Asynchronously updates the specified collection of 2D buildings using a defined distance tolerance.
        /// </summary>
        /// <param name="building2Ds">The enumerable collection of <see cref="Building2D"/> objects to be updated; may be null.</param>
        /// <param name="tolerance">The double precision value used as the distance tolerance for the update operation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers written and the rows dropped before the database, or null when the update could not be attempted at all - no connection, or the table could not be created.</returns>
        public async Task<PostgreSQLUpdateResult?> UpdateAsync(IEnumerable<Building2D>? building2Ds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (building2Ds is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool succeded = await Create.TableAsync_Building2D(npgsqlConnection, commandTimeout, cancellationToken);
            if (!succeded)
            {
                return null;
            }

            HashSet<long> ids = [];
            List<Rejection> rejections = [];

            if (!building2Ds.Any())
            {
                return new PostgreSQLUpdateResult(ids, rejections);
            }

            Dictionary<int, List<Building2D>> dictionary_Building2D = [];

            Dictionary<string, List<AdministrativeAreal2D>> dictionary_Code = [];

            // The parts of a code are cached above; their polygons are cached here, because deriving one
            // deserializes a county-sized geometry and every building carrying the code is tested against
            // the same set.
            Dictionary<string, Dictionary<int, Geometry.Planar.Interfaces.IPolygonal2D>> dictionary_Polygonal2D = [];

            // The code a named part belongs to, cached so that a batch filed under one part costs one
            // lookup rather than one per building.
            Dictionary<int, string?> dictionary_CodeByCountyId = [];

            // County assignment runs in three tiers, in descending reliability:
            //   1. building2D.CountyId - names a part outright, and is kept unless the code that part
            //                            belongs to holds more than one, in which case tier 3 decides
            //                            between them.
            //   2. building2D.Code     - names the county but not which of its parts. A code holding a
            //                            single part resolves outright; a multi-part code only narrows
            //                            the candidates and hands them to tier 3.
            //   3. geometry            - decides which candidate part really contains the building, and
            //                            is the only tier that can.
            // Tier 2 used to collapse a multi-part county onto one part by itself, which is why
            // building_2d holds ~86k rows duplicated across sibling parts: separate runs resolved the
            // same code to different parts back when that resolution had no ORDER BY. Narrowing instead
            // of choosing is what stops that recurring, and it also makes tier 3 cheaper - the candidate
            // set is the parts of one county rather than every county overlapping the bounding box.
            //
            // Tier 1 used to be taken on trust, and that is how a wrong answer became permanent: a caller
            // reads the part from the building_2d row it is about to overwrite, so a row filed under the
            // wrong part of a multi-part county rewrote itself there for as long as it was touched
            // (DiGi-Organization/DiGi.GIS.PostgreSQL#68). Checking it costs one cached lookup per distinct
            // part named in the batch, and changes nothing for the 362 counties stored as a single part.
            foreach (Building2D building2D in building2Ds)
            {
                if (building2D is null)
                {
                    // Recorded rather than skipped in silence, so the rejection count stays an exact
                    // account of the rows that never reached the database.
                    rejections.Add(new Rejection(null, UpdateRejectionReason.Undefined));
                    continue;
                }

                int? countyId = building2D.CountyId;

                List<AdministrativeAreal2D>? administrativeAreal2Ds_Candidate = null;
                string? code_Candidate = null;

                if (countyId is not null && countyId.HasValue)
                {
                    if (!dictionary_CodeByCountyId.TryGetValue(countyId.Value, out string? code_CountyId))
                    {
                        List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, [countyId.Value], commandTimeout, cancellationToken);
                        code_CountyId = administrativeAreal2DReferences is null || administrativeAreal2DReferences.Count == 0 ? null : administrativeAreal2DReferences[0]?.Code;
                        dictionary_CodeByCountyId[countyId.Value] = code_CountyId;
                    }

                    if (!string.IsNullOrWhiteSpace(code_CountyId))
                    {
                        if (!dictionary_Code.TryGetValue(code_CountyId!, out List<AdministrativeAreal2D>? administrativeAreal2Ds_CountyId) || administrativeAreal2Ds_CountyId is null)
                        {
                            administrativeAreal2Ds_CountyId = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByCodeAsync(npgsqlConnection, code_CountyId!, AdministrativeArealType.County, commandTimeout, cancellationToken) ?? [];
                            dictionary_Code[code_CountyId!] = administrativeAreal2Ds_CountyId;
                        }

                        // Only a code stored as several parts can have been filed under the wrong one.
                        if (administrativeAreal2Ds_CountyId.Count > 1)
                        {
                            if (!dictionary_Polygonal2D.TryGetValue(code_CountyId!, out Dictionary<int, Geometry.Planar.Interfaces.IPolygonal2D>? polygonal2Ds_ByCountyId_Named) || polygonal2Ds_ByCountyId_Named is null)
                            {
                                polygonal2Ds_ByCountyId_Named = administrativeAreal2Ds_CountyId.Polygonal2DsByCountyId();
                                dictionary_Polygonal2D[code_CountyId!] = polygonal2Ds_ByCountyId_Named;
                            }

                            // A footprint that cannot be read leaves the caller answer standing: there is
                            // nothing better to put in its place.
                            int? countyId_Resolved = polygonal2Ds_ByCountyId_Named.CountyId(building2D.ToDiGi()?.PolygonalFace2D?.ExternalEdge, tolerance);
                            if (countyId_Resolved is not null && countyId_Resolved.HasValue)
                            {
                                countyId = countyId_Resolved;
                            }
                        }
                    }
                }

                if (countyId is null || !countyId.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(building2D.Code))
                    {
                        if (!dictionary_Code.TryGetValue(building2D.Code, out List<AdministrativeAreal2D>? administrativeAreal2Ds_Code) || administrativeAreal2Ds_Code is null)
                        {
                            // Cached per batch: the parts of a code and their polygons are the same for
                            // every building carrying it, and re-reading them per row would put a query
                            // and a polygon parse on each one.
                            administrativeAreal2Ds_Code = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByCodeAsync(npgsqlConnection, building2D.Code, AdministrativeArealType.County, commandTimeout, cancellationToken) ?? [];
                            dictionary_Code[building2D.Code] = administrativeAreal2Ds_Code;
                        }

                        if (administrativeAreal2Ds_Code.Count == 1)
                        {
                            countyId = administrativeAreal2Ds_Code[0].Id;
                        }
                        else if (administrativeAreal2Ds_Code.Count > 1)
                        {
                            administrativeAreal2Ds_Candidate = administrativeAreal2Ds_Code;
                            code_Candidate = building2D.Code;
                        }
                    }
                }

                if (countyId is null || !countyId.HasValue)
                {
                    BoundingBox2D? boundingBox2D = building2D.BoundingBox2D;

                    // Only a code that named nothing falls back to searching every county by bounding box.
                    List<AdministrativeAreal2D>? administrativeAreal2Ds = administrativeAreal2Ds_Candidate ?? await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, AdministrativeArealType.County, tolerance, commandTimeout, cancellationToken);
                    if (administrativeAreal2Ds is not null && administrativeAreal2Ds.Count != 0)
                    {
                        if (administrativeAreal2Ds.Count == 1)
                        {
                            countyId = administrativeAreal2Ds[0].Id;
                        }
                        else if (code_Candidate is not null)
                        {
                            if (!dictionary_Polygonal2D.TryGetValue(code_Candidate, out Dictionary<int, Geometry.Planar.Interfaces.IPolygonal2D>? polygonal2Ds_ByCountyId) || polygonal2Ds_ByCountyId is null)
                            {
                                polygonal2Ds_ByCountyId = administrativeAreal2Ds.Polygonal2DsByCountyId();
                                dictionary_Polygonal2D[code_Candidate] = polygonal2Ds_ByCountyId;
                            }

                            countyId = polygonal2Ds_ByCountyId.CountyId(building2D.ToDiGi()?.PolygonalFace2D?.ExternalEdge, tolerance);
                        }
                        else
                        {
                            // The bounding box path answers a different candidate set per building, so there
                            // is nothing to cache across them.
                            countyId = administrativeAreal2Ds.CountyId(building2D.ToDiGi()?.PolygonalFace2D?.ExternalEdge, tolerance);
                        }
                    }
                }

                if (countyId is null || !countyId.HasValue)
                {
                    // All three tiers ran and named no part. The building is dropped, and naming it here is
                    // the only place that can - nothing downstream knows which rows never made the batch.
                    rejections.Add(new Rejection(building2D.Reference, UpdateRejectionReason.CountyUnresolved));
                    continue;
                }

                if (!dictionary_Building2D.TryGetValue(countyId.Value, out List<Building2D>? building2Ds_CountyId) || building2Ds_CountyId is null)
                {
                    building2Ds_CountyId = [];
                    dictionary_Building2D[countyId.Value] = building2Ds_CountyId;
                }

                building2Ds_CountyId.Add(building2D);
            }

            await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection);
            npgsqlBatch.Timeout = commandTimeout;

            foreach (KeyValuePair<int, List<Building2D>> keyValuePair in dictionary_Building2D)
            {
                int countyId = keyValuePair.Key;

                succeded = await Create.TableAsync_Building2D_Partition(npgsqlConnection, countyId, commandTimeout, cancellationToken);
                if (!succeded)
                {
                    // A whole county's worth of rows disappears here, which is the largest silent drop of
                    // the three and the one least likely to be the caller's doing.
                    foreach (Building2D building2D in keyValuePair.Value)
                    {
                        rejections.Add(new Rejection(building2D.Reference, UpdateRejectionReason.PartitionUnavailable));
                    }

                    continue;
                }

                foreach (Building2D building2D in keyValuePair.Value)
                {
                    // SQL with full update on conflict (excluding ID)
                    NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                    INSERT INTO {Constants.TableName.Building2D} (county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object)
                    VALUES (@county_id, @reference, @code, @min_x, @min_y, @max_x, @max_y, @subdivision_id, @object)
                    ON CONFLICT (reference, county_id)
                    DO UPDATE SET
                        code = EXCLUDED.code,
                        min_x = EXCLUDED.min_x,
                        min_y = EXCLUDED.min_y,
                        max_x = EXCLUDED.max_x,
                        max_y = EXCLUDED.max_y,
                        subdivision_id = COALESCE(EXCLUDED.subdivision_id, {Constants.TableName.Building2D}.subdivision_id),
                        object = EXCLUDED.object
                    RETURNING id;");

                    BoundingBox2D? boundingBox2D = building2D.BoundingBox2D;

                    // Adding parameters with explicit NpgsqlDbType
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = building2D.Reference });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Text) { Value = building2D.Code });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Min.X });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Min.Y });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Max.X });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Max.Y });
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("subdivision_id", NpgsqlDbType.Integer) { Value = (object?)building2D.SubdivisionId ?? DBNull.Value });

                    // Handling potential null for JSONB object
                    npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb)
                    {
                        Value = (object?)building2D.Object?.ToJsonString() ?? DBNull.Value
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

        private static Building2D Create_Building2D(NpgsqlDataReader npgsqlDataReader)
        {
            return new Building2D
            {
                // Changed to GetInt64 because of BIGINT in SQL
                Id = npgsqlDataReader.GetInt64(0),
                CountyId = npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetInt32(1),
                // Added DBNull checks for strings as they are nullable in schema
                Reference = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2),
                Code = npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetString(3),
                BoundingBox2D = new BoundingBox2D(
                        new Point2D(npgsqlDataReader.IsDBNull(4) ? double.NaN : npgsqlDataReader.GetDouble(4),
                                    npgsqlDataReader.IsDBNull(5) ? double.NaN : npgsqlDataReader.GetDouble(5)),
                        new Point2D(npgsqlDataReader.IsDBNull(6) ? double.NaN : npgsqlDataReader.GetDouble(6),
                                    npgsqlDataReader.IsDBNull(7) ? double.NaN : npgsqlDataReader.GetDouble(7))),
                SubdivisionId = npgsqlDataReader.IsDBNull(8) ? null : npgsqlDataReader.GetInt32(8),
                // Object is JSONB, so it can be null
                Object = npgsqlDataReader.IsDBNull(9) ? null : JsonNode.Parse(npgsqlDataReader.GetString(9)) as JsonObject,
                CreatedAt = npgsqlDataReader.IsDBNull(10) ? null : npgsqlDataReader.GetDateTime(10),
            };
        }

        private static void AddBoundingBox2DParameters(NpgsqlCommand npgsqlCommand, HashSet<int> countyIds, BoundingBox2D boundingBox2D, double tolerance)
        {
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds.ToArray() });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMinX", NpgsqlDbType.Double) { Value = boundingBox2D.Min.X - tolerance });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMaxX", NpgsqlDbType.Double) { Value = boundingBox2D.Max.X + tolerance });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMinY", NpgsqlDbType.Double) { Value = boundingBox2D.Min.Y - tolerance });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sMaxY", NpgsqlDbType.Double) { Value = boundingBox2D.Max.Y + tolerance });
        }

        private static Building2DReference Create_Building2DReference(NpgsqlDataReader npgsqlDataReader)
        {
            return new Building2DReference
            {
                Id = npgsqlDataReader.GetInt64(0),
                CountyId = npgsqlDataReader.IsDBNull(1) ? null : (int?)npgsqlDataReader.GetInt32(1),
                Reference = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2),
                SubdivisionId = npgsqlDataReader.IsDBNull(3) ? null : (int?)npgsqlDataReader.GetInt32(3)
            };
        }

        private static async Task<int?> GetSubdivisionIdAsync(NpgsqlConnection npgsqlConnection, GIS.Classes.Building2D? building2D, double tolerance = Core.Constants.Tolerance.MacroDistance)
        {
            if (npgsqlConnection is null || building2D?.PolygonalFace2D is not PolygonalFace2D polygonalFace2D)
            {
                return null;
            }

            List<AdministrativeAreal2D>? administrativeAreal2Ds = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, polygonalFace2D.GetBoundingBox(), [AdministrativeArealType.Subdivision], tolerance);
            if (administrativeAreal2Ds is null || administrativeAreal2Ds.Count == 0)
            {
                return null;
            }

            List<Tuple<AdministrativeAreal2D, AdministrativeDivision>> tuples_AdministrativeDivision = [];
            List<Tuple<AdministrativeAreal2D, AdministrativeSubdivision>> tuples_AdministrativeSubdivision = [];

            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
            {
                GIS.Classes.AdministrativeAreal2D? administrativeAreal2D_GIS = administrativeAreal2D.ToDiGi();
                if (administrativeAreal2D_GIS is null)
                {
                    continue;
                }

                if (administrativeAreal2D_GIS is AdministrativeDivision administrativeDivision)
                {
                    tuples_AdministrativeDivision.Add(new Tuple<AdministrativeAreal2D, AdministrativeDivision>(administrativeAreal2D, administrativeDivision));
                }
                else if (administrativeAreal2D_GIS is AdministrativeSubdivision administrativeSubdivision)
                {
                    tuples_AdministrativeSubdivision.Add(new Tuple<AdministrativeAreal2D, AdministrativeSubdivision>(administrativeAreal2D, administrativeSubdivision));
                }
            }

            List<Tuple<AdministrativeAreal2D, double>>? tuples_Area = null;

            if (tuples_AdministrativeSubdivision is null || tuples_AdministrativeSubdivision.Count == 0)
            {
                if (tuples_AdministrativeDivision is null || tuples_AdministrativeDivision.Count == 0)
                {
                    return null;
                }

                if (tuples_AdministrativeDivision.Count == 1)
                {
                    return tuples_AdministrativeDivision[0].Item1.Id;
                }

                tuples_Area = [];
                foreach (Tuple<AdministrativeAreal2D, AdministrativeDivision> tuple in tuples_AdministrativeDivision)
                {
                    if (tuple?.Item2?.PolygonalFace2D is not PolygonalFace2D polygonalFace2D_AdministrativeDivision)
                    {
                        continue;
                    }

                    List<PolygonalFace2D>? polygonal2Ds_Intersection = Geometry.Planar.Query.Intersection(polygonalFace2D, polygonalFace2D_AdministrativeDivision);

                    double area = 0;
                    if (polygonal2Ds_Intersection is not null && polygonal2Ds_Intersection.Count != 0)
                    {
                        area = polygonal2Ds_Intersection.ConvertAll(x => x.GetArea()).Sum();
                    }

                    if (area <= tolerance)
                    {
                        continue;
                    }

                    tuples_Area.Add(new Tuple<AdministrativeAreal2D, double>(tuple.Item1, area));
                }

                if (tuples_Area.Count != 0)
                {
                    tuples_Area.Sort((x, y) =>
                    {
                        int result = y.Item2.CompareTo(x.Item2);
                        return result != 0 ? result : x.Item1.Id.CompareTo(y.Item1.Id);
                    });

                    return tuples_Area[0].Item1.Id;
                }

                return null;
            }

            if (tuples_AdministrativeSubdivision.Count == 1)
            {
                return tuples_AdministrativeSubdivision[0].Item1.Id;
            }

            // The subdivision layer nests - a city, its districts and their neighbourhoods are all subdivisions of the
            // one municipality - so a building is usually inside several of them at once, and every container yields
            // the same intersection: the building's own area. The intersection alone cannot choose between them, and
            // breaking that tie by the lowest identifier filed 155 307 Warsaw buildings under the 49 lowest ids and
            // none under any district (DiGi.GIS.PostgreSQL#76). The tie goes to the smallest container instead: the
            // most specific unit the building sits in, which is what the per-building consumers (occupancy, building
            // data) need exactly one of (DiGi.GIS.PostgreSQL#77). The identifier stays only as the last resort.
            List<(int Id, double Area_Intersection, double Area_Container)> tuples_Subdivision = [];
            foreach (Tuple<AdministrativeAreal2D, AdministrativeSubdivision> tuple in tuples_AdministrativeSubdivision)
            {
                if (tuple?.Item2?.PolygonalFace2D is not PolygonalFace2D polygonalFace2D_AdministrativeSubdivision)
                {
                    continue;
                }

                List<PolygonalFace2D>? polygonal2Ds_Intersection = Geometry.Planar.Query.Intersection(polygonalFace2D, polygonalFace2D_AdministrativeSubdivision);

                double area_Intersection = 0;
                if (polygonal2Ds_Intersection is not null && polygonal2Ds_Intersection.Count != 0)
                {
                    area_Intersection = polygonal2Ds_Intersection.ConvertAll(x => x.GetArea()).Sum();
                }

                if (area_Intersection <= tolerance)
                {
                    continue;
                }

                tuples_Subdivision.Add((tuple.Item1.Id, area_Intersection, polygonalFace2D_AdministrativeSubdivision.GetArea()));
            }

            return Query.SubdivisionId(tuples_Subdivision, tolerance);
        }

        private static async Task<List<Building2D>?> ReadAsync_Building2D(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_Building2D(npgsqlDataReader, cancellationToken);
        }

        private static async Task<List<Building2D>> ReadAsync_Building2D(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<Building2D> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_Building2D(npgsqlDataReader));
            }

            return result;
        }

        private static async Task<List<Building2DReference>?> ReadAsync_Building2DReference(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_Building2DReference(npgsqlDataReader, cancellationToken);
        }

        /// <summary>
        /// Deletes the rows holding the given references under a single county row.
        /// <para>A reference is unique only per <c>county_id</c>: the same building is held once per county row it was imported under, so a delete has to name the row as well as the reference. Deleting by reference alone would take the building out of every part of the county.</para>
        /// <para>Intended for repairing the parts a building was filed under by mistake. It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it, and make sure the building survives under the part it belongs to first.</para>
        /// </summary>
        /// <param name="references">The references to delete.</param>
        /// <param name="countyId">The identifier of the county row to delete them from.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, which is how many of the references were really there.</returns>
        public async Task<HashSet<long>?> RemoveAsync(IEnumerable<string>? references, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            string[] references_Array = [.. references];
            if (references_Array.Length == 0)
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            // UNNEST keeps this one statement rather than one per reference, and RETURNING reports what was
            // really removed - the count is the only evidence that the delete matched what was intended.
            string commandText = $@"
                DELETE FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId
                  AND reference = ANY(@references)
                RETURNING id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("countyId", countyId);
            npgsqlCommand.Parameters.AddWithValue("references", references_Array);

            HashSet<long> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(npgsqlDataReader.GetInt64(0));
            }

            return result;
        }

        private static async Task<List<Building2DReference>> ReadAsync_Building2DReference(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<Building2DReference> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_Building2DReference(npgsqlDataReader));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves duplicate building references that occur across multiple counties, ordered by collision count descending.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the query.</param>
        /// <param name="limit">The maximum number of duplicate references to return. Defaults to 100.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of <see cref="Building2DReferenceDuplicate"/> instances if any duplicates exist; otherwise, null.</returns>
        public static async Task<List<Building2DReferenceDuplicate>?> GetDuplicateReferencesAsync(NpgsqlConnection? npgsqlConnection, int limit = 100, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT reference, COUNT(*) AS count, ARRAY_AGG(county_id ORDER BY county_id) AS county_ids
                FROM {Constants.TableName.Building2D}
                GROUP BY reference
                HAVING COUNT(*) > 1
                ORDER BY count DESC
                LIMIT @limit;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("limit", limit);

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
        /// Asynchronously retrieves duplicate building references that occur across multiple counties, ordered by collision count descending.
        /// </summary>
        /// <param name="limit">The maximum number of duplicate references to return. Defaults to 100.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of <see cref="Building2DReferenceDuplicate"/> instances if any duplicates exist; otherwise, null.</returns>
        public async Task<List<Building2DReferenceDuplicate>?> GetDuplicateReferencesAsync(int limit = 100, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetDuplicateReferencesAsync(npgsqlConnection, limit, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts, for every polygon part of every multi-part county, the buildings it holds whose bounding box lies outside that part.
        /// <para>A county code names one <c>administrative_areal_2d</c> row per polygon part, and only geometry can say which of them a building belongs to. This measures how badly the stored answer disagrees with geometry, without deserializing anything: a building whose stored box does not intersect the box of the part holding it cannot be inside that part.</para>
        /// <para>Counted rows are <b>certainly</b> misfiled, uncounted ones merely unproven - a building inside the box of a part but outside its polygon needs the polygon to settle and is not counted here. The figure is therefore a lower bound, which is what makes it safe to read as the number of rows known to be wrong before and after a repair.</para>
        /// <para>Single-part codes are left out entirely: with one part there is nothing to be filed under by mistake. Only parts actually holding buildings are returned.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the query.</param>
        /// <param name="code">An optional county code to restrict the measurement to. When null every multi-part code is measured.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one entry per county part holding buildings, ordered by code then part identifier, an empty list when no multi-part code holds any, or null when the connection is null.</returns>
        public static async Task<List<Building2DCountyPartMismatchResult>?> GetCountyPartMismatchesAsync(NpgsqlConnection? npgsqlConnection, string? code = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // The parts are read first and their boxes carried into the second statement as arrays, rather
            // than joining the two tables in one query. building_2d holds millions of rows across 406
            // partitions, and only an explicit county_id = ANY(...) prunes it to the handful of partitions
            // a multi-part code owns - a join against a CTE gives the planner no such promise.
            string commandText_Part = $@"
                SELECT id, code, min_x, min_y, max_x, max_y
                FROM {Constants.TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                  AND code IS NOT NULL
                  AND min_x IS NOT NULL AND min_y IS NOT NULL AND max_x IS NOT NULL AND max_y IS NOT NULL
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
            List<double> minXs = [];
            List<double> minYs = [];
            List<double> maxXs = [];
            List<double> maxYs = [];

            await using (NpgsqlCommand npgsqlCommand_Part = new(commandText_Part, npgsqlConnection))
            {
                npgsqlCommand_Part.CommandTimeout = commandTimeout;
                npgsqlCommand_Part.Parameters.AddWithValue("typeId", (short)AdministrativeArealType.County);
                npgsqlCommand_Part.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Text) { Value = string.IsNullOrWhiteSpace(code) ? DBNull.Value : code });

                await using NpgsqlDataReader npgsqlDataReader_Part = await npgsqlCommand_Part.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader_Part.ReadAsync(cancellationToken))
                {
                    countyIds.Add(npgsqlDataReader_Part.GetInt32(0));
                    codes.Add(npgsqlDataReader_Part.GetString(1));
                    minXs.Add(npgsqlDataReader_Part.GetDouble(2));
                    minYs.Add(npgsqlDataReader_Part.GetDouble(3));
                    maxXs.Add(npgsqlDataReader_Part.GetDouble(4));
                    maxYs.Add(npgsqlDataReader_Part.GetDouble(5));
                }
            }

            List<Building2DCountyPartMismatchResult> result = [];

            if (countyIds.Count == 0)
            {
                return result;
            }

            Dictionary<string, List<int>> countyIds_ByCode = [];
            for (int i = 0; i < countyIds.Count; i++)
            {
                if (!countyIds_ByCode.TryGetValue(codes[i], out List<int>? countyIds_Code) || countyIds_Code is null)
                {
                    countyIds_Code = [];
                    countyIds_ByCode[codes[i]] = countyIds_Code;
                }

                countyIds_Code.Add(countyIds[i]);
            }

            // A row with no stored box is left uncounted rather than reported as outside: box() is strict,
            // so a null ordinate makes the comparison null and FILTER drops the row. Nothing is claimed
            // about a building whose extent was never written.
            string commandText_Count = $@"
                WITH part AS (
                    SELECT *
                    FROM unnest(@countyIds, @minXs, @minYs, @maxXs, @maxYs) AS t(county_id, min_x, min_y, max_x, max_y)
                )
                SELECT b.county_id,
                       COUNT(*) AS count,
                       COUNT(*) FILTER (
                           WHERE NOT (box(point(b.min_x, b.min_y), point(b.max_x, b.max_y))
                                   && box(point(p.min_x, p.min_y), point(p.max_x, p.max_y)))) AS count_outside
                FROM {Constants.TableName.Building2D} b
                JOIN part p ON p.county_id = b.county_id
                WHERE b.county_id = ANY(@countyIds)
                GROUP BY b.county_id;";

            Dictionary<int, Tuple<long, long>> counts_ByCountyId = [];

            await using (NpgsqlCommand npgsqlCommand_Count = new(commandText_Count, npgsqlConnection))
            {
                npgsqlCommand_Count.CommandTimeout = commandTimeout;
                npgsqlCommand_Count.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds.ToArray() });
                npgsqlCommand_Count.Parameters.Add(new NpgsqlParameter("minXs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = minXs.ToArray() });
                npgsqlCommand_Count.Parameters.Add(new NpgsqlParameter("minYs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = minYs.ToArray() });
                npgsqlCommand_Count.Parameters.Add(new NpgsqlParameter("maxXs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = maxXs.ToArray() });
                npgsqlCommand_Count.Parameters.Add(new NpgsqlParameter("maxYs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = maxYs.ToArray() });

                await using NpgsqlDataReader npgsqlDataReader_Count = await npgsqlCommand_Count.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader_Count.ReadAsync(cancellationToken))
                {
                    counts_ByCountyId[npgsqlDataReader_Count.GetInt32(0)] = new Tuple<long, long>(npgsqlDataReader_Count.GetInt64(1), npgsqlDataReader_Count.GetInt64(2));
                }
            }

            for (int i = 0; i < countyIds.Count; i++)
            {
                if (!counts_ByCountyId.TryGetValue(countyIds[i], out Tuple<long, long>? counts) || counts is null)
                {
                    continue;
                }

                result.Add(new Building2DCountyPartMismatchResult(codes[i], countyIds[i], countyIds_ByCode[codes[i]], counts.Item1, counts.Item2));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously counts, for every polygon part of every multi-part county, the buildings it holds whose bounding box lies outside that part.
        /// <para>A county code names one <c>administrative_areal_2d</c> row per polygon part, and only geometry can say which of them a building belongs to. This measures how badly the stored answer disagrees with geometry, without deserializing anything: a building whose stored box does not intersect the box of the part holding it cannot be inside that part.</para>
        /// <para>Counted rows are <b>certainly</b> misfiled, uncounted ones merely unproven - a building inside the box of a part but outside its polygon needs the polygon to settle and is not counted here. The figure is therefore a lower bound, which is what makes it safe to read as the number of rows known to be wrong before and after a repair.</para>
        /// <para>Single-part codes are left out entirely: with one part there is nothing to be filed under by mistake. Only parts actually holding buildings are returned.</para>
        /// </summary>
        /// <param name="code">An optional county code to restrict the measurement to. When null every multi-part code is measured.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one entry per county part holding buildings, ordered by code then part identifier, an empty list when no multi-part code holds any, or null when the connection could not be created.</returns>
        public async Task<List<Building2DCountyPartMismatchResult>?> GetCountyPartMismatchesAsync(string? code = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetCountyPartMismatchesAsync(npgsqlConnection, code, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously moves the rows holding the given references onto <paramref name="countyId"/>, so that a building sits under the county polygon part its footprint lies in.
        /// <para>This repairs buildings filed under the wrong part of a multi-part county. Everything keyed on a building follows <c>building_2d</c>, so this is the move the other tables are made consistent with - and their rows have to be moved as well, or they answer nothing under the part the building has left.</para>
        /// <para><c>county_id</c> is the <b>partition key</b>, so this is a row movement between partitions rather than an ordinary column update. The destination partition is created first - PostgreSQL cannot move a row into a partition that does not exist, and the part a county belongs to may never have held one - and the identifiers of the rows are preserved, so anything holding an <c>id</c> from before the call still addresses the same record.</para>
        /// <para><b>Nothing is deleted.</b> The table constrains <c>UNIQUE (reference, county_id)</c>, so a row cannot move onto a destination already holding that reference, and two rows carrying one reference under two different parts cannot both arrive. Such a row is left exactly where it is and its reference is <b>not</b> reported, so a caller comparing the result against what it passed in learns which references still hold something to settle by hand rather than having it silently discarded here.</para>
        /// <para>Naming the parts the rows may currently sit under in <paramref name="countyIds_Source"/> is what keeps the cost down: without it the statement cannot be pruned to a partition and reads every partition of the table.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="references">The references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county polygon part every one of those references should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched.</param>
        /// <param name="batchSize">The number of references sent in one statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references that had a row moved, or null when no references were given or the connection is null.</returns>
        public static async Task<HashSet<string>?> RefreshCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int countyId, IEnumerable<int>? countyIds_Source = null, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            HashSet<string> references_Set = [];
            foreach (string reference in references)
            {
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    references_Set.Add(reference);
                }
            }

            HashSet<string> result = [];

            if (references_Set.Count == 0)
            {
                return result;
            }

            bool exists = await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, Constants.TableName.Building2D);
            if (!exists)
            {
                return result;
            }

            // A row cannot move into a partition that does not exist, and the part a whole county belongs to
            // may never have held a building - without this the update fails outright. It is an idempotent
            // catalog check once the partition is there.
            await npgsqlConnection.TableAsync_Building2D_Partition(countyId, commandTimeout, cancellationToken);

            List<string> references_Unique = [.. references_Set];
            int[]? countyIds_Array = countyIds_Source is null ? null : [.. new HashSet<int>(countyIds_Source)];

            // NOT EXISTS is the collision guard for UNIQUE (reference, county_id). It is cheap despite the
            // surrounding scan: county_id is fixed, so it prunes to the destination partition and probes the
            // index that constraint is already backed by.
            //
            // ROW_NUMBER guards the other collision - two rows carrying one reference under two different
            // parts both pass NOT EXISTS and would then collide with each other on arrival. One moves, the
            // rest stay. created_at alone does not settle which one, because a bulk write stamps every row
            // of a batch identically; id is what decides.
            //
            // The update addresses the row by its primary key with the partition key present, so it is a
            // pruned index lookup rather than a second scan. ctid would not do - it is unique only within a
            // partition.
            string commandText = $@"
                WITH stray AS MATERIALIZED (
                    SELECT t.id AS id,
                           t.county_id AS county_id,
                           ROW_NUMBER() OVER (PARTITION BY t.reference ORDER BY t.created_at DESC, t.id DESC) AS move_rank
                    FROM {Constants.TableName.Building2D} t
                    WHERE t.reference = ANY(@references)
                      AND t.county_id <> @countyId
                      AND (@countyIds IS NULL OR t.county_id = ANY(@countyIds))
                      AND NOT EXISTS (
                              SELECT 1
                              FROM {Constants.TableName.Building2D} t_Target
                              WHERE t_Target.county_id = @countyId
                                AND t_Target.reference = t.reference)
                )
                UPDATE {Constants.TableName.Building2D} t
                SET county_id = @countyId
                FROM stray s
                WHERE t.id = s.id
                  AND t.county_id = s.county_id
                  AND s.move_rank = 1
                RETURNING t.reference;";

            int batchSize_Effective = batchSize < 1 ? 1 : batchSize;

            for (int i = 0; i < references_Unique.Count; i += batchSize_Effective)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string[] references_Batch = [.. references_Unique.GetRange(i, Math.Min(batchSize_Effective, references_Unique.Count - i))];

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId);
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Batch });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array is null ? (object)DBNull.Value : countyIds_Array });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    result.Add(npgsqlDataReader.GetString(0));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously moves the rows holding the given references onto <paramref name="countyId"/>, so that a building sits under the county polygon part its footprint lies in.
        /// <para>The move, its collision guards and what it costs are described on the overload taking a connection.</para>
        /// </summary>
        /// <param name="references">The references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county polygon part every one of those references should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched.</param>
        /// <param name="batchSize">The number of references sent in one statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references that had a row moved, or null when no references were given or the connection could not be created.</returns>
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
        /// Asynchronously retrieves overall building reference uniqueness metrics across all partitions in the database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the query.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, returning a <see cref="Building2DReferenceUniquenessSummary"/> object containing total, distinct, and duplicate metrics; or null if the connection is null.</returns>
        public static async Task<Building2DReferenceUniquenessSummary?> GetReferenceUniquenessSummaryAsync(NpgsqlConnection? npgsqlConnection, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT
                    COUNT(*) AS total_count,
                    COUNT(DISTINCT reference) AS distinct_count
                FROM {Constants.TableName.Building2D};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            if (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                long totalCount = npgsqlDataReader.IsDBNull(0) ? 0 : npgsqlDataReader.GetInt64(0);
                long distinctReferenceCount = npgsqlDataReader.IsDBNull(1) ? 0 : npgsqlDataReader.GetInt64(1);
                long duplicateReferenceCount = totalCount - distinctReferenceCount;
                bool isUnique = duplicateReferenceCount == 0;

                return new Building2DReferenceUniquenessSummary(totalCount, distinctReferenceCount, duplicateReferenceCount, isUnique);
            }

            return null;
        }

        /// <summary>
        /// Asynchronously retrieves overall building reference uniqueness metrics across all partitions in the database.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, returning a <see cref="Building2DReferenceUniquenessSummary"/> object containing total, distinct, and duplicate metrics; or null if connection fails.</returns>
        public async Task<Building2DReferenceUniquenessSummary?> GetReferenceUniquenessSummaryAsync(int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetReferenceUniquenessSummaryAsync(npgsqlConnection, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the buildings of one county whose subdivision has not been resolved.
        /// <para>These buildings belong to no subdivision, so anything driven by subdivisions - the building data update among them - never visits them. The figure is therefore the part of a coverage shortfall that no run can currently close.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county to count.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or -1 when no connection could be built.</returns>
        public async Task<long> GetCountWithoutSubdivisionAsync(int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetCountWithoutSubdivisionAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the buildings of one county whose subdivision has not been resolved, over the given connection.
        /// </summary>
        /// <param name="npgsqlConnection">The Npgsql connection instance used to execute the query. This value can be null.</param>
        /// <param name="countyId">The identifier of the county to count.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or -1 when the connection is null.</returns>
        public static async Task<long> GetCountWithoutSubdivisionAsync(NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            string commandText = $@"
                SELECT COUNT(*)
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId
                  AND subdivision_id IS NULL;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });

            object? @object = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);

            return @object is long @long ? @long : -1;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building2D"/> objects for a specified county identifier whose subdivision has not been resolved.
        /// </summary>
        /// <param name="countyId">The integer identifier of the county used to filter the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects if matches are found; otherwise, null.</returns>
        public async Task<List<Building2D>?> GetBuilding2DsWithoutSubdivisionAsync(int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBuilding2DsWithoutSubdivisionAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Building2D"/> objects for a specified county identifier whose subdivision has not been resolved, over the given connection.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command. This value can be null.</param>
        /// <param name="countyId">The integer identifier of the county used to filter the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects if matches are found; otherwise, null.</returns>
        public static async Task<List<Building2D>?> GetBuilding2DsWithoutSubdivisionAsync(NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            string commandText = $@"
                SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId AND subdivision_id IS NULL;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });

            return await ReadAsync_Building2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the buildings of one county that the subdivision loop cannot reach: those with no subdivision, and those whose subdivision is not in the named in-scope set.
        /// <para>The in-scope set is exactly what the subdivision loop reaches under the county part, so this returns its complement - the unassigned and the cross-county border buildings the final per-county pass must write.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county polygon part used to filter the search.</param>
        /// <param name="subdivisionIds_InScope">The subdivision identifiers the subdivision loop reaches under this county part. Buildings whose subdivision is not in this set are returned; an empty or null set returns every building that names a subdivision.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects if matches are found; otherwise, null.</returns>
        public async Task<List<Building2D>?> GetBuilding2DsUnreachedByCountyAsync(int countyId, IEnumerable<int>? subdivisionIds_InScope, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBuilding2DsUnreachedByCountyAsync(npgsqlConnection, countyId, subdivisionIds_InScope, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the buildings of one county that the subdivision loop cannot reach, over the given connection.
        /// <para>The in-scope set is exactly what the subdivision loop reaches under the county part, so this returns its complement - the unassigned and the cross-county border buildings the final per-county pass must write.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command. This value can be null.</param>
        /// <param name="countyId">The identifier of the county polygon part used to filter the search.</param>
        /// <param name="subdivisionIds_InScope">The subdivision identifiers the subdivision loop reaches under this county part. Buildings whose subdivision is not in this set are returned; an empty or null set returns every building that names a subdivision.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Building2D"/> objects if matches are found; otherwise, null.</returns>
        public static async Task<List<Building2D>?> GetBuilding2DsUnreachedByCountyAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<int>? subdivisionIds_InScope, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            int[] subdivisionIds_Array = subdivisionIds_InScope?.ToArray() ?? [];

            string commandText = $@"
                SELECT id, county_id, reference, code, min_x, min_y, max_x, max_y, subdivision_id, object, created_at
                FROM {Constants.TableName.Building2D}
                WHERE county_id = @countyId
                  AND (subdivision_id IS NULL OR NOT (subdivision_id = ANY(@subdivisionIds_InScope)));";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("subdivisionIds_InScope", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = subdivisionIds_Array });

            return await ReadAsync_Building2D(npgsqlCommand, cancellationToken);
        }
    }
}
