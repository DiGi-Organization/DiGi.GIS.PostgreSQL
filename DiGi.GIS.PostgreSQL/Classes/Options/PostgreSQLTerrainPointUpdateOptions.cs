using DiGi.Core.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides options for filling the terrain point table by sampling elevations on a regular grid.
    /// </summary>
    public class PostgreSQLTerrainPointUpdateOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLTerrainPointUpdateOptions"/> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the configuration settings.</param>
        public PostgreSQLTerrainPointUpdateOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLTerrainPointUpdateOptions"/> class.
        /// </summary>
        public PostgreSQLTerrainPointUpdateOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLTerrainPointUpdateOptions"/> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLTerrainPointUpdateOptions">The source options instance to copy from.</param>
        public PostgreSQLTerrainPointUpdateOptions(PostgreSQLTerrainPointUpdateOptions postgreSQLTerrainPointUpdateOptions)
            : base(postgreSQLTerrainPointUpdateOptions)
        {
            if (postgreSQLTerrainPointUpdateOptions is not null)
            {
                CountyIds = postgreSQLTerrainPointUpdateOptions.CountyIds == null ? null : [.. postgreSQLTerrainPointUpdateOptions.CountyIds];
                GridSize = postgreSQLTerrainPointUpdateOptions.GridSize;
                MaxConcurrentRequests = postgreSQLTerrainPointUpdateOptions.MaxConcurrentRequests;
                OriginX = postgreSQLTerrainPointUpdateOptions.OriginX;
                OriginY = postgreSQLTerrainPointUpdateOptions.OriginY;
                OverrideExisting = postgreSQLTerrainPointUpdateOptions.OverrideExisting;
                RetryCount = postgreSQLTerrainPointUpdateOptions.RetryCount;
                RetryDelayMilliseconds = postgreSQLTerrainPointUpdateOptions.RetryDelayMilliseconds;
                TileSize = postgreSQLTerrainPointUpdateOptions.TileSize;
                Tolerance = postgreSQLTerrainPointUpdateOptions.Tolerance;
            }
        }

        /// <summary>
        /// Gets or sets the counties to sample, by identifier. Null samples every county.
        /// <para>Identifiers rather than codes: a county whose territory is in several pieces is held as one row per piece, each with its own identifier and its own subdivisions, so a code names several of them.</para>
        /// <para>Each identifier names one polygon part, and the part is widened to every part sharing its code before the run starts - the county is sampled once as a whole, and each point is filed under the part containing it.</para>
        /// <para>Naming a few counties is the ordinary way to use this task at a fine grid size - see <see cref="GridSize"/>.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the spacing of the sampling grid, in model units.
        /// <para>Cost rises with the square of how fine this is, and every point is one request to the elevation service. Over a whole country 100 gives about 31 million points, 50 about 125 million, and 10 about 3.1 billion - the last is a setting for a named county, not for a country.</para>
        /// <para>Keep it a whole multiple of the finest value ever intended, and leave <see cref="OriginX"/> and <see cref="OriginY"/> alone, so that a county sampled coarsely can later be sampled finely without re-visiting the points it already holds. 100, 50 and 10 nest that way; 30 and 100 do not.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(GridSize))]
        public double GridSize { get; set; } = 50;

        /// <summary>
        /// Gets or sets how many elevation requests may be in flight at once.
        /// <para>This is the whole of the throughput of the task, and it is asked of a public service - raising it shortens a run in proportion until the service starts refusing, at which point <see cref="RetryCount"/> turns the refusals into a longer run rather than into missing points.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(MaxConcurrentRequests))]
        public int MaxConcurrentRequests { get; set; } = 16;

        /// <summary>
        /// Gets or sets the X coordinate the sampling grid is anchored at.
        /// <para>Leave at zero. Every tile of every county is cut from the one grid this anchors, which is what lets counties meet without a seam, lets a run be repeated without shifting, and lets a coarse sampling be reused by a finer one. Moving the anchor - to sample cell centres, say - gives up the last of those.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(OriginX))]
        public double OriginX { get; set; } = 0;

        /// <summary>
        /// Gets or sets the Y coordinate the sampling grid is anchored at.
        /// <para>Leave at zero, for the reasons given on <see cref="OriginX"/>.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(OriginY))]
        public double OriginY { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether points already stored are sampled again.
        /// <para>Left false the task reads back what a tile already holds and asks the elevation service only for the rest, so a run that was stopped picks up where it left off and a county sampled coarsely can be densified cheaply. Set true to pay for every point again.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(OverrideExisting))]
        public bool OverrideExisting { get; set; } = false;

        /// <summary>
        /// Gets or sets how many times a request the elevation service refused for a transient reason is sent again.
        /// <para>Zero sends each request once, which over a long run turns a burst of refusals into a band of points that have no elevation and that nothing goes back for.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(RetryCount))]
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the delay before the first retry, in milliseconds, doubling for each attempt after that.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(RetryDelayMilliseconds))]
        public double RetryDelayMilliseconds { get; set; } = 500;

        /// <summary>
        /// Gets or sets the edge of one work tile, counted in grid steps.
        /// <para>One tile is one read of what is already stored, one batch of elevation requests and one write, so this governs how much is held at once and how often progress is reported. The default of 128 is 16 384 points per tile.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(TileSize))]
        public int TileSize { get; set; } = 128;

        /// <summary>
        /// Gets or sets the distance tolerance.
        /// <para>Used both when deciding whether a sampled point falls within an area and when reading back the points a tile already holds. It is capped at half of <see cref="GridSize"/> while the task runs, so that a point can never be taken for a point of the neighbouring tile.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Tolerance))]
        public double Tolerance { get; set; } = Core.Constants.Tolerance.MacroDistance;
    }
}
