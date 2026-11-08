using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that populates the PostgreSQL table for territorial units from the Central Statistical Office (BDL) API.
    /// </summary>
    public class PostgreSQLUnitPopulateTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        private readonly UnitPostgreSQLConverter unitPostgreSQLConverter;

        /// <summary>
        /// Gets or sets the options used to configure the unit population process.
        /// </summary>
        public PostgreSQLUnitPopulateOptions PostgreSQLUnitPopulateOptions { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLUnitPopulateTask"/> class with a converter.
        /// </summary>
        /// <param name="unitPostgreSQLConverter">The unit PostgreSQL converter used to populate the table.</param>
        public PostgreSQLUnitPopulateTask(UnitPostgreSQLConverter unitPostgreSQLConverter)
        {
            this.unitPostgreSQLConverter = unitPostgreSQLConverter ?? throw new ArgumentNullException(nameof(unitPostgreSQLConverter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLUnitPopulateTask"/> class from a manager.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS converter manager containing the unit converter.</param>
        public PostgreSQLUnitPopulateTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            ArgumentNullException.ThrowIfNull(gISPostgreSQLConverterManager);

            unitPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<UnitPostgreSQLConverter>() ?? throw new InvalidOperationException($"{nameof(UnitPostgreSQLConverter)} not registered in converter manager.");
        }

        /// <summary>
        /// Executes the background task to fetch territorial units from the BDL API and insert them into PostgreSQL.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of processed items.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if the population was successful; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            PostgreSQLUnitPopulateOptions ??= new();
            return await unitPostgreSQLConverter.PopulateAsync(PostgreSQLUnitPopulateOptions.PageSize, PostgreSQLUnitPopulateOptions.Clear, PostgreSQLUnitPopulateOptions.BatchSize, PostgreSQLUnitPopulateOptions.ClientId, progress, commandTimeout: 600, cancellationToken: cancellationToken);
        }
    }
}
