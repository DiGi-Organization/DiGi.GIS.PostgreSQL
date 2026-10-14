using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Carries the rows of the <c>unique_id</c>-keyed tables - <c>building_model</c>, <c>year_built_data</c> and <c>occupancy_data_building_2d</c> - onto the county part their building sits on.
    /// <para>Every read of those tables filters on <c>county_id</c> first, so a row filed under a part other than the one holding its building reads back missing. The mover for those tables is <c>RefreshCountyIdsAsync</c> on their converter, and this task is its permanent production caller: unlike the temporary county part repair of issue DiGi-Organization/DiGi.GIS.PostgreSQL#68, which was deleted with a TODO marker once its estate was clean, this is the standing repair path for the rows keyed on a building, and it stays.</para>
    /// <para>Per part it reads the part's buildings from <c>building_2d</c> - the sweep covers every building of every part of the county, not only the ones a given run noticed - probes each table for the references that would move and the ones the destination part would refuse, and - unless <see cref="PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.DryRun"/> is on - moves the movable ones. A mover only touches a row sitting under a different part, so a run over a healthy county costs the reads and nothing else, and a re-run is the recovery from an interrupted one.</para>
    /// <para><b>Reports by default and writes nothing.</b> <see cref="PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.DryRun"/> defaults to true and has to be turned off deliberately; the report a dry run produces is what the move should be reviewed against.</para>
    /// <para>The report is written into <see cref="PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.ReportDirectory"/> as well as to the log: one row per stray reference in <c>Building2D_ReferencedObjects_CountyPartRefresh.csv</c> and per-code totals in <c>Building2D_ReferencedObjects_CountyPartRefresh_Summary.txt</c>. The row file is flushed per part, so a run interrupted late still leaves everything it had already reported.</para>
    /// <para>Nothing is deleted anywhere. A reference the destination part will not take - it already holds the stored object - stays where it is and is reported as blocked, for a person to settle: deleting either copy is a decision this task does not make.</para>
    /// </summary>
    public class PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The converter manager the run draws its converters from.
        /// </summary>
        private readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        /// <summary>
        /// Constructor with Dependency Injection.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager holding the converters the run reads and writes through.</param>
        public PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <summary>
        /// Gets the configuration for the run. These options are used when the task is started.
        /// </summary>
        public PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions { get; set; } = new PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions();

        /// <summary>
        /// Gets what the last run read, reported and moved.
        /// </summary>
        public PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult? PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult { get; private set; }

        /// <summary>
        /// Executes the background task, carrying the <c>unique_id</c>-keyed referenced objects of every multi-part county onto the part their building sits on.
        /// </summary>
        /// <param name="progress">A progress reporter carrying the running total of references the run moved, or would move on a dry run.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true unless the run could not be attempted, was cancelled, or stepped over a county after a failure.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult = null;

            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions = PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions ?? new PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions();

            bool dryRun = postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.DryRun;
            int batchSize = postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.BatchSize;
            int commandTimeout = postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.CommandTimeout;

            string directory = string.IsNullOrWhiteSpace(postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.ReportDirectory) ? AppContext.BaseDirectory : postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.ReportDirectory!;
            if (!Directory.Exists(directory))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "Report directory {Directory} does not exist", directory);
                return false;
            }

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();

            if (administrativeAreal2DPostgreSQLConverter is null || building2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "PostgreSQL converters could not be resolved");
                return false;
            }

            // A table the manager does not hold is skipped rather than failing the run: an installation
            // that stores no year built data has nothing to carry, and that is not an error.
            BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<YearBuiltDataPostgreSQLConverter>();
            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();

            List<AdministrativeAreal2D>? administrativeAreal2Ds = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType.County, cancellationToken: cancellationToken);
            if (administrativeAreal2Ds is null || administrativeAreal2Ds.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "County rows could not be retrieved");
                return false;
            }

            HashSet<string>? codes = postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.Codes is null ? null : [.. postgreSQLBuilding2DReferencedObjectsCountyPartRefreshOptions.Codes];

            Dictionary<string, List<AdministrativeAreal2D>> administrativeAreal2Ds_ByCode = [];
            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
            {
                string? code = administrativeAreal2D?.Code;
                if (string.IsNullOrWhiteSpace(code) || (codes is not null && !codes.Contains(code!)))
                {
                    continue;
                }

                if (!administrativeAreal2Ds_ByCode.TryGetValue(code!, out List<AdministrativeAreal2D>? administrativeAreal2Ds_Code) || administrativeAreal2Ds_Code is null)
                {
                    administrativeAreal2Ds_Code = [];
                    administrativeAreal2Ds_ByCode[code!] = administrativeAreal2Ds_Code;
                }

                administrativeAreal2Ds_Code.Add(administrativeAreal2D!);
            }

            // A named code the table does not hold would otherwise be skipped silently, and a run that
            // examines zero codes and reports success reads as though the estate were clean.
            if (codes is not null)
            {
                foreach (string code in codes.OrderBy(x => x))
                {
                    if (!administrativeAreal2Ds_ByCode.ContainsKey(code))
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Code {Code} is not a county code, nothing to examine", code);
                    }
                }
            }

            LongProgressWrapper? longProgressWrapper = Core.Create.LongProgressWrapper(progress);

            Serilog.Modify.Log("{Type} started. DryRun: {DryRun}. Codes examined: {Count}", nameof(PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask), dryRun, administrativeAreal2Ds_ByCode.Count);

            long codeCount = 0;
            long partCount = 0;
            long referenceCount = 0;
            long strayReferenceCount = 0;
            long movableReferenceCount = 0;
            long blockedReferenceCount = 0;
            long movedReferenceCount = 0;
            long failedCodeCount = 0;
            bool cancelled = false;

            List<string> summaryLines =
            [
                "Building2D referenced objects county part refresh",
                $"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"DryRun: {dryRun}",
                string.Empty,
                "Code;Parts;Stray;Movable;Blocked;Moved"
            ];

            using StreamWriter streamWriter = new(System.IO.Path.Combine(directory, "Building2D_ReferencedObjects_CountyPartRefresh.csv"), false, Encoding.UTF8);
            await streamWriter.WriteLineAsync("Code;Part;Table;Reference;State");

            // The three tables carry different type arguments, so there is no common type to loop over;
            // the shared work - probe, report, move, count - lives here, and each table is one call site.
            async Task<bool> RefreshTableAsync(string code, int countyId, string tableName, Func<Task<Building2DReferencedObjectStrayResult?>> probeAsync, Func<Task<HashSet<string>? >> moveAsync)
            {
                Building2DReferencedObjectStrayResult? strayReferences = await probeAsync();
                if (strayReferences is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Code {Code}: {Table}: the probe onto part {CountyId} could not be attempted", code, tableName, countyId);
                    return false;
                }

                long movable = strayReferences.MovableReferences.Count;
                long blocked = strayReferences.BlockedReferences.Count;

                strayReferenceCount += movable + blocked;
                movableReferenceCount += movable;

                // One row per stray reference: the review of what the move will touch and what it will
                // refuse, written before a live run writes anything.
                foreach (string reference in strayReferences.MovableReferences.OrderBy(x => x))
                {
                    await streamWriter.WriteLineAsync($"{code};{countyId};{tableName};{reference};movable");
                }

                foreach (string reference in strayReferences.BlockedReferences.OrderBy(x => x))
                {
                    await streamWriter.WriteLineAsync($"{code};{countyId};{tableName};{reference};blocked");
                }

                if (movable != 0 || blocked != 0)
                {
                    Serilog.Modify.Log("{Table}: code {Code}, part {CountyId}: {Movable} would move, {Blocked} refused by the destination", tableName, code, countyId, movable, blocked);
                }

                if (dryRun)
                {
                    blockedReferenceCount += blocked;
                    longProgressWrapper?.Increment(movable);
                    return true;
                }

                HashSet<string>? references_Moved = await moveAsync();
                if (references_Moved is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Code {Code}: {Table}: the move onto part {CountyId} could not be attempted", code, tableName, countyId);
                    return false;
                }

                long moved = references_Moved.Count;
                movedReferenceCount += moved;
                longProgressWrapper?.Increment(moved);

                // The probe and the mover apply the same rule to the same data, so what the probe found
                // but the move did not report is what the destination refused. If the two statements ever
                // drift, the differential Fact in DiGi.Test fails before this arithmetic hides it.
                blocked = Math.Max(0, movable + blocked - moved);
                blockedReferenceCount += blocked;

                if (blocked != 0)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Code {Code}: {Table}: {Blocked} references could not move onto part {CountyId} - it already holds the stored object", code, tableName, blocked, countyId);
                }

                return true;
            }

            try
            {
                foreach (string code in administrativeAreal2Ds_ByCode.Keys.OrderBy(x => x))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<AdministrativeAreal2D> administrativeAreal2Ds_Code = administrativeAreal2Ds_ByCode[code];
                    administrativeAreal2Ds_Code.Sort((x, y) => x.Id.CompareTo(y.Id));

                    // One part cannot hold a building's row under the wrong one of itself. A named code
                    // with a single part is still examined, because the row can sit under another code's
                    // part - the option's contract.
                    if (codes is null && administrativeAreal2Ds_Code.Count < 2)
                    {
                        continue;
                    }

                    codeCount++;

                    List<int> countyIds_Code = administrativeAreal2Ds_Code.ConvertAll(x => x.Id);

                    bool failed_Code = false;

                    long strayReferenceCount_Before = strayReferenceCount;
                    long movableReferenceCount_Before = movableReferenceCount;
                    long blockedReferenceCount_Before = blockedReferenceCount;
                    long movedReferenceCount_Before = movedReferenceCount;

                    // One county failing does not end the run. The counties are independent of each other,
                    // and a mover only touches a row sitting under a different part, so a county stepped
                    // over here is finished by running the task again rather than by starting from nothing.
                    try
                    {
                        foreach (int countyId in countyIds_Code)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            partCount++;

                            List<Building2DReference>? building2DReferences = await building2DPostgreSQLConverter.GetBuilding2DReferencesByCountyIdAsync(countyId, cancellationToken: cancellationToken);
                            if (building2DReferences is null)
                            {
                                failed_Code = true;
                                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Code {Code}: the buildings of part {CountyId} could not be read", code, countyId);
                                break;
                            }

                            List<string> references_Part = [];
                            foreach (Building2DReference building2DReference in building2DReferences)
                            {
                                if (!string.IsNullOrWhiteSpace(building2DReference?.Reference))
                                {
                                    references_Part.Add(building2DReference.Reference!);
                                }
                            }

                            if (references_Part.Count == 0)
                            {
                                Serilog.Modify.Log("Code {Code}: part {CountyId} holds no buildings, nothing to carry", code, countyId);
                                continue;
                            }

                            referenceCount += references_Part.Count;

                            // Naming the parts the rows can be sitting under is what prunes the probe and
                            // the move to this county instead of scanning every partition of the table.
                            // A named code with a single part names no sibling, and its rows can sit under
                            // another code's part - so it searches every part rather than none.
                            List<int> countyIds_Source_List = countyIds_Code.FindAll(x => x != countyId);
                            IEnumerable<int>? countyIds_Source = countyIds_Source_List.Count > 0 ? countyIds_Source_List : null;

                            if (buildingModelPostgreSQLConverter is not null)
                            {
                                if (!await RefreshTableAsync(code, countyId, buildingModelPostgreSQLConverter.TableName,
                                    () => buildingModelPostgreSQLConverter.GetStrayReferencesAsync(references_Part, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken: cancellationToken),
                                    () => buildingModelPostgreSQLConverter.RefreshCountyIdsAsync(references_Part, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken: cancellationToken)))
                                {
                                    failed_Code = true;
                                }
                            }

                            if (yearBuiltDataPostgreSQLConverter is not null)
                            {
                                if (!await RefreshTableAsync(code, countyId, yearBuiltDataPostgreSQLConverter.TableName,
                                    () => yearBuiltDataPostgreSQLConverter.GetStrayReferencesAsync(references_Part, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken: cancellationToken),
                                    () => yearBuiltDataPostgreSQLConverter.RefreshCountyIdsAsync(references_Part, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken: cancellationToken)))
                                {
                                    failed_Code = true;
                                }
                            }

                            if (building2DOccupancyDataPostgreSQLConverter is not null)
                            {
                                if (!await RefreshTableAsync(code, countyId, building2DOccupancyDataPostgreSQLConverter.TableName,
                                    () => building2DOccupancyDataPostgreSQLConverter.GetStrayReferencesAsync(references_Part, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken: cancellationToken),
                                    () => building2DOccupancyDataPostgreSQLConverter.RefreshCountyIdsAsync(references_Part, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken: cancellationToken)))
                                {
                                    failed_Code = true;
                                }
                            }

                            await streamWriter.FlushAsync(cancellationToken);
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        failed_Code = true;
                        Serilog.Modify.Log(exception, "Code {Code}: the run failed, county stepped over", code);
                    }

                    if (failed_Code)
                    {
                        failedCodeCount++;

                        // A county stepped over leaves the run incomplete, and a run that reports success
                        // is a run nobody goes back to.
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "Code {Code}: stepped over after a failure; running the task again finishes it", code);
                    }

                    long strayReferenceCount_Code = strayReferenceCount - strayReferenceCount_Before;
                    long movableReferenceCount_Code = movableReferenceCount - movableReferenceCount_Before;
                    long blockedReferenceCount_Code = blockedReferenceCount - blockedReferenceCount_Before;
                    long movedReferenceCount_Code = movedReferenceCount - movedReferenceCount_Before;

                    Serilog.Modify.Log("Code {Code}: parts {Parts}, stray {Stray}, movable {Movable}, blocked {Blocked}, moved {Moved}", code, string.Join(", ", countyIds_Code), strayReferenceCount_Code, movableReferenceCount_Code, blockedReferenceCount_Code, movedReferenceCount_Code);

                    summaryLines.Add($"{code};{string.Join(" ", countyIds_Code)};{strayReferenceCount_Code};{movableReferenceCount_Code};{blockedReferenceCount_Code};{movedReferenceCount_Code}");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                cancelled = true;
            }

            summaryLines.Add(string.Empty);
            summaryLines.Add($"Codes examined: {codeCount}");
            summaryLines.Add($"Parts examined: {partCount}");
            summaryLines.Add($"Buildings read: {referenceCount}");
            summaryLines.Add($"Stray references: {strayReferenceCount}");
            summaryLines.Add($"Movable references: {movableReferenceCount}");
            summaryLines.Add($"Blocked references: {blockedReferenceCount}");
            summaryLines.Add($"Moved references: {movedReferenceCount}");
            summaryLines.Add($"Codes stepped over after a failure: {failedCodeCount}");
            summaryLines.Add($"Ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{(cancelled ? " after being cancelled" : string.Empty)}");

            await streamWriter.FlushAsync(CancellationToken.None);

            await File.WriteAllLinesAsync(System.IO.Path.Combine(directory, "Building2D_ReferencedObjects_CountyPartRefresh_Summary.txt"), summaryLines, CancellationToken.None);

            PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult = new PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshResult(codeCount, partCount, referenceCount, strayReferenceCount, movableReferenceCount, blockedReferenceCount, movedReferenceCount, failedCodeCount, cancelled);

            Serilog.Modify.Log(
                cancelled || failedCodeCount != 0 ? Serilog.Enums.LogEventLevel.Warning : Serilog.Enums.LogEventLevel.Information,
                "{Type} ended{Cancelled}. DryRun: {DryRun}. Codes {Codes}, parts {Parts}, buildings {References}, stray {Stray}, movable {Movable}, blocked {Blocked}, moved {Moved}, codes stepped over {Failed}. Report written to {Directory}",
                nameof(PostgreSQLBuilding2DReferencedObjectsCountyPartRefreshTask), cancelled ? " after being cancelled" : string.Empty, dryRun, codeCount, partCount, referenceCount, strayReferenceCount, movableReferenceCount, blockedReferenceCount, movedReferenceCount, failedCodeCount, directory);

            // Cancellation and a failed county are both reasons to look at the log.
            return !cancelled && failedCodeCount == 0;
        }
    }
}
