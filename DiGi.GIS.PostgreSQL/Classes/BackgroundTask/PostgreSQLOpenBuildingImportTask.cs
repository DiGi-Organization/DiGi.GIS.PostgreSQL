using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.IO.Classes;
using DiGi.GIS.IO.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.OSM.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that imports one release of an open building dataset - EUBUCCO, Overture, OSM, DBSM, Microsoft or GHS-OBAT - into its footprint table, with the release recorded in the <c>building_dataset</c> register (DiGi.GIS.PostgreSQL#125).
    /// <para>The task owns the run: it validates the options, resolves the scope (county parts widened to their siblings, or a pilot-area bounding box resolved against the database), reads the county parts from the Main database once, opens the readers for the dataset's file format and hands the footprint stream, the county parts and the shared <see cref="OpenBuildingSourcePosition"/> to <see cref="OpenBuildingPostgreSQLConverter.ImportAsync(IAsyncEnumerable{Footprint}, BuildingDatasetRelease, IEnumerable{int}, IReadOnlyList{AdministrativeAreal2D}, BoundingBox2D?, IProgress{long}, OpenBuildingSourcePosition?, double, int, int, CancellationToken)"/>, which writes to the Storage database. The converter owns the load path, including the transformations: one per source coordinate reference system, built lazily as locals of the run - never per footprint, never shared between runs.</para>
    /// <para>The bounding box is applied after the reprojection leg, never handed to a reader: every reader interprets a box in its own file's source coordinate reference system, and an EPSG:2180 box passed into an EPSG:3035 file would filter everything out silently. The OSM branch reads synchronously through <c>DiGi.OSM</c> and its reader-side rejection summary is read after the stream has been enumerated to its end, which is when that summary completes.</para>
    /// <para>A run reports success unless it was cancelled. Footprints outside the scope and footprints that file nowhere are tallied rather than gated on - a nationwide file read for pilot areas leaves a large ordinary residue, and gating on it would report a correct run as a failure while naming nothing.</para>
    /// </summary>
    public class PostgreSQLOpenBuildingImportTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// Bulk imports and whole-county reads exceed the 30 second default; allow up to 10 minutes per statement.
        /// </summary>
        private const int commandTimeout = 600;

        /// <summary>
        /// The distance in metres a source file's extent may stay away from the scope and still be read (DiGi.GIS.PostgreSQL#170): it covers a building filed under a Microsoft tile but reaching beyond its edge, the filing tolerance and the 32-bit rounding of a GeoPackage R-tree, with room to spare.
        /// </summary>
        private const double distance_SourceExtentMargin = 1000;

        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Constructor with Dependency Injection.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager that provides the import and administrative-area converters.</param>
        public PostgreSQLOpenBuildingImportTask(GISPostgreSQLConverterManager? gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the number of footprints skipped as out of the run's scope.
        /// </summary>
        public long FilteredCount { get; private set; }

        /// <summary>
        /// Gets the configuration for the PostgreSQL import. These options are used when the task is started.
        /// </summary>
        public PostgreSQLOpenBuildingImportTaskOptions PostgreSQLOpenBuildingImportTaskOptions { get; set; } = new PostgreSQLOpenBuildingImportTaskOptions();

        /// <summary>
        /// Gets the identifier of the register row the run upserted in building_dataset, or null when no run completed (DiGi.GIS.PostgreSQL#131). Read after the run, like the tallies.
        /// </summary>
        public int? DatasetId { get; private set; }

        /// <summary>
        /// Gets the source files the run left unread because their own extent cannot reach the scope (DiGi.GIS.PostgreSQL#170); empty when every file was read. Their footprints are counted nowhere - neither stored, out of scope nor rejected.
        /// </summary>
        public List<string> SkippedSourcePaths { get; private set; } = [];

        /// <summary>
        /// Gets the number of footprints the readers refused before the load path saw them (OSM PBF objects that could not become a footprint). Read after the run, when the reader summaries are complete.
        /// </summary>
        public long ReaderRejectionCount { get; private set; }

        /// <summary>
        /// Gets the number of footprints rejected before the database - untransformable geometry, a footprint no county part reaches, a missing partition.
        /// </summary>
        public long RejectionCount { get; private set; }

        /// <summary>
        /// Gets the number of rows the import stored.
        /// </summary>
        public long RowCount { get; private set; }

        /// <summary>
        /// Executes the background task: validating the options, resolving the scope, opening the readers and importing the release.
        /// </summary>
        /// <param name="progress">A progress reporter carrying the running total of rows written.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true unless the run was cancelled.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            RowCount = 0;
            FilteredCount = 0;
            RejectionCount = 0;
            ReaderRejectionCount = 0;
            DatasetId = null;
            SkippedSourcePaths = [];

            PostgreSQLOpenBuildingImportTaskOptions options = PostgreSQLOpenBuildingImportTaskOptions;
            if (options is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no options - nothing to import", nameof(PostgreSQLOpenBuildingImportTask));
                return false;
            }

            OpenBuildingPostgreSQLConverter? openBuildingPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<OpenBuildingPostgreSQLConverter>();
            if (openBuildingPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the open building tables cannot be written", nameof(PostgreSQLOpenBuildingImportTask), nameof(OpenBuildingPostgreSQLConverter));
                return false;
            }

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the scope cannot be resolved", nameof(PostgreSQLOpenBuildingImportTask), nameof(AdministrativeAreal2DPostgreSQLConverter));
                return false;
            }

            string? datasetKey = options.DatasetKey;
            string? tableName = Query.OpenBuildingTableName(datasetKey);
            if (tableName is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} names no open building dataset", nameof(PostgreSQLOpenBuildingImportTask), datasetKey ?? "(null)");
                return false;
            }

            if (string.IsNullOrWhiteSpace(options.Version) || options.DownloadDate is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the release needs both a version and a download date - the register keys on them", nameof(PostgreSQLOpenBuildingImportTask));
                return false;
            }

            HashSet<string>? sourcePaths = options.SourcePaths;
            if (sourcePaths is null || sourcePaths.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no source file was named", nameof(PostgreSQLOpenBuildingImportTask));
                return false;
            }

            foreach (string path in sourcePaths)
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: source file {Path} does not exist", nameof(PostgreSQLOpenBuildingImportTask), path ?? "(null)");
                    return false;
                }
            }

            // Every Microsoft file needs its own quadkey - it is the source identity, and the reader restarts the
            // feature index per file - so the resolution is checked here, before anything is truncated
            // (DiGi.GIS.PostgreSQL#132).
            Dictionary<string, string>? quadkeys = null;
            if (string.Equals(tableName, Constants.TableName.Microsoft_Building2D, StringComparison.Ordinal) && !Query.TryMicrosoftQuadkeys(sourcePaths, options.Quadkey, out quadkeys, out string? quadkeyMessage))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {Message}", nameof(PostgreSQLOpenBuildingImportTask), quadkeyMessage ?? "the Microsoft quadkeys could not be resolved");
                return false;
            }

            bool hasCountyScope = options.CountyIds is not null && options.CountyIds.Count > 0;
            bool hasBoundingBoxScope = options.BoundingBox2D is not null;

            if (hasCountyScope == hasBoundingBoxScope)
            {
                // Exactly one form: both is ambiguous (which one truncated the partitions?) and neither has no run.
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the scope must be a county list or a bounding box, exactly one of the two", nameof(PostgreSQLOpenBuildingImportTask));
                return false;
            }

            // The schema first: a database that cannot hold the rows should stop the run now rather than
            // after the scope has been resolved and every file has been opened.
            if (!await openBuildingPostgreSQLConverter.CreateTablesAsync(commandTimeout, cancellationToken))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the open building tables could not be created - the run would have no place to write to", nameof(PostgreSQLOpenBuildingImportTask));
                return false;
            }

            HashSet<int>? countyIds_Scope;
            BoundingBox2D? boundingBox2D_Scope = null;

            if (hasBoundingBoxScope)
            {
                boundingBox2D_Scope = options.BoundingBox2D;

                // The SQL prefilter of the scope: the county parts whose stored envelopes reach the pilot box
                // (grown by the tolerance). The static flat overload is deliberate - the instance overload
                // delegates to the hierarchical Country/Voivodeship/County search, which narrows through
                // ancestor rows and answers empty on a database that holds no ancestors over the box, while
                // the scope is exactly "every county whose envelope overlaps the box", the same test the
                // import's in-memory filing reproduces.
                await using NpgsqlConnection? npgsqlConnection_Scope = DiGi.PostgreSQL.Create.NpgsqlConnection(administrativeAreal2DPostgreSQLConverter.ConnectionData);
                if (npgsqlConnection_Scope is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no connection to resolve the bounding-box scope against - nothing was imported", nameof(PostgreSQLOpenBuildingImportTask));
                    return false;
                }

                await npgsqlConnection_Scope.OpenAsync(cancellationToken);

                List<AdministrativeAreal2D>? counties_Scope = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection_Scope, boundingBox2D_Scope, Enums.AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                countyIds_Scope = counties_Scope is null ? null : [.. counties_Scope.ConvertAll(x => x.Id)];
            }
            else
            {
                // Named parts are widened to their siblings: a county held in several pieces imports as one
                // whole, so its partitions are replaced together and no sibling part keeps an old release.
                countyIds_Scope = await administrativeAreal2DPostgreSQLConverter.SiblingCountyIdsAsync(options.CountyIds, commandTimeout, cancellationToken);
            }

            if (countyIds_Scope is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the scope could not be resolved against the database - nothing was imported", nameof(PostgreSQLOpenBuildingImportTask));
                return false;
            }

            if (countyIds_Scope.Count == 0)
            {
                Serilog.Modify.Log("{Type}: no county part is in scope of {DatasetKey} - nothing to import", nameof(PostgreSQLOpenBuildingImportTask), datasetKey!);
                return true;
            }

            // The county parts live on the Main database and the open building tables on Storage, so the
            // converter cannot read them: they are read here, once, and handed over (no join across the two).
            List<AdministrativeAreal2D>? counties = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(Enums.AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (counties is null || counties.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county part could be read from the Main database - nothing can be filed", nameof(PostgreSQLOpenBuildingImportTask));
                return false;
            }

            // Files that cannot reach the scope are left unread (DiGi.GIS.PostgreSQL#170): a nationwide release read
            // for a few counties otherwise spends most of its time on files that keep nothing. The scope is the
            // envelope of every county part in it - the stored envelope the import's own filing prefilter uses - or
            // the scope box; a part the county list does not carry has no envelope, and then every file is read.
            PostgreSQLOpenBuildingImportTaskOptions options_Read = options;
            if (options.SkipSourcesOutOfScope && BuildingDatasetFromKey(datasetKey) is BuildingDataset buildingDataset_Skip)
            {
                List<BoundingBox2D?> boundingBox2Ds_Scope = [];
                if (boundingBox2D_Scope is not null)
                {
                    boundingBox2Ds_Scope.Add(boundingBox2D_Scope);
                }
                else
                {
                    Dictionary<int, AdministrativeAreal2D> counties_ById = [];
                    foreach (AdministrativeAreal2D county in counties)
                    {
                        counties_ById[county.Id] = county;
                    }

                    foreach (int countyId in countyIds_Scope)
                    {
                        boundingBox2Ds_Scope.Add(counties_ById.TryGetValue(countyId, out AdministrativeAreal2D? county) ? county.BoundingBox2D : null);
                    }
                }

                SkippedSourcePaths = await Query.SourcePathsOutOfScopeAsync(sourcePaths, buildingDataset_Skip, boundingBox2Ds_Scope, distance_SourceExtentMargin, cancellationToken);

                if (SkippedSourcePaths.Count > 0)
                {
                    HashSet<string> sourcePaths_Skipped = [.. SkippedSourcePaths];
                    options_Read = new PostgreSQLOpenBuildingImportTaskOptions(options)
                    {
                        SourcePaths = [.. sourcePaths.Where(x => !sourcePaths_Skipped.Contains(x))],
                    };

                    Serilog.Modify.Log(
                        "{Type}: {SkippedCount} of {PathCount} source file(s) of {DatasetKey} cannot reach the scope and are left unread: {SkippedFileNames}",
                        nameof(PostgreSQLOpenBuildingImportTask), SkippedSourcePaths.Count, sourcePaths.Count, datasetKey!, string.Join(", ", RelativePaths(sourcePaths, SkippedSourcePaths)));
                }
            }

            List<BuildingReadResult> buildingReadResults = [];

            // The position the stream fills in as each source file starts and the converter reads back when
            // it logs progress: one instance, both ends of the run (DiGi.GIS.PostgreSQL#169).
            OpenBuildingSourcePosition sourcePosition = new();

            IAsyncEnumerable<Footprint>? footprints;
            try
            {
                footprints = Footprints(tableName, options_Read, quadkeys, buildingReadResults, sourcePosition, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                // The readers reject a dataset/format mismatch eagerly with ArgumentException and a file whose
                // coordinate reference system they do not support at enumeration start; either ends the run here.
                Serilog.Modify.Log(exception, "{Type}: the readers of {DatasetKey} could not be opened - nothing was imported", nameof(PostgreSQLOpenBuildingImportTask), datasetKey!);
                return false;
            }

            if (footprints is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no footprint stream for {DatasetKey} - nothing was imported", nameof(PostgreSQLOpenBuildingImportTask), datasetKey!);
                return false;
            }

            BuildingDatasetRelease buildingDatasetRelease = new(
                datasetKey,
                options.Version,
                options.DownloadDate,
                options.SourceUrl,
                options.Licence,
                options.SourceObservationDate,
                options.FileName,
                options.Sha256,
                SkippedSourcePaths.Count == 0 ? null : RelativePaths(sourcePaths, SkippedSourcePaths));

            Serilog.Modify.Log(
                "{Type} started: {DatasetKey}/{Version} from {PathCount} of {SourcePathCount} file(s), scope {Scope}, {CountyCount} county parts",
                nameof(PostgreSQLOpenBuildingImportTask), datasetKey!, options.Version!, options_Read.SourcePaths!.Count, sourcePaths.Count,
                boundingBox2D_Scope is null ? "county list" : "bounding box", countyIds_Scope.Count);

            OpenBuildingImportResult? openBuildingImportResult = await openBuildingPostgreSQLConverter.ImportAsync(
                footprints,
                buildingDatasetRelease,
                countyIds_Scope,
                counties,
                boundingBox2D_Scope,
                progress,
                sourcePosition,
                commandTimeout: commandTimeout,
                maxDegreeOfParallelism: options.MaxDegreeOfParallelism,
                cancellationToken: cancellationToken);

            if (openBuildingImportResult is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the import of {DatasetKey}/{Version} failed", nameof(PostgreSQLOpenBuildingImportTask), datasetKey!, options.Version!);
                return false;
            }

            RowCount = openBuildingImportResult.RowCount;
            FilteredCount = openBuildingImportResult.FilteredCount;
            RejectionCount = openBuildingImportResult.RejectionCount;
            DatasetId = openBuildingImportResult.DatasetId;

            // The OSM summary is complete only now: the reader fills it while the sequence is enumerated, and
            // the import has enumerated it to its end (or it would not have returned).
            foreach (BuildingReadResult buildingReadResult in buildingReadResults)
            {
                ReaderRejectionCount += buildingReadResult.Summary.RejectionCount;
            }

            bool cancelled = cancellationToken.IsCancellationRequested;

            Serilog.Modify.Log(
                cancelled ? Serilog.Enums.LogEventLevel.Warning : Serilog.Enums.LogEventLevel.Information,
                "{Type} finished{Cancelled}: {RowCount} rows stored, {FilteredCount} out of scope, {RejectionCount} rejected, {ReaderRejectionCount} refused by the readers, register row {DatasetId}, of {DatasetKey}/{Version}",
                nameof(PostgreSQLOpenBuildingImportTask), cancelled ? " after being cancelled" : string.Empty,
                RowCount, FilteredCount, RejectionCount, ReaderRejectionCount, DatasetId?.ToString(CultureInfo.CurrentCulture) ?? "(null)", datasetKey!, options.Version!);

            return !cancelled;
        }

        /// <summary>
        /// Opens the readers of the dataset's file format for every source path and concatenates them into one stream; the OSM branch reads the PBF synchronously and keeps its read results so their summaries can be read after the run.
        /// <para>Each stream is paired with the source path it reads, so the concatenated stream can fill <paramref name="sourcePosition"/> as every file starts and a progress line can name the file and its position (n of N) while the run is open (DiGi.GIS.PostgreSQL#169).</para>
        /// </summary>
        /// <param name="tableName">The table the run writes, already resolved through <see cref="Query.OpenBuildingTableName(string?)"/>; it selects the reader.</param>
        /// <param name="options">The validated run options.</param>
        /// <param name="quadkeys">The quadkey of every Microsoft source path from <see cref="Query.TryMicrosoftQuadkeys(IEnumerable{string}?, string?, out Dictionary{string, string}?, out string?)"/>, or null for every other dataset.</param>
        /// <param name="buildingReadResults">Collects the OSM read results for the post-run summary.</param>
        /// <param name="sourcePosition">The shared position the stream fills in as each source file starts, or null to report no file position.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>The concatenated footprint stream, or null when a reader rejects its input.</returns>
        /// <exception cref="ArgumentException">Thrown when the dataset does not match the reader the file requires.</exception>
        public static IAsyncEnumerable<Footprint>? Footprints(string tableName, PostgreSQLOpenBuildingImportTaskOptions options, Dictionary<string, string>? quadkeys, List<BuildingReadResult> buildingReadResults, OpenBuildingSourcePosition? sourcePosition, CancellationToken cancellationToken)
        {
            List<IAsyncEnumerable<Footprint>> streams = [];
            List<string> paths = [];

            bool isOsm = string.Equals(tableName, Constants.TableName.OSM_Building2D, StringComparison.Ordinal);

            BuildingDataset? buildingDataset = isOsm ? null : BuildingDatasetFromKey(options.DatasetKey);
            if (!isOsm && buildingDataset is null)
            {
                return null;
            }

            foreach (string path in options.SourcePaths!)
            {
                if (isOsm)
                {
                    if (DiGi.OSM.Query.Footprints_Pbf(path) is not BuildingReadResult buildingReadResult)
                    {
                        return null;
                    }

                    buildingReadResults.Add(buildingReadResult);
                    streams.Add(AsAsyncEnumerable(buildingReadResult.Footprints, cancellationToken));
                    paths.Add(path);
                    continue;
                }

                IAsyncEnumerable<Footprint>? stream = buildingDataset switch
                {
                    BuildingDataset.GhsObat when path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) => DiGi.GIS.IO.Query.Footprints_CsvAsync(path, buildingDataset.Value, cancellationToken: cancellationToken),
                    BuildingDataset.GhsObat => DiGi.GIS.IO.Query.Footprints_GeoPackageAsync(path, buildingDataset.Value, cancellationToken: cancellationToken),
                    BuildingDataset.Eubucco => DiGi.GIS.IO.Query.Footprints_GeoPackageAsync(path, buildingDataset.Value, cancellationToken: cancellationToken),
                    BuildingDataset.Dbsm => DiGi.GIS.IO.Query.Footprints_GeoPackageAsync(path, buildingDataset.Value, cancellationToken: cancellationToken),
                    BuildingDataset.Overture => DiGi.GIS.IO.Query.Footprints_GeoParquetAsync(path, buildingDataset.Value, cancellationToken: cancellationToken),
                    BuildingDataset.Microsoft when quadkeys is not null && quadkeys.TryGetValue(path, out string? quadkey) => DiGi.GIS.IO.Query.Footprints_GeoJsonSequenceAsync(path, buildingDataset.Value, quadkey, cancellationToken: cancellationToken),
                    _ => null,
                };

                if (stream is null)
                {
                    return null;
                }

                streams.Add(stream);
                paths.Add(path);
            }

            return Concatenate(streams, paths, sourcePosition, cancellationToken);
        }

        /// <summary>
        /// Names source files relative to the directory every source file of the run shares, so the register row and the log name a Microsoft chunk by its <c>quadkey=</c> directory rather than a machine path (DiGi.GIS.PostgreSQL#170).
        /// </summary>
        /// <param name="paths_All">Every source file of the run; their shared directory is the base.</param>
        /// <param name="paths">The files to name.</param>
        /// <returns>The relative paths with forward slashes, in the order of <paramref name="paths"/>; a file outside the shared directory keeps its file name.</returns>
        private static List<string> RelativePaths(IEnumerable<string> paths_All, IEnumerable<string> paths)
        {
            char[] separators = ['\\', '/'];

            string[]? segments_Common = null;
            foreach (string path in paths_All)
            {
                string[] segments = (System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path)) ?? string.Empty).Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (segments_Common is null)
                {
                    segments_Common = segments;
                    continue;
                }

                int count = 0;
                while (count < segments_Common.Length && count < segments.Length && string.Equals(segments_Common[count], segments[count], StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }

                segments_Common = segments_Common[..count];
            }

            List<string> result = [];
            foreach (string path in paths)
            {
                string[] segments = System.IO.Path.GetFullPath(path).Split(separators, StringSplitOptions.RemoveEmptyEntries);
                int count = segments_Common is null ? 0 : segments_Common.Length;

                result.Add(count > 0 && count < segments.Length ? string.Join("/", segments[count..]) : System.IO.Path.GetFileName(path));
            }

            return result;
        }

        /// <summary>
        /// Maps a dataset key to the reader's dataset enum; null for the OSM key (its reader lives in DiGi.OSM and takes no enum) and for an unknown key.
        /// </summary>
        /// <param name="datasetKey">The dataset key from the options.</param>
        /// <returns>The reader enum, or null.</returns>
        private static BuildingDataset? BuildingDatasetFromKey(string? datasetKey)
        {
            if (string.IsNullOrWhiteSpace(datasetKey))
            {
                return null;
            }

            switch (datasetKey!.Trim().ToUpperInvariant())
            {
                case "EUBUCCO":
                    return BuildingDataset.Eubucco;

                case "OVERTURE":
                    return BuildingDataset.Overture;

                case "DBSM":
                    return BuildingDataset.Dbsm;

                case "MICROSOFT":
                    return BuildingDataset.Microsoft;

                case "GHS_OBAT":
                    return BuildingDataset.GhsObat;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Wraps a synchronous footprint sequence - the OSM reader's - as an awaitable stream without hopping threads between rows.
        /// </summary>
        /// <param name="footprints">The synchronous sequence, or null.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>The stream yielding the same footprints.</returns>
        private static async IAsyncEnumerable<Footprint> AsAsyncEnumerable(IEnumerable<Footprint>? footprints, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // One synchronous await keeps the iterator awaitable; it completes inline, so no thread is taken
            // per row.
            await Task.CompletedTask;

            if (footprints is null)
            {
                yield break;
            }

            foreach (Footprint footprint in footprints)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return footprint;
            }
        }

        /// <summary>
        /// Yields every footprint of every source stream in order, so a dataset released as several files imports as one release.
        /// <para>Before a stream's first footprint is yielded, <paramref name="sourcePosition"/> is moved to that stream's file - path, 1-based index and the number of files - so the import's progress lines name the file being read while the run is open (DiGi.GIS.PostgreSQL#169).</para>
        /// </summary>
        /// <param name="streams">The source streams; null entries are skipped.</param>
        /// <param name="paths">The source path of each stream, index-aligned with <paramref name="streams"/> by <see cref="Footprints(string, PostgreSQLOpenBuildingImportTaskOptions, Dictionary{string, string}, List{BuildingReadResult}, OpenBuildingSourcePosition?, CancellationToken)"/>.</param>
        /// <param name="sourcePosition">The shared position to move as each file starts, or null to report no file position.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>The concatenated stream.</returns>
        private static async IAsyncEnumerable<Footprint> Concatenate(List<IAsyncEnumerable<Footprint>> streams, List<string> paths, OpenBuildingSourcePosition? sourcePosition, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (sourcePosition is not null)
            {
                sourcePosition.Count = streams.Count;
                sourcePosition.Index = 0;
                sourcePosition.Path = null;
            }

            for (int index = 0; index < streams.Count; index++)
            {
                IAsyncEnumerable<Footprint> stream = streams[index];

                if (stream is null)
                {
                    continue;
                }

                if (sourcePosition is not null)
                {
                    sourcePosition.Index = index + 1;
                    sourcePosition.Path = index < paths.Count ? paths[index] : null;
                }

                await foreach (Footprint footprint in stream.WithCancellation(cancellationToken))
                {
                    yield return footprint;
                }
            }
        }
    }
}
