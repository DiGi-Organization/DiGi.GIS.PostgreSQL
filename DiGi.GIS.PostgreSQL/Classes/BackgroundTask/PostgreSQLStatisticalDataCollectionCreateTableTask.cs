using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that creates the PostgreSQL table for statistical data collections.
    /// </summary>
    public class PostgreSQLStatisticalDataCollectionCreateTableTask : BackgroundTask, IGISPostgreSQLObject
    {
        private readonly StatisticalDataCollectionPostgreSQLConverter statisticalDataCollectionPostgreSQLConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLStatisticalDataCollectionCreateTableTask"/> class with a converter.
        /// </summary>
        /// <param name="statisticalDataCollectionPostgreSQLConverter">The statistical data collection PostgreSQL converter used to create the table.</param>
        public PostgreSQLStatisticalDataCollectionCreateTableTask(StatisticalDataCollectionPostgreSQLConverter statisticalDataCollectionPostgreSQLConverter)
        {
            this.statisticalDataCollectionPostgreSQLConverter = statisticalDataCollectionPostgreSQLConverter ?? throw new ArgumentNullException(nameof(statisticalDataCollectionPostgreSQLConverter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLStatisticalDataCollectionCreateTableTask"/> class from a manager.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS converter manager containing the statistical data collection converter.</param>
        public PostgreSQLStatisticalDataCollectionCreateTableTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            ArgumentNullException.ThrowIfNull(gISPostgreSQLConverterManager);

            statisticalDataCollectionPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<StatisticalDataCollectionPostgreSQLConverter>() ?? throw new InvalidOperationException($"{nameof(StatisticalDataCollectionPostgreSQLConverter)} not registered in converter manager.");
        }

        /// <summary>
        /// Executes the background task to create the statistical data collection table in PostgreSQL.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns true if the table was created successfully; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync()
        {
            return await statisticalDataCollectionPostgreSQLConverter.CreateTableAsync(600);
        }
    }
}
