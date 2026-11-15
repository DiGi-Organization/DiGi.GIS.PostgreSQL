using DiGi.Analytical.Building.Solar;
using DiGi.Core.Classes;
using DiGi.EPW.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Analytical.Solar;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.Solar.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Solar.Classes
{
    /// <summary>
    /// Represents a background task that solves the annual solar radiation of every building of a county part for one or more neighbour radii and stores it in the building solar radiation table (DiGi.GIS.PostgreSQL#136).
    /// <para>Each building is solved exactly as the viewer solves it (DiGi.GIS.WebAPI.UI <c>SolarController</c>): the same building model detail level, the same neighbour circle (<c>DiGi.GIS.Analytical.Solar.Query.SolarNeighbourCircle</c>), the same nearest EPW station, the same solver options and solver version, and the same stored form - so a result does not depend on which of the two produced it. Radius 0 solves the building alone (<c>SolarRadiusIsolated</c>): it still shades and blocks the view of its own surfaces, but no neighbour does.</para>
    /// <para>A county part is solved in square tiles of <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.TileSize"/>, by the centroid of each building, at most <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize"/> buildings at a time: one read of the batch's models, one read of every neighbour any of them can see at the largest radius, and one write of the batch's results. The neighbours of each building and radius are then picked in memory, as <c>gis/buildingmodel/itemsbycircle</c> picks them - every model whose bounding box lies within or intersects the building's circle.</para>
    /// <para>A building that cannot be solved - no stored model, no location, no closed envelope, no weather, or above a ceiling - is counted and stepped over. A county part whose buildings, models or neighbours cannot be read, or whose results cannot be written, is failed and logged with the exception, and the run keeps going over the other parts; a run with a failed part reports itself as not succeeded. Batch rows carry no user email: no user asked for them.</para>
    /// </summary>
    public class PostgreSQLBuildingSolarRadiationSolveTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The largest number of failure descriptions kept in <see cref="FailureSamples"/>; the counts are never capped.
        /// </summary>
        private const int failureSampleCountMax = 50;

        /// <summary>
        /// The GIS PostgreSQL converter manager used to retrieve converters and execute operations.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        private readonly List<string> failureSamples = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationSolveTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager used to retrieve converters and execute operations.</param>
        public PostgreSQLBuildingSolarRadiationSolveTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the number of building and radius solves that failed with an exception or produced a result that cannot be stored during the last run.
        /// </summary>
        public long FailedCount { get; private set; }

        /// <summary>
        /// Gets the number of county parts that failed outright and were stepped over during the last run.
        /// </summary>
        public long FailedCountyCount { get; private set; }

        /// <summary>
        /// Gets a bounded sample of the failures of the last run, at most 50 descriptions; <see cref="FailedCount"/> and <see cref="FailedCountyCount"/> are the full counts.
        /// </summary>
        public IReadOnlyList<string> FailureSamples => failureSamples;

        /// <summary>
        /// Gets the number of building and radius solves stepped over during the last run because a ceiling was exceeded: more receiving surfaces than <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.ReceiverCountMax"/> (counted once per radius) or more caster triangles than <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.CasterTriangleCountMax"/>.
        /// </summary>
        public long OverCeilingCount { get; private set; }

        /// <summary>
        /// Gets the number of building and radius solves stepped over during the last run because the building has no stored model, cannot be located, has no closed external envelope or has no weather.
        /// </summary>
        public long NotSolvableCount { get; private set; }

        /// <summary>
        /// Gets or sets the options used to configure the run.
        /// </summary>
        public PostgreSQLBuildingSolarRadiationSolveOptions PostgreSQLBuildingSolarRadiationSolveOptions { get; set; } = new();

        /// <summary>
        /// Gets the number of buildings that were considered during the last run, whatever happened to them; the progress reported.
        /// </summary>
        public long ProcessedBuildingCount { get; private set; }

        /// <summary>
        /// Gets the number of county parts that were processed during the last run.
        /// </summary>
        public long ProcessedCountyCount { get; private set; }

        /// <summary>
        /// Gets the number of building and radius solves stepped over during the last run because a result of the current solver version was already stored (<see cref="PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting"/>).
        /// </summary>
        public long SkippedExistingCount { get; private set; }

        /// <summary>
        /// Gets the number of building and radius results solved and stored during the last run.
        /// </summary>
        public long SolvedCount { get; private set; }

        /// <summary>
        /// Gets the total time, in milliseconds, spent building shading models and solving during the last run - not reading or writing. Divided by <see cref="SolvedCount"/> it is the solve time per building and radius the runtime evidence of a run is reported from.
        /// </summary>
        public long SolveMilliseconds { get; private set; }

        /// <summary>
        /// Executes the background task that solves and stores the annual solar radiation of every building of the county parts in scope.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of buildings considered.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every county part in scope was processed without error; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            FailedCount = 0;
            FailedCountyCount = 0;
            OverCeilingCount = 0;
            NotSolvableCount = 0;
            ProcessedBuildingCount = 0;
            ProcessedCountyCount = 0;
            SkippedExistingCount = 0;
            SolvedCount = 0;
            SolveMilliseconds = 0;
            failureSamples.Clear();

            PostgreSQLBuildingSolarRadiationSolveOptions ??= new();

            int commandTimeout = PostgreSQLBuildingSolarRadiationSolveOptions.CommandTimeout;
            int batchSize = Math.Max(1, PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize);
            double tileSize = Math.Max(1, PostgreSQLBuildingSolarRadiationSolveOptions.TileSize);
            bool skipExisting = PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting;
            int receiverCountMax = PostgreSQLBuildingSolarRadiationSolveOptions.ReceiverCountMax;
            int casterTriangleCountMax = PostgreSQLBuildingSolarRadiationSolveOptions.CasterTriangleCountMax;
            int solverVersion = Analytical.Solar.Constants.Default.SolarSolverVersion;

            // Each radius is rounded once, to the value it is solved and stored under.
            List<double> radii = [];
            foreach (double radius in PostgreSQLBuildingSolarRadiationSolveOptions.Radii ?? [])
            {
                if (double.IsFinite(radius) && radius >= 0)
                {
                    double radius_Key = Analytical.Solar.Query.SolarRadiusKey(radius);
                    if (!radii.Contains(radius_Key))
                    {
                        radii.Add(radius_Key);
                    }
                }
            }

            radii.Sort();
            if (radii.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no usable radius - nothing to solve", nameof(PostgreSQLBuildingSolarRadiationSolveTask));
                return false;
            }

            double radius_Max = radii[^1];

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            BuildingSolarRadiationPostgreSQLConverter? buildingSolarRadiationPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingSolarRadiationPostgreSQLConverter>();
            EPWFilePostgreSQLConverter? ePWFilePostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<EPWFilePostgreSQLConverter>();
            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (building2DPostgreSQLConverter is null || buildingModelPostgreSQLConverter is null || buildingSolarRadiationPostgreSQLConverter is null || ePWFilePostgreSQLConverter is null || administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: a converter is missing - building 2D {Building2D}, building model {BuildingModel}, solar radiation {SolarRadiation}, EPW {EPW}, counties {Counties}", nameof(PostgreSQLBuildingSolarRadiationSolveTask), building2DPostgreSQLConverter is not null, buildingModelPostgreSQLConverter is not null, buildingSolarRadiationPostgreSQLConverter is not null, ePWFilePostgreSQLConverter is not null, administrativeAreal2DPostgreSQLConverter is not null);
                return false;
            }

            List<(int Id, double X, double Y)>? ePWFileLocations = await ePWFilePostgreSQLConverter.GetEPWFileLocationsAsync(commandTimeout, cancellationToken);
            if (ePWFileLocations is null || ePWFileLocations.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no EPW station could be read - no building can be given weather", nameof(PostgreSQLBuildingSolarRadiationSolveTask));
                return false;
            }

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (countyReferences is null || countyReferences.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county could be read - the run cannot be scoped", nameof(PostgreSQLBuildingSolarRadiationSolveTask));
                return false;
            }

            HashSet<int>? countyIds = PostgreSQLBuildingSolarRadiationSolveOptions.CountyIds;

            Serilog.Modify.Log(
                "{Type}: starting over {CountyCount} counties, scope {CountyScope}, radii {Radii} m, tile {TileSize} m, batch size {BatchSize}, skip existing {SkipExisting}, receiver ceiling {ReceiverCountMax}, caster ceiling {CasterTriangleCountMax}, solver version {SolverVersion}",
                nameof(PostgreSQLBuildingSolarRadiationSolveTask),
                countyReferences.Count,
                countyIds is null ? "all" : string.Join(", ", countyIds),
                string.Join(", ", radii.Select(x => x.ToString(CultureInfo.InvariantCulture))),
                tileSize,
                batchSize,
                skipExisting,
                receiverCountMax,
                casterTriangleCountMax,
                solverVersion);

            foreach (AdministrativeAreal2DReference? countyReference in countyReferences.OrderBy(x => x?.Id ?? int.MaxValue))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Id is the county row identifier - the part a building is filed under - never the code.
                if (countyReference is null || (countyIds is not null && !countyIds.Contains(countyReference.Id)))
                {
                    continue;
                }

                int countyId = countyReference.Id;
                bool countyFailed = false;
                long countySolvedCount = 0;
                long countySolveMilliseconds = 0;
                long countyBuildingCount = 0;

                // Weather files read for this part, by station id: a part uses a handful of stations, each read once.
                Dictionary<int, EPWFile?> ePWFiles = [];

                try
                {
                    List<Building2DCentroid>? building2DCentroids = await building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    if (building2DCentroids is null)
                    {
                        throw new InvalidOperationException("the building centroids could not be read");
                    }

                    // The radii each building still needs: every radius, less the ones already stored for this solver version.
                    List<HashSet<string>> references_Done = [];
                    foreach (double radius in radii)
                    {
                        HashSet<string>? references = skipExisting ? await buildingSolarRadiationPostgreSQLConverter.GetReferencesAsync(countyId, commandTimeout: commandTimeout, radius: radius, solverVersion: solverVersion, cancellationToken: cancellationToken) : null;
                        if (skipExisting && references is null)
                        {
                            throw new InvalidOperationException("the stored results could not be read");
                        }

                        references_Done.Add(references ?? []);
                    }

                    Dictionary<string, List<double>> radii_ByReference = [];
                    List<Building2DCentroid> building2DCentroids_Pending = [];
                    foreach (Building2DCentroid building2DCentroid in building2DCentroids)
                    {
                        string? reference = building2DCentroid?.Reference;
                        if (string.IsNullOrWhiteSpace(reference) || !double.IsFinite(building2DCentroid!.X) || !double.IsFinite(building2DCentroid.Y) || radii_ByReference.ContainsKey(reference!))
                        {
                            continue;
                        }

                        List<double> radii_Pending = [];
                        for (int i = 0; i < radii.Count; i++)
                        {
                            if (references_Done[i].Contains(reference!))
                            {
                                SkippedExistingCount++;
                            }
                            else
                            {
                                radii_Pending.Add(radii[i]);
                            }
                        }

                        if (radii_Pending.Count == 0)
                        {
                            ProcessedBuildingCount++;
                            continue;
                        }

                        radii_ByReference[reference!] = radii_Pending;
                        building2DCentroids_Pending.Add(building2DCentroid);
                    }

                    progress.Report(ProcessedBuildingCount);

                    // Tiles in row order, buildings within a tile by reference, so a re-run walks the same batches.
                    IEnumerable<IGrouping<(long, long), Building2DCentroid>> tiles = building2DCentroids_Pending
                        .GroupBy(x => ((long)Math.Floor(x.X / tileSize), (long)Math.Floor(x.Y / tileSize)))
                        .OrderBy(x => x.Key.Item2)
                        .ThenBy(x => x.Key.Item1);

                    foreach (IGrouping<(long, long), Building2DCentroid> tile in tiles)
                    {
                        List<string> references_Tile = [.. tile.Select(x => x.Reference!).OrderBy(x => x, StringComparer.Ordinal)];
                        for (int offset = 0; offset < references_Tile.Count; offset += batchSize)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            List<string> references_Batch = references_Tile.GetRange(offset, Math.Min(batchSize, references_Tile.Count - offset));
                            (long solvedCount, long solveMilliseconds) = await SolveBatchAsync(countyId, references_Batch, radii_ByReference);

                            countySolvedCount += solvedCount;
                            countySolveMilliseconds += solveMilliseconds;
                            countyBuildingCount += references_Batch.Count;
                            ProcessedBuildingCount += references_Batch.Count;
                            progress.Report(ProcessedBuildingCount);
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    countyFailed = true;
                    AddFailureSample(string.Format(CultureInfo.InvariantCulture, "county {0}: {1}", countyId, exception.Message));
                    Serilog.Modify.Log(exception, "{Type}: county part {CountyId} failed after {SolvedCount} results", nameof(PostgreSQLBuildingSolarRadiationSolveTask), countyId, countySolvedCount);
                }

                if (countyFailed)
                {
                    FailedCountyCount++;
                    continue;
                }

                ProcessedCountyCount++;
                Serilog.Modify.Log(
                    Serilog.Enums.LogEventLevel.Information,
                    "{Type}: county part {CountyId} processed - {BuildingCount} buildings solved for, {SolvedCount} results stored, {SolveMilliseconds} ms solving, {AverageMilliseconds} ms per result",
                    nameof(PostgreSQLBuildingSolarRadiationSolveTask),
                    countyId,
                    countyBuildingCount,
                    countySolvedCount,
                    countySolveMilliseconds,
                    countySolvedCount == 0 ? 0 : countySolveMilliseconds / countySolvedCount);

                ePWFiles.Clear();

                // Solves one batch of buildings of the part for their pending radii and writes the results in one statement.
                // Throws when the batch's models or neighbours cannot be read or its results cannot be written; a building
                // that cannot be solved is counted, not thrown.
                async Task<(long SolvedCount, long SolveMilliseconds)> SolveBatchAsync(int countyId_Batch, List<string> references_Batch, Dictionary<string, List<double>> radii_ByReference_Batch)
                {
                    List<BuildingModel>? buildingModels_Stored = await buildingModelPostgreSQLConverter.GetItemsByReferencesAsync(references_Batch, countyId_Batch, null, true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    if (buildingModels_Stored is null)
                    {
                        throw new InvalidOperationException("the building models could not be read");
                    }

                    // The receivers: stamped, with their outward normals and their circle at the largest radius.
                    List<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel, string Reference, Dictionary<string, Vector3D> Normals, Circle2D Circle)> receivers = [];
                    HashSet<string> references_Found = [];
                    foreach (BuildingModel? buildingModel_Stored in buildingModels_Stored)
                    {
                        if (buildingModel_Stored?.ToDiGi() is not DiGi.Analytical.Building.Classes.BuildingModel buildingModel)
                        {
                            continue;
                        }

                        if (!buildingModel.TryGetValue(GIS.Analytical.Enums.BuildingModelParameter.Reference, out string? reference) || string.IsNullOrWhiteSpace(reference) || !radii_ByReference_Batch.TryGetValue(reference!, out List<double>? radii_Building) || !references_Found.Add(reference!))
                        {
                            continue;
                        }

                        if (!GIS.Analytical.Modify.UpdateBuildingInformation(buildingModel))
                        {
                            NotSolvable(reference!, radii_Building.Count, "it cannot be located");
                            continue;
                        }

                        Dictionary<string, Vector3D>? normals = buildingModel.SolarReceiverNormals();
                        if (normals is null || normals.Count == 0)
                        {
                            NotSolvable(reference!, radii_Building.Count, "it has no closed external envelope");
                            continue;
                        }

                        if (normals.Count > receiverCountMax)
                        {
                            OverCeilingCount += radii_Building.Count;
                            continue;
                        }

                        Circle2D? circle2D = buildingModel.SolarNeighbourCircle(radius_Max);
                        if (circle2D is null)
                        {
                            NotSolvable(reference!, radii_Building.Count, "it has no bounding box");
                            continue;
                        }

                        receivers.Add((buildingModel, reference!, normals, circle2D));
                    }

                    foreach (string reference in references_Batch)
                    {
                        if (!references_Found.Contains(reference))
                        {
                            NotSolvableCount += radii_ByReference_Batch[reference].Count;
                        }
                    }

                    if (receivers.Count == 0)
                    {
                        return (0, 0);
                    }

                    // Every neighbour any receiver can see at the largest radius, in one read: the smallest circle around the
                    // batch's centre that holds every receiver's circle.
                    double x_Centre = receivers.Average(x => x.Circle.Center!.X);
                    double y_Centre = receivers.Average(x => x.Circle.Center!.Y);
                    double radius_Batch = receivers.Max(x => Math.Sqrt(Math.Pow(x.Circle.Center!.X - x_Centre, 2) + Math.Pow(x.Circle.Center!.Y - y_Centre, 2)) + x.Circle.Radius);

                    List<(int CountyId, DiGi.Analytical.Building.Classes.BuildingModel BuildingModel)>? neighbours_Batch = radii.Any(x => x > 0)
                        ? await building2DPostgreSQLConverter.BuildingModelsByCircle2DAsync(buildingModelPostgreSQLConverter, new Circle2D(new Point2D(x_Centre, y_Centre), radius_Batch), commandTimeout: commandTimeout, cancellationToken: cancellationToken)
                        : [];
                    if (neighbours_Batch is null)
                    {
                        throw new InvalidOperationException("the neighbours could not be read");
                    }

                    // The planar bounding box of each neighbour, computed once for every receiver and radius.
                    List<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel, double XMin, double YMin, double XMax, double YMax)> neighbours = [];
                    foreach ((int _, DiGi.Analytical.Building.Classes.BuildingModel buildingModel_Neighbour) in neighbours_Batch)
                    {
                        BoundingBox3D? boundingBox3D = buildingModel_Neighbour.GetBoundingBox();
                        Point3D? min = boundingBox3D?.Min;
                        Point3D? max = boundingBox3D?.Max;
                        if (min is not null && max is not null)
                        {
                            neighbours.Add((buildingModel_Neighbour, min.X, min.Y, max.X, max.Y));
                        }
                    }

                    List<GIS.Classes.BuildingSolarRadiationResult> results = [];
                    long solveMilliseconds_Batch = 0;

                    foreach ((DiGi.Analytical.Building.Classes.BuildingModel buildingModel, string reference, Dictionary<string, Vector3D> normals, Circle2D _) in receivers)
                    {
                        Point2D? centre = buildingModel.SolarNeighbourCircle(0)?.Center;
                        int? ePWFileId = centre is null ? null : ePWFileLocations.NearestEPWFileId(centre.X, centre.Y);
                        EPWFile? ePWFile = null;
                        if (ePWFileId is not null && !ePWFiles.TryGetValue(ePWFileId.Value, out ePWFile))
                        {
                            ePWFile = await ePWFilePostgreSQLConverter.GetEPWFileByIdAsync(ePWFileId.Value, commandTimeout, cancellationToken);
                            ePWFiles[ePWFileId.Value] = ePWFile;
                        }

                        List<double> radii_Building = radii_ByReference_Batch[reference];
                        if (ePWFileId is null || ePWFile is null)
                        {
                            NotSolvable(reference, radii_Building.Count, "no weather file could be read for it");
                            continue;
                        }

                        foreach (double radius in radii_Building)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            try
                            {
                                Stopwatch stopwatch = Stopwatch.StartNew();

                                List<DiGi.Analytical.Building.Classes.BuildingModel>? buildingModels_Surrounding = null;
                                if (radius > 0)
                                {
                                    Circle2D? circle2D = buildingModel.SolarNeighbourCircle(radius);
                                    buildingModels_Surrounding = circle2D is null ? [] : [.. neighbours.Where(x => Intersects(circle2D, x.XMin, x.YMin, x.XMax, x.YMax)).Select(x => x.BuildingModel)];
                                }

                                ShadingModel? shadingModel = buildingModel.ToSolar(buildingModels_Surrounding, x => new GuidReference(x).ToString() is string reference_Component && normals.ContainsKey(reference_Component));
                                if (shadingModel is null)
                                {
                                    NotSolvable(reference, 1, "it cannot be converted into a shading model");
                                    continue;
                                }

                                if (shadingModel.CasterTriangleCount() > casterTriangleCountMax)
                                {
                                    OverCeilingCount++;
                                    continue;
                                }

                                List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations = shadingModel.SurfaceSolarRadiations(normals, ePWFile, Analytical.Solar.Create.SolarShadingSolverOptions());

                                stopwatch.Stop();
                                solveMilliseconds_Batch += stopwatch.ElapsedMilliseconds;

                                GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult = buildingModel.BuildingSolarRadiationResult(countyId_Batch, radius, ePWFileId.Value, surfaceSolarRadiations);
                                if (buildingSolarRadiationResult is null)
                                {
                                    Failed(reference, radius, "the solve produced no result that can be stored");
                                    continue;
                                }

                                results.Add(buildingSolarRadiationResult);
                            }
                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                            {
                                throw;
                            }
                            catch (Exception exception)
                            {
                                Failed(reference, radius, exception.Message);
                                Serilog.Modify.Log(exception, "{Type}: building {Reference} of county part {CountyId} failed at radius {Radius} m", nameof(PostgreSQLBuildingSolarRadiationSolveTask), reference, countyId_Batch, radius);
                            }
                        }
                    }

                    if (results.Count == 0)
                    {
                        return (0, solveMilliseconds_Batch);
                    }

                    int? written = await buildingSolarRadiationPostgreSQLConverter.UpdateAsync(countyId_Batch, results, commandTimeout, cancellationToken);
                    if (written is null)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "the {0} results of a batch could not be written", results.Count));
                    }

                    SolvedCount += results.Count;
                    SolveMilliseconds += solveMilliseconds_Batch;
                    return (results.Count, solveMilliseconds_Batch);
                }
            }

            Serilog.Modify.Log(
                "{Type}: finished - {ProcessedCountyCount} county parts, {FailedCountyCount} failed, {BuildingCount} buildings, {SolvedCount} results stored, {SkippedExistingCount} already stored, {OverCeilingCount} over a ceiling, {NotSolvableCount} not solvable, {FailedCount} failed, {SolveMilliseconds} ms solving",
                nameof(PostgreSQLBuildingSolarRadiationSolveTask),
                ProcessedCountyCount,
                FailedCountyCount,
                ProcessedBuildingCount,
                SolvedCount,
                SkippedExistingCount,
                OverCeilingCount,
                NotSolvableCount,
                FailedCount,
                SolveMilliseconds);

            return FailedCountyCount == 0;

            void NotSolvable(string reference, int count, string reason)
            {
                NotSolvableCount += count;
                AddFailureSample(string.Format(CultureInfo.InvariantCulture, "{0}: not solvable, {1}", reference, reason));
            }

            void Failed(string reference, double radius, string reason)
            {
                FailedCount++;
                AddFailureSample(string.Format(CultureInfo.InvariantCulture, "{0} at {1} m: {2}", reference, radius, reason));
            }
        }

        // Keeps a failure description while the sample has room; the counts are kept by the callers.
        private void AddFailureSample(string text)
        {
            if (failureSamples.Count < failureSampleCountMax)
            {
                failureSamples.Add(text);
            }
        }

        // Whether a planar bounding box lies within or intersects a circle, as gis/buildingmodel/itemsbycircle selects a
        // building by its footprint's bounding box (widened by the default distance tolerance).
        private static bool Intersects(Circle2D circle2D, double xMin, double yMin, double xMax, double yMax)
        {
            Point2D? center = circle2D.Center;
            if (center is null)
            {
                return false;
            }

            double dx = Math.Max(Math.Max(xMin - center.X, 0), center.X - xMax);
            double dy = Math.Max(Math.Max(yMin - center.Y, 0), center.Y - yMax);
            double radius = circle2D.Radius + Core.Constants.Tolerance.Distance;

            return (dx * dx) + (dy * dy) <= radius * radius;
        }
    }
}
