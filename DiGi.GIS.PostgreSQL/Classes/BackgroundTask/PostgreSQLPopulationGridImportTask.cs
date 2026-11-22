using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Imports a downloaded GeoJSON export of the GUS 1 km census population grid into <see cref="Constants.TableName.PopulationGrid"/> (DiGi.GIS.PostgreSQL#106).
    /// <para>The file is obtained by hand from the Geostatistics Portal - the portal's regulations forbid automated downloading - and handed to <see cref="PopulationGridPostgreSQLConverter.PopulateAsync(string?, int?, bool, int, IProgress{long}?, int, CancellationToken)"/>, which reads and validates it in full before anything is cleared or written. A run refuses, with the reason in the log, when <see cref="PostgreSQLPopulationGridImportTaskOptions.SourcePath"/> or <see cref="PostgreSQLPopulationGridImportTaskOptions.CensusYear"/> is missing or the file is not there.</para>
    /// <para>Tray registration lives in <c>DiGi.GIS.PostgreSQL.UI</c> and is filed with the production run (DiGi.GIS.PostgreSQL#156); this class is the task itself.</para>
    /// </summary>
    public class PostgreSQLPopulationGridImportTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        private readonly PopulationGridPostgreSQLConverter populationGridPostgreSQLConverter;

        /// <summary>
        /// Gets or sets the options used to configure the census grid import.
        /// </summary>
        public PostgreSQLPopulationGridImportTaskOptions PostgreSQLPopulationGridImportTaskOptions { get; set; } = new();

        /// <summary>
        /// Gets the number of grid cells the last run wrote, 0 when it refused or failed.
        /// </summary>
        public long ImportedCount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLPopulationGridImportTask"/> class with a converter.
        /// </summary>
        /// <param name="populationGridPostgreSQLConverter">The <see cref="PopulationGridPostgreSQLConverter"/> used to write the grid.</param>
        /// <exception cref="ArgumentNullException">The converter is null.</exception>
        public PostgreSQLPopulationGridImportTask(PopulationGridPostgreSQLConverter populationGridPostgreSQLConverter)
        {
            this.populationGridPostgreSQLConverter = populationGridPostgreSQLConverter ?? throw new ArgumentNullException(nameof(populationGridPostgreSQLConverter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLPopulationGridImportTask"/> class from a manager.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS converter manager containing the census grid converter.</param>
        /// <exception cref="ArgumentNullException">The manager is null.</exception>
        /// <exception cref="InvalidOperationException">The census grid converter is not registered in the manager.</exception>
        public PostgreSQLPopulationGridImportTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            ArgumentNullException.ThrowIfNull(gISPostgreSQLConverterManager);

            populationGridPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<PopulationGridPostgreSQLConverter>() ?? throw new InvalidOperationException($"{nameof(PopulationGridPostgreSQLConverter)} not registered in converter manager.");
        }

        /// <summary>
        /// Executes the census grid import: reads the export, optionally replaces the census year's stored rows and upserts the cells, reporting the written count.
        /// </summary>
        /// <param name="progress">A progress reporter reporting the number of cells written.</param>
        /// <param name="cancellationToken">A cancellation token for monitoring cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when at least one cell was written; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            PostgreSQLPopulationGridImportTaskOptions ??= new();
            ImportedCount = 0;

            string? sourcePath = PostgreSQLPopulationGridImportTaskOptions.SourcePath;
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: SourcePath is required - point it at the GeoJSON export downloaded from the Geostatistics Portal", nameof(PostgreSQLPopulationGridImportTask));
                return false;
            }

            if (PostgreSQLPopulationGridImportTaskOptions.CensusYear is not int censusYear)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: CensusYear is required - the census the grid was counted in, for example 2021, not the download year", nameof(PostgreSQLPopulationGridImportTask));
                return false;
            }

            if (!File.Exists(sourcePath))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the export file does not exist: {SourcePath}", nameof(PostgreSQLPopulationGridImportTask), sourcePath);
                return false;
            }

            long? writtenCount = await populationGridPostgreSQLConverter.PopulateAsync(
                sourcePath,
                censusYear,
                PostgreSQLPopulationGridImportTaskOptions.Clear,
                PostgreSQLPopulationGridImportTaskOptions.BatchSize,
                progress,
                PostgreSQLPopulationGridImportTaskOptions.CommandTimeout,
                cancellationToken);

            if (writtenCount is not long writtenCount_Value || writtenCount_Value == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no grid cell was imported from {SourcePath}", nameof(PostgreSQLPopulationGridImportTask), sourcePath);
                return false;
            }

            ImportedCount = writtenCount_Value;
            return true;
        }
    }
}
