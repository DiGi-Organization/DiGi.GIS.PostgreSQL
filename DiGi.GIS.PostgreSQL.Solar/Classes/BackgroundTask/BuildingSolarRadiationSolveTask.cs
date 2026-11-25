using DiGi.Analytical.Building.Solar;
using DiGi.Core.Classes;
using DiGi.EPW.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Analytical.Solar;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Solar.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Solar.Classes
{
    /// <summary>
    /// Represents a background task that solves the annual solar radiation of every building of a county part for one or more neighbour radii and stores it, whatever the inputs are read from and the results written to (DiGi.GIS.PostgreSQL#136, DiGi.GIS.PostgreSQL.UI#48).
    /// <para>Every read and the write is a protected hook: <see cref="PostgreSQLBuildingSolarRadiationSolveTask"/> implements them with the storage database converters, a client implements them with the GIS Web API. The walk, the skip of stored results, the tiling, the batching and read-ahead, the parallel solve, the counting and the logging are this class's alone, so a result and every count do not depend on the transport.</para>
    /// <para>Each building is solved exactly as the viewer solves it (DiGi.GIS.WebAPI.UI <c>SolarController</c>): the same building model detail level, the same neighbour circle (<c>DiGi.GIS.Analytical.Solar.Query.SolarNeighbourCircle</c>), the same nearest EPW station, the same solver options and solver version, and the same stored form - so a result does not depend on which of the two produced it. Radius 0 solves the building alone (<c>SolarRadiusIsolated</c>): it still shades and blocks the view of its own surfaces, but no neighbour does.</para>
    /// <para>A county part is solved in square tiles of <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.TileSize"/>, by the centroid of each building, at most <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize"/> buildings at a time: one read of the batch's models, one read of every neighbour any of them can see at the largest radius, and one write of the batch's results. The neighbours of each building and radius are then picked in memory, as <c>gis/buildingmodel/itemsbycircle</c> picks them - every model whose bounding box lies within or intersects the building's circle.</para>
    /// <para>The jobs of a batch - one per building and pending radius - are solved several at a time (<see cref="PostgreSQLBuildingSolarRadiationSolveOptions.MaxDegreeOfParallelism"/>, <see cref="Query.BuildingSolarRadiationSolveOutcomes"/>), and the next batch is read while they run; the results are still written one batch at a time, in order, and counted in job order, so the counts, failure samples and stored results are those of a sequential run (DiGi.GIS.PostgreSQL#139). Each batch logs the time spent waiting for its read, solving and writing, its neighbour and caster triangle counts, and the process memory - working set, managed heap after the last collection and after the last full collection, fragmentation, committed memory, GC pause share, full collection count and thread count - so a slow batch can be told apart as heavy geometry or a grown process; a batch after which the machine's memory load reaches the garbage collector's high load threshold is also logged as a warning and counted (<see cref="HighMemoryLoadBatchCount"/>), and each part line names the GC mode (DiGi.GIS.PostgreSQL#167). A cancellation waits for the solves in flight.</para>
    /// <para><see cref="PostgreSQLBuildingSolarRadiationSolveOptions.BuildingCountMax"/> cuts the walk of each part after its first buildings in that order (<see cref="Query.SolarSolveTiles"/>), for a pilot that still logs its part and run summaries (DiGi.GIS.PostgreSQL#138).</para>
    /// <para>A building that cannot be solved - no stored model, no location, no closed envelope, no weather, or above a ceiling - is counted and stepped over. A county part whose buildings, models or neighbours cannot be read, or whose results cannot be written, is failed and logged with the exception, and the run keeps going over the other parts; a run with a failed part reports itself as not succeeded.</para>
    /// </summary>
    public abstract class BuildingSolarRadiationSolveTask : ReportableBackgroundTask<long>
    {
        /// <summary>
        /// The largest number of failure descriptions kept in <see cref="FailureSamples"/>; the counts are never capped.
        /// </summary>
        private const int failureSampleCountMax = 50;

        private readonly List<string> failureSamples = [];

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
        /// Gets the number of batches of the last run after which the machine's memory load was at or above the garbage collector's high memory load threshold (<see cref="Query.IsHighMemoryLoad"/>); each one is logged as a warning (DiGi.GIS.PostgreSQL#167).
        /// <para>Such a batch is not failed: the run recovers once the heavy cell is behind it, and a restart resumes where it stopped (<see cref="PostgreSQLBuildingSolarRadiationSolveOptions.SkipExisting"/>).</para>
        /// </summary>
        public long HighMemoryLoadBatchCount { get; private set; }

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
        /// Gets the number of buildings that were considered during the last run, whatever happened to them; the progress reported. Buildings left out by <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.BuildingCountMax"/> are not counted.
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
            HighMemoryLoadBatchCount = 0;
            OverCeilingCount = 0;
            NotSolvableCount = 0;
            ProcessedBuildingCount = 0;
            ProcessedCountyCount = 0;
            SkippedExistingCount = 0;
            SolvedCount = 0;
            SolveMilliseconds = 0;
            failureSamples.Clear();

            string type = GetType().Name;

            PostgreSQLBuildingSolarRadiationSolveOptions ??= new();

            int commandTimeout = PostgreSQLBuildingSolarRadiationSolveOptions.CommandTimeout;
            int batchSize = Math.Max(1, PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize);
            int? buildingCountMax = PostgreSQLBuildingSolarRadiationSolveOptions.BuildingCountMax is int buildingCountMax_Value ? Math.Max(1, buildingCountMax_Value) : null;
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
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no usable radius - nothing to solve", type);
                return false;
            }

            double radius_Max = radii[^1];

            // The transport logs its own reason for refusing.
            if (!TryInitialize())
            {
                return false;
            }

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

            HashSet<int>? countyIds = PostgreSQLBuildingSolarRadiationSolveOptions.CountyIds;

            int maxDegreeOfParallelism = PostgreSQLBuildingSolarRadiationSolveOptions.MaxDegreeOfParallelism > 0 ? PostgreSQLBuildingSolarRadiationSolveOptions.MaxDegreeOfParallelism : Math.Max(1, Core.Query.ProcessorCount(0.5));

            Serilog.Modify.Log(
                "{Type}: starting over {CountyCount} counties, scope {CountyScope}, radii {Radii} m, tile {TileSize} m, batch size {BatchSize}, building cap {BuildingCountMax}, skip existing {SkipExisting}, receiver ceiling {ReceiverCountMax}, caster ceiling {CasterTriangleCountMax}, max degree of parallelism {MaxDegreeOfParallelism} (used {DegreeOfParallelism}), solver version {SolverVersion}",
                type,
                countyReferences.Count,
                countyIds is null ? "all" : string.Join(", ", countyIds),
                string.Join(", ", radii.Select(x => x.ToString(CultureInfo.InvariantCulture))),
                tileSize,
                batchSize,
                buildingCountMax is null ? "none" : buildingCountMax.Value.ToString(CultureInfo.InvariantCulture),
                skipExisting,
                receiverCountMax,
                casterTriangleCountMax,
                PostgreSQLBuildingSolarRadiationSolveOptions.MaxDegreeOfParallelism,
                maxDegreeOfParallelism,
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
                long countyReadMilliseconds = 0;
                long countySolveWallMilliseconds = 0;
                long countyWriteMilliseconds = 0;
                long countyBuildingCount = 0;
                long countyPendingCount = 0;
                long countyHighMemoryLoadBatchCount = 0;
                bool countyCapped = false;

                // Weather files read for this part, by station id: a part uses a handful of stations, each read once. Only
                // the read of a batch touches it, and a batch is read only once the previous read has finished.
                Dictionary<int, EPWFile?> ePWFiles = [];

                // The read of the next batch, started while the current one is solved; awaited before the part is left.
                Task<BuildingSolarRadiationSolveBatch>? task_Next = null;

                try
                {
                    List<Building2DCentroid>? building2DCentroids = await ReadBuilding2DCentroidsAsync(countyId, commandTimeout, cancellationToken);
                    if (building2DCentroids is null)
                    {
                        throw new InvalidOperationException("the building centroids could not be read");
                    }

                    // The radii each building still needs: every radius, less the ones already stored for this solver version.
                    List<HashSet<string>> references_Done = [];
                    foreach (double radius in radii)
                    {
                        HashSet<string>? references = skipExisting ? await ReadSolvedReferencesAsync(countyId, radius, solverVersion, commandTimeout, cancellationToken) : null;
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

                    // Tiles in row order, buildings within a tile by reference, so a re-run walks the same batches; the
                    // cap cuts the walk after ordering, so a capped run solves a compact cluster. The batches of every tile
                    // form one walk, so the read-ahead crosses tile boundaries.
                    countyPendingCount = building2DCentroids_Pending.Count;
                    countyCapped = buildingCountMax is not null && countyPendingCount > buildingCountMax.Value;
                    List<List<string>> tiles = building2DCentroids_Pending.SolarSolveTiles(tileSize, buildingCountMax) ?? [];

                    List<List<string>> references_Batches = [];
                    foreach (List<string> references_Tile in tiles)
                    {
                        for (int offset = 0; offset < references_Tile.Count; offset += batchSize)
                        {
                            references_Batches.Add(references_Tile.GetRange(offset, Math.Min(batchSize, references_Tile.Count - offset)));
                        }
                    }

                    if (references_Batches.Count != 0)
                    {
                        task_Next = ReadBatchAsync(countyId, references_Batches[0], radii_ByReference);
                    }

                    for (int batchIndex = 0; batchIndex < references_Batches.Count; batchIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Stopwatch stopwatch = Stopwatch.StartNew();
                        BuildingSolarRadiationSolveBatch buildingSolarRadiationSolveBatch = await task_Next!;
                        long readMilliseconds = stopwatch.ElapsedMilliseconds;

                        task_Next = batchIndex + 1 < references_Batches.Count ? ReadBatchAsync(countyId, references_Batches[batchIndex + 1], radii_ByReference) : null;

                        // The solve leaves the asynchronous flow, so the next read goes on while the jobs run.
                        stopwatch.Restart();
                        List<BuildingSolarRadiationSolveOutcome> buildingSolarRadiationSolveOutcomes = buildingSolarRadiationSolveBatch.BuildingSolarRadiationSolveJobs.Count == 0
                            ? []
                            : await Task.Run(() => buildingSolarRadiationSolveBatch.BuildingSolarRadiationSolveJobs.BuildingSolarRadiationSolveOutcomes(buildingSolarRadiationSolveBatch.Neighbours, casterTriangleCountMax, maxDegreeOfParallelism, cancellationToken), cancellationToken);
                        long solveWallMilliseconds = stopwatch.ElapsedMilliseconds;

                        // The results are collected in job order and written in one call per batch.
                        List<GIS.Classes.BuildingSolarRadiationResult> results = [];
                        long solveMilliseconds = 0;
                        long casterTriangleCount = 0;
                        int casterTriangleCount_Max = 0;
                        foreach (BuildingSolarRadiationSolveOutcome buildingSolarRadiationSolveOutcome in buildingSolarRadiationSolveOutcomes)
                        {
                            solveMilliseconds += buildingSolarRadiationSolveOutcome.Milliseconds;
                            casterTriangleCount += buildingSolarRadiationSolveOutcome.CasterTriangleCount;
                            casterTriangleCount_Max = Math.Max(casterTriangleCount_Max, buildingSolarRadiationSolveOutcome.CasterTriangleCount);
                            if (buildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcomeType == BuildingSolarRadiationSolveOutcomeType.Solved && buildingSolarRadiationSolveOutcome.BuildingSolarRadiationResult is not null)
                            {
                                results.Add(buildingSolarRadiationSolveOutcome.BuildingSolarRadiationResult);
                            }
                        }

                        // Counted before the write, as a sequential run counts a building it steps over before it writes.
                        Aggregate(countyId, buildingSolarRadiationSolveBatch, buildingSolarRadiationSolveOutcomes);

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
                        SolveMilliseconds += solveMilliseconds;
                        countySolvedCount += results.Count;
                        countySolveMilliseconds += solveMilliseconds;
                        countyReadMilliseconds += readMilliseconds;
                        countySolveWallMilliseconds += solveWallMilliseconds;
                        countyWriteMilliseconds += writeMilliseconds;
                        countyBuildingCount += buildingSolarRadiationSolveBatch.BuildingCount;
                        ProcessedBuildingCount += buildingSolarRadiationSolveBatch.BuildingCount;
                        progress.Report(ProcessedBuildingCount);

                        (long workingSetMB, long heapMB, long heapMB_FullCollection, long fragmentedMB, long committedMB, double pauseTimePercentage, int fullCollectionCount, int threadCount) = Memory();

                        Serilog.Modify.Log(
                            Serilog.Enums.LogEventLevel.Information,
                            "{Type}: county part {CountyId} batch {BatchNumber} of {BatchCount} - {BuildingCount} buildings, {JobCount} jobs, {ResultCount} results, {NeighbourCount} neighbours, {CasterTriangleCount} caster triangles summed, {CasterTriangleCountMax} at most, {ReadMilliseconds} ms waiting for the read, {SolveWallMilliseconds} ms solve wall, {SolveMilliseconds} ms solve summed, {WriteMilliseconds} ms writing; memory {WorkingSetMB} MB working set, {HeapMB} MB heap, {HeapMBFullCollection} MB heap after the last full collection, {FragmentedMB} MB fragmented, {CommittedMB} MB committed, {PauseTimePercentage} % GC pause, {FullCollectionCount} full collections, {ThreadCount} threads",
                            type,
                            countyId,
                            batchIndex + 1,
                            references_Batches.Count,
                            buildingSolarRadiationSolveBatch.BuildingCount,
                            buildingSolarRadiationSolveBatch.BuildingSolarRadiationSolveJobs.Count,
                            results.Count,
                            buildingSolarRadiationSolveBatch.Neighbours.Count,
                            casterTriangleCount,
                            casterTriangleCount_Max,
                            readMilliseconds,
                            solveWallMilliseconds,
                            solveMilliseconds,
                            writeMilliseconds,
                            workingSetMB,
                            heapMB,
                            heapMB_FullCollection,
                            fragmentedMB,
                            committedMB,
                            pauseTimePercentage,
                            fullCollectionCount,
                            threadCount);

                        // The machine is short of memory: say so beside the batch that got it there, instead of letting
                        // the next batches crawl through the pagefile unexplained (DiGi.GIS.PostgreSQL#167).
                        GCMemoryInfo gCMemoryInfo_Load = GC.GetGCMemoryInfo();
                        if (Query.IsHighMemoryLoad(gCMemoryInfo_Load.MemoryLoadBytes, gCMemoryInfo_Load.HighMemoryLoadThresholdBytes))
                        {
                            HighMemoryLoadBatchCount++;
                            countyHighMemoryLoadBatchCount++;

                            Serilog.Modify.Log(
                                Serilog.Enums.LogEventLevel.Warning,
                                "{Type}: county part {CountyId} batch {BatchNumber} of {BatchCount} - machine memory load {MemoryLoadMB} MB is at or above the high load threshold {HighMemoryLoadThresholdMB} MB; working set {WorkingSetMB} MB, largest job {CasterTriangleCountMax} caster triangles, degree of parallelism {DegreeOfParallelism}",
                                type,
                                countyId,
                                batchIndex + 1,
                                references_Batches.Count,
                                gCMemoryInfo_Load.MemoryLoadBytes / (1024 * 1024),
                                gCMemoryInfo_Load.HighMemoryLoadThresholdBytes / (1024 * 1024),
                                workingSetMB,
                                casterTriangleCount_Max,
                                maxDegreeOfParallelism);
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
                    Serilog.Modify.Log(exception, "{Type}: county part {CountyId} failed after {SolvedCount} results", type, countyId, countySolvedCount);
                }
                finally
                {
                    // A read still in flight when the part stops is finished and observed before anything else starts; what
                    // it found is dropped with the part.
                    if (task_Next is not null)
                    {
                        try
                        {
                            await task_Next;
                        }
                        catch (Exception)
                        {
                            // Observed only: the part has already stopped for its own reason.
                        }
                    }
                }

                if (countyFailed)
                {
                    FailedCountyCount++;
                    continue;
                }

                ProcessedCountyCount++;
                (long workingSetMB_County, long heapMB_County, long heapMB_FullCollection_County, long fragmentedMB_County, long committedMB_County, double pauseTimePercentage_County, int fullCollectionCount_County, int threadCount_County) = Memory();
                Serilog.Modify.Log(
                    Serilog.Enums.LogEventLevel.Information,
                    "{Type}: county part {CountyId} processed - {BuildingCount} of {PendingCount} pending buildings solved for, capped {Capped}, {SolvedCount} results stored, {SolveMilliseconds} ms solving, {AverageMilliseconds} ms per result, {ReadMilliseconds} ms waiting for reads, {SolveWallMilliseconds} ms solve wall, {WriteMilliseconds} ms writing, degree of parallelism {DegreeOfParallelism}; memory {WorkingSetMB} MB working set, {HeapMB} MB heap, {HeapMBFullCollection} MB heap after the last full collection, {FragmentedMB} MB fragmented, {CommittedMB} MB committed, {PauseTimePercentage} % GC pause, {FullCollectionCount} full collections, {ThreadCount} threads, server GC {IsServerGC}, {HighMemoryLoadBatchCount} batches at high memory load",
                    type,
                    countyId,
                    countyBuildingCount,
                    countyPendingCount,
                    countyCapped,
                    countySolvedCount,
                    countySolveMilliseconds,
                    countySolvedCount == 0 ? 0 : countySolveMilliseconds / countySolvedCount,
                    countyReadMilliseconds,
                    countySolveWallMilliseconds,
                    countyWriteMilliseconds,
                    maxDegreeOfParallelism,
                    workingSetMB_County,
                    heapMB_County,
                    heapMB_FullCollection_County,
                    fragmentedMB_County,
                    committedMB_County,
                    pauseTimePercentage_County,
                    fullCollectionCount_County,
                    threadCount_County,
                    GCSettings.IsServerGC,
                    countyHighMemoryLoadBatchCount);

                ePWFiles.Clear();

                // Reads one batch of buildings of the part: the stored models, the neighbours any of them can see at the
                // largest radius and the weather of each, and turns them into solve jobs. It counts nothing - it runs while
                // the previous batch is solved - and records the buildings it steps over, in order. Throws when the models or
                // neighbours cannot be read.
                async Task<BuildingSolarRadiationSolveBatch> ReadBatchAsync(int countyId_Batch, List<string> references_Batch, Dictionary<string, List<double>> radii_ByReference_Batch)
                {
                    List<(string? Reference, BuildingSolarRadiationSolveOutcomeType BuildingSolarRadiationSolveOutcomeType, int Count, string? Reason)> issues = [];
                    List<(string Reference, int Count, bool HasWeather)> receivers_Batch = [];
                    List<BuildingSolarRadiationSolveJob> buildingSolarRadiationSolveJobs = [];
                    List<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel, double XMin, double YMin, double XMax, double YMax)> neighbours = [];

                    List<DiGi.Analytical.Building.Classes.BuildingModel?>? buildingModels_Stored = await ReadBuildingModelsAsync(countyId_Batch, references_Batch, commandTimeout, cancellationToken);
                    if (buildingModels_Stored is null)
                    {
                        throw new InvalidOperationException("the building models could not be read");
                    }

                    // The receivers: stamped, with their outward normals and their circle at the largest radius.
                    List<(DiGi.Analytical.Building.Classes.BuildingModel BuildingModel, string Reference, Dictionary<string, Vector3D> Normals, Circle2D Circle)> receivers = [];
                    HashSet<string> references_Found = [];
                    foreach (DiGi.Analytical.Building.Classes.BuildingModel? buildingModel in buildingModels_Stored)
                    {
                        if (buildingModel is null)
                        {
                            continue;
                        }

                        if (!buildingModel.TryGetValue(GIS.Analytical.Enums.BuildingModelParameter.Reference, out string? reference) || string.IsNullOrWhiteSpace(reference) || !radii_ByReference_Batch.TryGetValue(reference!, out List<double>? radii_Building) || !references_Found.Add(reference!))
                        {
                            continue;
                        }

                        if (!GIS.Analytical.Modify.UpdateBuildingInformation(buildingModel))
                        {
                            issues.Add((reference, BuildingSolarRadiationSolveOutcomeType.NotSolvable, radii_Building.Count, "it cannot be located"));
                            continue;
                        }

                        Dictionary<string, Vector3D>? normals = buildingModel.SolarReceiverNormals();
                        if (normals is null || normals.Count == 0)
                        {
                            issues.Add((reference, BuildingSolarRadiationSolveOutcomeType.NotSolvable, radii_Building.Count, "it has no closed external envelope"));
                            continue;
                        }

                        if (normals.Count > receiverCountMax)
                        {
                            issues.Add((reference, BuildingSolarRadiationSolveOutcomeType.OverCeiling, radii_Building.Count, null));
                            continue;
                        }

                        Circle2D? circle2D = buildingModel.SolarNeighbourCircle(radius_Max);
                        if (circle2D is null)
                        {
                            issues.Add((reference, BuildingSolarRadiationSolveOutcomeType.NotSolvable, radii_Building.Count, "it has no bounding box"));
                            continue;
                        }

                        receivers.Add((buildingModel, reference!, normals, circle2D));
                    }

                    // A building with no stored model is counted without a sample.
                    foreach (string reference in references_Batch)
                    {
                        if (!references_Found.Contains(reference))
                        {
                            issues.Add((null, BuildingSolarRadiationSolveOutcomeType.NotSolvable, radii_ByReference_Batch[reference].Count, null));
                        }
                    }

                    if (receivers.Count == 0)
                    {
                        return new BuildingSolarRadiationSolveBatch(references_Batch.Count, buildingSolarRadiationSolveJobs, neighbours, issues, receivers_Batch);
                    }

                    // Every neighbour any receiver can see at the largest radius, in one read: the smallest circle around the
                    // batch's centre that holds every receiver's circle.
                    double x_Centre = receivers.Average(x => x.Circle.Center!.X);
                    double y_Centre = receivers.Average(x => x.Circle.Center!.Y);
                    double radius_Batch = receivers.Max(x => Math.Sqrt(Math.Pow(x.Circle.Center!.X - x_Centre, 2) + Math.Pow(x.Circle.Center!.Y - y_Centre, 2)) + x.Circle.Radius);

                    List<DiGi.Analytical.Building.Classes.BuildingModel?>? neighbours_Batch = radii.Any(x => x > 0)
                        ? await ReadNeighbourBuildingModelsAsync(new Circle2D(new Point2D(x_Centre, y_Centre), radius_Batch), commandTimeout, cancellationToken)
                        : [];
                    if (neighbours_Batch is null)
                    {
                        throw new InvalidOperationException("the neighbours could not be read");
                    }

                    // The planar bounding box of each neighbour, computed once for every receiver and radius.
                    foreach (DiGi.Analytical.Building.Classes.BuildingModel? buildingModel_Neighbour in neighbours_Batch)
                    {
                        BoundingBox3D? boundingBox3D = buildingModel_Neighbour?.GetBoundingBox();
                        Point3D? min = boundingBox3D?.Min;
                        Point3D? max = boundingBox3D?.Max;
                        if (min is not null && max is not null)
                        {
                            neighbours.Add((buildingModel_Neighbour!, min.X, min.Y, max.X, max.Y));
                        }
                    }

                    // The weather of each receiver, read before the solve and kept for the part.
                    foreach ((DiGi.Analytical.Building.Classes.BuildingModel buildingModel, string reference, Dictionary<string, Vector3D> normals, Circle2D _) in receivers)
                    {
                        Point2D? centre = buildingModel.SolarNeighbourCircle(0)?.Center;
                        int? ePWFileId = centre is null ? null : ePWFileLocations.NearestEPWFileId(centre.X, centre.Y);
                        EPWFile? ePWFile = null;
                        if (ePWFileId is not null && !ePWFiles.TryGetValue(ePWFileId.Value, out ePWFile))
                        {
                            ePWFile = await ReadEPWFileAsync(ePWFileId.Value, commandTimeout, cancellationToken);
                            ePWFiles[ePWFileId.Value] = ePWFile;
                        }

                        List<double> radii_Building = radii_ByReference_Batch[reference];
                        if (ePWFileId is null || ePWFile is null)
                        {
                            receivers_Batch.Add((reference, radii_Building.Count, false));
                            continue;
                        }

                        receivers_Batch.Add((reference, radii_Building.Count, true));
                        foreach (double radius in radii_Building)
                        {
                            buildingSolarRadiationSolveJobs.Add(new BuildingSolarRadiationSolveJob(buildingModel, reference, normals, radius, countyId_Batch, ePWFileId.Value, ePWFile));
                        }
                    }

                    return new BuildingSolarRadiationSolveBatch(references_Batch.Count, buildingSolarRadiationSolveJobs, neighbours, issues, receivers_Batch);
                }
            }

            Serilog.Modify.Log(
                "{Type}: finished - {ProcessedCountyCount} county parts, {FailedCountyCount} failed, {BuildingCount} buildings, {SolvedCount} results stored, {SkippedExistingCount} already stored, {OverCeilingCount} over a ceiling, {NotSolvableCount} not solvable, {FailedCount} failed, {SolveMilliseconds} ms solving, {HighMemoryLoadBatchCount} batches at high memory load",
                type,
                ProcessedCountyCount,
                FailedCountyCount,
                ProcessedBuildingCount,
                SolvedCount,
                SkippedExistingCount,
                OverCeilingCount,
                NotSolvableCount,
                FailedCount,
                SolveMilliseconds,
                HighMemoryLoadBatchCount);

            return FailedCountyCount == 0;

            // Counts a solved and written batch in the order a sequential run met its buildings: the buildings stepped over
            // while reading, then receiver by receiver its missing weather or its outcomes, radius by radius.
            void Aggregate(int countyId_Batch, BuildingSolarRadiationSolveBatch buildingSolarRadiationSolveBatch, List<BuildingSolarRadiationSolveOutcome> buildingSolarRadiationSolveOutcomes)
            {
                foreach ((string? reference, BuildingSolarRadiationSolveOutcomeType buildingSolarRadiationSolveOutcomeType, int count, string? reason) in buildingSolarRadiationSolveBatch.Issues)
                {
                    if (buildingSolarRadiationSolveOutcomeType == BuildingSolarRadiationSolveOutcomeType.OverCeiling)
                    {
                        OverCeilingCount += count;
                    }
                    else if (reference is null || reason is null)
                    {
                        NotSolvableCount += count;
                    }
                    else
                    {
                        NotSolvable(reference, count, reason);
                    }
                }

                int index = 0;
                foreach ((string reference, int count, bool hasWeather) in buildingSolarRadiationSolveBatch.Receivers)
                {
                    if (!hasWeather)
                    {
                        NotSolvable(reference, count, "no weather file could be read for it");
                        continue;
                    }

                    for (int i = 0; i < count && index < buildingSolarRadiationSolveOutcomes.Count; i++, index++)
                    {
                        BuildingSolarRadiationSolveOutcome buildingSolarRadiationSolveOutcome = buildingSolarRadiationSolveOutcomes[index];
                        double radius = buildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveJob.Radius;
                        switch (buildingSolarRadiationSolveOutcome.BuildingSolarRadiationSolveOutcomeType)
                        {
                            case BuildingSolarRadiationSolveOutcomeType.Solved:
                                break;

                            case BuildingSolarRadiationSolveOutcomeType.NotSolvable:
                                NotSolvable(reference, 1, buildingSolarRadiationSolveOutcome.Reason ?? "it cannot be solved");
                                break;

                            case BuildingSolarRadiationSolveOutcomeType.OverCeiling:
                                OverCeilingCount++;
                                break;

                            default:
                                Failed(reference, radius, buildingSolarRadiationSolveOutcome.Reason ?? "the solve did not run");
                                if (buildingSolarRadiationSolveOutcome.Exception is Exception exception)
                                {
                                    Serilog.Modify.Log(exception, "{Type}: building {Reference} of county part {CountyId} failed at radius {Radius} m", type, reference, countyId_Batch, radius);
                                }

                                break;
                        }
                    }
                }
            }

            // The process memory a batch and a part line log, in MB: the working set, the managed heap after the last
            // collection and after the last full (blocking or background) one - the figure that grows across batches only
            // when something keeps their data - its fragmentation, the committed memory, the share of time paused for
            // collections, the number of full collections and the number of threads (DiGi.GIS.PostgreSQL#167).
            static (long WorkingSetMB, long HeapMB, long HeapMBFullCollection, long FragmentedMB, long CommittedMB, double PauseTimePercentage, int FullCollectionCount, int ThreadCount) Memory()
            {
                const long bytesPerMB = 1024 * 1024;

                GCMemoryInfo gCMemoryInfo = GC.GetGCMemoryInfo();
                GCMemoryInfo gCMemoryInfo_FullBlocking = GC.GetGCMemoryInfo(GCKind.FullBlocking);
                GCMemoryInfo gCMemoryInfo_Background = GC.GetGCMemoryInfo(GCKind.Background);
                GCMemoryInfo gCMemoryInfo_FullCollection = gCMemoryInfo_FullBlocking.Index >= gCMemoryInfo_Background.Index ? gCMemoryInfo_FullBlocking : gCMemoryInfo_Background;

                return (
                    Environment.WorkingSet / bytesPerMB,
                    gCMemoryInfo.HeapSizeBytes / bytesPerMB,
                    gCMemoryInfo_FullCollection.HeapSizeBytes / bytesPerMB,
                    gCMemoryInfo.FragmentedBytes / bytesPerMB,
                    gCMemoryInfo.TotalCommittedBytes / bytesPerMB,
                    Math.Round(gCMemoryInfo.PauseTimePercentage, 1),
                    GC.CollectionCount(2),
                    ThreadPool.ThreadCount);
            }

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

        /// <summary>
        /// Reads the stored building models of the given references of one county part - the receivers of a batch.
        /// </summary>
        /// <param name="countyId">The identifier of the county part the references are filed under.</param>
        /// <param name="references">The <c>building_2d</c> references of the batch, at most <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.BatchSize"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains the latest stored model of each reference found, stamped with its reference (a reference with no model is simply absent, an entry that cannot be converted may be null), or null when the read failed.</returns>
        protected abstract Task<List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadBuildingModelsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the centroid of every building filed under one county part, which the part is tiled and scoped by.
        /// </summary>
        /// <param name="countyId">The identifier of the county part; only buildings filed under this part may be returned.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains the centroids (empty when the part has no building), or null when the read failed.</returns>
        protected abstract Task<List<Building2DCentroid>?> ReadBuilding2DCentroidsAsync(int countyId, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads every county part the run may walk; <see cref="PostgreSQLBuildingSolarRadiationSolveOptions.CountyIds"/> then narrows them.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains every county part - one per part, not one per code - or null when the read failed.</returns>
        protected abstract Task<List<AdministrativeAreal2DReference>?> ReadCountyReferencesAsync(int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads one weather file by its station identifier; each station a county part uses is read once.
        /// </summary>
        /// <param name="id">The identifier of the EPW file, from <see cref="ReadEPWFileLocationsAsync"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains the EPW file, or null when it cannot be read - the buildings that need it are then counted as not solvable.</returns>
        protected abstract Task<EPWFile?> ReadEPWFileAsync(int id, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the identifier and location of every stored EPW station, from which each building is given its nearest one.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains the stations, or null when the read failed.</returns>
        protected abstract Task<List<(int Id, double X, double Y)>?> ReadEPWFileLocationsAsync(int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the stored building models of every building whose footprint lies within or intersects a circle, across county borders - the neighbours of a batch.
        /// </summary>
        /// <param name="circle2D">The circle around the batch that holds every receiver's circle at the largest radius.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains the models (empty when the circle holds no building), or null when the read failed - never read a failure as no neighbour.</returns>
        protected abstract Task<List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadNeighbourBuildingModelsAsync(Circle2D circle2D, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the references of the buildings of one county part that already carry a result for a radius and solver version.
        /// </summary>
        /// <param name="countyId">The identifier of the county part.</param>
        /// <param name="radius">The radius key, in m, as stored (<c>SolarRadiusKey</c>).</param>
        /// <param name="solverVersion">The solver version a stored result must carry to count.</param>
        /// <param name="commandTimeout">The timeout in seconds of the read; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains the references (empty when none is stored), or null when the read failed - never read a failure as nothing solved.</returns>
        protected abstract Task<HashSet<string>?> ReadSolvedReferencesAsync(int countyId, double radius, int solverVersion, int commandTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Checks, before anything is read, that the transport has what the run needs, logging the reason at Error level when it has not.
        /// </summary>
        /// <returns>True when the run can start; otherwise, false, and the run ends without reading anything.</returns>
        protected virtual bool TryInitialize()
        {
            return true;
        }

        /// <summary>
        /// Writes the results of one batch of one county part.
        /// </summary>
        /// <param name="countyId">The identifier of the county part every result is filed under.</param>
        /// <param name="buildingSolarRadiationResults">The results of the batch, in job order; never empty.</param>
        /// <param name="commandTimeout">The timeout in seconds of the write; 0 disables it.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task whose result contains the number of results written, or null when the write failed - the part is then failed.</returns>
        protected abstract Task<int?> WriteAsync(int countyId, List<GIS.Classes.BuildingSolarRadiationResult> buildingSolarRadiationResults, int commandTimeout, CancellationToken cancellationToken);

        // Keeps a failure description while the sample has room; the counts are kept by the callers.
        private void AddFailureSample(string text)
        {
            if (failureSamples.Count < failureSampleCountMax)
            {
                failureSamples.Add(text);
            }
        }
    }
}
