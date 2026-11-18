using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Regulation.Classes;
using DiGi.Analytical.Building.Regulation.Enums;
using DiGi.Core.Classes;
using DiGi.EPW.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Analytical.Energy;
using DiGi.GIS.Analytical.Energy.Classes;
using DiGi.GIS.Analytical.Energy.Enums;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Solar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Building2D = DiGi.GIS.Classes.Building2D;
using BuildingModel = DiGi.Analytical.Building.Classes.BuildingModel;

namespace DiGi.GIS.PostgreSQL.Energy.Classes
{
    /// <summary>
    /// Represents a background task that calculates the monthly heat demand of every building of a county part and stores it, whatever the inputs are read from and the results written to (DiGi.GIS.PostgreSQL#143, for DiGi-Organization/DiGi.Analytical#10).
    /// <para>Every read and the write is a protected hook: <see cref="PostgreSQLBuildingHeatDemandSolveTask"/> implements them with the database converters, a test or a client implements them otherwise. The walk, the skip of stored results, the tiling, the batching, the parallel calculation, the counting and the logging are this class's alone, as in <c>BuildingSolarRadiationSolveTask</c>.</para>
    /// <para>A county part is walked by its <c>id</c> - every part of a county on its own, never deduplicated (<c>Coding - GIS Administrative Data.md</c>) - in square tiles of <see cref="PostgreSQLBuildingHeatDemandSolveOptions.TileSize"/> by the centroid of each building (<c>Solar.Query.SolarSolveTiles</c>), at most <see cref="PostgreSQLBuildingHeatDemandSolveOptions.BatchSize"/> buildings at a time. A batch reads its models, 2D buildings, year built, occupancy, address points and stored solar results, and every neighbour within <see cref="PostgreSQLBuildingHeatDemandSolveOptions.NeighbourRadius"/> of any of them in one read; it calculates its buildings in parallel and writes their results in one call.</para>
    /// <para><b>Per building</b>, <c>DiGi.GIS.Analytical.Energy.Create.BuildingHeatDemandResult</c> splits the party walls, places the openings, assigns the internal conditions, assembles the monthly climate and runs the engine. The climate zone comes from the climate-zone map read once per run; the weather is the EPW of the building's stored solar result, else of the nearest station; the solar result is the one of <see cref="PostgreSQLBuildingHeatDemandSolveOptions.SolarRadius"/>, else the latest of any radius, else none (the irradiation is then unshaded and the building counted in <see cref="UnshadedBuildingCount"/>). The construction library and the opening-size library are read once per run.</para>
    /// <para>A building that cannot be calculated - no stored model, no 2D building, no location, no weather, or a null result (no opening-size set for it, a month without a valid temperature, no heated space) - is counted in <see cref="NotSolvableCount"/> with a reason sample and stepped over. A building whose calculation throws is counted in <see cref="FailedCount"/>. A county part whose reads or write fail is counted in <see cref="FailedCountyCount"/>, logged with the exception, and the run goes on with the other parts; such a run reports itself as not successful. A requested cancellation is rethrown, never counted.</para>
    /// </summary>
    public abstract class BuildingHeatDemandSolveTask : ReportableBackgroundTask<long>
    {
        /// <summary>
        /// The largest number of failure descriptions kept in <see cref="FailureSamples"/>; the counts are never capped.
        /// </summary>
        private const int failureSampleCountMax = 50;

        private readonly List<string> failureSamples = [];

        /// <summary>
        /// Gets the number of buildings whose calculation threw during the last run.
        /// </summary>
        public long FailedCount { get; private set; }

        /// <summary>
        /// Gets the number of county parts that failed outright and were stepped over during the last run.
        /// </summary>
        public long FailedCountyCount { get; private set; }

        /// <summary>
        /// Gets a bounded sample of the failures and of the buildings that could not be calculated during the last run, at most 50 descriptions; the counts are the full counts.
        /// </summary>
        public IReadOnlyList<string> FailureSamples => failureSamples;

        /// <summary>
        /// Gets the number of buildings that could not be calculated during the last run: no stored model, no 2D building, no location, no weather, or a null result.
        /// </summary>
        public long NotSolvableCount { get; private set; }

        /// <summary>
        /// Gets or sets the options used to configure the run.
        /// </summary>
        public PostgreSQLBuildingHeatDemandSolveOptions PostgreSQLBuildingHeatDemandSolveOptions { get; set; } = new();

        /// <summary>
        /// Gets the number of buildings considered during the last run, whatever happened to them; the progress reported.
        /// </summary>
        public long ProcessedBuildingCount { get; private set; }

        /// <summary>
        /// Gets the number of county parts processed during the last run.
        /// </summary>
        public long ProcessedCountyCount { get; private set; }

        /// <summary>
        /// Gets the number of buildings stepped over during the last run because a result of the current method version was already stored (<see cref="PostgreSQLBuildingHeatDemandSolveOptions.SkipCompleted"/>).
        /// </summary>
        public long SkippedCompletedCount { get; private set; }

        /// <summary>
        /// Gets the number of buildings calculated and stored during the last run.
        /// </summary>
        public long SolvedCount { get; private set; }

        /// <summary>
        /// Gets the number of stored buildings of the last run whose irradiation is unshaded, because they had no stored solar result.
        /// </summary>
        public long UnshadedBuildingCount { get; private set; }

        /// <summary>
        /// Executes the background task that calculates and stores the monthly heat demand of every building of the county parts in scope.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of buildings considered.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every county part in scope was processed without error; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            FailedCount = 0;
            FailedCountyCount = 0;
            NotSolvableCount = 0;
            ProcessedBuildingCount = 0;
            ProcessedCountyCount = 0;
            SkippedCompletedCount = 0;
            SolvedCount = 0;
            UnshadedBuildingCount = 0;
            failureSamples.Clear();

            string type = GetType().Name;

            PostgreSQLBuildingHeatDemandSolveOptions ??= new();

            int commandTimeout = PostgreSQLBuildingHeatDemandSolveOptions.CommandTimeout;
            int batchSize = Math.Max(1, PostgreSQLBuildingHeatDemandSolveOptions.BatchSize);
            int? buildingCountMax = PostgreSQLBuildingHeatDemandSolveOptions.BuildingCountMax is int buildingCountMax_Value ? Math.Max(1, buildingCountMax_Value) : null;
            double tileSize = Math.Max(1, PostgreSQLBuildingHeatDemandSolveOptions.TileSize);
            double neighbourRadius = Math.Max(0, PostgreSQLBuildingHeatDemandSolveOptions.NeighbourRadius);
            double solarRadius = GIS.Analytical.Solar.Query.SolarRadiusKey(PostgreSQLBuildingHeatDemandSolveOptions.SolarRadius);
            bool skipCompleted = PostgreSQLBuildingHeatDemandSolveOptions.SkipCompleted;
            ConstructionFlavour constructionFlavour = PostgreSQLBuildingHeatDemandSolveOptions.ConstructionFlavour;
            int methodVersion = DiGi.Analytical.Building.Energy.Constants.Default.HeatDemandMethodVersion;
            int maxDegreeOfParallelism = PostgreSQLBuildingHeatDemandSolveOptions.MaxDegreeOfParallelism > 0 ? PostgreSQLBuildingHeatDemandSolveOptions.MaxDegreeOfParallelism : Math.Max(1, Core.Query.ProcessorCount(0.5));

            // The transport logs its own reason for refusing.
            if (!TryInitialize())
            {
                return false;
            }

            ConstructionLibrary? constructionLibrary = DiGi.Analytical.Building.Regulation.Query.EmbeddedConstructionLibrary_PL();
            OpeningSizeLibrary? openingSizeLibrary = DiGi.Analytical.Building.Regulation.Create.OpeningSizeLibrary_PL();
            ClimateZoneMap? climateZoneMap = GIS.Analytical.Query.EmbeddedClimateZoneMap_PL();
            if (constructionLibrary is null || openingSizeLibrary is null || climateZoneMap is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: a library could not be read - construction library {ConstructionLibrary}, opening sizes {OpeningSizeLibrary}, climate zones {ClimateZoneMap}", type, constructionLibrary is not null, openingSizeLibrary is not null, climateZoneMap is not null);
                return false;
            }

            HeatDemandContext heatDemandContext = new(constructionLibrary, openingSizeLibrary, climateZoneMap, PostgreSQLBuildingHeatDemandSolveOptions.HeatDemandOptions);

            List<(int Id, double X, double Y)>? ePWFileLocations = await ReadEPWFileLocationsAsync(commandTimeout, cancellationToken);
            if (ePWFileLocations is null || ePWFileLocations.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no EPW station could be read - no building can be given weather", type);
                return false;
            }

            List<AdministrativeAreal2DReference>? countyReferences = await ReadCountyReferencesAsync(commandTimeout, cancellationToken);
            if (countyReferences is null || countyReferences.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county could be read - the run cannot be scoped", type);
                return false;
            }

            HashSet<int>? countyIds = PostgreSQLBuildingHeatDemandSolveOptions.CountyIds;

            Serilog.Modify.Log(
                "{Type}: starting over {CountyCount} counties, scope {CountyScope}, method version {MethodVersion}, construction flavour {ConstructionFlavour}, construction library {ConstructionLibraryVersion}, solar radius {SolarRadius} m, neighbour radius {NeighbourRadius} m, tile {TileSize} m, batch size {BatchSize}, building cap {BuildingCountMax}, skip completed {SkipCompleted}, degree of parallelism {DegreeOfParallelism}",
                type,
                countyReferences.Count,
                countyIds is null ? "all" : string.Join(", ", countyIds),
                methodVersion,
                constructionFlavour,
                constructionLibrary.Version ?? "unknown",
                solarRadius,
                neighbourRadius,
                tileSize,
                batchSize,
                buildingCountMax is null ? "none" : buildingCountMax.Value.ToString(CultureInfo.InvariantCulture),
                skipCompleted,
                maxDegreeOfParallelism);

            foreach (AdministrativeAreal2DReference? countyReference in countyReferences.OrderBy(x => x?.Id ?? int.MaxValue))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Id is the county row identifier - the part a building is filed under - never the code.
                if (countyReference is null || (countyIds is not null && !countyIds.Contains(countyReference.Id)))
                {
                    continue;
                }

                int countyId = countyReference.Id;
                long countySolvedCount = 0;
                long countyBuildingCount = 0;
                bool countyFailed = false;

                // Weather files of this part by station id, each read once; dropped with the part.
                Dictionary<int, EPWFile?> ePWFiles = [];

                try
                {
                    List<Building2DCentroid>? building2DCentroids = await ReadBuilding2DCentroidsAsync(countyId, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the building centroids could not be read");

                    HashSet<string> references_Completed = [];
                    if (skipCompleted)
                    {
                        references_Completed = await ReadCompletedReferencesAsync(countyId, methodVersion, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the stored results could not be read");
                    }

                    List<Building2DCentroid> building2DCentroids_Pending = [];
                    HashSet<string> references_Seen = [];
                    foreach (Building2DCentroid? building2DCentroid in building2DCentroids)
                    {
                        string? reference = building2DCentroid?.Reference;
                        if (string.IsNullOrWhiteSpace(reference) || !double.IsFinite(building2DCentroid!.X) || !double.IsFinite(building2DCentroid.Y) || !references_Seen.Add(reference!))
                        {
                            continue;
                        }

                        if (references_Completed.Contains(reference!))
                        {
                            SkippedCompletedCount++;
                            ProcessedBuildingCount++;
                            continue;
                        }

                        building2DCentroids_Pending.Add(building2DCentroid);
                    }

                    progress.Report(ProcessedBuildingCount);

                    // Tiles in row order, buildings within a tile by reference, so a re-run walks the same batches.
                    List<List<string>> tiles = building2DCentroids_Pending.SolarSolveTiles(tileSize, buildingCountMax) ?? [];
                    List<List<string>> references_Batches = [];
                    foreach (List<string> references_Tile in tiles)
                    {
                        for (int offset = 0; offset < references_Tile.Count; offset += batchSize)
                        {
                            references_Batches.Add(references_Tile.GetRange(offset, Math.Min(batchSize, references_Tile.Count - offset)));
                        }
                    }

                    for (int batchIndex = 0; batchIndex < references_Batches.Count; batchIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Stopwatch stopwatch = Stopwatch.StartNew();
                        List<string> references_Batch = references_Batches[batchIndex];
                        List<(BuildingModel BuildingModel, Building2D Building2D, HeatDemandInput HeatDemandInput, List<BuildingModel?> Neighbours, BuildingSolarRadiationResult? BuildingSolarRadiationResult, EPWFile EPWFile)> jobs = await ReadBatchAsync(countyId, references_Batch);
                        long readMilliseconds = stopwatch.ElapsedMilliseconds;

                        // The calculation leaves the asynchronous flow; each building is calculated on its own and its outcome kept at its index.
                        stopwatch.Restart();
                        (BuildingHeatDemandResult? Result, Exception? Exception)[] outcomes = new (BuildingHeatDemandResult?, Exception?)[jobs.Count];
                        await Task.Run(
                            () => Parallel.For(0, jobs.Count, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cancellationToken }, i =>
                            {
                                try
                                {
                                    (BuildingModel buildingModel, Building2D building2D, HeatDemandInput heatDemandInput, List<BuildingModel?> neighbours, BuildingSolarRadiationResult? buildingSolarRadiationResult, EPWFile ePWFile) = jobs[i];
                                    outcomes[i] = (buildingModel.BuildingHeatDemandResult(building2D, buildingModel.BuildingInformation, heatDemandInput, neighbours, buildingSolarRadiationResult, ePWFile, heatDemandContext), null);
                                }
                                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                                {
                                    throw;
                                }
                                catch (Exception exception)
                                {
                                    outcomes[i] = (null, exception);
                                }
                            }),
                            cancellationToken);
                        long solveMilliseconds = stopwatch.ElapsedMilliseconds;

                        List<BuildingHeatDemandResult> results = [];
                        for (int i = 0; i < outcomes.Length; i++)
                        {
                            string reference = jobs[i].Building2D.Reference ?? string.Empty;
                            if (outcomes[i].Exception is Exception exception)
                            {
                                FailedCount++;
                                AddFailureSample(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", reference, exception.Message));
                                Serilog.Modify.Log(exception, "{Type}: building {Reference} of county part {CountyId} failed", type, reference, countyId);
                            }
                            else if (outcomes[i].Result is BuildingHeatDemandResult buildingHeatDemandResult)
                            {
                                results.Add(buildingHeatDemandResult);
                                if (buildingHeatDemandResult.SolarSource == HeatDemandSolarSource.Unshaded)
                                {
                                    UnshadedBuildingCount++;
                                }
                            }
                            else
                            {
                                NotSolvable(reference, "no result: no opening-size set for it, a month without a valid temperature, or no heated space");
                            }
                        }

                        stopwatch.Restart();
                        if (results.Count != 0)
                        {
                            int? written = await WriteAsync(countyId, results, commandTimeout, cancellationToken);
                            if (written is null)
                            {
                                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "the {0} results of a batch could not be written", results.Count));
                            }
                        }

                        long writeMilliseconds = stopwatch.ElapsedMilliseconds;

                        SolvedCount += results.Count;
                        countySolvedCount += results.Count;
                        countyBuildingCount += references_Batch.Count;
                        ProcessedBuildingCount += references_Batch.Count;
                        progress.Report(ProcessedBuildingCount);

                        Serilog.Modify.Log(
                            Serilog.Enums.LogEventLevel.Information,
                            "{Type}: county part {CountyId} batch {BatchNumber} of {BatchCount} - {BuildingCount} buildings, {JobCount} calculated, {ResultCount} results, {ReadMilliseconds} ms reading, {SolveMilliseconds} ms calculating, {WriteMilliseconds} ms writing",
                            type,
                            countyId,
                            batchIndex + 1,
                            references_Batches.Count,
                            references_Batch.Count,
                            jobs.Count,
                            results.Count,
                            readMilliseconds,
                            solveMilliseconds,
                            writeMilliseconds);
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
                    Serilog.Modify.Log(exception, "{Type}: county part {CountyId} failed after {SolvedCount} results", type, countyId, countySolvedCount);
                }

                if (countyFailed)
                {
                    FailedCountyCount++;
                    continue;
                }

                ProcessedCountyCount++;
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Information, "{Type}: county part {CountyId} processed - {BuildingCount} buildings walked, {SolvedCount} results stored", type, countyId, countyBuildingCount, countySolvedCount);

                // Reads one batch of buildings of the part and turns it into calculation jobs. Every building that cannot become a job is counted here.
                async Task<List<(BuildingModel BuildingModel, Building2D Building2D, HeatDemandInput HeatDemandInput, List<BuildingModel?> Neighbours, BuildingSolarRadiationResult? BuildingSolarRadiationResult, EPWFile EPWFile)>> ReadBatchAsync(int countyId_Batch, List<string> references_Batch)
                {
                    List<(BuildingModel, Building2D, HeatDemandInput, List<BuildingModel?>, BuildingSolarRadiationResult?, EPWFile)> result = [];

                    List<BuildingModel?> buildingModels = await ReadBuildingModelsAsync(countyId_Batch, references_Batch, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the building models could not be read");
                    List<Building2D?> building2Ds = await ReadBuilding2DsAsync(countyId_Batch, references_Batch, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the 2D buildings could not be read");
                    Dictionary<string, (short? YearBuilt, YearBuiltSource? YearBuiltSource)> yearBuilts = await ReadYearBuiltsAsync(countyId_Batch, references_Batch, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the year built could not be read");
                    Dictionary<string, OccupancyData> occupancyDatas = await ReadOccupancyDatasAsync(countyId_Batch, references_Batch, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the occupancy could not be read");
                    Dictionary<string, Point2D> addressPoints = await ReadAddressPointsAsync(countyId_Batch, references_Batch, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the address points could not be read");

                    // The solar result of the chosen radius, else the latest of any radius.
                    Dictionary<string, BuildingSolarRadiationResult> buildingSolarRadiationResults = [];
                    foreach (BuildingSolarRadiationResult buildingSolarRadiationResult in await ReadBuildingSolarRadiationResultsAsync(countyId_Batch, references_Batch, solarRadius, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the solar results could not be read"))
                    {
                        if (buildingSolarRadiationResult?.Reference is string reference)
                        {
                            buildingSolarRadiationResults[reference] = buildingSolarRadiationResult;
                        }
                    }

                    List<string> references_NoSolar = references_Batch.FindAll(x => !buildingSolarRadiationResults.ContainsKey(x));
                    if (references_NoSolar.Count != 0)
                    {
                        foreach (BuildingSolarRadiationResult buildingSolarRadiationResult in await ReadBuildingSolarRadiationResultsAsync(countyId_Batch, references_NoSolar, null, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the solar results could not be read"))
                        {
                            if (buildingSolarRadiationResult?.Reference is string reference && !buildingSolarRadiationResults.ContainsKey(reference))
                            {
                                buildingSolarRadiationResults[reference] = buildingSolarRadiationResult;
                            }
                        }
                    }

                    Dictionary<string, Building2D> building2Ds_ByReference = [];
                    foreach (Building2D? building2D in building2Ds)
                    {
                        if (building2D?.Reference is string reference && !building2Ds_ByReference.ContainsKey(reference))
                        {
                            building2Ds_ByReference[reference] = building2D;
                        }
                    }

                    // The located buildings of the batch, by reference.
                    List<(string Reference, BuildingModel BuildingModel, BoundingBox3D BoundingBox3D)> buildingModels_Located = [];
                    HashSet<string> references_Found = [];
                    foreach (BuildingModel? buildingModel in buildingModels)
                    {
                        if (buildingModel is null || !buildingModel.TryGetValue(GIS.Analytical.Enums.BuildingModelParameter.Reference, out string? reference) || string.IsNullOrWhiteSpace(reference) || !references_Found.Add(reference!))
                        {
                            continue;
                        }

                        if (!GIS.Analytical.Modify.UpdateBuildingInformation(buildingModel) || buildingModel.GetBoundingBox() is not BoundingBox3D boundingBox3D)
                        {
                            NotSolvable(reference!, "it cannot be located");
                            continue;
                        }

                        buildingModels_Located.Add((reference!, buildingModel, boundingBox3D));
                    }

                    foreach (string reference in references_Batch)
                    {
                        if (!references_Found.Contains(reference))
                        {
                            NotSolvable(reference, "it has no stored building model");
                        }
                    }

                    if (buildingModels_Located.Count == 0)
                    {
                        return result;
                    }

                    // Every neighbour within the radius of any building of the batch, in one read.
                    double xMin = buildingModels_Located.Min(x => x.BoundingBox3D.Min.X) - neighbourRadius;
                    double yMin = buildingModels_Located.Min(x => x.BoundingBox3D.Min.Y) - neighbourRadius;
                    double xMax = buildingModels_Located.Max(x => x.BoundingBox3D.Max.X) + neighbourRadius;
                    double yMax = buildingModels_Located.Max(x => x.BoundingBox3D.Max.Y) + neighbourRadius;
                    Circle2D circle2D = new(new Point2D((xMin + xMax) / 2, (yMin + yMax) / 2), Math.Sqrt(Math.Pow(xMax - xMin, 2) + Math.Pow(yMax - yMin, 2)) / 2);

                    List<(BuildingModel BuildingModel, string? Reference, BoundingBox3D BoundingBox3D)> neighbours_Batch = [];
                    foreach (BuildingModel? buildingModel_Neighbour in await ReadNeighbourBuildingModelsAsync(circle2D, commandTimeout, cancellationToken) ?? throw new InvalidOperationException("the neighbours could not be read"))
                    {
                        if (buildingModel_Neighbour?.GetBoundingBox() is BoundingBox3D boundingBox3D_Neighbour)
                        {
                            buildingModel_Neighbour.TryGetValue(GIS.Analytical.Enums.BuildingModelParameter.Reference, out string? reference_Neighbour);
                            neighbours_Batch.Add((buildingModel_Neighbour, reference_Neighbour, boundingBox3D_Neighbour));
                        }
                    }

                    foreach ((string reference, BuildingModel buildingModel, BoundingBox3D boundingBox3D) in buildingModels_Located)
                    {
                        if (!building2Ds_ByReference.TryGetValue(reference, out Building2D? building2D))
                        {
                            NotSolvable(reference, "it has no 2D building");
                            continue;
                        }

                        buildingSolarRadiationResults.TryGetValue(reference, out BuildingSolarRadiationResult? buildingSolarRadiationResult);

                        // The weather of the solar result, so temperature and irradiation describe the same year; else of the nearest station.
                        int? ePWFileId = buildingSolarRadiationResult is not null && buildingSolarRadiationResult.WeatherSourceType == WeatherSourceType.EPW && int.TryParse(buildingSolarRadiationResult.WeatherSourceId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ePWFileId_Solar)
                            ? ePWFileId_Solar
                            : ePWFileLocations.NearestEPWFileId((boundingBox3D.Min.X + boundingBox3D.Max.X) / 2, (boundingBox3D.Min.Y + boundingBox3D.Max.Y) / 2);

                        EPWFile? ePWFile = null;
                        if (ePWFileId is not null && !ePWFiles.TryGetValue(ePWFileId.Value, out ePWFile))
                        {
                            ePWFile = await ReadEPWFileAsync(ePWFileId.Value, commandTimeout, cancellationToken);
                            ePWFiles[ePWFileId.Value] = ePWFile;
                        }

                        if (ePWFileId is null || ePWFile is null)
                        {
                            NotSolvable(reference, "no weather file could be read for it");
                            continue;
                        }

                        List<BuildingModel?> neighbours = [];
                        foreach ((BuildingModel buildingModel_Neighbour, string? reference_Neighbour, BoundingBox3D boundingBox3D_Neighbour) in neighbours_Batch)
                        {
                            if (reference_Neighbour == reference)
                            {
                                continue;
                            }

                            if (boundingBox3D_Neighbour.Min.X - boundingBox3D.Max.X <= neighbourRadius && boundingBox3D.Min.X - boundingBox3D_Neighbour.Max.X <= neighbourRadius && boundingBox3D_Neighbour.Min.Y - boundingBox3D.Max.Y <= neighbourRadius && boundingBox3D.Min.Y - boundingBox3D_Neighbour.Max.Y <= neighbourRadius)
                            {
                                neighbours.Add(buildingModel_Neighbour);
                            }
                        }

                        yearBuilts.TryGetValue(reference, out (short? YearBuilt, YearBuiltSource? YearBuiltSource) yearBuilt);
                        occupancyDatas.TryGetValue(reference, out OccupancyData? occupancyData);
                        addressPoints.TryGetValue(reference, out Point2D? addressPoint);

                        // A Regulation run takes the regime's options with the flavour set; a TypicalAsBuilt run leaves the regime's own options to the generator.
                        OpeningGenerationOptions? openingGenerationOptions = null;
                        if (constructionFlavour != ConstructionFlavour.TypicalAsBuilt)
                        {
                            BuildingRegulation buildingRegulation = DiGi.Analytical.Building.Regulation.Query.BuildingRegulation(yearBuilt.YearBuilt, new OpeningGenerationOptions().DesignLag);
                            openingGenerationOptions = GIS.Analytical.Create.OpeningGenerationOptions(buildingRegulation);
                            openingGenerationOptions.ConstructionFlavour = constructionFlavour;
                        }

                        ClimateZone climateZone = GIS.Analytical.Query.ClimateZone(building2D, climateZoneMap);
                        OpeningGenerationInput openingGenerationInput = new(yearBuilt.YearBuilt, yearBuilt.YearBuiltSource, addressPoint, climateZone, null, openingGenerationOptions);
                        HeatDemandInput heatDemandInput = new(openingGenerationInput, occupancyData, occupancyData is null ? null : "occupancy_data_building_2d", countyId_Batch, WeatherSourceType.EPW, ePWFileId.Value.ToString(CultureInfo.InvariantCulture));

                        result.Add((buildingModel, building2D, heatDemandInput, neighbours, buildingSolarRadiationResult, ePWFile));
                    }

                    return result;
                }
            }

            Serilog.Modify.Log(
                "{Type}: finished - {ProcessedCountyCount} county parts, {FailedCountyCount} failed, {BuildingCount} buildings, {SolvedCount} results stored, {SkippedCompletedCount} already stored, {UnshadedBuildingCount} unshaded, {NotSolvableCount} not solvable, {FailedCount} failed",
                type,
                ProcessedCountyCount,
                FailedCountyCount,
                ProcessedBuildingCount,
                SolvedCount,
                SkippedCompletedCount,
                UnshadedBuildingCount,
                NotSolvableCount,
                FailedCount);

            return FailedCountyCount == 0;

            void NotSolvable(string reference, string reason)
            {
                NotSolvableCount++;
                AddFailureSample(string.Format(CultureInfo.InvariantCulture, "{0}: not solvable, {1}", reference, reason));
            }
        }

        /// <summary>
        /// Reads the 2D buildings of the given references of one county part.
        /// </summary>
        /// <param name="countyId">The id of the county part the references are filed under.</param>
        /// <param name="references">The <c>building_2d</c> references of the batch.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the 2D building of each reference found, or null when the read failed.</returns>
        protected abstract Task<List<Building2D?>?> ReadBuilding2DsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the centroids of the buildings of one county part, the walk of the part.
        /// </summary>
        /// <param name="countyId">The id of the county part.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the centroid and reference of every building of the part, or null when the read failed.</returns>
        protected abstract Task<List<Building2DCentroid>?> ReadBuilding2DCentroidsAsync(int countyId, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the stored building models of the given references of one county part.
        /// </summary>
        /// <param name="countyId">The id of the county part the references are filed under.</param>
        /// <param name="references">The <c>building_2d</c> references of the batch.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the latest stored model of each reference found, stamped with its reference, or null when the read failed.</returns>
        protected abstract Task<List<BuildingModel?>?> ReadBuildingModelsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the stored solar results of the given references of one county part, with their shading factors.
        /// </summary>
        /// <param name="countyId">The id of the county part the references are filed under.</param>
        /// <param name="references">The references to read.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored; null for the latest result of any radius.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds at most one result per reference, or null when the read failed.</returns>
        protected abstract Task<List<BuildingSolarRadiationResult>?> ReadBuildingSolarRadiationResultsAsync(int countyId, List<string> references, double? radius, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the references of one county part that already hold a result of the given method version.
        /// </summary>
        /// <param name="countyId">The id of the county part.</param>
        /// <param name="methodVersion">The method version.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the references, or null when the read failed.</returns>
        protected abstract Task<HashSet<string>?> ReadCompletedReferencesAsync(int countyId, int methodVersion, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the county parts the run may walk.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds one reference per county part, or null when the read failed.</returns>
        protected abstract Task<List<AdministrativeAreal2DReference>?> ReadCountyReferencesAsync(int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the address points matched to the given references of one county part.
        /// </summary>
        /// <param name="countyId">The id of the county part the references are filed under.</param>
        /// <param name="references">The references to read.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the address point of each reference that has one, or null when the read failed.</returns>
        protected abstract Task<Dictionary<string, Point2D>?> ReadAddressPointsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads one EPW file.
        /// </summary>
        /// <param name="id">The id of the EPW file.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result is the EPW file, or null when it cannot be read.</returns>
        protected abstract Task<EPWFile?> ReadEPWFileAsync(int id, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the location of every EPW station, in the coordinates of the building models.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the id and position of every station, or null when the read failed.</returns>
        protected abstract Task<List<(int Id, double X, double Y)>?> ReadEPWFileLocationsAsync(int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads every building model within a circle, the neighbours of a batch.
        /// </summary>
        /// <param name="circle2D">The circle.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the models, stamped with their references, or null when the read failed.</returns>
        protected abstract Task<List<BuildingModel?>?> ReadNeighbourBuildingModelsAsync(Circle2D circle2D, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the occupancy estimate of the given references of one county part.
        /// </summary>
        /// <param name="countyId">The id of the county part the references are filed under.</param>
        /// <param name="references">The references to read.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the occupancy of each reference that has one, or null when the read failed.</returns>
        protected abstract Task<Dictionary<string, OccupancyData>?> ReadOccupancyDatasAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the year built of the given references of one county part, with its source: the user's year when there is one, else the predicted year.
        /// </summary>
        /// <param name="countyId">The id of the county part the references are filed under.</param>
        /// <param name="references">The references to read.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result holds the year built of each reference that has one, or null when the read failed.</returns>
        protected abstract Task<Dictionary<string, (short? YearBuilt, YearBuiltSource? YearBuiltSource)>?> ReadYearBuiltsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Prepares the transport before the run: resolves its converters or clients. A refusal logs its own reason.
        /// </summary>
        /// <returns>True when the run can go on; otherwise, false.</returns>
        protected virtual bool TryInitialize()
        {
            return true;
        }

        /// <summary>
        /// Writes the results of one batch: the rows of <c>building_heat_demand</c> and the heat-demand columns of <c>building_data</c>.
        /// </summary>
        /// <param name="countyId">The id of the county part the buildings are filed under.</param>
        /// <param name="buildingHeatDemandResults">The results of the batch.</param>
        /// <param name="commandTimeout">The timeout in seconds of the write; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result is the number of results written, or null when the write failed.</returns>
        protected abstract Task<int?> WriteAsync(int countyId, List<BuildingHeatDemandResult> buildingHeatDemandResults, int commandTimeout, CancellationToken cancellationToken);

        private void AddFailureSample(string text)
        {
            if (failureSamples.Count < failureSampleCountMax)
            {
                failureSamples.Add(text);
            }
        }
    }
}
