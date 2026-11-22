using DiGi.Core.Classes;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides options for importing a downloaded GeoJSON export of the GUS 1 km census population grid into <see cref="Constants.TableName.PopulationGrid"/> (DiGi.GIS.PostgreSQL#106).
    /// <para><see cref="SourcePath"/> and <see cref="CensusYear"/> are required and the run refuses without them, naming the missing one in the log: the census year is the year the grid was counted in (2021 for the NSP 2021 export), never the download year, and it is what the imported rows are filed under. <see cref="Clear"/> makes the run a clean replace of that census year's rows - the file is fully read and validated before anything is cleared, so a malformed download leaves the stored grid untouched; a run that fails midway after a clear leaves the cells written so far, and a re-run converges on them because the write is an upsert keyed on <c>(cell_id, census_year)</c>.</para>
    /// </summary>
    public class PostgreSQLPopulationGridImportTaskOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLPopulationGridImportTaskOptions"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the options.</param>
        public PostgreSQLPopulationGridImportTaskOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLPopulationGridImportTaskOptions"/> class.
        /// </summary>
        public PostgreSQLPopulationGridImportTaskOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLPopulationGridImportTaskOptions"/> class by copying another instance.
        /// </summary>
        /// <param name="postgreSQLPopulationGridImportTaskOptions">The source options to copy.</param>
        public PostgreSQLPopulationGridImportTaskOptions(PostgreSQLPopulationGridImportTaskOptions postgreSQLPopulationGridImportTaskOptions)
            : base(postgreSQLPopulationGridImportTaskOptions)
        {
            if (postgreSQLPopulationGridImportTaskOptions is not null)
            {
                BatchSize = postgreSQLPopulationGridImportTaskOptions.BatchSize;
                CensusYear = postgreSQLPopulationGridImportTaskOptions.CensusYear;
                Clear = postgreSQLPopulationGridImportTaskOptions.Clear;
                CommandTimeout = postgreSQLPopulationGridImportTaskOptions.CommandTimeout;
                SourcePath = postgreSQLPopulationGridImportTaskOptions.SourcePath;
            }
        }

        /// <summary>
        /// Gets or sets the number of grid cells sent per insert command. Defaults to 1000.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BatchSize))]
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the census year the grid was counted in (2021 for the NSP 2021 export). Required; the run refuses without it.
        /// <para>The year the file was downloaded is not this value: rows are filed under the census they count, so the same table can hold the NSP 2021 grid and, once published, an NSP 2011 one side by side.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CensusYear))]
        public int? CensusYear { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the rows of <see cref="CensusYear"/> are deleted before the import writes. Defaults to false.
        /// <para>The file is read and validated in full before anything is cleared, so a malformed download cannot empty the stored grid. False keeps the import an upsert over whatever the year already holds.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Clear))]
        public bool Clear { get; set; } = false;

        /// <summary>
        /// Gets or sets the timeout in seconds for each database command. Defaults to 600.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CommandTimeout))]
        public int CommandTimeout { get; set; } = 600;

        /// <summary>
        /// Gets or sets the path of the downloaded GeoJSON export of the grid. Required; the run refuses without it.
        /// <para>The file is obtained by hand from the Geostatistics Portal - the portal's regulations forbid automated downloading - and lives outside the repository, in the project's data input folder.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SourcePath))]
        public string? SourcePath { get; set; } = null;
    }
}
