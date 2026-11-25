using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.PostgreSQL.Interfaces;
using NetTopologySuite.Geometries;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that matches the footprints of the open building datasets to BDOT10k and stores the links in <c>building_dataset_match</c> (DiGi.GIS.PostgreSQL#127; paper 01, result blocks 2–3).
    /// <para>The two sides live on different databases - BDOT10k <c>building_2d</c> and the county parts on Main, the open building tables and the link table on Storage - so nothing is joined: for every dataset and every county group the task reads both sides through their own converters, matches them in memory with <see cref="Query.OpenBuildingMatches(IEnumerable{OpenBuildingMatchItem}?, IEnumerable{OpenBuildingMatchItem}?, OpenBuildingMatchParameters?, ISet{int}?)"/> and replaces the group's stored snapshot (Coding - PostgreSQL.md section 6).</para>
    /// <para>A county group is every sibling part of one county code, read whole, plus the footprints of the neighbouring parts within <see cref="OpenBuildingMatchParameters.BoundaryMargin"/> of its envelope, so a building on a county edge finds its partner across it; the ownership rule of the matching returns each component to exactly one group. Groups run one after another on one connection per read. A group whose parts hold no footprint of the dataset (not imported there) is skipped rather than reported as all omissions; a group whose parts hold rows of two releases is refused.</para>
    /// <para>The run succeeds when every group did; a group that fails is logged, counted in <see cref="FailedGroupCount"/>, and the run moves on to the next one.</para>
    /// </summary>
    public class PostgreSQLOpenBuildingMatchTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// Whole-county reads of the largest groups (Warszawa) exceed the 30 second default; allow up to 10 minutes per statement.
        /// </summary>
        private const int commandTimeout = 600;

        /// <summary>
        /// The five footprint datasets matched when the options name none. GHS-OBAT holds centroids and is never matched.
        /// </summary>
        private static readonly string[] datasetKeys_Default = ["eubucco", "overture", "osm", "dbsm", "microsoft"];

        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Constructor with Dependency Injection.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager that provides the open building converter (Storage) and the BDOT10k and administrative-area converters (Main).</param>
        public PostgreSQLOpenBuildingMatchTask(GISPostgreSQLConverterManager? gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the number of county groups, across every dataset, that failed and kept their previous snapshot.
        /// </summary>
        public int FailedGroupCount { get; private set; }

        /// <summary>
        /// Gets what the run did for every dataset and county group, in run order.
        /// </summary>
        public List<OpenBuildingMatchResult> OpenBuildingMatchResults { get; private set; } = [];

        /// <summary>
        /// Gets the completeness and geometry measures of every dataset over the run's county parts: read back from the stored links when <see cref="PostgreSQLOpenBuildingMatchTaskOptions.Persist"/> is set, computed in memory from the run's links otherwise.
        /// </summary>
        public List<OpenBuildingMatchSummaryResult> OpenBuildingMatchSummaryResults { get; private set; } = [];

        /// <summary>
        /// Gets or sets the options of the run.
        /// </summary>
        public PostgreSQLOpenBuildingMatchTaskOptions PostgreSQLOpenBuildingMatchTaskOptions { get; set; } = new PostgreSQLOpenBuildingMatchTaskOptions();

        /// <summary>
        /// Gets the number of link rows the run produced, across every dataset and group.
        /// </summary>
        public long RowCount { get; private set; }

        /// <summary>
        /// Executes the matching for every dataset and county group in scope.
        /// </summary>
        /// <param name="progress">A progress reporter carrying the running total of link rows produced.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every county group of every dataset was matched; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            FailedGroupCount = 0;
            OpenBuildingMatchResults = [];
            OpenBuildingMatchSummaryResults = [];
            RowCount = 0;

            PostgreSQLOpenBuildingMatchTaskOptions options = PostgreSQLOpenBuildingMatchTaskOptions;
            if (options is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no options - nothing to match", nameof(PostgreSQLOpenBuildingMatchTask));
                return false;
            }

            OpenBuildingPostgreSQLConverter? openBuildingPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<OpenBuildingPostgreSQLConverter>();
            if (openBuildingPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the open building tables cannot be read", nameof(PostgreSQLOpenBuildingMatchTask), nameof(OpenBuildingPostgreSQLConverter));
                return false;
            }

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            if (building2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - BDOT10k cannot be read", nameof(PostgreSQLOpenBuildingMatchTask), nameof(Building2DPostgreSQLConverter));
                return false;
            }

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the scope cannot be resolved", nameof(PostgreSQLOpenBuildingMatchTask), nameof(AdministrativeAreal2DPostgreSQLConverter));
                return false;
            }

            List<string> datasetKeys = options.DatasetKeys is null ? [.. datasetKeys_Default] : [.. options.DatasetKeys];
            datasetKeys.Sort(StringComparer.Ordinal);

            foreach (string datasetKey in datasetKeys)
            {
                string? tableName = Query.OpenBuildingTableName(datasetKey);
                if (tableName is null || string.Equals(tableName, Constants.TableName.GHS_OBAT_Building, StringComparison.Ordinal))
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} names no open building footprint dataset (GHS-OBAT holds centroids and is not matched)", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey ?? "(null)");
                    return false;
                }
            }

            if (datasetKeys.Count == 0)
            {
                Serilog.Modify.Log("{Type}: no dataset was named - nothing to match", nameof(PostgreSQLOpenBuildingMatchTask));
                return true;
            }

            bool hasCountyScope = options.CountyIds is not null && options.CountyIds.Count > 0;
            bool hasBoundingBoxScope = options.BoundingBox2D is not null;

            if (hasCountyScope == hasBoundingBoxScope)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the scope must be a county list or a bounding box, exactly one of the two", nameof(PostgreSQLOpenBuildingMatchTask));
                return false;
            }

            OpenBuildingMatchParameters openBuildingMatchParameters = options.OpenBuildingMatchParameters ?? new OpenBuildingMatchParameters();

            IEnumerable<int>? countyIds_Named;
            if (hasBoundingBoxScope)
            {
                // The flat static prefilter, as in the import: every county part whose stored envelope reaches the
                // box. The instance overload narrows through ancestor rows and answers empty without them.
                await using NpgsqlConnection? npgsqlConnection_Scope = DiGi.PostgreSQL.Create.NpgsqlConnection(administrativeAreal2DPostgreSQLConverter.ConnectionData);
                if (npgsqlConnection_Scope is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no connection to resolve the bounding-box scope against - nothing was matched", nameof(PostgreSQLOpenBuildingMatchTask));
                    return false;
                }

                await npgsqlConnection_Scope.OpenAsync(cancellationToken);

                List<AdministrativeAreal2D>? counties_Scope = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection_Scope, options.BoundingBox2D, Enums.AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                countyIds_Named = counties_Scope?.ConvertAll(x => x.Id);
            }
            else
            {
                countyIds_Named = options.CountyIds;
            }

            if (countyIds_Named is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the scope could not be resolved against the database - nothing was matched", nameof(PostgreSQLOpenBuildingMatchTask));
                return false;
            }

            // One group per county code, widened to every part the database holds for it: a multi-part county is
            // matched as one whole, so its parts' snapshots are replaced together.
            List<List<int>>? countyIdGroups = await administrativeAreal2DPostgreSQLConverter.CountyIdGroupsAsync(countyIds_Named, commandTimeout, cancellationToken);
            if (countyIdGroups is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the county groups could not be resolved - nothing was matched", nameof(PostgreSQLOpenBuildingMatchTask));
                return false;
            }

            if (countyIdGroups.Count == 0)
            {
                Serilog.Modify.Log("{Type}: no county part is in scope - nothing to match", nameof(PostgreSQLOpenBuildingMatchTask));
                return true;
            }

            // The envelopes of every county part, read once from Main: they give each group its read window and
            // its neighbours.
            List<AdministrativeAreal2D>? counties = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(Enums.AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (counties is null || counties.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county part could be read from the Main database - nothing was matched", nameof(PostgreSQLOpenBuildingMatchTask));
                return false;
            }

            Dictionary<int, Envelope> envelopes_ByCountyId = [];
            foreach (KeyValuePair<int, IPolygonal2D> pair in counties.Polygonal2DsByCountyId())
            {
                if (pair.Value.GetBoundingBox() is BoundingBox2D boundingBox2D)
                {
                    envelopes_ByCountyId[pair.Key] = new Envelope(boundingBox2D.Min.X, boundingBox2D.Max.X, boundingBox2D.Min.Y, boundingBox2D.Max.Y);
                }
            }

            List<int> countyIds_Scope = [.. countyIdGroups.SelectMany(x => x).Distinct().OrderBy(x => x)];

            Stopwatch stopwatch = Stopwatch.StartNew();
            bool isSummaryFailed = false;

            foreach (string datasetKey in datasetKeys)
            {
                List<OpenBuildingMatch> openBuildingMatches_Run = [];
                HashSet<int> datasetIds_Run = [];

                foreach (List<int> countyIds_Group in countyIdGroups)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        Envelope envelope_Window = new();
                        foreach (int countyId in countyIds_Group)
                        {
                            if (envelopes_ByCountyId.TryGetValue(countyId, out Envelope? envelope_County))
                            {
                                envelope_Window.ExpandToInclude(envelope_County);
                            }
                        }

                        if (envelope_Window.IsNull)
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - county group {CountyIds} has no readable envelope and was skipped", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group));
                            FailedGroupCount++;
                            continue;
                        }

                        envelope_Window.ExpandBy(Math.Max(0, openBuildingMatchParameters.BoundaryMargin));
                        BoundingBox2D boundingBox2D_Window = new(new Point2D(envelope_Window.MinX, envelope_Window.MinY), new Point2D(envelope_Window.MaxX, envelope_Window.MaxY));

                        HashSet<int> countyIds_Owned = [.. countyIds_Group];
                        List<int> countyIds_Neighbour = [.. envelopes_ByCountyId.Where(x => !countyIds_Owned.Contains(x.Key) && x.Value.Intersects(envelope_Window)).Select(x => x.Key).OrderBy(x => x)];

                        // BDOT10k from Main, the dataset from Storage: each side whole for the group, by box for the margin.
                        List<OpenBuildingMatchItem>? openBuildingMatchItems_BDOT = await building2DPostgreSQLConverter.GetOpenBuildingMatchItemsAsync(countyIds_Group, null, commandTimeout, cancellationToken);
                        List<OpenBuildingMatchItem>? openBuildingMatchItems_Dataset = await openBuildingPostgreSQLConverter.GetOpenBuildingMatchItemsAsync(datasetKey, countyIds_Group, null, commandTimeout, cancellationToken);

                        if (openBuildingMatchItems_BDOT is null || openBuildingMatchItems_Dataset is null)
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - county group {CountyIds} could not be read (BDOT10k on Main or the dataset table on Storage) and kept its snapshot", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group));
                            FailedGroupCount++;
                            continue;
                        }

                        HashSet<int> datasetIds = [.. openBuildingMatchItems_Dataset.Select(x => x.DatasetId ?? 0)];
                        if (datasetIds.Count == 0)
                        {
                            // Not imported here: reporting every BDOT10k footprint as an omission would be wrong.
                            Serilog.Modify.Log("{Type}: {DatasetKey} - county group {CountyIds} holds no footprint of the dataset and was skipped", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group));
                            OpenBuildingMatchResults.Add(new OpenBuildingMatchResult(datasetKey, null, countyIds_Group, openBuildingMatchItems_BDOT.Count, 0, 0, 0, 0, 0, 0, false));
                            continue;
                        }

                        if (datasetIds.Count > 1)
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - county group {CountyIds} holds rows of {DatasetIdCount} releases; re-import the group as one release before matching", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group), datasetIds.Count);
                            FailedGroupCount++;
                            continue;
                        }

                        int datasetId = datasetIds.First();

                        long count_BDOT = openBuildingMatchItems_BDOT.Count;
                        long count_Dataset = openBuildingMatchItems_Dataset.Count;
                        long count_BDOTRejected = openBuildingMatchItems_BDOT.Count(x => x.Geometry is null);
                        long count_DatasetRejected = openBuildingMatchItems_Dataset.Count(x => x.Geometry is null);

                        if (countyIds_Neighbour.Count > 0)
                        {
                            List<OpenBuildingMatchItem>? openBuildingMatchItems_BDOTMargin = await building2DPostgreSQLConverter.GetOpenBuildingMatchItemsAsync(countyIds_Neighbour, boundingBox2D_Window, commandTimeout, cancellationToken);
                            List<OpenBuildingMatchItem>? openBuildingMatchItems_DatasetMargin = await openBuildingPostgreSQLConverter.GetOpenBuildingMatchItemsAsync(datasetKey, countyIds_Neighbour, boundingBox2D_Window, commandTimeout, cancellationToken);

                            if (openBuildingMatchItems_BDOTMargin is null || openBuildingMatchItems_DatasetMargin is null)
                            {
                                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - the margin of county group {CountyIds} could not be read and the group kept its snapshot", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group));
                                FailedGroupCount++;
                                continue;
                            }

                            openBuildingMatchItems_BDOT.AddRange(openBuildingMatchItems_BDOTMargin);
                            openBuildingMatchItems_Dataset.AddRange(openBuildingMatchItems_DatasetMargin);
                        }

                        long count_Repaired = openBuildingMatchItems_BDOT.Count(x => x.Repaired) + openBuildingMatchItems_Dataset.Count(x => x.Repaired);

                        List<OpenBuildingMatch>? openBuildingMatches = Query.OpenBuildingMatches(openBuildingMatchItems_Dataset, openBuildingMatchItems_BDOT, openBuildingMatchParameters, countyIds_Owned);
                        if (openBuildingMatches is null)
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - county group {CountyIds} could not be matched and kept its snapshot", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group));
                            FailedGroupCount++;
                            continue;
                        }

                        // Components reaching a footprint that touches the edge of what was read may continue beyond
                        // it, where the neighbouring group sees more of them: counted, so the margin can be checked.
                        HashSet<(bool, int, string)> keys_Edge = [];
                        foreach (OpenBuildingMatchItem openBuildingMatchItem in openBuildingMatchItems_BDOT)
                        {
                            if (openBuildingMatchItem.Geometry is not null && !envelope_Window.Contains(openBuildingMatchItem.Geometry.EnvelopeInternal))
                            {
                                keys_Edge.Add((true, openBuildingMatchItem.CountyId, openBuildingMatchItem.Key));
                            }
                        }

                        foreach (OpenBuildingMatchItem openBuildingMatchItem in openBuildingMatchItems_Dataset)
                        {
                            if (openBuildingMatchItem.Geometry is not null && !envelope_Window.Contains(openBuildingMatchItem.Geometry.EnvelopeInternal))
                            {
                                keys_Edge.Add((false, openBuildingMatchItem.CountyId, openBuildingMatchItem.Key));
                            }
                        }

                        HashSet<(int, long)> components_Edge = [];
                        if (keys_Edge.Count > 0)
                        {
                            foreach (OpenBuildingMatch openBuildingMatch in openBuildingMatches)
                            {
                                bool isEdge =
                                    (openBuildingMatch.BDOTCountyId is int countyId_BDOT && openBuildingMatch.BDOTReference is string reference && keys_Edge.Contains((true, countyId_BDOT, reference))) ||
                                    (openBuildingMatch.SourceCountyId is int countyId_Source && openBuildingMatch.SourceId is string sourceId && keys_Edge.Contains((false, countyId_Source, sourceId)));

                                if (isEdge)
                                {
                                    components_Edge.Add((openBuildingMatch.CountyId, openBuildingMatch.ComponentId));
                                }
                            }
                        }

                        bool persisted = false;
                        if (options.Persist)
                        {
                            long? count_Written = await openBuildingPostgreSQLConverter.ReplaceMatchesAsync(datasetKey, datasetId, countyIds_Group, openBuildingMatches, openBuildingMatchParameters, commandTimeout, cancellationToken);
                            if (count_Written is null)
                            {
                                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - the links of county group {CountyIds} could not be stored and the group kept its snapshot", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group));
                                FailedGroupCount++;
                                continue;
                            }

                            persisted = true;
                        }
                        else
                        {
                            openBuildingMatches_Run.AddRange(openBuildingMatches);
                        }

                        datasetIds_Run.Add(datasetId);
                        RowCount += openBuildingMatches.Count;
                        progress?.Report(RowCount);

                        OpenBuildingMatchResults.Add(new OpenBuildingMatchResult(
                            datasetKey,
                            datasetId,
                            countyIds_Group,
                            count_BDOT,
                            count_Dataset,
                            count_BDOTRejected,
                            count_DatasetRejected,
                            count_Repaired,
                            openBuildingMatches.Count,
                            components_Edge.Count,
                            persisted));

                        Serilog.Modify.Log(
                            "{Type}: {DatasetKey} - county group {CountyIds}: {BDOTCount} BDOT10k and {DatasetCount} dataset footprints ({BDOTRejectedCount} and {DatasetRejectedCount} unusable, {RepairedCount} repaired), {RowCount} link rows, {MarginReachCount} components at the margin, {Persisted}, {ElapsedSeconds:0} s elapsed",
                            nameof(PostgreSQLOpenBuildingMatchTask),
                            datasetKey,
                            string.Join(",", countyIds_Group),
                            count_BDOT,
                            count_Dataset,
                            count_BDOTRejected,
                            count_DatasetRejected,
                            count_Repaired,
                            openBuildingMatches.Count,
                            components_Edge.Count,
                            persisted ? "stored" : "not stored",
                            stopwatch.Elapsed.TotalSeconds);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - county group {CountyIds} failed and kept its snapshot: {Message}", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, string.Join(",", countyIds_Group), exception.Message);
                        FailedGroupCount++;
                    }
                }

                // The measures over the run's county parts: from the stored links, or from this run's own when they
                // were not stored (a sensitivity run).
                if (options.Persist)
                {
                    List<OpenBuildingMatchSummaryResult>? openBuildingMatchSummaryResults = null;
                    try
                    {
                        openBuildingMatchSummaryResults = await openBuildingPostgreSQLConverter.GetMatchSummariesByCountyIdsAsync(datasetKey, countyIds_Scope, commandTimeout, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {DatasetKey} - the summaries could not be read back: {Message}", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, exception.Message);
                        isSummaryFailed = true;
                    }

                    if (openBuildingMatchSummaryResults is not null)
                    {
                        OpenBuildingMatchSummaryResults.AddRange(openBuildingMatchSummaryResults);
                    }
                }
                else if (datasetIds_Run.Count == 1)
                {
                    List<OpenBuildingMatchSummaryResult>? openBuildingMatchSummaryResults = Query.OpenBuildingMatchSummaries(openBuildingMatches_Run, datasetKey, datasetIds_Run.First(), openBuildingMatchParameters, DateTimeOffset.UtcNow);
                    if (openBuildingMatchSummaryResults is not null)
                    {
                        OpenBuildingMatchSummaryResults.AddRange(openBuildingMatchSummaryResults);
                    }
                }
                else if (datasetIds_Run.Count > 1)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "{Type}: {DatasetKey} - the scope spans {DatasetIdCount} releases; no summary is computed across them", nameof(PostgreSQLOpenBuildingMatchTask), datasetKey, datasetIds_Run.Count);
                }
            }

            if (FailedGroupCount > 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: {FailedGroupCount} county group(s) failed and kept their previous snapshot - see the lines above", nameof(PostgreSQLOpenBuildingMatchTask), FailedGroupCount);
                return false;
            }

            if (isSummaryFailed)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the links were stored, but the summaries of at least one dataset could not be read back - see the lines above", nameof(PostgreSQLOpenBuildingMatchTask));
                return false;
            }

            Serilog.Modify.Log("{Type}: {RowCount} link rows over {GroupCount} county group(s) and {DatasetCount} dataset(s), {ElapsedSeconds:0} s", nameof(PostgreSQLOpenBuildingMatchTask), RowCount, countyIdGroups.Count, datasetKeys.Count, stopwatch.Elapsed.TotalSeconds);
            return true;
        }
    }
}
