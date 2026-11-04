using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that backfills the acquisition date, sheet and content hash of the stored images of the selected county parts from <c>orto_sheet</c> into <c>orto_data_acquisition</c> (<see cref="Modify.UpdateOrtoDatasAcquisitionAsync"/>), one county part after another.
    /// <para>Progress is the number of county parts done; every part is logged with its counters, and the totals are in <see cref="OrtoDatasAcquisitionUpdateResult"/>. Import the sheet index first (<see cref="PostgreSQLOrtoSheetUpdateTask"/>): without it every image is recorded as undated. Re-running is safe: an acquisition row that does not change is not written.</para>
    /// </summary>
    public class PostgreSQLOrtoDatasAcquisitionUpdateTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The manager the converters are read from.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoDatasAcquisitionUpdateTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The manager the <c>orto_datas</c> and <c>orto_sheet</c> converters are read from.</param>
        public PostgreSQLOrtoDatasAcquisitionUpdateTask(GISPostgreSQLConverterManager? gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the totals of the last run, or null before a run or when it could not start.
        /// </summary>
        public OrtoDatasAcquisitionUpdateResult? OrtoDatasAcquisitionUpdateResult { get; private set; }

        /// <summary>
        /// Gets or sets the options used when the task runs.
        /// </summary>
        public PostgreSQLOrtoDatasAcquisitionUpdateOptions PostgreSQLOrtoDatasAcquisitionUpdateOptions { get; set; } = new();

        /// <summary>
        /// Executes the backfill.
        /// </summary>
        /// <param name="progress">Reports the number of county parts done.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true when every county part was walked; otherwise, false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            OrtoDatasAcquisitionUpdateResult = null;

            OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<OrtoDatasPostgreSQLConverter>();
            OrtoSheetPostgreSQLConverter? ortoSheetPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<OrtoSheetPostgreSQLConverter>();
            if (ortoDatasPostgreSQLConverter is null || ortoSheetPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "The orto_datas or orto_sheet converter is missing - the acquisition backfill cannot start");
                return false;
            }

            int[] countyIds = PostgreSQLOrtoDatasAcquisitionUpdateOptions.CountyIds is null ? [] : [.. PostgreSQLOrtoDatasAcquisitionUpdateOptions.CountyIds.OrderBy(x => x)];
            if (countyIds.Length == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "No county parts were selected - the acquisition backfill has nothing to walk");
                return false;
            }

            long sheets = 0;
            long rows = 0;
            long changed = 0;
            long images = 0;
            long dated = 0;
            long done = 0;

            foreach (int countyId in countyIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                OrtoDatasAcquisitionUpdateResult? ortoDatasAcquisitionUpdateResult = await ortoDatasPostgreSQLConverter.UpdateOrtoDatasAcquisitionAsync(ortoSheetPostgreSQLConverter, [countyId], PostgreSQLOrtoDatasAcquisitionUpdateOptions.LastYear, PostgreSQLOrtoDatasAcquisitionUpdateOptions.PageSize, PostgreSQLOrtoDatasAcquisitionUpdateOptions.CommandTimeout, cancellationToken);
                if (ortoDatasAcquisitionUpdateResult is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "Acquisition backfill of county part {CountyId} could not run - the backfill stops", countyId);
                    return false;
                }

                sheets += ortoDatasAcquisitionUpdateResult.Sheets;
                rows += ortoDatasAcquisitionUpdateResult.Rows;
                changed += ortoDatasAcquisitionUpdateResult.Changed;
                images += ortoDatasAcquisitionUpdateResult.Images;
                dated += ortoDatasAcquisitionUpdateResult.Dated;

                Serilog.Modify.Log("Acquisition backfill: county part {CountyId} - {Sheets} sheets, {Rows} rows read, {Changed} written, {Images} images, {Dated} dated", countyId, ortoDatasAcquisitionUpdateResult.Sheets, ortoDatasAcquisitionUpdateResult.Rows, ortoDatasAcquisitionUpdateResult.Changed, ortoDatasAcquisitionUpdateResult.Images, ortoDatasAcquisitionUpdateResult.Dated);

                done++;
                progress?.Report(done);
            }

            OrtoDatasAcquisitionUpdateResult = new OrtoDatasAcquisitionUpdateResult(countyIds, sheets, rows, changed, images, dated);

            Serilog.Modify.Log("Acquisition backfill done: {Parts} county parts, {Rows} rows read, {Changed} written, {Images} images, {Dated} dated", countyIds.Length, rows, changed, images, dated);

            return true;
        }
    }
}
