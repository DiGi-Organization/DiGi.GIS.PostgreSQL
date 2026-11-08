using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that goes back for the terrain points a sampling run left behind.
    /// <para>A run of tens of millions of single requests to a public service loses a few of them. The point is simply absent afterwards, and nothing in the run says which: the tallies count what went unanswered without naming it. Re-running the sampling task does recover them, but it pays to read back every tile of every county to find the handful that are short - hours of work to repair minutes of it.</para>
    /// <para>This asks the question directly instead. A county held in several pieces is one row per piece sharing one code, so its pieces are walked as one group and measured together by <see cref="TerrainPointPostgreSQLConverter.GetCoverageByCountyIdAsync(IEnumerable{int}, Dictionary{int, PolygonalFace2D}, BoundingBox2D, double, Point2D, double, int, long, int, int, CancellationToken)"/> - the same comparison the coverage and gap endpoints report - and only the nodes it names are sampled, each filled node filed under the piece containing it. A county that is already complete costs the measurement and nothing else.</para>
    /// <para>The write is the ordinary one, which leaves points already stored as they are, so the task is idempotent and a run that was stopped can simply be repeated.</para>
    /// <para>What this cannot do is invent a spacing. <see cref="PostgreSQLTerrainPointFillGapsOptions.GridSize"/> has to be the spacing the county was sampled at, or every node in between reads as a gap and the repair becomes a densification.</para>
    /// </summary>
    public class PostgreSQLTerrainPointFillGapsTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// Bulk reads over a whole county exceed the 30 second default; allow up to 10 minutes per statement.
        /// </summary>
        private const int commandTimeout = 600;

        /// <summary>
        /// The most unresolved coordinates kept in memory for the caller. The count is never capped, only the list.
        /// </summary>
        private const int count_Point2Ds_Unresolved_Maximum = 1000;

        /// <summary>
        /// The most unresolved coordinates named in a single log entry, so one bad county cannot fill the log.
        /// </summary>
        private const int count_Point2Ds_Unresolved_Logged = 20;

        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;
        protected readonly HttpClient? httpClient;

        private readonly List<Point2D> point2Ds_Unresolved = [];

        /// <summary>
        /// Constructor with Dependency Injection.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to reach the elevation service.</param>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager used to read the areas and write the points.</param>
        public PostgreSQLTerrainPointFillGapsTask(HttpClient? httpClient, GISPostgreSQLConverterManager? gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Gets the number of counties that failed outright and were stepped over.
        /// <para>Each one is logged with the exception that caused it, so this figure is a count of entries to go and read rather than the whole of what is known.</para>
        /// </summary>
        public long FailedBatchCount { get; private set; }

        /// <summary>
        /// Gets the number of nodes found to be missing across every county measured.
        /// <para>What the run set out to fill. <see cref="PointCount"/> is what it managed, and <see cref="UnresolvedPointCount"/> is the difference the elevation service would not answer for.</para>
        /// </summary>
        public long MissingCount { get; private set; }

        /// <summary>
        /// Gets the coordinates of the nodes the elevation service returned nothing for, up to the first thousand of them.
        /// <para>A node that stays unresolved through a repair is a different thing to one that was merely lost: the service has now been asked for it twice. These are the ones to look at by hand.</para>
        /// </summary>
        public IReadOnlyList<Point2D> Point2Ds_Unresolved
        {
            get
            {
                return point2Ds_Unresolved;
            }
        }

        /// <summary>
        /// Gets the number of points the database accepted.
        /// </summary>
        public long PointCount { get; private set; }

        /// <summary>
        /// Gets the configuration for the PostgreSQL operation.
        /// These options will be used when the task is started.
        /// </summary>
        public PostgreSQLTerrainPointFillGapsOptions PostgreSQLTerrainPointFillGapsOptions { get; set; } = new PostgreSQLTerrainPointFillGapsOptions();

        /// <summary>
        /// Gets the number of points the database declined to store.
        /// </summary>
        public long RejectionCount { get; private set; }

        /// <summary>
        /// Gets the number of missing nodes the elevation service returned nothing for, even after the retries allowed.
        /// </summary>
        public long UnresolvedPointCount { get; private set; }

        /// <summary>
        /// Executes the background task, measuring each county against the lattice and sampling only the nodes it is short of.
        /// </summary>
        /// <param name="progress">A progress reporter carrying the running total of points stored.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true unless the run was cancelled.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            MissingCount = 0;
            PointCount = 0;
            RejectionCount = 0;
            UnresolvedPointCount = 0;
            point2Ds_Unresolved.Clear();
            FailedBatchCount = 0;

            if (httpClient is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no elevation service client - nothing can be sampled", nameof(PostgreSQLTerrainPointFillGapsTask));
                return false;
            }

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the administrative areas cannot be read", nameof(PostgreSQLTerrainPointFillGapsTask), nameof(AdministrativeAreal2DPostgreSQLConverter));
                return false;
            }

            TerrainPointPostgreSQLConverter? terrainPointPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<TerrainPointPostgreSQLConverter>();
            if (terrainPointPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the points cannot be written", nameof(PostgreSQLTerrainPointFillGapsTask), nameof(TerrainPointPostgreSQLConverter));
                return false;
            }

            double gridSize = PostgreSQLTerrainPointFillGapsOptions.GridSize;
            int tileSize = PostgreSQLTerrainPointFillGapsOptions.TileSize;
            if (double.IsNaN(gridSize) || double.IsInfinity(gridSize) || gridSize <= 0 || tileSize < 1)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: grid size {GridSize} and tile size {TileSize} do not describe a lattice", nameof(PostgreSQLTerrainPointFillGapsTask), gridSize, tileSize);
                return false;
            }

            // Capped at half a step, for the same reason the sampling task caps it: neighbouring nodes are one step
            // apart, so anything larger would let a stored point stand in for the node next to the one it belongs to.
            double tolerance = PostgreSQLTerrainPointFillGapsOptions.Tolerance;
            if (double.IsNaN(tolerance) || tolerance < 0)
            {
                tolerance = Core.Constants.Tolerance.MacroDistance;
            }

            if (tolerance > gridSize / 2)
            {
                tolerance = gridSize / 2;
            }

            int batchSize = PostgreSQLTerrainPointFillGapsOptions.BatchSize < 1 ? 1 : PostgreSQLTerrainPointFillGapsOptions.BatchSize;
            int maxConcurrentRequests = PostgreSQLTerrainPointFillGapsOptions.MaxConcurrentRequests;
            int retryCount = PostgreSQLTerrainPointFillGapsOptions.RetryCount;
            TimeSpan retryDelay = TimeSpan.FromMilliseconds(PostgreSQLTerrainPointFillGapsOptions.RetryDelayMilliseconds);
            Point2D origin = new(PostgreSQLTerrainPointFillGapsOptions.OriginX, PostgreSQLTerrainPointFillGapsOptions.OriginY);

            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(terrainPointPostgreSQLConverter.ConnectionData, Constants.TableName.TerrainPoint, cancellationToken: cancellationToken))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no terrain point table exists, so nothing has ever been sampled and there is nothing to repair", nameof(PostgreSQLTerrainPointFillGapsTask));
                return false;
            }

            // Identifiers, then codes: the options name county parts, and a county held in several pieces is one
            // row per piece sharing one code. Each named piece is widened to every piece of its code, so a
            // multi-part county is repaired as one group - measured against what all of its partitions hold
            // together, and with each filled node filed under the piece containing it.
            HashSet<int>? countyIds = PostgreSQLTerrainPointFillGapsOptions.CountyIds;
            countyIds ??= await administrativeAreal2DPostgreSQLConverter.GetIdsAsync(Enums.AdministrativeArealType.County, cancellationToken: cancellationToken);
            if (countyIds is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the counties could not be read - the run cannot be scoped", nameof(PostgreSQLTerrainPointFillGapsTask));
                return false;
            }

            if (countyIds.Count == 0)
            {
                Serilog.Modify.Log("{Type}: no county was named - nothing to repair", nameof(PostgreSQLTerrainPointFillGapsTask));
                return true;
            }

            // Sorted so that a run stopped and started again covers the counties in the same order.
            List<int> countyIds_Sorted = [.. countyIds];
            countyIds_Sorted.Sort();

            List<List<int>>? countyIdGroups = await administrativeAreal2DPostgreSQLConverter.CountyIdGroupsAsync(countyIds_Sorted, commandTimeout, cancellationToken);
            if (countyIdGroups is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the counties could not be resolved into code groups - the run would not know which parts share a county", nameof(PostgreSQLTerrainPointFillGapsTask));
                return false;
            }

            Serilog.Modify.Log(
                "{Type} started: {CountyCount} counties in {CountyGroupCount} groups, grid {GridSize}, tile {TileSize}, origin {OriginX}/{OriginY}, tolerance {Tolerance}, {MaxConcurrentRequests} requests in flight, {RetryCount} retries",
                nameof(PostgreSQLTerrainPointFillGapsTask), countyIds_Sorted.Count, countyIdGroups.Count, gridSize, tileSize, origin.X, origin.Y, tolerance, maxConcurrentRequests, retryCount);

            foreach (List<int> countyIds_Group in countyIdGroups)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // The group's lowest identifier names it in the log and receives the nodes that reach no part.
                int countyId = countyIds_Group[0];

                try
                {
                    List<AdministrativeAreal2D>? administrativeAreal2Ds;
                    Dictionary<int, PolygonalFace2D> polygonalFace2Ds_CountyById = [];

                    await using (NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(administrativeAreal2DPostgreSQLConverter.ConnectionData))
                    {
                        if (npgsqlConnection is null)
                        {
                            FailedBatchCount++;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill skipped - county {CountyId}, no connection to the administrative database could be built", countyId);
                            continue;
                        }

                        await npgsqlConnection.OpenAsync(cancellationToken);

                        // The read widens the parent to every part sharing its code, which is right here because
                        // the group IS that widening: the subdivisions of the whole county decide which nodes are
                        // on land, and the parts' own outlines, read below, decide which partition each filled
                        // node is filed under.
                        administrativeAreal2Ds = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(npgsqlConnection, Enums.AdministrativeArealType.Subdivision, countyId, cancellationToken: cancellationToken);

                        if (administrativeAreal2Ds is not null && administrativeAreal2Ds.Count > 0 && countyIds_Group.Count > 1)
                        {
                            // The parts' own outlines are read only where the county is held in several pieces:
                            // one part is the whole answer, and testing every node against a single face would
                            // only restate the identifier the group already carries.
                            List<AdministrativeAreal2D>? administrativeAreal2Ds_County = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(npgsqlConnection, countyIds_Group, commandTimeout, cancellationToken);
                            polygonalFace2Ds_CountyById = administrativeAreal2Ds_County.PolygonalFace2DsById();

                            if (polygonalFace2Ds_CountyById.Count != countyIds_Group.Count)
                            {
                                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill - county {CountyId} holds {PartCount} parts but only {FaceCount} of them carries an outline - the nodes of the rest are filed under the lowest part", countyId, countyIds_Group.Count, polygonalFace2Ds_CountyById.Count);
                            }
                        }
                    }

                    if (administrativeAreal2Ds is null || administrativeAreal2Ds.Count == 0)
                    {
                        Serilog.Modify.Log("Terrain gap fill skipped - county {CountyId} has no subdivisions, so nothing decides which of its nodes are on land", countyId);
                        continue;
                    }

                    Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = administrativeAreal2Ds.PolygonalFace2DsById();
                    if (polygonalFace2Ds_ById.Count == 0)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill skipped - county {CountyId} has {Count} subdivisions but none of them carries an outline", countyId, administrativeAreal2Ds.Count);
                        continue;
                    }

                    // No node ceiling: this is a background task working through a county it was told to repair,
                    // not an unauthenticated request that has to be defended against its own size. Every missing
                    // node is wanted, so the list is not capped either. Measured against every partition of the
                    // group, so a node stored under whichever part holds it counts as stored.
                    TerrainPointCoverageResult? terrainPointCoverageResult = await terrainPointPostgreSQLConverter.GetCoverageByCountyIdAsync(countyIds_Group, polygonalFace2Ds_ById, null, gridSize, origin, tolerance, int.MaxValue, 0, tileSize, commandTimeout, cancellationToken);
                    if (terrainPointCoverageResult is null)
                    {
                        FailedBatchCount++;
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill skipped - county {CountyId} could not be measured against the lattice", countyId);
                        continue;
                    }

                    List<Point2D>? point2Ds_Missing = terrainPointCoverageResult.Point2Ds_Missing;
                    if (point2Ds_Missing is null || point2Ds_Missing.Count == 0)
                    {
                        Serilog.Modify.Log(
                            "Terrain gap fill - county {CountyId} is complete at grid {GridSize}: {ExpectedCount} nodes, {OffGridCount} stored off grid",
                            countyId, gridSize, terrainPointCoverageResult.ExpectedCount, terrainPointCoverageResult.OffGridCount);
                        continue;
                    }

                    MissingCount += point2Ds_Missing.Count;

                    Serilog.Modify.Log(
                        "Terrain gap fill - county {CountyId} is short {MissingCount} of {ExpectedCount} nodes at grid {GridSize}",
                        countyId, point2Ds_Missing.Count, terrainPointCoverageResult.ExpectedCount, gridSize);

                    long pointCount_County = PointCount;
                    long unresolvedPointCount_County = UnresolvedPointCount;

                    for (int index = 0; index < point2Ds_Missing.Count; index += batchSize)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int count_Batch = Math.Min(batchSize, point2Ds_Missing.Count - index);

                        List<Point2D> point2Ds_Batch = point2Ds_Missing.GetRange(index, count_Batch);

                        // The coverage already dropped the nodes that lie in no subdivision, but the write needs to
                        // know which subdivision each one belongs to, and only this answers that.
                        int?[]? subdivisionIds = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds_Batch, tolerance);
                        if (subdivisionIds is null)
                        {
                            FailedBatchCount++;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill batch skipped - county {CountyId}, {PointCount} nodes could not be placed in a subdivision", countyId, point2Ds_Batch.Count);
                            continue;
                        }

                        // One node, one part: the piece whose polygon contains the node is the partition it is
                        // filed under. A node that reaches no part - the residue a tolerance leaves on a part
                        // boundary, or a piece whose outline could not be read - is filed under the group's
                        // lowest identifier rather than dropped: dropping it would leave the gap it came to fill.
                        int?[]? countyIds_Routed = polygonalFace2Ds_CountyById.Count > 1 ? polygonalFace2Ds_CountyById.IdsByPoint2Ds(point2Ds_Batch, tolerance) : null;

                        List<int> countyIds_Kept = [];
                        int count_Unrouted = 0;
                        for (int i = 0; i < point2Ds_Batch.Count; i++)
                        {
                            if (countyIds_Routed is not null && countyIds_Routed[i] is int countyId_Routed)
                            {
                                countyIds_Kept.Add(countyId_Routed);
                            }
                            else
                            {
                                count_Unrouted++;
                                countyIds_Kept.Add(countyId);
                            }
                        }

                        if (count_Unrouted > 0)
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill nodes outside every part - county {CountyId}, {UnroutedCount} of {PointCount} filed under the lowest part", countyId, count_Unrouted, point2Ds_Batch.Count);
                        }

                        List<Point3D?>? point3Ds = await GIS.Query.ElevationsAsync(httpClient, point2Ds_Batch, maxConcurrentRequests, retryCount, retryDelay, cancellationToken);
                        if (point3Ds is null)
                        {
                            FailedBatchCount++;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill batch not sampled - county {CountyId}, the elevation service returned nothing for {PointCount} nodes", countyId, point2Ds_Batch.Count);
                            continue;
                        }

                        List<TerrainPoint> terrainPoints = [];
                        List<Point2D> point2Ds_Unresolved_Batch = [];
                        for (int i = 0; i < point3Ds.Count; i++)
                        {
                            if (point3Ds[i] is not Point3D point3D)
                            {
                                UnresolvedPointCount++;
                                point2Ds_Unresolved_Batch.Add(point2Ds_Batch[i]);
                                continue;
                            }

                            terrainPoints.Add(new TerrainPoint(countyIds_Kept[i], point3D, subdivisionIds[i]));
                        }

                        if (point2Ds_Unresolved_Batch.Count != 0)
                        {
                            foreach (Point2D point2D_Unresolved in point2Ds_Unresolved_Batch)
                            {
                                if (point2Ds_Unresolved.Count >= count_Point2Ds_Unresolved_Maximum)
                                {
                                    break;
                                }

                                point2Ds_Unresolved.Add(point2D_Unresolved);
                            }

                            List<string> texts = [];
                            foreach (Point2D point2D_Unresolved in point2Ds_Unresolved_Batch)
                            {
                                if (texts.Count >= count_Point2Ds_Unresolved_Logged)
                                {
                                    texts.Add("...");
                                    break;
                                }

                                texts.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1}", point2D_Unresolved.X, point2D_Unresolved.Y));
                            }

                            // A node that will not resolve on a second asking is worth naming: the service has now
                            // refused it twice, which is the difference between a point that was lost and a point
                            // there is no elevation for.
                            Serilog.Modify.Log(
                                Serilog.Enums.LogEventLevel.Warning,
                                "Terrain gap fill unresolved - county {CountyId}, {UnresolvedCount} of {AskedCount} asked: {Point2Ds}",
                                countyId, point2Ds_Unresolved_Batch.Count, point2Ds_Batch.Count, string.Join("; ", texts));
                        }

                        if (terrainPoints.Count == 0)
                        {
                            continue;
                        }

                        // The ordinary write, which leaves what is already stored as it is. Never the binary one:
                        // that cannot skip an existing point and aborts the whole batch on the first repeat.
                        TerrainPointUpdateResult? terrainPointUpdateResult = await terrainPointPostgreSQLConverter.UpdateAsync(terrainPoints, binaryInsert: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        if (terrainPointUpdateResult is null)
                        {
                            FailedBatchCount++;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Terrain gap fill batch not written - county {CountyId}, {PointCount} nodes sampled but the write returned nothing", countyId, terrainPoints.Count);
                            continue;
                        }

                        PointCount += terrainPointUpdateResult.Count;
                        RejectionCount += terrainPointUpdateResult.Rejections.Count;

                        progress.Report(PointCount);
                    }

                    Serilog.Modify.Log(
                        "Terrain gap fill - county {CountyId} done: {PointCount} of {MissingCount} filled, {UnresolvedPointCount} unresolved",
                        countyId, PointCount - pointCount_County, point2Ds_Missing.Count, UnresolvedPointCount - unresolvedPointCount_County);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    FailedBatchCount++;
                    Serilog.Modify.Log(exception, "Terrain gap fill county failed - county {CountyId}", countyId);
                    continue;
                }
            }

            bool cancelled = cancellationToken.IsCancellationRequested;

            Serilog.Modify.Log(
                cancelled ? Serilog.Enums.LogEventLevel.Warning : Serilog.Enums.LogEventLevel.Information,
                "{Type} finished{Cancelled}: {MissingCount} nodes missing, {PointCount} filled, {UnresolvedPointCount} unresolved, {RejectionCount} rejected, {FailedBatchCount} batches stepped over, over {CountyCount} counties",
                nameof(PostgreSQLTerrainPointFillGapsTask), cancelled ? " after being cancelled" : string.Empty, MissingCount, PointCount, UnresolvedPointCount, RejectionCount, FailedBatchCount, countyIds_Sorted.Count);

            return !cancelled;
        }
    }
}
