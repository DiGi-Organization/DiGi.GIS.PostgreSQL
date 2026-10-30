using DiGi.Core.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides options for updating the external components area columns of the building data table from the stored building models.
    /// </summary>
    public class PostgreSQLBuildingDataExternalComponentsUpdateOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingDataExternalComponentsUpdateOptions"/> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the configuration settings.</param>
        public PostgreSQLBuildingDataExternalComponentsUpdateOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingDataExternalComponentsUpdateOptions"/> class.
        /// </summary>
        public PostgreSQLBuildingDataExternalComponentsUpdateOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingDataExternalComponentsUpdateOptions"/> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLBuildingDataExternalComponentsUpdateOptions">The source options instance to copy from.</param>
        public PostgreSQLBuildingDataExternalComponentsUpdateOptions(PostgreSQLBuildingDataExternalComponentsUpdateOptions postgreSQLBuildingDataExternalComponentsUpdateOptions)
            : base(postgreSQLBuildingDataExternalComponentsUpdateOptions)
        {
            if (postgreSQLBuildingDataExternalComponentsUpdateOptions is not null)
            {
                BatchSize = postgreSQLBuildingDataExternalComponentsUpdateOptions.BatchSize;
                CommandTimeout = postgreSQLBuildingDataExternalComponentsUpdateOptions.CommandTimeout;
                SkipCompleted = postgreSQLBuildingDataExternalComponentsUpdateOptions.SkipCompleted;
                PartyWalls = postgreSQLBuildingDataExternalComponentsUpdateOptions.PartyWalls;
                PartyWallMaxDistance = postgreSQLBuildingDataExternalComponentsUpdateOptions.PartyWallMaxDistance;
                CountyIds = postgreSQLBuildingDataExternalComponentsUpdateOptions.CountyIds == null ? null : [.. postgreSQLBuildingDataExternalComponentsUpdateOptions.CountyIds];
            }
        }

        /// <summary>
        /// Gets or sets the timeout in seconds applied to every statement the update issues. A value of 0 disables the timeout.
        /// <para>Well above the 30 second default, because these are bulk reads and writes over a partitioned table: a county can carry tens of thousands of building models, and the push writes every external components column of each of them.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CommandTimeout))]
        public int CommandTimeout { get; set; } = 600;

        /// <summary>
        /// Gets or sets the set of county identifiers the run is limited to. Null updates every county.
        /// <para>Identifiers rather than codes: a county whose territory is in several pieces is held as one row per piece, each with its own identifier, so a code names several of them.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the number of references read, classified and written together. Values below 1 are treated as 1.
        /// <para>This bounds the memory of the run: only one batch of stored building models is held at a time, so a county of any size costs the same. Lower it when a batch of large models still weighs too much.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BatchSize))]
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether references whose building data row already carries an external components area total are stepped over.
        /// <para>This resumes an interrupted run without recomputing what it finished. It is sound only while a non-null total means "written by this kind of run", that is, when the columns were empty before the first run. After the classification changes, clear it to rewrite every row.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SkipCompleted))]
        public bool SkipCompleted { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the party wall area columns are filled too: the wall area each building shares with adjacent buildings (DiGi-Organization/DiGi.GIS.Analytical#4).
        /// <para>It costs a neighbour search per batch and a read of the neighbours' stored models, which may be filed under another county. Cleared, the party wall columns are left as they are.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(PartyWalls))]
        public bool PartyWalls { get; set; } = true;

        /// <summary>
        /// Gets or sets the largest distance [m] between two outlines, and between two wall planes, at which two buildings still share a wall.
        /// <para>0.2 m by default: adjacent outlines of the national topographic database meet within it, while a gap above it is a passage or a separate building.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(PartyWallMaxDistance))]
        public double PartyWallMaxDistance { get; set; } = 0.2;
    }
}
