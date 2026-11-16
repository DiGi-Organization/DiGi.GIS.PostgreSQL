using DiGi.Core.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Solar.Classes
{
    /// <summary>
    /// Provides options for solving the annual solar radiation of every building of a county part, for one or more neighbour radii, and storing it in the building solar radiation table.
    /// </summary>
    public class PostgreSQLBuildingSolarRadiationSolveOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationSolveOptions"/> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the configuration settings.</param>
        public PostgreSQLBuildingSolarRadiationSolveOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationSolveOptions"/> class.
        /// </summary>
        public PostgreSQLBuildingSolarRadiationSolveOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationSolveOptions"/> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLBuildingSolarRadiationSolveOptions">The source options instance to copy from.</param>
        public PostgreSQLBuildingSolarRadiationSolveOptions(PostgreSQLBuildingSolarRadiationSolveOptions postgreSQLBuildingSolarRadiationSolveOptions)
            : base(postgreSQLBuildingSolarRadiationSolveOptions)
        {
            if (postgreSQLBuildingSolarRadiationSolveOptions is not null)
            {
                BatchSize = postgreSQLBuildingSolarRadiationSolveOptions.BatchSize;
                BuildingCountMax = postgreSQLBuildingSolarRadiationSolveOptions.BuildingCountMax;
                CasterTriangleCountMax = postgreSQLBuildingSolarRadiationSolveOptions.CasterTriangleCountMax;
                CommandTimeout = postgreSQLBuildingSolarRadiationSolveOptions.CommandTimeout;
                CountyIds = postgreSQLBuildingSolarRadiationSolveOptions.CountyIds == null ? null : [.. postgreSQLBuildingSolarRadiationSolveOptions.CountyIds];
                Radii = postgreSQLBuildingSolarRadiationSolveOptions.Radii == null ? null : [.. postgreSQLBuildingSolarRadiationSolveOptions.Radii];
                ReceiverCountMax = postgreSQLBuildingSolarRadiationSolveOptions.ReceiverCountMax;
                SkipExisting = postgreSQLBuildingSolarRadiationSolveOptions.SkipExisting;
                TileSize = postgreSQLBuildingSolarRadiationSolveOptions.TileSize;
            }
        }

        /// <summary>
        /// Gets or sets the largest number of buildings read, solved and written together. Values below 1 are treated as 1.
        /// <para>A tile with more buildings is solved in several batches; each batch reads its neighbours once and writes its results in one statement, so a smaller batch holds less in memory and loses less to an interruption.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BatchSize))]
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the largest number of buildings solved per county part. Null solves every pending building of the part; values below 1 are treated as 1.
        /// <para>The cap applies to the buildings still pending after <see cref="SkipExisting"/>, cut after they are put in solve order (tiles by row, references ordinal within a tile), so a capped run solves a compact cluster with realistic neighbours, and a repeat run with <see cref="SkipExisting"/> takes the next buildings. Meant for a pilot that measures time and storage before a full run (DiGi.GIS.PostgreSQL#137, #138).</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BuildingCountMax))]
        public int? BuildingCountMax { get; set; } = null;

        /// <summary>
        /// Gets or sets the largest number of shading-only triangles - neighbours and the building's own non-receiving components - one solve is run against; above it the building is skipped for that radius and counted.
        /// <para>The default is the background-job ceiling of the viewer (<c>SolarJobCasterTriangleCountMax</c> of DiGi.GIS.WebAPI.UI), measured on its web server. A sensitivity run above 100 m needs a higher one.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CasterTriangleCountMax))]
        public int CasterTriangleCountMax { get; set; } = 32_000;

        /// <summary>
        /// Gets or sets the timeout in seconds applied to every statement the run issues. A value of 0 disables the timeout.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CommandTimeout))]
        public int CommandTimeout { get; set; } = 600;

        /// <summary>
        /// Gets or sets the set of county identifiers the run is limited to. Null solves every county; empty solves none.
        /// <para>Identifiers rather than codes: a county whose territory is in several pieces is held as one row per piece, each with its own identifier, so a code names several of them (<c>Coding - GIS Administrative Data.md</c>).</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the neighbour radii, in m, every building is solved for. Each is rounded to 0.1 m (<c>DiGi.GIS.Analytical.Solar.Query.SolarRadiusKey</c>), the value it is stored under; a negative or non-finite radius is ignored, and null or empty solves nothing.
        /// <para>0 is the building alone (<c>SolarRadiusIsolated</c>): no neighbour casts shade, while the building still shades and blocks the view of its own surfaces - the reference of the urban-context effect (DiGi.GIS.PostgreSQL#107). The viewer solves 50 m by default and at most 100 m.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Radii))]
        public List<double>? Radii { get; set; } = [0, 50, 100];

        /// <summary>
        /// Gets or sets the largest number of receiving surfaces - external walls and roofs - of a building that is solved; above it the building is skipped and counted.
        /// <para>The default is the background-job ceiling of the viewer (<c>SolarJobReceiverCountMax</c> of DiGi.GIS.WebAPI.UI).</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(ReceiverCountMax))]
        public int ReceiverCountMax { get; set; } = 400;

        /// <summary>
        /// Gets or sets a value indicating whether a building and radius that already carry a result of the current solver version are stepped over.
        /// <para>This resumes an interrupted run, and leaves a result the viewer stored for the same radius in place. Clear it to solve everything again, for instance after the stored buildings changed.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SkipExisting))]
        public bool SkipExisting { get; set; } = true;

        /// <summary>
        /// Gets or sets the edge length, in m, of the square tiles a county part is solved in. Values below 1 are treated as 1.
        /// <para>Buildings are grouped by the tile their centroid falls in, and the neighbours of a whole tile are read in one query, so a tile trades the number of neighbour reads against the area each one covers.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(TileSize))]
        public double TileSize { get; set; } = 250;
    }
}
