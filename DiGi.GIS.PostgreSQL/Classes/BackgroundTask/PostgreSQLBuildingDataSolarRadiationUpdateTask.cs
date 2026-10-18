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
    /// Represents a background task that fills the yearly solar radiation summary columns of the building data table from the stored solar radiation.
    /// <para>The run is driven by counties: for each one it reads the stored solar radiation of its buildings, aggregates every usable surface into the building totals and the eight direction sectors, and upserts one building data row per building keyed on county and reference.</para>
    /// <para>A county whose buildings carry no stored solar radiation is processed, not failed: there is simply nothing to aggregate there, and the buildings keep their current values. A county whose stored solar radiation cannot be read or whose rows cannot be written is failed and logged with the exception, and the run keeps going over the other counties. <see cref="FailedCountyCount"/> is what tells the two apart, and a run with a failed county reports itself as not succeeded.</para>
    /// <para>The run is idempotent: the read is deterministic (the highest solver version of a reference wins), the aggregation is pure, and the push upserts on county and reference, so a re-run writes the same values.</para>
    /// </summary>
    public class PostgreSQLBuildingDataSolarRadiationUpdateTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The GIS PostgreSQL converter manager used to retrieve converters and execute operations.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Gets or sets the options used to configure the run.
        /// </summary>
        public PostgreSQLBuildingDataSolarRadiationUpdateOptions PostgreSQLBuildingDataSolarRadiationUpdateOptions { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingDataSolarRadiationUpdateTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager used to retrieve converters and execute operations.</param>
        public PostgreSQLBuildingDataSolarRadiationUpdateTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the number of counties that failed outright and were stepped over during the last run.
        /// <para>Each one is logged with the exception that caused it, so this figure is a count of entries to go and read rather than the whole of what is known.</para>
        /// </summary>
        public long FailedCountyCount { get; private set; }

        /// <summary>
        /// Gets the number of counties that were processed during the last run.
        /// <para>A county whose buildings carry no stored solar radiation is processed, not failed: there is simply nothing to aggregate there.</para>
        /// </summary>
        public long ProcessedCountyCount { get; private set; }

        /// <summary>
        /// Gets the number of buildings that got a row written during the last run.
        /// <para>Buildings rather than stored results: a reference with several stored solver versions is aggregated from the latest one only.</para>
        /// </summary>
        public long ProcessedReferenceCount { get; private set; }

        /// <summary>
        /// Gets the number of references stepped over during the last run because their building data row already carried a yearly solar radiation total.
        /// <para>Always 0 unless <see cref="PostgreSQLBuildingDataSolarRadiationUpdateOptions.SkipCompleted"/> is set.</para>
        /// </summary>
        public long SkippedReferenceCount { get; private set; }

        /// <summary>
        /// Gets the number of usable surfaces aggregated during the last run.
        /// <para>Usable is a finite area greater than zero; the building totals rest on exactly this many surfaces.</para>
        /// </summary>
        public long SurfaceCount { get; private set; }

        /// <summary>
        /// Gets the number of usable surfaces with no classifiable facing direction during the last run.
        /// <para>A missing or vertical outward normal, typically a flat roof: the surface counts toward the building totals but toward no direction sector.</para>
        /// </summary>
        public long SurfaceWithoutSectorCount { get; private set; }

        /// <summary>
        /// Gets the number of surfaces skipped because they carry no usable area during the last run.
        /// <para>They contribute nothing to the totals or the sectors; the count is the share of a run's surfaces a reader should treat as missing data.</para>
        /// </summary>
        public long SkippedSurfaceCount { get; private set; }

        /// <summary>
        /// Gets the number of building data rows written during the last run.
        /// <para>Rows rather than buildings: the same building is counted again on a later run.</para>
        /// </summary>
        public long UpdatedRowCount { get; private set; }

        /// <summary>
        /// Executes the background task that fills the yearly solar radiation summary columns of the building data table from the stored solar radiation.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of rows written.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every county in scope was processed without error; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            FailedCountyCount = 0;
            ProcessedCountyCount = 0;
            ProcessedReferenceCount = 0;
            SkippedReferenceCount = 0;
            SurfaceCount = 0;
            SurfaceWithoutSectorCount = 0;
            SkippedSurfaceCount = 0;
            UpdatedRowCount = 0;

            PostgreSQLBuildingDataSolarRadiationUpdateOptions ??= new();

            int commandTimeout = PostgreSQLBuildingDataSolarRadiationUpdateOptions.CommandTimeout;
            int batchSize = Math.Max(1, PostgreSQLBuildingDataSolarRadiationUpdateOptions.BatchSize);
            bool skipCompleted = PostgreSQLBuildingDataSolarRadiationUpdateOptions.SkipCompleted;

            BuildingSolarRadiationPostgreSQLConverter? buildingSolarRadiationPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingSolarRadiationPostgreSQLConverter>();
            if (buildingSolarRadiationPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the stored solar radiation cannot be read", nameof(PostgreSQLBuildingDataSolarRadiationUpdateTask), nameof(BuildingSolarRadiationPostgreSQLConverter));
                return false;
            }

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            if (buildingDataPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - there is nowhere to write to", nameof(PostgreSQLBuildingDataSolarRadiationUpdateTask), nameof(BuildingDataPostgreSQLConverter));
                return false;
            }

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no {Converter} - the counties that scope the run cannot be read", nameof(PostgreSQLBuildingDataSolarRadiationUpdateTask), nameof(AdministrativeAreal2DPostgreSQLConverter));
                return false;
            }

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (countyReferences is null || countyReferences.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county could be read - the run cannot be scoped", nameof(PostgreSQLBuildingDataSolarRadiationUpdateTask));
                return false;
            }

            HashSet<int>? countyIds = PostgreSQLBuildingDataSolarRadiationUpdateOptions.CountyIds;

            Serilog.Modify.Log(
                "{Type}: starting over {CountyCount} counties, scope {CountyScope}, batch size {BatchSize}, skip completed {SkipCompleted}",
                nameof(PostgreSQLBuildingDataSolarRadiationUpdateTask),
                countyReferences.Count,
                countyIds is null ? "all" : string.Join(", ", countyIds),
                batchSize,
                skipCompleted);

            // The columns the resume check reads. The reference column is asked for explicitly so the rows can be
            // matched back; the total is non-null only on a row a run has written.
            string? uniqueId_Reference = Core.IO.Query.UniqueId(IO.Constants.Column.Reference);
            // Through the base-typed list, whose second entry is the energy total: the constant is a UnitColumn, and
            // naming it would need a DiGi.Unit.IO reference this project stays free of (see Modify.Update_ExternalComponentsArea).
            List<Column> columns_SolarRadiation = IO.Create.Columns_SolarRadiation();
            string? uniqueId_Total = columns_SolarRadiation.Count > 1 ? Core.IO.Query.UniqueId(columns_SolarRadiation[1]) : null;

            foreach (AdministrativeAreal2DReference? countyReference in countyReferences)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (countyReference is null)
                {
                    continue;
                }

                // Id is the county row identifier - the key the solar radiation is filed under - never the code,
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
                    HashSet<string>? references = await buildingSolarRadiationPostgreSQLConverter.GetReferencesAsync(countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
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
                    Serilog.Modify.Log(exception, "Solar radiation county failed - county {CountyId}, the stored solar radiation references could not be read", countyId);
                }

                if (references_List.Count == 0 && !countyFailed)
                {
                    // No stored solar radiation in this county: nothing to aggregate, and that is not a failure.
                    ProcessedCountyCount++;
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Information, "Solar radiation county processed - county {CountyId} carries no stored solar radiation", countyId);
                    continue;
                }

                long countyReferenceCount = 0;
                long countyRowCount = 0;
                long countySurfaceCount = 0;
                long countySurfaceWithoutSectorCount = 0;
                long countySkippedSurfaceCount = 0;
                long countySkippedReferenceCount = 0;

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

                    List<GIS.Classes.BuildingSolarRadiationResult>? results_Batch = null;
                    try
                    {
                        // No fallback by reference: a reference the county does not hold is absent from the read, and
                        // the record that comes back carries this county's identifier, so nothing is filed elsewhere.
                        results_Batch = await buildingSolarRadiationPostgreSQLConverter.GetItemsByReferencesAsync(references_Batch, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        if (results_Batch is null)
                        {
                            countyFailed = true;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Solar radiation county failed - county {CountyId}, offset {Offset}, the read of the {ReferenceCount} references came back empty", countyId, offset, references_Batch.Count);
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        countyFailed = true;
                        Serilog.Modify.Log(exception, "Solar radiation county failed - county {CountyId}, offset {Offset}, the stored solar radiation of {ReferenceCount} references could not be read", countyId, offset, references_Batch.Count);
                    }

                    if (countyFailed || results_Batch is null)
                    {
                        break;
                    }

                    Table table = new();
                    SolarRadiationSummaryResult batchResult;
                    try
                    {
                        batchResult = Modify.Update_SolarRadiation(table, results_Batch);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        countyFailed = true;
                        Serilog.Modify.Log(exception, "Solar radiation county failed - county {CountyId}, offset {Offset}, the {ReferenceCount} stored results could not be aggregated", countyId, offset, results_Batch.Count);
                        break;
                    }

                    results_Batch = null;

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
                            Serilog.Modify.Log(exception, "Solar radiation county failed - county {CountyId}, offset {Offset}, the {RowCount} rows built could not be written", countyId, offset, table.RowCount);
                            break;
                        }

                        if (!updated)
                        {
                            countyFailed = true;
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Solar radiation county failed - county {CountyId}, offset {Offset}, the write of {RowCount} rows was rolled back", countyId, offset, table.RowCount);
                            break;
                        }
                    }

                    countyReferenceCount += batchResult.ProcessedReferenceCount;
                    countyRowCount += table.RowCount;
                    countySurfaceCount += batchResult.SurfaceCount;
                    countySurfaceWithoutSectorCount += batchResult.SurfaceWithoutSectorCount;
                    countySkippedSurfaceCount += batchResult.SkippedSurfaceCount;

                    ProcessedReferenceCount += batchResult.ProcessedReferenceCount;
                    SurfaceCount += batchResult.SurfaceCount;
                    SurfaceWithoutSectorCount += batchResult.SurfaceWithoutSectorCount;
                    SkippedSurfaceCount += batchResult.SkippedSurfaceCount;
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

                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Information, "Solar radiation county processed - county {CountyId}, {ReferenceCount} buildings, {RowCount} rows, {SurfaceCount} surfaces, {WithoutSectorCount} without a sector, {SkippedSurfaceCount} surfaces skipped, {SkippedReferenceCount} references already done", countyId, countyReferenceCount, countyRowCount, countySurfaceCount, countySurfaceWithoutSectorCount, countySkippedSurfaceCount, countySkippedReferenceCount);
            }

            Serilog.Modify.Log(
                "{Type}: finished - {ProcessedCount} counties, {FailedCount} counties failed, {ReferenceCount} buildings, {SurfaceCount} surfaces, {WithoutSectorCount} without a sector, {SkippedSurfaceCount} surfaces skipped, {SkippedReferenceCount} references already done, {RowCount} rows",
                nameof(PostgreSQLBuildingDataSolarRadiationUpdateTask),
                ProcessedCountyCount,
                FailedCountyCount,
                ProcessedReferenceCount,
                SurfaceCount,
                SurfaceWithoutSectorCount,
                SkippedSurfaceCount,
                SkippedReferenceCount,
                UpdatedRowCount);

            return FailedCountyCount == 0;

            // The references of the batch whose building data row already carries a yearly solar radiation total - the work an
            // earlier, interrupted run finished. Null when the check cannot be made: the batch is then processed in full,
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
                    Serilog.Modify.Log(exception, "Solar radiation resume check failed - county {CountyId}, the {ReferenceCount} references are processed in full", countyId, references.Count);
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

                    // A NULL total must never read as a 0 that marks the building done.
                    if (!row.TryGetValue(column_Total.Index, out object? value_Total) || value_Total is null || value_Total is DBNull)
                    {
                        continue;
                    }

                    if (Core.Query.TryConvert(value_Total, out double total_Row) && double.IsFinite(total_Row))
                    {
                        result.Add(reference_Row!);
                    }
                }

                return result;
            }
        }
    }
}
