using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that populates the PostgreSQL table for statistical data collections from .sdcf files.
    /// </summary>
    public class PostgreSQLStatisticalDataCollectionPopulateTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        private readonly StatisticalDataCollectionPostgreSQLConverter statisticalDataCollectionPostgreSQLConverter;

        /// <summary>
        /// Gets or sets the options used to configure the statistical data collection population process.
        /// </summary>
        public PostgreSQLStatisticalDataCollectionPopulateOptions PostgreSQLStatisticalDataCollectionPopulateOptions { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLStatisticalDataCollectionPopulateTask"/> class with a converter.
        /// </summary>
        /// <param name="statisticalDataCollectionPostgreSQLConverter">The statistical data collection PostgreSQL converter used to populate the table.</param>
        public PostgreSQLStatisticalDataCollectionPopulateTask(StatisticalDataCollectionPostgreSQLConverter statisticalDataCollectionPostgreSQLConverter)
        {
            this.statisticalDataCollectionPostgreSQLConverter = statisticalDataCollectionPostgreSQLConverter ?? throw new ArgumentNullException(nameof(statisticalDataCollectionPostgreSQLConverter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLStatisticalDataCollectionPopulateTask"/> class from a manager.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS converter manager containing the statistical data collection converter.</param>
        public PostgreSQLStatisticalDataCollectionPopulateTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            ArgumentNullException.ThrowIfNull(gISPostgreSQLConverterManager);

            statisticalDataCollectionPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<StatisticalDataCollectionPostgreSQLConverter>() ?? throw new InvalidOperationException($"{nameof(StatisticalDataCollectionPostgreSQLConverter)} not registered in converter manager.");
        }

        /// <summary>
        /// Executes the background task to read statistical data collections from .sdcf files and insert them into PostgreSQL.
        /// </summary>
        /// <param name="progress">A progress reporter for reporting the number of processed items.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if the population was successful; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            PostgreSQLStatisticalDataCollectionPopulateOptions ??= new();
            return await statisticalDataCollectionPostgreSQLConverter.PopulateAsync(
                PostgreSQLStatisticalDataCollectionPopulateOptions.Path,
                PostgreSQLStatisticalDataCollectionPopulateOptions.Clear,
                PostgreSQLStatisticalDataCollectionPopulateOptions.BatchSize,
                progress,
                commandTimeout: PostgreSQLStatisticalDataCollectionPopulateOptions.CommandTimeout,
                cancellationToken: cancellationToken);
        }
    }
}
