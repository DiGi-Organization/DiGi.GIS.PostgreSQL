using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using Npgsql;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a background task that backfills the acquisition date, sheet and content hash of the stored images of the selected county parts from <c>orto_sheet</c> into <c>orto_data_acquisition</c> (<see cref="Modify.UpdateOrtoDatasAcquisitionAsync"/>), one county part after another or several at once (DiGi.GIS.PostgreSQL#177).
    /// <para>Progress is the number of county parts done; every part is logged with its counters, and the totals are in <see cref="OrtoDatasAcquisitionUpdateResult"/>. Import the sheet index first (<see cref="PostgreSQLOrtoSheetUpdateTask"/>): without it every image is recorded as undated. Re-running is safe: an acquisition row that does not change is not written.</para>
    /// <para>How many parts walk at once comes from <see cref="PostgreSQLOrtoDatasAcquisitionUpdateOptions.MaxDegreeOfParallelism"/>: one walks them one after another exactly as before, a higher degree walks several parts concurrently - each part still runs inside its own Modify.UpdateOrtoDatasAcquisitionAsync call with its own read, write and sheet connections, the side table and every partition created before the workers start, the counters accumulated with Interlocked and the progress reported under a lock. The stored rows do not depend on the degree; memory grows with the degree times the page size.</para>
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

            int degreeOfParallelism = PostgreSQLOrtoDatasAcquisitionUpdateOptions.MaxDegreeOfParallelism > 0
                ? PostgreSQLOrtoDatasAcquisitionUpdateOptions.MaxDegreeOfParallelism
                : Math.Max(1, Core.Query.ProcessorCount(0.5));
            degreeOfParallelism = Math.Min(degreeOfParallelism, countyIds.Length);

            Serilog.Modify.Log(
                "Acquisition backfill starting: {Parts} county parts, last year {LastYear}, page size {PageSize}, max degree of parallelism {MaxDegreeOfParallelism} (used {DegreeOfParallelism})",
                countyIds.Length,
                PostgreSQLOrtoDatasAcquisitionUpdateOptions.LastYear,
                PostgreSQLOrtoDatasAcquisitionUpdateOptions.PageSize,
                PostgreSQLOrtoDatasAcquisitionUpdateOptions.MaxDegreeOfParallelism,
                degreeOfParallelism);

            if (degreeOfParallelism == 1)
            {
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
            }
            else
            {
                // The side table and every part's partition are created before the workers start: CREATE TABLE IF NOT
                // EXISTS from several workers at once can race on the catalog of the shared parent (the same race
                // DiGi.GIS.PostgreSQL#154 guards its first county push against). Each part then walks inside its own
                // Modify call, which opens its own read, write and sheet connections - the workers share only the
                // counters below.
                await using (NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ortoDatasPostgreSQLConverter.ConnectionData))
                {
                    if (npgsqlConnection is null)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "The storage connection could not be opened - the acquisition backfill cannot start");
                        return false;
                    }

                    await npgsqlConnection.OpenAsync(cancellationToken);

                    if (!await Create.TableAsync_OrtoDataAcquisition(npgsqlConnection, PostgreSQLOrtoDatasAcquisitionUpdateOptions.CommandTimeout, cancellationToken))
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "The orto_data_acquisition table could not be created - the acquisition backfill cannot start");
                        return false;
                    }

                    foreach (int countyId in countyIds)
                    {
                        if (!await Create.TableAsync_OrtoDataAcquisition_Partition(npgsqlConnection, countyId, PostgreSQLOrtoDatasAcquisitionUpdateOptions.CommandTimeout, cancellationToken))
                        {
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "The acquisition partition of county part {CountyId} could not be created - the acquisition backfill cannot start", countyId);
                            return false;
                        }
                    }
                }

                // 1 once a part has failed: the parts not yet started then step over and the workers in flight stop at
                // their next page, so the run ends with today's false return and every row written so far stays.
                int failed = 0;

                // Held while a part adds its counters and reports the progress, so the reported total never goes back.
                Lock progressLock = new();

                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                try
                {
                    await Parallel.ForEachAsync(
                        countyIds,
                        new ParallelOptions() { MaxDegreeOfParallelism = degreeOfParallelism, CancellationToken = cancellationTokenSource.Token },
                        async (countyId, token) =>
                        {
                            if (Volatile.Read(ref failed) != 0)
                            {
                                return;
                            }

                            try
                            {
                                OrtoDatasAcquisitionUpdateResult? ortoDatasAcquisitionUpdateResult = await ortoDatasPostgreSQLConverter.UpdateOrtoDatasAcquisitionAsync(ortoSheetPostgreSQLConverter, [countyId], PostgreSQLOrtoDatasAcquisitionUpdateOptions.LastYear, PostgreSQLOrtoDatasAcquisitionUpdateOptions.PageSize, PostgreSQLOrtoDatasAcquisitionUpdateOptions.CommandTimeout, token);
                                if (ortoDatasAcquisitionUpdateResult is null)
                                {
                                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "Acquisition backfill of county part {CountyId} could not run - the backfill stops", countyId);
                                    Interlocked.Exchange(ref failed, 1);
                                    cancellationTokenSource.Cancel();
                                    return;
                                }

                                Interlocked.Add(ref sheets, ortoDatasAcquisitionUpdateResult.Sheets);
                                Interlocked.Add(ref rows, ortoDatasAcquisitionUpdateResult.Rows);
                                Interlocked.Add(ref changed, ortoDatasAcquisitionUpdateResult.Changed);
                                Interlocked.Add(ref images, ortoDatasAcquisitionUpdateResult.Images);
                                Interlocked.Add(ref dated, ortoDatasAcquisitionUpdateResult.Dated);

                                Serilog.Modify.Log("Acquisition backfill: county part {CountyId} - {Sheets} sheets, {Rows} rows read, {Changed} written, {Images} images, {Dated} dated", countyId, ortoDatasAcquisitionUpdateResult.Sheets, ortoDatasAcquisitionUpdateResult.Rows, ortoDatasAcquisitionUpdateResult.Changed, ortoDatasAcquisitionUpdateResult.Images, ortoDatasAcquisitionUpdateResult.Dated);

                                lock (progressLock)
                                {
                                    done++;
                                    progress?.Report(done);
                                }
                            }
                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                            {
                                // A stop of the whole run - surfaced as the cancellation it is (Coding - General.md §1.22).
                                throw;
                            }
                            catch (OperationCanceledException)
                            {
                                // Another part failed and cancelled this run - stop quietly at the next page boundary.
                            }
                            catch (Exception exception)
                            {
                                Serilog.Modify.Log(exception, "Acquisition backfill of county part {CountyId} failed - the backfill stops", countyId);
                                Interlocked.Exchange(ref failed, 1);
                                cancellationTokenSource.Cancel();
                            }
                        });
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    // Another part failed and cancelled this run.
                }

                if (Volatile.Read(ref failed) != 0)
                {
                    return false;
                }
            }

            OrtoDatasAcquisitionUpdateResult = new OrtoDatasAcquisitionUpdateResult(countyIds, sheets, rows, changed, images, dated);

            Serilog.Modify.Log("Acquisition backfill done: {Parts} county parts, {Rows} rows read, {Changed} written, {Images} images, {Dated} dated", countyIds.Length, rows, changed, images, dated);

            return true;
        }
    }
}
