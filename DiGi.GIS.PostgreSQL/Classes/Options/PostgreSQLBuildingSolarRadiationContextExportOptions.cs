using DiGi.Core.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides options for exporting the urban-context table of stored solar radiation - irradiation per wall sector, roof and building for each neighbour radius, its shortfall against the isolated building, and the building descriptors - one file per county part.
    /// </summary>
    public class PostgreSQLBuildingSolarRadiationContextExportOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationContextExportOptions"/> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the configuration settings.</param>
        public PostgreSQLBuildingSolarRadiationContextExportOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationContextExportOptions"/> class.
        /// </summary>
        public PostgreSQLBuildingSolarRadiationContextExportOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationContextExportOptions"/> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLBuildingSolarRadiationContextExportOptions">The source options instance to copy from.</param>
        public PostgreSQLBuildingSolarRadiationContextExportOptions(PostgreSQLBuildingSolarRadiationContextExportOptions postgreSQLBuildingSolarRadiationContextExportOptions)
            : base(postgreSQLBuildingSolarRadiationContextExportOptions)
        {
            if (postgreSQLBuildingSolarRadiationContextExportOptions is not null)
            {
                BatchSize = postgreSQLBuildingSolarRadiationContextExportOptions.BatchSize;
                CommandTimeout = postgreSQLBuildingSolarRadiationContextExportOptions.CommandTimeout;
                CountyIds = postgreSQLBuildingSolarRadiationContextExportOptions.CountyIds == null ? null : [.. postgreSQLBuildingSolarRadiationContextExportOptions.CountyIds];
                OutputDirectory = postgreSQLBuildingSolarRadiationContextExportOptions.OutputDirectory;
                Radii = postgreSQLBuildingSolarRadiationContextExportOptions.Radii == null ? null : [.. postgreSQLBuildingSolarRadiationContextExportOptions.Radii];
                ResidentialOnly = postgreSQLBuildingSolarRadiationContextExportOptions.ResidentialOnly;
            }
        }

        /// <summary>
        /// Gets or sets the number of buildings read together. Values below 1 are treated as 1.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BatchSize))]
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the timeout in seconds applied to every statement the export issues. A value of 0 disables the timeout.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CommandTimeout))]
        public int CommandTimeout { get; set; } = 600;

        /// <summary>
        /// Gets or sets the set of county identifiers the export is limited to. Null exports every county; empty exports none.
        /// <para>Identifiers rather than codes: a county whose territory is in several pieces is held as one row per piece, each with its own identifier, and gets one file per piece (<c>Coding - GIS Administrative Data.md</c>).</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the folder the files are written to, one <c>solar_context_county_{id}.csv</c> per county part. Null refuses the run: an export has no sensible default location on a server.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(OutputDirectory))]
        public string? OutputDirectory { get; set; } = null;

        /// <summary>
        /// Gets or sets the neighbour radii, in m, exactly as stored, exported in this column order. The shortfall columns need 0, the isolated building; null or empty exports nothing.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Radii))]
        public List<double>? Radii { get; set; } = [0, 50, 100];

        /// <summary>
        /// Gets or sets a value indicating whether only buildings whose building data row is residential are exported.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(ResidentialOnly))]
        public bool ResidentialOnly { get; set; } = true;
    }
}
