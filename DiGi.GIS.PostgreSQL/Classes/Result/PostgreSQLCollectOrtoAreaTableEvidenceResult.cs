using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents the outcome of collecting the production evidence of the four full-interpretation test-area tables (DiGi.GIS.PostgreSQL#130).
    /// <para>The per-table rows live in the report file, not here: this carries only the rollup - which tables were present, which round trips succeeded, and where the report was written.</para>
    /// </summary>
    public class PostgreSQLCollectOrtoAreaTableEvidenceResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(TablePresentCount))]
        private readonly long tablePresentCount;

        [JsonInclude, JsonPropertyName(nameof(TableExpectedCount))]
        private readonly long tableExpectedCount;

        [JsonInclude, JsonPropertyName(nameof(RoundTripOkCount))]
        private readonly long roundTripOkCount;

        [JsonInclude, JsonPropertyName(nameof(RoundTripExpectedCount))]
        private readonly long roundTripExpectedCount;

        [JsonInclude, JsonPropertyName(nameof(ReportPath))]
        private readonly string? reportPath;

        [JsonInclude, JsonPropertyName(nameof(AllTablesPresent))]
        private readonly bool allTablesPresent;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLCollectOrtoAreaTableEvidenceResult"/> class.
        /// </summary>
        /// <param name="tablePresentCount">The number of the four tables found on their databases.</param>
        /// <param name="tableExpectedCount">The number of tables the run checks (four).</param>
        /// <param name="roundTripOkCount">The number of per-database scratch insert/delete round trips that succeeded and left zero rows.</param>
        /// <param name="roundTripExpectedCount">The number of round trips attempted (two: one per database).</param>
        /// <param name="reportPath">The absolute path of the written report file, or null when it could not be written.</param>
        /// <param name="allTablesPresent">Whether all four tables were present on their databases.</param>
        public PostgreSQLCollectOrtoAreaTableEvidenceResult(long tablePresentCount, long tableExpectedCount, long roundTripOkCount, long roundTripExpectedCount, string? reportPath, bool allTablesPresent)
        {
            this.tablePresentCount = tablePresentCount;
            this.tableExpectedCount = tableExpectedCount;
            this.roundTripOkCount = roundTripOkCount;
            this.roundTripExpectedCount = roundTripExpectedCount;
            this.reportPath = reportPath;
            this.allTablesPresent = allTablesPresent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLCollectOrtoAreaTableEvidenceResult"/> class by copying an existing instance.
        /// </summary>
        /// <param name="postgreSQLCollectOrtoAreaTableEvidenceResult">The <see cref="PostgreSQLCollectOrtoAreaTableEvidenceResult"/> instance to copy from.</param>
        public PostgreSQLCollectOrtoAreaTableEvidenceResult(PostgreSQLCollectOrtoAreaTableEvidenceResult? postgreSQLCollectOrtoAreaTableEvidenceResult)
            : base(postgreSQLCollectOrtoAreaTableEvidenceResult)
        {
            if (postgreSQLCollectOrtoAreaTableEvidenceResult is not null)
            {
                tablePresentCount = postgreSQLCollectOrtoAreaTableEvidenceResult.tablePresentCount;
                tableExpectedCount = postgreSQLCollectOrtoAreaTableEvidenceResult.tableExpectedCount;
                roundTripOkCount = postgreSQLCollectOrtoAreaTableEvidenceResult.roundTripOkCount;
                roundTripExpectedCount = postgreSQLCollectOrtoAreaTableEvidenceResult.roundTripExpectedCount;
                reportPath = postgreSQLCollectOrtoAreaTableEvidenceResult.reportPath;
                allTablesPresent = postgreSQLCollectOrtoAreaTableEvidenceResult.allTablesPresent;
            }
        }

        /// <summary>
        /// Gets the number of the four tables found on their databases.
        /// </summary>
        public long TablePresentCount => tablePresentCount;

        /// <summary>
        /// Gets the number of tables the run checks (four).
        /// </summary>
        public long TableExpectedCount => tableExpectedCount;

        /// <summary>
        /// Gets the number of per-database scratch insert/delete round trips that succeeded and left zero rows.
        /// </summary>
        public long RoundTripOkCount => roundTripOkCount;

        /// <summary>
        /// Gets the number of round trips attempted (two: one per database).
        /// </summary>
        public long RoundTripExpectedCount => roundTripExpectedCount;

        /// <summary>
        /// Gets the absolute path of the written report file, or null when it could not be written.
        /// </summary>
        public string? ReportPath => reportPath;

        /// <summary>
        /// Gets a value indicating whether all four tables were present on their databases.
        /// </summary>
        public bool AllTablesPresent => allTablesPresent;
    }
}
