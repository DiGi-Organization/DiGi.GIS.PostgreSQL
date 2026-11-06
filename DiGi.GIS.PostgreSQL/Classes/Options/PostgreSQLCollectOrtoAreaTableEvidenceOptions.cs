using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Options for collecting the production evidence of the four full-interpretation test-area tables (DiGi.GIS.PostgreSQL#130).
    /// <para>Nothing is written to the schema: the run is read-only, apart from a single scratch row that is inserted and deleted in the same connection to prove read/write access of the service account, and left at zero rows afterwards.</para>
    /// </summary>
    public class PostgreSQLCollectOrtoAreaTableEvidenceOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLCollectOrtoAreaTableEvidenceOptions"/> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object used to initialize the options.</param>
        public PostgreSQLCollectOrtoAreaTableEvidenceOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLCollectOrtoAreaTableEvidenceOptions"/> class.
        /// </summary>
        public PostgreSQLCollectOrtoAreaTableEvidenceOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLCollectOrtoAreaTableEvidenceOptions"/> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLCollectOrtoAreaTableEvidenceOptions">The source options to copy from.</param>
        public PostgreSQLCollectOrtoAreaTableEvidenceOptions(PostgreSQLCollectOrtoAreaTableEvidenceOptions postgreSQLCollectOrtoAreaTableEvidenceOptions)
            : base(postgreSQLCollectOrtoAreaTableEvidenceOptions)
        {
            if (postgreSQLCollectOrtoAreaTableEvidenceOptions is not null)
            {
                CommandTimeout = postgreSQLCollectOrtoAreaTableEvidenceOptions.CommandTimeout;
                ReportDirectory = postgreSQLCollectOrtoAreaTableEvidenceOptions.ReportDirectory;
            }
        }

        /// <summary>
        /// Gets or sets the timeout in seconds for the statements the run executes.
        /// </summary>
        [JsonInclude, JsonPropertyName("CommandTimeout")]
        public int CommandTimeout { get; set; } = 60;

        /// <summary>
        /// Gets or sets the directory the evidence report is written into. When null the directory the application was launched from is used.
        /// </summary>
        [JsonInclude, JsonPropertyName("ReportDirectory")]
        public string? ReportDirectory { get; set; } = null;
    }
}
