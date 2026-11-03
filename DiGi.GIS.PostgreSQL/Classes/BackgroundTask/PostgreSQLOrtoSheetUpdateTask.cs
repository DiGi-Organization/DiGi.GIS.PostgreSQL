using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that imports the GUGiK orthophoto sheet index into <c>orto_sheet</c> (<see cref="Modify.UpdateOrtoSheetsAsync"/>), one index year after another from <see cref="PostgreSQLOrtoSheetUpdateOptions.FirstYear"/> to <see cref="PostgreSQLOrtoSheetUpdateOptions.LastYear"/>.
    /// <para>Progress is the number of years done; every year is logged with its count from the service and the rows read and written, and the totals are in <see cref="OrtoSheetUpdateResult"/>. A year the service does not publish is skipped. Run it before the acquisition backfill (<see cref="PostgreSQLOrtoDatasAcquisitionUpdateTask"/>), and again when GUGiK publishes a new year.</para>
    /// </summary>
    public class PostgreSQLOrtoSheetUpdateTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The manager the sheet converter is read from.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// The client the index service is read with.
        /// </summary>
        protected readonly HttpClient? httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoSheetUpdateTask"/> class.
        /// </summary>
        /// <param name="httpClient">The client the index service is read with.</param>
        /// <param name="gISPostgreSQLConverterManager">The manager the sheet converter is read from.</param>
        public PostgreSQLOrtoSheetUpdateTask(HttpClient? httpClient, GISPostgreSQLConverterManager? gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Gets the totals of the last run, or null before a run or when it could not start.
        /// </summary>
        public OrtoSheetUpdateResult? OrtoSheetUpdateResult { get; private set; }

        /// <summary>
        /// Gets or sets the options used when the task runs.
        /// </summary>
        public PostgreSQLOrtoSheetUpdateOptions PostgreSQLOrtoSheetUpdateOptions { get; set; } = new();

        /// <summary>
        /// Executes the import.
        /// </summary>
        /// <param name="progress">Reports the number of index years done.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every year was read in full; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            OrtoSheetUpdateResult = null;

            OrtoSheetPostgreSQLConverter? ortoSheetPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<OrtoSheetPostgreSQLConverter>();
            if (ortoSheetPostgreSQLConverter is null || httpClient is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "The orto_sheet converter or the index service client is missing - the sheet import cannot start");
                return false;
            }

            List<int> years = [];
            List<int> missingYears = [];
            List<int> mismatchedYears = [];
            long hits = 0;
            long rows = 0;
            long canonical = 0;
            long written = 0;

            for (int year = PostgreSQLOrtoSheetUpdateOptions.FirstYear; year <= PostgreSQLOrtoSheetUpdateOptions.LastYear; year++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                OrtoSheetUpdateResult? ortoSheetUpdateResult = await ortoSheetPostgreSQLConverter.UpdateOrtoSheetsAsync(httpClient, [year], PostgreSQLOrtoSheetUpdateOptions.PageSize, PostgreSQLOrtoSheetUpdateOptions.RetryCount, TimeSpan.FromSeconds(PostgreSQLOrtoSheetUpdateOptions.RetryDelaySeconds), cancellationToken: cancellationToken);
                if (ortoSheetUpdateResult is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "Sheet import of index year {Year} could not be written - the import stops", year);
                    return false;
                }

                years.Add(year);
                missingYears.AddRange(ortoSheetUpdateResult.MissingYears ?? []);
                mismatchedYears.AddRange(ortoSheetUpdateResult.MismatchedYears ?? []);
                hits += ortoSheetUpdateResult.Hits;
                rows += ortoSheetUpdateResult.Rows;
                canonical += ortoSheetUpdateResult.Canonical;
                written += ortoSheetUpdateResult.Written;

                if (ortoSheetUpdateResult.MissingYears is { Count: > 0 })
                {
                    Serilog.Modify.Log("Sheet import: index year {Year} is not published", year);
                }
                else
                {
                    Serilog.Modify.Log("Sheet import: index year {Year} - {Hits} counted, {Rows} read, {Canonical} after duplicates, {Written} written{Mismatch}", year, ortoSheetUpdateResult.Hits, ortoSheetUpdateResult.Rows, ortoSheetUpdateResult.Canonical, ortoSheetUpdateResult.Written, ortoSheetUpdateResult.MismatchedYears is { Count: > 0 } ? " - ROWS DIFFER FROM THE COUNT" : string.Empty);
                }

                progress?.Report(years.Count);
            }

            OrtoSheetUpdateResult = new OrtoSheetUpdateResult(years, missingYears, mismatchedYears, hits, rows, canonical, written);

            Serilog.Modify.Log("Sheet import done: {Years} years requested, {Missing} not published, {Mismatched} mismatched, {Hits} counted, {Rows} read, {Written} written", years.Count, missingYears.Count, mismatchedYears.Count, hits, rows, written);

            return mismatchedYears.Count == 0;
        }
    }
}
