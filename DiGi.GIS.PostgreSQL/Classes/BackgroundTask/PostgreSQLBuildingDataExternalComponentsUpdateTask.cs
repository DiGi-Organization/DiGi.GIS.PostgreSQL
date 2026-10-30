using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that fills the external components area columns of the building data table from the stored building models.
    /// <para>The run is driven by counties: for each one it reads the stored building models, classifies the components of every model into the wall, roof and floor buckets, and upserts one building data row per building keyed on county and reference.</para>
    /// <para>A county whose buildings carry no stored model is processed, not failed: there is simply nothing to classify there, and the buildings keep their current values. A county whose stored models cannot be read or whose rows cannot be written is failed and logged with the exception, and the run keeps going over the other counties. <see cref="FailedCountyCount"/> is what tells the two apart, and a run with a failed county reports itself as not succeeded.</para>
    /// <para>A model the classification refuses as a defect in its space structure costs that model alone: it gets no row, is logged as an error with its reference and reason, and counted in <see cref="FailedModelCount"/>; the rest of its batch and county is still written, and the run reports itself as not succeeded. A degenerate model - components but no external envelope, typically a sliver footprint with walls and no roof or floor - gets no row either, but is logged as a warning and counted in <see cref="DegenerateModelCount"/> without failing the run.</para>
    /// <para>A component that is valid but has no definable bucket - a wall whose normal is vertical - is skipped and counted in <see cref="SkippedComponentCount"/> rather than failing the county.</para>
    /// <para>A model whose external envelope does not close is not failed either: its row is written with a null closing tolerance and counted in <see cref="OpenEnvelopeCount"/>, because the sector and tilt values of such a row may rest on an arbitrary face side - the count is the share of rows of the run to treat with that caution.</para>
    /// <para>With <see cref="PostgreSQLBuildingDataExternalComponentsUpdateOptions.PartyWalls"/> set, the party wall area columns are filled in the same pass (DiGi-Organization/DiGi.GIS.Analytical#4). The references of a county are ordered along a Z-order curve of their outlines (<see cref="DiGi.Geometry.Planar.Query.ZOrderComparison{TKey}"/>), so a batch covers a compact area. For each batch, the outlines touching its buildings are read - across county borders too - and the stored models of those neighbours are read by reference with a fallback to any county, since a county is not a key of the building table. <see cref="DiGi.Analytical.Building.Query.PartyWalls(System.Collections.Generic.IEnumerable{DiGi.Analytical.Building.Classes.BuildingModel?}?, System.Collections.Generic.IEnumerable{DiGi.Analytical.Building.Classes.BuildingModel?}?, double, double, double, double)"/> then measures the shared walls of the whole batch in one call, so each model's envelope is built once. A neighbour with no stored model is counted in <see cref="MissingNeighbourModelCount"/>, and a batch whose neighbours cannot be read keeps its party wall columns as they were and is counted in <see cref="PartyWallFailedBatchCount"/>; neither fails the run.</para>
    /// <para>The run is idempotent: the read is deterministic (the latest stored version of a model wins), the classification is pure, and the push upserts on county and reference, so a re-run writes the same values.</para>
    /// </summary>
    public class PostgreSQLBuildingDataExternalComponentsUpdateTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The GIS PostgreSQL converter manager used to retrieve converters and execute operations.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Gets or sets the options used to configure the run.
        /// </summary>
        public PostgreSQLBuildingDataExternalComponentsUpdateOptions PostgreSQLBuildingDataExternalComponentsUpdateOptions { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingDataExternalComponentsUpdateTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager used to retrieve converters and execute operations.</param>
        public PostgreSQLBuildingDataExternalComponentsUpdateTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the number of models that carry components but no external envelope, so no row was written for them, during the last run.
        /// <para>Each one is logged as a warning with its reference. It is a property of the stored data rather than a defect of the run, so it does not fail the run.</para>
        /// </summary>
        public long DegenerateModelCount { get; private set; }

        /// <summary>
        /// Gets the number of counties that failed outright and were stepped over during the last run.
        /// <para>Each one is logged with the exception that caused it, so this figure is a count of entries to go and read rather than the whole of what is known.</para>
        /// </summary>
        public long FailedCountyCount { get; private set; }

        /// <summary>
        /// Gets the number of models the classification refused as a defect in their space structure during the last run.
        /// <para>Each one is logged as an error with its reference and reason and gets no row; the other models of its batch and county are still written. A non-zero count makes the run report itself as not succeeded.</para>
        /// </summary>
        public long FailedModelCount { get; private set; }

        /// <summary>
        /// Gets the number of classified models whose external envelope does not close on the tolerance ladder during the last run.
        /// <para>Each such row is written with a null closing tolerance: the orientation of an envelope face is decided by ray parity, which is sound only for a closed face set, so the sector and tilt values of these rows may rest on an arbitrary face side.</para>
        /// </summary>
        public long OpenEnvelopeCount { get; private set; }

        /// <summary>
        /// Gets the number of counties that were processed during the last run.
        /// <para>A county whose buildings carry no stored model is processed, not failed: there is simply nothing to classify there.</para>
        /// </summary>
        public long ProcessedCountyCount { get; private set; }

        /// <summary>
        /// Gets the number of building models read for classification during the last run, degenerate and refused models included.
        /// <para>Models rather than records: a reference with several stored versions is read as the latest one only, and that one is what is counted.</para>
        /// </summary>
        public long ProcessedModelCount { get; private set; }

        /// <summary>
        /// Gets the number of components skipped because their target bucket is undefined during the last run.
        /// <para>A wall whose normal is vertical, so its azimuth is undefined, or whose orientation against the building interior is degenerate. A component that cannot be classified at all does not count here - that is a data defect and fails its model (<see cref="FailedModelCount"/>).</para>
        /// </summary>
        public long SkippedComponentCount { get; private set; }

        /// <summary>
        /// Gets the number of references stepped over during the last run because their building data row already carried an external components area total.
        /// <para>Always 0 unless <see cref="PostgreSQLBuildingDataExternalComponentsUpdateOptions.SkipCompleted"/> is set.</para>
        /// </summary>
        public long SkippedReferenceCount { get; private set; }

        /// <summary>
        /// Gets the number of building data rows written during the last run.
        /// <para>Rows rather than buildings: the same building is counted again on a later run.</para>
        /// </summary>
        public long UpdatedRowCount { get; private set; }

        /// <summary>
        /// Gets the number of buildings found to share at least one wall with an adjacent building during the last run.
        /// <para>Always 0 unless <see cref="PostgreSQLBuildingDataExternalComponentsUpdateOptions.PartyWalls"/> is set.</para>
        /// </summary>
        public long PartyWallBuildingCount { get; private set; }

        /// <summary>
        /// Gets the number of batches, or counties, whose party walls could not be measured during the last run, because the outlines or the stored models of their neighbours could not be read.
        /// <para>The party wall columns of those buildings are left as they were; each one is logged with the exception.</para>
        /// </summary>
        public long PartyWallFailedBatchCount { get; private set; }

        /// <summary>
        /// Gets the number of neighbours, counted once per building they touch, whose outline touches a building of the run but which carry no stored model, during the last run.
        /// <para>The walls such a neighbour shares cannot be measured, so the party wall area of the building it touches is understated.</para>
        /// </summary>
        public long MissingNeighbourModelCount { get; private set; }

        /// <summary>
        /// Executes the background task that fills the external components area columns of the building data table from the stored building models.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of rows written.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every county in scope was processed without error and no model was refused by the classification; otherwise, false. Degenerate models do not make it false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            DegenerateModelCount = 0;
            FailedCountyCount = 0;
            FailedModelCount = 0;
            OpenEnvelopeCount = 0;
            ProcessedCountyCount = 0;
            ProcessedModelCount = 0;
            SkippedComponentCount = 0;
            SkippedReferenceCount = 0;
            UpdatedRowCount = 0;
            PartyWallBuildingCount = 0;
            PartyWallFailedBatchCount = 0;
            MissingNeighbourModelCount = 0;

            PostgreSQLBuildingDataExternalComponentsUpdateOptions ??= new();

            int commandTimeout = PostgreSQLBuildingDataExternalComponentsUpdateOptions.CommandTimeout;
            int batchSize = Math.Max(1, PostgreSQLBuildingDataExternalComponentsUpdateOptions.BatchSize);
            bool skipCompleted = PostgreSQLBuildingDataExternalComponentsUpdateOptions.SkipCompleted;
            bool partyWalls = PostgreSQLBuildingDataExternalComponentsUpdateOptions.PartyWalls;
            double partyWallMaxDistance = PostgreSQLBuildingDataExternalComponentsUpdateOptions.PartyWallMaxDistance;

            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            if (buildingModelPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the stored building models cannot be read", nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask), nameof(BuildingModelPostgreSQLConverter));
                return false;
            }

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            if (buildingDataPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - there is nowhere to write to", nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask), nameof(BuildingDataPostgreSQLConverter));
                return false;
            }

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the counties that scope the run cannot be read", nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask), nameof(AdministrativeAreal2DPostgreSQLConverter));
                return false;
            }

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = null;
            if (partyWalls)
            {
                building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
                if (building2DPostgreSQLConverter is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the neighbours of the buildings cannot be found, so their party walls cannot be measured", nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask), nameof(Building2DPostgreSQLConverter));
                    return false;
                }
            }

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (countyReferences is null || countyReferences.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county could be read - the run cannot be scoped", nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask));
                return false;
            }

            HashSet<int>? countyIds = PostgreSQLBuildingDataExternalComponentsUpdateOptions.CountyIds;

            Serilog.Modify.Log(
                "{Type}: starting over {CountyCount} counties, scope {CountyScope}, batch size {BatchSize}, skip completed {SkipCompleted}, party walls {PartyWalls} within {PartyWallMaxDistance} m",
                nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask),
                countyReferences.Count,
                countyIds is null ? "all" : string.Join(", ", countyIds),
                batchSize,
                skipCompleted,
                partyWalls,
                partyWallMaxDistance);

            // The columns the resume check reads. The reference column is asked for explicitly so the rows can be
            // matched back; the total is non-null only on a row a run has written.
            string? uniqueId_Reference = Core.IO.Query.UniqueId(IO.Constants.Column.Reference);
            // Through the base-typed list, whose last entry is the total: the constant is a UnitColumn, and naming it
            // would need a DiGi.Unit.IO reference this project stays free of (see Modify.Update_ExternalComponentsArea).
            List<Column> columns_ExternalComponentsArea = IO.Create.Columns_ExternalComponentsArea();
            string? uniqueId_Total = columns_ExternalComponentsArea.Count == 0 ? null : Core.IO.Query.UniqueId(columns_ExternalComponentsArea[^1]);

            foreach (AdministrativeAreal2DReference? countyReference in countyReferences)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (countyReference is null)
                {
                    continue;
                }

                // Id is the county row identifier - the key building_model_component is filed under - never the code,
                // which names several rows for a multi-part county.
                int countyId = countyReference.Id;

                if (countyIds is not null && !countyIds.Contains(countyId))
                {
                    continue;
                }

                bool countyFailed = false;

                List<string> references_List = [];
                try
                {
                    HashSet<string>? references = await buildingModelPostgreSQLConverter.GetReferencesAsync(countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    if (references is not null)
                    {
                        references_List = [.. references];
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    countyFailed = true;
                    Serilog.Modify.Log(exception, "External components county failed - county {CountyId}, the stored model references could not be read", countyId);
                }

                if (references_List.Count == 0 && !countyFailed)
                {
                    // No stored models in this county: nothing to classify, and that is not a failure.
                    ProcessedCountyCount++;
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Information, "External components county processed - county {CountyId} carries no stored building models", countyId);
                    continue;
                }

                // The outlines of the county, read once: they order the references so that a batch covers a compact area,
                // and they are the outlines the neighbour search starts from. A county whose outlines cannot be read is
                // still classified; only its party wall columns are left as they are.
                Dictionary<string, GIS.Classes.Building2D> building2Ds_ByReference = [];
                bool partyWalls_County = partyWalls && building2DPostgreSQLConverter is not null;
                if (partyWalls_County)
                {
                    List<Building2D>? building2Ds_County = null;
                    try
                    {
                        building2Ds_County = await building2DPostgreSQLConverter!.GetBuilding2DsByCountyIdAsync(countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        Serilog.Modify.Log(exception, "External components party walls not measured - county {CountyId}, its outlines could not be read", countyId);
                    }

                    if (building2Ds_County is null)
                    {
                        partyWalls_County = false;
                        PartyWallFailedBatchCount++;
                    }
                    else
                    {
                        Dictionary<string, DiGi.Geometry.Planar.Classes.BoundingBox2D> boundingBox2Ds_ByReference = [];
                        foreach (Building2D building2D in building2Ds_County)
                        {
                            if (building2D?.Reference is string reference_Building2D && building2D.ToDiGi() is GIS.Classes.Building2D building2D_GIS && building2D_GIS.PolygonalFace2D is not null)
                            {
                                building2Ds_ByReference[reference_Building2D] = building2D_GIS;
                                if (building2D_GIS.PolygonalFace2D.GetBoundingBox() is DiGi.Geometry.Planar.Classes.BoundingBox2D boundingBox2D_Building2D)
                                {
                                    boundingBox2Ds_ByReference[reference_Building2D] = boundingBox2D_Building2D;
                                }
                            }
                        }

                        // Along a Z-order curve of 100 m cells, so that a batch covers a compact area.
                        references_List.Sort(DiGi.Geometry.Planar.Query.ZOrderComparison(boundingBox2Ds_ByReference, 100.0));
                    }
                }

                // One batch is read, classified and written before the next is read, so memory follows the batch
                // size rather than the county: holding every stored model of a city county at once starved the
                // production host (DiGi.GIS.PostgreSQL#97). A county is therefore no longer written in one
                // push, and a failed batch leaves the batches before it written - harmless, the run is idempotent.
                long countyModelCount = 0;
                long countyRowCount = 0;
                long countySkippedComponentCount = 0;
                long countyOpenEnvelopeCount = 0;
                long countySkippedReferenceCount = 0;
                long countyDegenerateModelCount = 0;
                long countyFailedModelCount = 0;

                for (int offset = 0; offset < references_List.Count && !countyFailed; offset += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<string> references_Batch = references_List.GetRange(offset, Math.Min(batchSize, references_List.Count - offset));

                    if (skipCompleted)
                    {
                        HashSet<string>? references_Completed = await CompletedReferencesAsync(references_Batch, countyId);
                        if (references_Completed is not null && references_Completed.Count > 0)
                        {
                            countySkippedReferenceCount += references_Batch.RemoveAll(references_Completed.Contains);
                        }

                        if (references_Batch.Count == 0)
                        {
                            continue;
                        }
                    }

                    List<BuildingModel>? models_Batch = null;
                    try
                    {
                        // No fallback by reference: a reference the county does not hold is answered out of some
                        // other county, and the record that comes back carries that county's identifier. Writing
                        // it would file a building data row under a county this run is not processing.
                        models_Batch = await buildingModelPostgreSQLConverter.GetItemsByReferencesAsync(references_Batch, countyId, fallbackByReference: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        if (models_Batch is null)
                        {
                            countyFailed = true;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "External components county failed - county {CountyId}, offset {Offset}, the read of the {ReferenceCount} references came back empty", countyId, offset, references_Batch.Count);
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        countyFailed = true;
                        Serilog.Modify.Log(exception, "External components county failed - county {CountyId}, offset {Offset}, the stored models of {ReferenceCount} references could not be read", countyId, offset, references_Batch.Count);
                    }

                    if (countyFailed || models_Batch is null)
                    {
                        break;
                    }

                    // The read is ordered created_at DESC, id DESC, so the first record of a reference is the latest
                    // stored version - and every version of a reference arrives in the batch that asked for it.
                    HashSet<string> references_Written = [];
                    List<BuildingModel> models_Latest = [];
                    foreach (BuildingModel? model in models_Batch)
                    {
                        if (model is null)
                        {
                            continue;
                        }

                        if (model.Reference is string reference_Model && !string.IsNullOrWhiteSpace(reference_Model) && references_Written.Add(reference_Model))
                        {
                            models_Latest.Add(model);
                        }
                    }

                    models_Batch = null;

                    List<DiGi.Analytical.Building.Classes.PartyWallResult>? partyWallResults = null;
                    if (partyWalls_County)
                    {
                        partyWallResults = await PartyWallResultsAsync(models_Latest, countyId, offset, building2Ds_ByReference);
                    }

                    Table table = new();

                    ExternalComponentsAreaResult batchResult;
                    try
                    {
                        batchResult = Modify.Update_ExternalComponentsArea(table, models_Latest, partyWallResults);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        // A model the classification refuses is caught per model inside the method; reaching this is an
                        // unexpected fault, so the county fails visibly instead of writing partial rows.
                        countyFailed = true;
                        Serilog.Modify.Log(exception, "External components county failed - county {CountyId}, offset {Offset}, the {ModelCount} stored models could not be classified", countyId, offset, models_Latest.Count);
                        break;
                    }

                    List<string>? references_Degenerate = batchResult.DegenerateReferences;
                    if (references_Degenerate is not null)
                    {
                        foreach (string reference_Degenerate in references_Degenerate)
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "External components model skipped - county {CountyId}, building {Reference}: the model carries components but no external envelope, so no row is written", countyId, reference_Degenerate);
                        }
                    }

                    List<string>? references_Failed = batchResult.FailedReferences;
                    if (references_Failed is not null)
                    {
                        foreach (string reference_Failed in references_Failed)
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "External components model failed - county {CountyId}, {Failure}", countyId, reference_Failed);
                        }
                    }

                    if (table.RowCount > 0)
                    {
                        bool updated;
                        try
                        {
                            updated = await buildingDataPostgreSQLConverter.PushAsync(table, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception exception)
                        {
                            countyFailed = true;
                            Serilog.Modify.Log(exception, "External components county failed - county {CountyId}, offset {Offset}, the {RowCount} rows built could not be written", countyId, offset, table.RowCount);
                            break;
                        }

                        if (!updated)
                        {
                            countyFailed = true;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "External components county failed - county {CountyId}, offset {Offset}, the write of {RowCount} rows was rolled back", countyId, offset, table.RowCount);
                            break;
                        }
                    }

                    countyModelCount += models_Latest.Count;
                    countyRowCount += table.RowCount;
                    countySkippedComponentCount += batchResult.SkippedComponentCount;
                    countyOpenEnvelopeCount += batchResult.OpenEnvelopeCount;
                    countyDegenerateModelCount += batchResult.DegenerateModelCount;
                    countyFailedModelCount += batchResult.FailedModelCount;

                    DegenerateModelCount += batchResult.DegenerateModelCount;
                    FailedModelCount += batchResult.FailedModelCount;
                    OpenEnvelopeCount += batchResult.OpenEnvelopeCount;
                    SkippedComponentCount += batchResult.SkippedComponentCount;
                    ProcessedModelCount += models_Latest.Count;
                    UpdatedRowCount += table.RowCount;
                    progress.Report(UpdatedRowCount);
                }

                SkippedReferenceCount += countySkippedReferenceCount;

                if (countyFailed)
                {
                    FailedCountyCount++;
                    continue;
                }

                ProcessedCountyCount++;

                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Information, "External components county processed - county {CountyId}, {ModelCount} models, {RowCount} rows, {SkippedCount} components skipped, {OpenEnvelopeCount} open envelopes, {DegenerateModelCount} degenerate models, {FailedModelCount} models failed, {SkippedReferenceCount} references already done", countyId, countyModelCount, countyRowCount, countySkippedComponentCount, countyOpenEnvelopeCount, countyDegenerateModelCount, countyFailedModelCount, countySkippedReferenceCount);
            }

            Serilog.Modify.Log(
                "{Type}: finished - {ProcessedCount} counties, {FailedCount} counties failed, {ModelCount} models, {DegenerateModelCount} degenerate models, {FailedModelCount} models failed, {OpenEnvelopeCount} open envelopes, {SkippedCount} components skipped, {SkippedReferenceCount} references already done, {RowCount} rows",
                nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask),
                ProcessedCountyCount,
                FailedCountyCount,
                ProcessedModelCount,
                DegenerateModelCount,
                FailedModelCount,
                OpenEnvelopeCount,
                SkippedComponentCount,
                SkippedReferenceCount,
                UpdatedRowCount);

            if (partyWalls)
            {
                Serilog.Modify.Log("{Type}: party walls - {PartyWallBuildingCount} buildings share a wall, {MissingNeighbourModelCount} touching neighbours carry no stored model, {PartyWallFailedBatchCount} batches not measured", nameof(PostgreSQLBuildingDataExternalComponentsUpdateTask), PartyWallBuildingCount, MissingNeighbourModelCount, PartyWallFailedBatchCount);
            }

            return FailedCountyCount == 0 && FailedModelCount == 0;

            // The references of the batch whose building data row already carries a total - the work an earlier,
            // interrupted run finished. Null when the check cannot be made: the batch is then processed in full,
            // because recomputing a building costs time and skipping one wrongly costs its data.
            async Task<HashSet<string>?> CompletedReferencesAsync(List<string> references, int countyId)
            {
                if (string.IsNullOrWhiteSpace(uniqueId_Reference) || string.IsNullOrWhiteSpace(uniqueId_Total))
                {
                    return null;
                }

                Table? table_Existing;
                try
                {
                    table_Existing = await buildingDataPostgreSQLConverter.PullAsync(references, countyId, columnUniqueIds: [uniqueId_Reference!, uniqueId_Total!], batchSize: batchSize, fallbackByReference: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Serilog.Modify.Log(exception, "External components resume check failed - county {CountyId}, the {ReferenceCount} references are processed in full", countyId, references.Count);
                    return null;
                }

                if (table_Existing is null)
                {
                    return null;
                }

                Column? column_Reference = null;
                Column? column_Total = null;
                foreach (Column column in table_Existing.Columns)
                {
                    string? uniqueId_Column = Core.IO.Query.UniqueId(column);
                    if (uniqueId_Column == uniqueId_Reference)
                    {
                        column_Reference = column;
                    }
                    else if (uniqueId_Column == uniqueId_Total)
                    {
                        column_Total = column;
                    }
                }

                HashSet<string> result = [];
                if (column_Reference is null || column_Total is null)
                {
                    return result;
                }

                int count = table_Existing.RowCount;
                for (int i = 0; i < count; i++)
                {
                    Row? row = table_Existing.GetRow(i);
                    if (row is null)
                    {
                        continue;
                    }

                    if (!row.TryGetValue(column_Reference.Index, out string? reference_Row) || string.IsNullOrWhiteSpace(reference_Row))
                    {
                        continue;
                    }

                    // The raw cell first: whether a null converts to a number is the converter's business, and a
                    // NULL total must never read as a 0 that marks the building done.
                    if (!row.TryGetValue(column_Total.Index, out object? value_Total) || value_Total is null || value_Total is DBNull)
                    {
                        continue;
                    }

                    if (Core.Query.TryConvert(value_Total, out double total_Row) && !double.IsNaN(total_Row))
                    {
                        result.Add(reference_Row!);
                    }
                }

                return result;
            }

            // The party walls of the buildings of a batch: the outlines touching each building are read from the
            // building table, the stored models of those neighbours read by reference, and the shared walls measured.
            // Null when a read fails, which leaves the party wall columns of the batch as they were.
            async Task<List<DiGi.Analytical.Building.Classes.PartyWallResult>?> PartyWallResultsAsync(List<BuildingModel> buildingModels, int countyId, int offset, Dictionary<string, GIS.Classes.Building2D> building2Ds_ByReference)
            {
                try
                {
                    // The subjects, as analytical models, with their outlines.
                    List<Tuple<BuildingModel, DiGi.Analytical.Building.Classes.BuildingModel, GIS.Classes.Building2D>> subjects = [];
                    foreach (BuildingModel buildingModel in buildingModels)
                    {
                        if (buildingModel.Reference is string reference_Subject && building2Ds_ByReference.TryGetValue(reference_Subject, out GIS.Classes.Building2D? building2D_Subject) && buildingModel.ToDiGi() is DiGi.Analytical.Building.Classes.BuildingModel buildingModel_Subject)
                        {
                            subjects.Add(new Tuple<BuildingModel, DiGi.Analytical.Building.Classes.BuildingModel, GIS.Classes.Building2D>(buildingModel, buildingModel_Subject, building2D_Subject));
                        }
                    }

                    List<DiGi.Analytical.Building.Classes.PartyWallResult> result = [];
                    if (subjects.Count == 0)
                    {
                        return result;
                    }

                    // One outline read per kilometre tile of the batch: the batch is compact along the Z-order curve, but a
                    // curve that jumps would make a single box span the gap.
                    Dictionary<Tuple<long, long>, List<DiGi.Geometry.Planar.Classes.BoundingBox2D>> boundingBox2Ds_ByTile = [];
                    foreach (Tuple<BuildingModel, DiGi.Analytical.Building.Classes.BuildingModel, GIS.Classes.Building2D> subject in subjects)
                    {
                        if (subject.Item3.PolygonalFace2D?.GetBoundingBox() is not DiGi.Geometry.Planar.Classes.BoundingBox2D boundingBox2D_Subject)
                        {
                            continue;
                        }

                        Tuple<long, long> tile = new((long)Math.Floor(boundingBox2D_Subject.Min.X / 1000.0), (long)Math.Floor(boundingBox2D_Subject.Min.Y / 1000.0));
                        if (!boundingBox2Ds_ByTile.TryGetValue(tile, out List<DiGi.Geometry.Planar.Classes.BoundingBox2D>? boundingBox2Ds_Tile))
                        {
                            boundingBox2Ds_Tile = [];
                            boundingBox2Ds_ByTile[tile] = boundingBox2Ds_Tile;
                        }

                        boundingBox2Ds_Tile.Add(boundingBox2D_Subject);
                    }

                    // Every outline near the batch, keyed by county and reference.
                    Dictionary<Tuple<int, string>, GIS.Classes.Building2D> candidates = [];
                    foreach (List<DiGi.Geometry.Planar.Classes.BoundingBox2D> boundingBox2Ds_Tile in boundingBox2Ds_ByTile.Values)
                    {
                        DiGi.Geometry.Planar.Classes.BoundingBox2D boundingBox2D_Tile = new(boundingBox2Ds_Tile);
                        boundingBox2D_Tile.Offset(partyWallMaxDistance);

                        List<Building2D>? building2Ds_Candidate = await building2DPostgreSQLConverter!.GetBuilding2DsByBoundingBox2DAsync(boundingBox2D_Tile, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        if (building2Ds_Candidate is null)
                        {
                            throw new InvalidOperationException($"The outlines around the batch at offset {offset} of county {countyId} could not be read.");
                        }

                        foreach (Building2D building2D_Candidate in building2Ds_Candidate)
                        {
                            if (building2D_Candidate?.Reference is string reference_Candidate && building2D_Candidate.CountyId is int countyId_Candidate && building2D_Candidate.ToDiGi() is GIS.Classes.Building2D building2D_Candidate_GIS && building2D_Candidate_GIS.PolygonalFace2D is not null)
                            {
                                candidates[new Tuple<int, string>(countyId_Candidate, reference_Candidate)] = building2D_Candidate_GIS;
                            }
                        }
                    }

                    // The candidates indexed once by the 50 m cells their boxes cover, so each subject is tested against the
                    // outlines around it rather than against the whole neighbourhood of the batch.
                    const double cellSize = 50.0;
                    Dictionary<Tuple<long, long>, List<Tuple<int, string>>> candidateKeys_ByCell = [];
                    foreach (KeyValuePair<Tuple<int, string>, GIS.Classes.Building2D> candidate in candidates)
                    {
                        if (candidate.Value.PolygonalFace2D?.GetBoundingBox() is not DiGi.Geometry.Planar.Classes.BoundingBox2D boundingBox2D_Candidate)
                        {
                            continue;
                        }

                        for (long x = (long)Math.Floor(boundingBox2D_Candidate.Min.X / cellSize); x <= (long)Math.Floor(boundingBox2D_Candidate.Max.X / cellSize); x++)
                        {
                            for (long y = (long)Math.Floor(boundingBox2D_Candidate.Min.Y / cellSize); y <= (long)Math.Floor(boundingBox2D_Candidate.Max.Y / cellSize); y++)
                            {
                                Tuple<long, long> cell = new(x, y);
                                if (!candidateKeys_ByCell.TryGetValue(cell, out List<Tuple<int, string>>? candidateKeys_Cell))
                                {
                                    candidateKeys_Cell = [];
                                    candidateKeys_ByCell[cell] = candidateKeys_Cell;
                                }

                                candidateKeys_Cell.Add(candidate.Key);
                            }
                        }
                    }

                    // The neighbours of each subject: outlines in range of its outline - touching, overlapping, inside it or
                    // within the distance - other than its own.
                    List<List<Tuple<int, string>>> neighbourKeys_BySubject = [];
                    HashSet<Tuple<int, string>> neighbourKeys = [];
                    foreach (Tuple<BuildingModel, DiGi.Analytical.Building.Classes.BuildingModel, GIS.Classes.Building2D> subject in subjects)
                    {
                        List<Tuple<int, string>> neighbourKeys_Subject = [];
                        neighbourKeys_BySubject.Add(neighbourKeys_Subject);

                        DiGi.Geometry.Planar.Classes.PolygonalFace2D polygonalFace2D_Subject = subject.Item3.PolygonalFace2D!;
                        if (polygonalFace2D_Subject.GetBoundingBox() is not DiGi.Geometry.Planar.Classes.BoundingBox2D boundingBox2D_Subject)
                        {
                            continue;
                        }

                        Tuple<int, string> key_Subject = new(countyId, subject.Item1.Reference!);
                        HashSet<Tuple<int, string>> candidateKeys_Tested = [];

                        for (long x = (long)Math.Floor((boundingBox2D_Subject.Min.X - partyWallMaxDistance) / cellSize); x <= (long)Math.Floor((boundingBox2D_Subject.Max.X + partyWallMaxDistance) / cellSize); x++)
                        {
                            for (long y = (long)Math.Floor((boundingBox2D_Subject.Min.Y - partyWallMaxDistance) / cellSize); y <= (long)Math.Floor((boundingBox2D_Subject.Max.Y + partyWallMaxDistance) / cellSize); y++)
                            {
                                if (!candidateKeys_ByCell.TryGetValue(new Tuple<long, long>(x, y), out List<Tuple<int, string>>? candidateKeys_Cell))
                                {
                                    continue;
                                }

                                foreach (Tuple<int, string> candidateKey in candidateKeys_Cell)
                                {
                                    if (candidateKey.Equals(key_Subject) || !candidateKeys_Tested.Add(candidateKey))
                                    {
                                        continue;
                                    }

                                    if (DiGi.Geometry.Planar.Query.InRange(polygonalFace2D_Subject, candidates[candidateKey].PolygonalFace2D, partyWallMaxDistance))
                                    {
                                        neighbourKeys_Subject.Add(candidateKey);
                                        neighbourKeys.Add(candidateKey);
                                    }
                                }
                            }
                        }
                    }

                    // The neighbours' models: the batch's own where the neighbour is in it, the rest read by reference, county by county.
                    Dictionary<Tuple<int, string>, DiGi.Analytical.Building.Classes.BuildingModel> buildingModels_Neighbour = [];
                    foreach (Tuple<BuildingModel, DiGi.Analytical.Building.Classes.BuildingModel, GIS.Classes.Building2D> subject in subjects)
                    {
                        buildingModels_Neighbour[new Tuple<int, string>(countyId, subject.Item1.Reference!)] = subject.Item2;
                    }

                    Dictionary<int, List<string>> references_ByCountyId = [];
                    foreach (Tuple<int, string> neighbourKey in neighbourKeys)
                    {
                        if (buildingModels_Neighbour.ContainsKey(neighbourKey))
                        {
                            continue;
                        }

                        if (!references_ByCountyId.TryGetValue(neighbourKey.Item1, out List<string>? references_County))
                        {
                            references_County = [];
                            references_ByCountyId[neighbourKey.Item1] = references_County;
                        }

                        references_County.Add(neighbourKey.Item2);
                    }

                    foreach (KeyValuePair<int, List<string>> keyValuePair in references_ByCountyId)
                    {
                        // A table keyed by county is read with the fallback by reference: a model filed under a sibling part
                        // of a multi-part county would otherwise silently not be returned.
                        List<BuildingModel>? buildingModels_Read = await buildingModelPostgreSQLConverter.GetItemsByReferencesAsync(keyValuePair.Value, keyValuePair.Key, fallbackByReference: true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        if (buildingModels_Read is null)
                        {
                            throw new InvalidOperationException($"The stored models of {keyValuePair.Value.Count} neighbours in county {keyValuePair.Key} of the batch at offset {offset} of county {countyId} could not be read.");
                        }

                        // Latest stored version first, so the first record of a reference is the one used.
                        foreach (BuildingModel buildingModel_Read in buildingModels_Read)
                        {
                            if (buildingModel_Read?.Reference is not string reference_Read)
                            {
                                continue;
                            }

                            Tuple<int, string> key_Read = new(keyValuePair.Key, reference_Read);
                            if (!buildingModels_Neighbour.ContainsKey(key_Read) && buildingModel_Read.ToDiGi() is DiGi.Analytical.Building.Classes.BuildingModel buildingModel_Analytical)
                            {
                                buildingModels_Neighbour[key_Read] = buildingModel_Analytical;
                            }
                        }
                    }

                    // The subjects with a neighbour, and the models of those neighbours, measured in one call: each model's
                    // envelope is then built once for the batch rather than once for every building it touches.
                    List<DiGi.Analytical.Building.Classes.BuildingModel> buildingModels_Examined = [];
                    List<DiGi.Analytical.Building.Classes.BuildingModel> buildingModels_Touching = [];
                    for (int i = 0; i < subjects.Count; i++)
                    {
                        bool touching = false;
                        foreach (Tuple<int, string> neighbourKey in neighbourKeys_BySubject[i])
                        {
                            if (buildingModels_Neighbour.TryGetValue(neighbourKey, out DiGi.Analytical.Building.Classes.BuildingModel? buildingModel_Neighbour))
                            {
                                touching = true;
                                buildingModels_Touching.Add(buildingModel_Neighbour);
                            }
                            else
                            {
                                MissingNeighbourModelCount++;
                            }
                        }

                        if (touching)
                        {
                            buildingModels_Examined.Add(subjects[i].Item2);
                        }
                    }

                    Dictionary<Guid, List<DiGi.Analytical.Building.Classes.PartyWallResult>>? partyWallResults_ByGuid = DiGi.Analytical.Building.Query.PartyWalls(buildingModels_Examined, buildingModels_Touching, partyWallMaxDistance);
                    if (partyWallResults_ByGuid is not null)
                    {
                        foreach (List<DiGi.Analytical.Building.Classes.PartyWallResult> partyWallResults_Subject in partyWallResults_ByGuid.Values)
                        {
                            if (partyWallResults_Subject.Count != 0)
                            {
                                PartyWallBuildingCount++;
                                result.AddRange(partyWallResults_Subject);
                            }
                        }
                    }

                    return result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    PartyWallFailedBatchCount++;
                    Serilog.Modify.Log(exception, "External components party walls not measured - county {CountyId}, offset {Offset}, the {ModelCount} buildings of the batch keep their party wall columns", countyId, offset, buildingModels.Count);
                    return null;
                }
            }
        }
    }
}
