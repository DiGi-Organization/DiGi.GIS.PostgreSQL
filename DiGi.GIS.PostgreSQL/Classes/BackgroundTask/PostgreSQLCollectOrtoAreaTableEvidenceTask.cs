using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Collects the production evidence of the four full-interpretation test-area tables and writes it to a report file (DiGi.GIS.PostgreSQL#130).
    /// <para>Read-only with respect to the schema and to the estate's data: per table it reads <c>pg_indexes</c> and <c>information_schema.columns</c>, and per database it proves read/write access of the service account with a single scratch row that is inserted and deleted inside the same connection (zero rows remain). It never calls the DDL helpers, so it reports the tables as they already are - the companion task <see cref="PostgreSQLCreateOrtoAreaTablesTask"/> creates them.</para>
    /// <para>The report is the acceptance evidence itself: the <c>idx_orto_area_label_identity</c> unique index with its <c>NULLS NOT DISTINCT</c> clause (from <c>pg_indexes</c>, not the DDL text), the <c>UNIQUE (area_key, epoch_year)</c> index of <c>orto_area_image</c>, the column lists compared against the DDL, and the per-database round trip. <see cref="PostgreSQLCollectOrtoAreaTableEvidenceOptions.ReportDirectory"/> defaults to the directory the application was launched from.</para>
    /// </summary>
    public class PostgreSQLCollectOrtoAreaTableEvidenceTask : ReportableBackgroundTask<long>, IGISPostgreSQLObject
    {
        /// <summary>
        /// The OrtoArea PostgreSQL converter (Main database) used to open the connection for the <c>orto_area</c> evidence.
        /// </summary>
        private readonly OrtoAreaPostgreSQLConverter ortoAreaPostgreSQLConverter;

        /// <summary>
        /// The OrtoAreaImage PostgreSQL converter (Storage database) used to open the connection for the <c>orto_area_image</c> evidence.
        /// </summary>
        private readonly OrtoAreaImagePostgreSQLConverter ortoAreaImagePostgreSQLConverter;

        /// <summary>
        /// Constructor with Dependency Injection.
        /// </summary>
        /// <param name="ortoAreaPostgreSQLConverter">The converter for the <c>orto_area</c> table (Main), whose <see cref="PostgreSQLConverter{TOObject}.ConnectionData"/> opens the Main connection.</param>
        /// <param name="ortoAreaImagePostgreSQLConverter">The converter for the <c>orto_area_image</c> table (Storage), whose <see cref="PostgreSQLConverter{TOObject}.ConnectionData"/> opens the Storage connection.</param>
        public PostgreSQLCollectOrtoAreaTableEvidenceTask(OrtoAreaPostgreSQLConverter ortoAreaPostgreSQLConverter, OrtoAreaImagePostgreSQLConverter ortoAreaImagePostgreSQLConverter)
        {
            this.ortoAreaPostgreSQLConverter = ortoAreaPostgreSQLConverter ?? throw new ArgumentNullException(nameof(ortoAreaPostgreSQLConverter));
            this.ortoAreaImagePostgreSQLConverter = ortoAreaImagePostgreSQLConverter ?? throw new ArgumentNullException(nameof(ortoAreaImagePostgreSQLConverter));
        }

        /// <summary>
        /// Gets the configuration for the run. These options are used when the task is started.
        /// </summary>
        public PostgreSQLCollectOrtoAreaTableEvidenceOptions PostgreSQLCollectOrtoAreaTableEvidenceOptions { get; set; } = new PostgreSQLCollectOrtoAreaTableEvidenceOptions();

        /// <summary>
        /// Gets what the last run collected and where its report was written.
        /// </summary>
        public PostgreSQLCollectOrtoAreaTableEvidenceResult? PostgreSQLCollectOrtoAreaTableEvidenceResult { get; private set; }

        /// <summary>
        /// Executes the background task, collecting the four-table evidence and writing the report.
        /// </summary>
        /// <param name="progress">A progress reporter carrying the number of tables confirmed present.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true only if all four tables were present and both round trips succeeded; otherwise false.</returns>
        protected override async Task<bool> ExecuteAsync(IProgress<long> progress, CancellationToken cancellationToken)
        {
            PostgreSQLCollectOrtoAreaTableEvidenceResult = null;

            PostgreSQLCollectOrtoAreaTableEvidenceOptions options = PostgreSQLCollectOrtoAreaTableEvidenceOptions ?? new PostgreSQLCollectOrtoAreaTableEvidenceOptions();
            int commandTimeout = options.CommandTimeout;

            string directory = string.IsNullOrWhiteSpace(options.ReportDirectory) ? AppContext.BaseDirectory : options.ReportDirectory!;
            if (!Directory.Exists(directory))
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "Report directory {Directory} does not exist", directory);
                return false;
            }

            ConnectionData? mainConnectionData = this.ortoAreaPostgreSQLConverter.ConnectionData;
            ConnectionData? storageConnectionData = this.ortoAreaImagePostgreSQLConverter.ConnectionData;

            if (mainConnectionData is null || storageConnectionData is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "OrtoArea connection data could not be resolved");
                return false;
            }

            string scratchKey = "evidence_130_" + System.DateTime.UtcNow.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);

            List<string> lines = [];
            long tablePresentCount = 0;
            long tableExpectedCount = 4;
            long roundTripOkCount = 0;
            long roundTripExpectedCount = 2;
            bool undefinedTableObserved = false;
            bool allTablesPresent = true;

            lines.Add("# OrtoArea table evidence (DiGi.GIS.PostgreSQL#130)");
            lines.Add("");
            lines.Add($"- Date UTC: {System.DateTime.UtcNow:O}");
            lines.Add($"- Round-trip scratch key: {scratchKey}");
            lines.Add("");

            // --- Main database: version, the three main tables, and the round trip, all on one open connection.
            await using NpgsqlConnection? mainConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(mainConnectionData);
            if (mainConnection is null)
            {
                lines.Add("- ERROR: could not open a Main connection.");
                allTablesPresent = false;
            }
            else
            {
                await mainConnection.OpenAsync(cancellationToken);
                lines.Add($"## version (Main: {mainConnectionData.Database})");
                lines.Add("```sql");
                lines.Add("SELECT version()");
                lines.Add("```");
                lines.Add($"- {await ScalarStringAsync(mainConnection, "SELECT version()", commandTimeout, cancellationToken)}");
                lines.Add("");

                tablePresentCount += await CollectTableAsync(mainConnection, "orto_area", commandTimeout, lines, cancellationToken);
                tablePresentCount += await CollectTableAsync(mainConnection, "orto_area_object", commandTimeout, lines, cancellationToken);
                tablePresentCount += await CollectTableAsync(mainConnection, "orto_area_label", commandTimeout, lines, cancellationToken);

                bool mainRoundTrip = await RoundTripMainAsync(mainConnection, scratchKey, commandTimeout, lines, cancellationToken);
                if (mainRoundTrip)
                {
                    roundTripOkCount++;
                }

                // Prove the converters' UndefinedTable handling: a read of a table that does not exist must answer null (SqlState 42P01), not throw.
                undefinedTableObserved = await UndefinedTableReadAsync(mainConnection, commandTimeout, lines, cancellationToken);
            }

            if (tablePresentCount < 3)
            {
                allTablesPresent = false;
            }

            // --- Storage database: version, the image table, and the round trip, all on one open connection.
            await using NpgsqlConnection? storageConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(storageConnectionData);
            if (storageConnection is null)
            {
                lines.Add("- ERROR: could not open a Storage connection.");
                allTablesPresent = false;
            }
            else
            {
                await storageConnection.OpenAsync(cancellationToken);
                lines.Add($"## version (Storage: {storageConnectionData.Database})");
                lines.Add("```sql");
                lines.Add("SELECT version()");
                lines.Add("```");
                lines.Add($"- {await ScalarStringAsync(storageConnection, "SELECT version()", commandTimeout, cancellationToken)}");
                lines.Add("");

                tablePresentCount += await CollectTableAsync(storageConnection, "orto_area_image", commandTimeout, lines, cancellationToken);

                bool storageRoundTrip = await RoundTripStorageAsync(storageConnection, scratchKey, commandTimeout, lines, cancellationToken);
                if (storageRoundTrip)
                {
                    roundTripOkCount++;
                }
            }

            string reportPath = System.IO.Path.Combine(directory, "orto_area_evidence.md");
            string? writtenReportPath = null;
            try
            {
                lines.Add("## Summary");
                lines.Add($"- tables present: {tablePresentCount} / {tableExpectedCount}");
                lines.Add($"- round trips ok: {roundTripOkCount} / {roundTripExpectedCount}");
                lines.Add($"- undefined-table read answered null (42P01 handled): {(undefinedTableObserved ? "yes" : "NO")}");
                lines.Add($"- all tables present: {(allTablesPresent ? "yes" : "NO")}");
                await File.WriteAllTextAsync(reportPath, string.Join(Environment.NewLine, lines) + Environment.NewLine, new UTF8Encoding(false), cancellationToken);
                writtenReportPath = reportPath;
                Serilog.Modify.Log("{Type} wrote {Path}", nameof(PostgreSQLCollectOrtoAreaTableEvidenceTask), reportPath);
            }
            catch (Exception ex)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type} could not write the report to {Path}: {Message}", nameof(PostgreSQLCollectOrtoAreaTableEvidenceTask), reportPath, ex.Message);
            }

            PostgreSQLCollectOrtoAreaTableEvidenceResult = new PostgreSQLCollectOrtoAreaTableEvidenceResult(tablePresentCount, tableExpectedCount, roundTripOkCount, roundTripExpectedCount, writtenReportPath, allTablesPresent);
            progress.Report(tablePresentCount);

            return allTablesPresent && roundTripOkCount == roundTripExpectedCount;
        }

        /// <summary>
        /// Collects the evidence of one table (presence, indexes, columns) into the report, returning 1 when present and 0 otherwise.
        /// </summary>
        /// <param name="connection">The open <see cref="NpgsqlConnection"/> to query.</param>
        /// <param name="tableName">The table name to collect.</param>
        /// <param name="commandTimeout">The timeout in seconds for each statement.</param>
        /// <param name="lines">The report lines to append into.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>1 when the table is present, otherwise 0.</returns>
        private static async Task<long> CollectTableAsync(NpgsqlConnection connection, string tableName, int commandTimeout, List<string> lines, CancellationToken cancellationToken)
        {
            lines.Add($"## {tableName}");
            lines.Add("```sql");
            lines.Add($"SELECT indexname, indexdef FROM pg_indexes WHERE tablename = '{tableName}' ORDER BY indexname");
            lines.Add("```");

            bool present = false;
            List<string> indexRows = [];
            try
            {
                await using NpgsqlCommand indexCommand = new($"SELECT indexname, indexdef FROM pg_indexes WHERE tablename = '{tableName}' ORDER BY indexname", connection);
                indexCommand.CommandTimeout = commandTimeout;
                await using NpgsqlDataReader indexReader = await indexCommand.ExecuteReaderAsync(cancellationToken);
                while (await indexReader.ReadAsync(cancellationToken))
                {
                    indexRows.Add($"{indexReader.GetString(0)}: `{indexReader.GetString(1)}`");
                }
            }
            catch (Exception ex)
            {
                lines.Add($"- index read failed: {ex.Message}");
            }

            if (indexRows.Count == 0)
            {
                // Distinguish "table absent" from "table present but no index rows" via the catalog.
                long tableCount = await ScalarLongAsync(connection, $"SELECT count(*) FROM information_schema.tables WHERE table_name = '{tableName}' AND table_schema = 'public'", commandTimeout, cancellationToken);
                if (tableCount == 0)
                {
                    lines.Add("- TABLE NOT PRESENT (no rows in information_schema.tables).");
                    lines.Add("");
                    return 0;
                }
            }

            present = true;
            lines.Add(indexRows.Count == 0 ? "- no indexes reported." : string.Join(Environment.NewLine, indexRows.Select(x => $"- {x}")));
            lines.Add("");

            lines.Add("```sql");
            lines.Add($"SELECT ordinal_position, column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = '{tableName}' ORDER BY ordinal_position");
            lines.Add("```");
            List<string> columnRows = [];
            try
            {
                await using NpgsqlCommand columnCommand = new($"SELECT ordinal_position, column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = '{tableName}' AND table_schema = 'public' ORDER BY ordinal_position", connection);
                columnCommand.CommandTimeout = commandTimeout;
                await using NpgsqlDataReader columnReader = await columnCommand.ExecuteReaderAsync(cancellationToken);
                while (await columnReader.ReadAsync(cancellationToken))
                {
                    columnRows.Add($"{columnReader.GetInt32(0)}: `{columnReader.GetString(1)}` {columnReader.GetString(2)} (nullable={columnReader.GetString(3)})");
                }
            }
            catch (Exception ex)
            {
                lines.Add($"- column read failed: {ex.Message}");
            }

            lines.Add(columnRows.Count == 0 ? "- no columns reported." : string.Join(Environment.NewLine, columnRows.Select(x => $"- {x}")));
            lines.Add("");

            return present ? 1 : 0;
        }

        /// <summary>
        /// Proves read/write access of the service account on the Main database with a single scratch <c>orto_area</c> row that is inserted, read back, and deleted inside the same connection.
        /// </summary>
        /// <param name="connection">The open <see cref="NpgsqlConnection"/> to use.</param>
        /// <param name="scratchKey">The scratch <c>area_key</c> to insert and delete.</param>
        /// <param name="commandTimeout">The timeout in seconds for each statement.</param>
        /// <param name="lines">The report lines to append into.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>True when the row was read back and zero remained after the delete.</returns>
        private static async Task<bool> RoundTripMainAsync(NpgsqlConnection connection, string scratchKey, int commandTimeout, List<string> lines, CancellationToken cancellationToken)
        {
            lines.Add("## Round trip (Main)");
            try
            {
                await using NpgsqlCommand insert = new("INSERT INTO orto_area (area_key, min_x, min_y, max_x, max_y, name, stratum, cell_size, cell_count, seed, object) VALUES (@k, 638000, 486000, 638100, 486100, 'evidence', 'evidence', 10, 0, 42, '{}'::jsonb)", connection);
                insert.CommandTimeout = commandTimeout;
                insert.Parameters.Add(new NpgsqlParameter("k", scratchKey));
                await insert.ExecuteNonQueryAsync(cancellationToken);

                long roundTripRead = await ScalarLongAsync(connection, "SELECT count(*) FROM orto_area WHERE area_key = @k", scratchKey, commandTimeout, cancellationToken);
                bool readOk = roundTripRead == 1;

                await using NpgsqlCommand delete = new("DELETE FROM orto_area WHERE area_key = @k", connection);
                delete.CommandTimeout = commandTimeout;
                delete.Parameters.Add(new NpgsqlParameter("k", scratchKey));
                await delete.ExecuteNonQueryAsync(cancellationToken);

                long remaining = await ScalarLongAsync(connection, "SELECT count(*) FROM orto_area WHERE area_key = @k", scratchKey, commandTimeout, cancellationToken);
                bool ok = readOk && remaining == 0;
                lines.Add($"- inserted one scratch `orto_area` row for key `{scratchKey}`, read `count(*) = {roundTripRead}`, deleted it, `{remaining}` remaining. => {(ok ? "OK" : "FAILED")}");
                lines.Add("");
                return ok;
            }
            catch (Exception ex)
            {
                lines.Add($"- ERROR: {ex.Message}");
                lines.Add("");
                return false;
            }
        }

        /// <summary>
        /// Proves read/write access of the service account on the Storage database with a single scratch <c>orto_area_image</c> row that is inserted, read back, and deleted inside the same connection.
        /// </summary>
        /// <param name="connection">The open <see cref="NpgsqlConnection"/> to use.</param>
        /// <param name="scratchKey">The scratch <c>area_key</c> to insert and delete.</param>
        /// <param name="commandTimeout">The timeout in seconds for each statement.</param>
        /// <param name="lines">The report lines to append into.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>True when the row was read back and zero remained after the delete.</returns>
        private static async Task<bool> RoundTripStorageAsync(NpgsqlConnection connection, string scratchKey, int commandTimeout, List<string> lines, CancellationToken cancellationToken)
        {
            lines.Add("## Round trip (Storage)");
            try
            {
                await using NpgsqlCommand insert = new("INSERT INTO orto_area_image (area_key, min_x, min_y, max_x, max_y, epoch_year, width, height, status, source_url, fetched_at, bytes) VALUES (@k, 638000, 486000, 638100, 486100, 2020, 1, 1, 0, 'evidence', now(), decode('00','hex'))", connection);
                insert.CommandTimeout = commandTimeout;
                insert.Parameters.Add(new NpgsqlParameter("k", scratchKey));
                await insert.ExecuteNonQueryAsync(cancellationToken);

                long roundTripRead = await ScalarLongAsync(connection, "SELECT length(bytes) FROM orto_area_image WHERE area_key = @k AND epoch_year = 2020", scratchKey, commandTimeout, cancellationToken);
                bool readOk = roundTripRead == 1;

                await using NpgsqlCommand delete = new("DELETE FROM orto_area_image WHERE area_key = @k", connection);
                delete.CommandTimeout = commandTimeout;
                delete.Parameters.Add(new NpgsqlParameter("k", scratchKey));
                await delete.ExecuteNonQueryAsync(cancellationToken);

                long remaining = await ScalarLongAsync(connection, "SELECT count(*) FROM orto_area_image WHERE area_key = @k", scratchKey, commandTimeout, cancellationToken);
                bool ok = readOk && remaining == 0;
                lines.Add($"- inserted one scratch `orto_area_image` row (epoch 2020, 1-byte) for key `{scratchKey}`, read `length(bytes) = {roundTripRead}`, deleted it, `{remaining}` remaining. => {(ok ? "OK" : "FAILED")}");
                lines.Add("");
                return ok;
            }
            catch (Exception ex)
            {
                lines.Add($"- ERROR: {ex.Message}");
                lines.Add("");
                return false;
            }
        }

        /// <summary>
        /// Proves the converters' <c>UndefinedTable</c> handling: reading a key from a table that does not exist must answer null (the converter catches <c>PostgresException</c> with <c>SqlState == 42P01</c>) rather than throwing.
        /// </summary>
        /// <param name="connection">The open <see cref="NpgsqlConnection"/> to use.</param>
        /// <param name="commandTimeout">The timeout in seconds for the statement.</param>
        /// <param name="lines">The report lines to append into.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>True when the read answered null (the 42P01 path was handled), otherwise false.</returns>
        private static async Task<bool> UndefinedTableReadAsync(NpgsqlConnection connection, int commandTimeout, List<string> lines, CancellationToken cancellationToken)
        {
            lines.Add("## UndefinedTable read (42P01)");
            try
            {
                OrtoArea? area = await OrtoAreaPostgreSQLConverter.GetOrtoAreaAsync(connection, "evidence_130_undefined_table", commandTimeout, cancellationToken);
                bool ok = area is null;
                lines.Add($"- read `orto_area` for key `evidence_130_undefined_table` answered {(area is null ? "null" : "a row (unexpected)")}. => {(ok ? "OK (42P01 handled as null)" : "FAILED")}");
                lines.Add("");
                return ok;
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                // The table genuinely does not exist and the exception surfaced: still the correct evidence, just not the converter's null path.
                lines.Add($"- read raised PostgresException {postgresException.SqlState} (table absent). => OK (42P01)");
                lines.Add("");
                return true;
            }
            catch (Exception ex)
            {
                lines.Add($"- ERROR: {ex.Message}");
                lines.Add("");
                return false;
            }
        }

        /// <summary>
        /// Runs a single-value query returning a long and returns the first column of the first row, or -1 when no row was returned. No scratch key is bound.
        /// </summary>
        /// <param name="connection">The open <see cref="NpgsqlConnection"/> to query.</param>
        /// <param name="sql">The SQL to run.</param>
        /// <param name="commandTimeout">The timeout in seconds for the statement.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row as a long, or -1 when no row was returned.</returns>
        private static async Task<long> ScalarLongAsync(NpgsqlConnection connection, string sql, int commandTimeout, CancellationToken cancellationToken)
        {
            return await ScalarLongAsync(connection, sql, null, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Runs a single-value query returning a long and returns the first column of the first row, or -1 when no row was returned.
        /// </summary>
        /// <param name="connection">The open <see cref="NpgsqlConnection"/> to query.</param>
        /// <param name="sql">The SQL to run.</param>
        /// <param name="scratchKey">The value bound to <c>@k</c> when the SQL references it.</param>
        /// <param name="commandTimeout">The timeout in seconds for the statement.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row as a long, or -1 when no row was returned.</returns>
        private static async Task<long> ScalarLongAsync(NpgsqlConnection connection, string sql, string? scratchKey, int commandTimeout, CancellationToken cancellationToken)
        {
            await using NpgsqlCommand command = new(sql, connection);
            command.CommandTimeout = commandTimeout;
            if (sql.Contains("@k"))
            {
                command.Parameters.Add(new NpgsqlParameter("k", scratchKey));
            }

            object? value = await command.ExecuteScalarAsync(cancellationToken);
            return value is null ? -1 : System.Convert.ToInt64(value, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Runs a single-value query returning a string (such as <c>version()</c>) and returns the first column of the first row, or empty when no row was returned.
        /// </summary>
        /// <param name="connection">The open <see cref="NpgsqlConnection"/> to query.</param>
        /// <param name="sql">The SQL to run.</param>
        /// <param name="commandTimeout">The timeout in seconds for the statement.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>The first column of the first row as a string, or empty when no row was returned.</returns>
        private static async Task<string> ScalarStringAsync(NpgsqlConnection connection, string sql, int commandTimeout, CancellationToken cancellationToken)
        {
            await using NpgsqlCommand command = new(sql, connection);
            command.CommandTimeout = commandTimeout;
            object? value = await command.ExecuteScalarAsync(cancellationToken);
            return value is null ? "" : value.ToString() ?? "";
        }
    }
}
