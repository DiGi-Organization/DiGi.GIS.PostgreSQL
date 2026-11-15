using DiGi.Core.Classes;
using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that exports the urban-context table of the stored solar radiation, one CSV file per county part (DiGi.GIS.PostgreSQL#107, #136).
    /// <para>Each row is one building: its county part and reference, its descriptors and radial ratios from <c>building_data</c>, the annual irradiation of its wall sectors, walls, roof and whole building for each radius, and the shortfall of each neighbour radius against the isolated building (<see cref="Create.Table_SolarRadiationContext"/>). The buildings are the ones with a stored result for any of the radii; with <see cref="PostgreSQLBuildingSolarRadiationContextExportOptions.ResidentialOnly"/> only the residential ones.</para>
    /// <para>A county part whose stored results or building data cannot be read, or whose file cannot be written, is failed and logged with the exception, and the export keeps going over the other parts.</para>
    /// </summary>
    public class PostgreSQLBuildingSolarRadiationContextExportTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The GIS PostgreSQL converter manager used to retrieve converters and execute operations.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationContextExportTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager used to retrieve converters and execute operations.</param>
        public PostgreSQLBuildingSolarRadiationContextExportTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the number of county parts that failed and were stepped over during the last run.
        /// </summary>
        public long FailedCountyCount { get; private set; }

        /// <summary>
        /// Gets the number of files written during the last run.
        /// </summary>
        public long FileCount { get; private set; }

        /// <summary>
        /// Gets or sets the options used to configure the run.
        /// </summary>
        public PostgreSQLBuildingSolarRadiationContextExportOptions PostgreSQLBuildingSolarRadiationContextExportOptions { get; set; } = new();

        /// <summary>
        /// Gets the number of building rows written during the last run; the progress reported.
        /// </summary>
        public long RowCount { get; private set; }

        /// <summary>
        /// Executes the background task that exports the urban-context table of every county part in scope.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of building rows written.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every county part in scope was exported without error; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            FailedCountyCount = 0;
            FileCount = 0;
            RowCount = 0;

            PostgreSQLBuildingSolarRadiationContextExportOptions ??= new();

            int commandTimeout = PostgreSQLBuildingSolarRadiationContextExportOptions.CommandTimeout;
            int batchSize = Math.Max(1, PostgreSQLBuildingSolarRadiationContextExportOptions.BatchSize);
            bool residentialOnly = PostgreSQLBuildingSolarRadiationContextExportOptions.ResidentialOnly;
            string? outputDirectory = PostgreSQLBuildingSolarRadiationContextExportOptions.OutputDirectory;
            List<double> radii = [.. (PostgreSQLBuildingSolarRadiationContextExportOptions.Radii ?? []).Where(double.IsFinite).Distinct()];

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no output directory - there is nowhere to write to", nameof(PostgreSQLBuildingSolarRadiationContextExportTask));
                return false;
            }

            if (radii.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no radius - nothing to export", nameof(PostgreSQLBuildingSolarRadiationContextExportTask));
                return false;
            }

            BuildingSolarRadiationPostgreSQLConverter? buildingSolarRadiationPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingSolarRadiationPostgreSQLConverter>();
            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (buildingSolarRadiationPostgreSQLConverter is null || buildingDataPostgreSQLConverter is null || administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: a converter is missing - solar radiation {SolarRadiation}, building data {BuildingData}, counties {Counties}", nameof(PostgreSQLBuildingSolarRadiationContextExportTask), buildingSolarRadiationPostgreSQLConverter is not null, buildingDataPostgreSQLConverter is not null, administrativeAreal2DPostgreSQLConverter is not null);
                return false;
            }

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (countyReferences is null || countyReferences.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county could be read - the export cannot be scoped", nameof(PostgreSQLBuildingSolarRadiationContextExportTask));
                return false;
            }

            System.IO.Directory.CreateDirectory(outputDirectory!);

            HashSet<int>? countyIds = PostgreSQLBuildingSolarRadiationContextExportOptions.CountyIds;

            Serilog.Modify.Log("{Type}: starting, scope {CountyScope}, radii {Radii} m, residential only {ResidentialOnly}, output {OutputDirectory}", nameof(PostgreSQLBuildingSolarRadiationContextExportTask), countyIds is null ? "all" : string.Join(", ", countyIds), string.Join(", ", radii.Select(x => x.ToString(CultureInfo.InvariantCulture))), residentialOnly, outputDirectory);

            foreach (AdministrativeAreal2DReference? countyReference in countyReferences.OrderBy(x => x?.Id ?? int.MaxValue))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (countyReference is null || (countyIds is not null && !countyIds.Contains(countyReference.Id)))
                {
                    continue;
                }

                int countyId = countyReference.Id;
                try
                {
                    // Every building with a result for any of the radii, in a fixed order.
                    HashSet<string> references = [];
                    foreach (double radius in radii)
                    {
                        HashSet<string>? references_Radius = await buildingSolarRadiationPostgreSQLConverter.GetReferencesAsync(countyId, commandTimeout: commandTimeout, radius: radius, cancellationToken: cancellationToken) ?? throw new InvalidOperationException("the stored results could not be read");
                        references.UnionWith(references_Radius);
                    }

                    if (references.Count == 0)
                    {
                        continue;
                    }

                    List<string> references_List = [.. references.OrderBy(x => x, StringComparer.Ordinal)];

                    Table? table_County = null;
                    for (int offset = 0; offset < references_List.Count; offset += batchSize)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        List<string> references_Batch = references_List.GetRange(offset, Math.Min(batchSize, references_List.Count - offset));

                        Table? table_BuildingData = await buildingDataPostgreSQLConverter.PullAsync(references_Batch, countyId, batchSize: batchSize, fallbackByReference: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken) ?? throw new InvalidOperationException("the building data could not be read");

                        List<GIS.Classes.BuildingSolarRadiationResult> results = [];
                        foreach (double radius in radii)
                        {
                            List<GIS.Classes.BuildingSolarRadiationResult>? results_Radius = await buildingSolarRadiationPostgreSQLConverter.GetItemsByReferencesAsync(references_Batch, countyId, commandTimeout: commandTimeout, radius: radius, cancellationToken: cancellationToken) ?? throw new InvalidOperationException("the stored results could not be read");
                            results.AddRange(results_Radius);
                        }

                        Table table_Batch = Create.Table_SolarRadiationContext(results, radii, table_BuildingData, residentialOnly) ?? throw new InvalidOperationException("the building data table has no county or reference column");
                        if (table_County is null)
                        {
                            table_County = table_Batch;
                        }
                        else
                        {
                            // Every batch reads the same columns, so the rows line up with the first batch's columns.
                            for (int i = 0; i < table_Batch.RowCount; i++)
                            {
                                if (table_Batch.GetRow(i) is Row row)
                                {
                                    table_County.AddRow(row, false);
                                }
                            }
                        }

                        RowCount += table_Batch.RowCount;
                        progress.Report(RowCount);
                    }

                    if (table_County is null || table_County.RowCount == 0)
                    {
                        continue;
                    }

                    string path = System.IO.Path.Combine(outputDirectory!, string.Format(CultureInfo.InvariantCulture, "solar_context_county_{0}.csv", countyId));
                    if (!Core.IO.DelimitedData.Modify.Write(table_County, path, Core.IO.DelimitedData.Enums.DelimitedDataSeparator.Comma, x => x is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : x?.ToString()))
                    {
                        throw new InvalidOperationException("the file could not be written: " + path);
                    }

                    FileCount++;
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Information, "{Type}: county part {CountyId} exported - {RowCount} buildings to {Path}", nameof(PostgreSQLBuildingSolarRadiationContextExportTask), countyId, table_County.RowCount, path);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    FailedCountyCount++;
                    Serilog.Modify.Log(exception, "{Type}: county part {CountyId} failed", nameof(PostgreSQLBuildingSolarRadiationContextExportTask), countyId);
                }
            }

            Serilog.Modify.Log("{Type}: finished - {FileCount} files, {RowCount} buildings, {FailedCountyCount} county parts failed", nameof(PostgreSQLBuildingSolarRadiationContextExportTask), FileCount, RowCount, FailedCountyCount);

            return FailedCountyCount == 0;
        }
    }
}
