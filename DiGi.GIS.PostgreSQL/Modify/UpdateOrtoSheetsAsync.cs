using DiGi.GIS.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Imports the GUGiK orthophoto sheet index into <c>orto_sheet</c>, one index year (<c>SkorowidzOrtofomapy&lt;year&gt;</c>) after another.
        /// <para>Each year is counted (<see cref="GIS.Query.OrtoSheetCountAsync"/>), read in full (<see cref="GIS.Query.OrtoSheetsAsync"/>), reduced to the canonical row of each duplicate group within the year (<see cref="GIS.Query.OrtoSheetDuplicateGroups"/>), and stored (<see cref="OrtoSheetPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection?, IEnumerable{OrtoSheet?}?, int, int, CancellationToken)"/>). Duplicates across years are removed by the table itself: it is unique on the download file and keeps the earliest listing. Only re-registrations of one image under different files in different years survive (324 groups in the national index of 2026-11-03); they carry the same date and do not change any acquisition date.</para>
        /// <para>A year the service answers no count for is recorded as missing and skipped; a year whose rows read differ from its count, or cannot be read, is recorded as mismatched, and what was read is stored. Re-running the import refreshes every row. One year at a time keeps memory to one year's rows (124 074 in 2015, the largest); a caller reporting progress calls it once per year. The converter reads and writes the Storage database.</para>
        /// </summary>
        /// <param name="ortoSheetPostgreSQLConverter">The converter of the sheet table.</param>
        /// <param name="httpClient">The HTTP client the index service is read with.</param>
        /// <param name="years">The index years to import.</param>
        /// <param name="pageSize">The number of rows per WFS page.</param>
        /// <param name="retryCount">The number of times a transient failure is retried.</param>
        /// <param name="retryDelay">The delay before the first retry.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>A task that represents the asynchronous operation. The task result counts the years and rows, or is null when the converter, the client or the years are missing, or a write could not run.</returns>
        public static async Task<OrtoSheetUpdateResult?> UpdateOrtoSheetsAsync(
            this OrtoSheetPostgreSQLConverter? ortoSheetPostgreSQLConverter,
            HttpClient? httpClient,
            IEnumerable<int>? years,
            int pageSize,
            int retryCount,
            TimeSpan retryDelay,
            int batchSize = 1000,
            int commandTimeout = 120,
            CancellationToken cancellationToken = default)
        {
            int[] years_Array = years is null ? [] : [.. years.Distinct().OrderBy(x => x)];
            if (ortoSheetPostgreSQLConverter is null || httpClient is null || years_Array.Length == 0)
            {
                return null;
            }

            List<int> missingYears = [];
            List<int> mismatchedYears = [];
            long hits = 0;
            long rows = 0;
            long canonical = 0;
            long written = 0;

            foreach (int year in years_Array)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string typeName = GIS.Query.OrtoSheetTypeName(year);

                int? count = await GIS.Query.OrtoSheetCountAsync(httpClient, typeName, null, retryCount, retryDelay, cancellationToken);
                if (count is null)
                {
                    missingYears.Add(year);
                    continue;
                }

                hits += count.Value;
                if (count.Value == 0)
                {
                    continue;
                }

                List<OrtoSheet>? ortoSheets = await GIS.Query.OrtoSheetsAsync(httpClient, typeName, null, pageSize, retryCount, retryDelay, DateTimeOffset.UtcNow, cancellationToken);
                if (ortoSheets is null || ortoSheets.Count != count.Value)
                {
                    mismatchedYears.Add(year);
                }

                if (ortoSheets is null || ortoSheets.Count == 0)
                {
                    continue;
                }

                rows += ortoSheets.Count;

                List<OrtoSheet> ortoSheets_Canonical = [.. (GIS.Query.OrtoSheetDuplicateGroups(ortoSheets) ?? []).Select(x => x[0])];
                canonical += ortoSheets_Canonical.Count;

                long? written_Year = await ortoSheetPostgreSQLConverter.UpdateAsync(ortoSheets_Canonical, batchSize, commandTimeout, cancellationToken);
                if (written_Year is null)
                {
                    return null;
                }

                written += written_Year.Value;
            }

            return new OrtoSheetUpdateResult(years_Array, missingYears, mismatchedYears, hits, rows, canonical, written);
        }
    }
}
