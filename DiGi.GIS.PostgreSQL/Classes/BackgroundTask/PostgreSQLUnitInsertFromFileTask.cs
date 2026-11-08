using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that populates the PostgreSQL table for territorial units from local JSON files.
    /// </summary>
    public class PostgreSQLUnitInsertFromFileTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        private readonly UnitPostgreSQLConverter unitPostgreSQLConverter;

        /// <summary>
        /// Gets or sets the options used to configure the file-based unit population process.
        /// </summary>
        public PostgreSQLUnitInsertFromFileOptions PostgreSQLUnitInsertFromFileOptions { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLUnitInsertFromFileTask"/> class with a converter.
        /// </summary>
        /// <param name="unitPostgreSQLConverter">The unit PostgreSQL converter used to populate the table.</param>
        public PostgreSQLUnitInsertFromFileTask(UnitPostgreSQLConverter unitPostgreSQLConverter)
        {
            this.unitPostgreSQLConverter = unitPostgreSQLConverter ?? throw new ArgumentNullException(nameof(unitPostgreSQLConverter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLUnitInsertFromFileTask"/> class from a manager.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS converter manager containing the unit converter.</param>
        public PostgreSQLUnitInsertFromFileTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            ArgumentNullException.ThrowIfNull(gISPostgreSQLConverterManager);

            unitPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<UnitPostgreSQLConverter>() ?? throw new InvalidOperationException($"{nameof(UnitPostgreSQLConverter)} not registered in converter manager.");
        }

        /// <summary>
        /// Executes the background task to read territorial units from JSON file(s) and insert them into PostgreSQL.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of processed items.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if the population was successful; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            PostgreSQLUnitInsertFromFileOptions ??= new();
            return await unitPostgreSQLConverter.PopulateAsync(
                PostgreSQLUnitInsertFromFileOptions.Path,
                PostgreSQLUnitInsertFromFileOptions.Clear,
                PostgreSQLUnitInsertFromFileOptions.BatchSize,
                progress,
                commandTimeout: PostgreSQLUnitInsertFromFileOptions.CommandTimeout,
                cancellationToken: cancellationToken);
        }
    }
}
