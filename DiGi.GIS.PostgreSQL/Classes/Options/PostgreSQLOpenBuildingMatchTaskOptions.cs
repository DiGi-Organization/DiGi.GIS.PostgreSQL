using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides options for matching the open building datasets to BDOT10k (DiGi.GIS.PostgreSQL#127).
    /// <para>The scope is exactly one of <see cref="CountyIds"/> or <see cref="BoundingBox2D"/>. Either way the run works in whole county groups - every sibling part of a multi-part county together - because a county part is the unit of the stored snapshot: a bounding box selects the county parts whose envelopes reach it, never a piece of one. The summaries of a box (a T30 area) are read afterwards through <see cref="OpenBuildingPostgreSQLConverter.GetMatchSummariesAsync(string?, BoundingBox2D?, int, System.Threading.CancellationToken)"/>.</para>
    /// </summary>
    public class PostgreSQLOpenBuildingMatchTaskOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOpenBuildingMatchTaskOptions"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the options.</param>
        public PostgreSQLOpenBuildingMatchTaskOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOpenBuildingMatchTaskOptions"/> class.
        /// </summary>
        public PostgreSQLOpenBuildingMatchTaskOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOpenBuildingMatchTaskOptions"/> class by copying another instance.
        /// </summary>
        /// <param name="postgreSQLOpenBuildingMatchTaskOptions">The source options to copy.</param>
        public PostgreSQLOpenBuildingMatchTaskOptions(PostgreSQLOpenBuildingMatchTaskOptions postgreSQLOpenBuildingMatchTaskOptions)
            : base(postgreSQLOpenBuildingMatchTaskOptions)
        {
            if (postgreSQLOpenBuildingMatchTaskOptions is not null)
            {
                BoundingBox2D = postgreSQLOpenBuildingMatchTaskOptions.BoundingBox2D is BoundingBox2D boundingBox2D
                    ? new BoundingBox2D(new Point2D(boundingBox2D.Min.X, boundingBox2D.Min.Y), new Point2D(boundingBox2D.Max.X, boundingBox2D.Max.Y))
                    : null;
                CountyIds = postgreSQLOpenBuildingMatchTaskOptions.CountyIds == null ? null : [.. postgreSQLOpenBuildingMatchTaskOptions.CountyIds];
                DatasetKeys = postgreSQLOpenBuildingMatchTaskOptions.DatasetKeys == null ? null : [.. postgreSQLOpenBuildingMatchTaskOptions.DatasetKeys];
                OpenBuildingMatchParameters = postgreSQLOpenBuildingMatchTaskOptions.OpenBuildingMatchParameters == null ? null : new OpenBuildingMatchParameters(postgreSQLOpenBuildingMatchTaskOptions.OpenBuildingMatchParameters);
                Persist = postgreSQLOpenBuildingMatchTaskOptions.Persist;
            }
        }

        /// <summary>
        /// Gets or sets the scope bounding box in EPSG:2180 (easting in X): the run matches every county group whose parts' envelopes reach it. Mutually exclusive with <see cref="CountyIds"/>.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BoundingBox2D))]
        public BoundingBox2D? BoundingBox2D { get; set; } = null;

        /// <summary>
        /// Gets or sets the county part identifiers to match, each widened to every sibling part sharing its code. Mutually exclusive with <see cref="BoundingBox2D"/>.
        /// <para>Identifiers, never codes: a county held in several pieces is one row per piece (Coding - GIS Administrative Data.md).</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the dataset keys to match (<c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c>). Null, the default, matches all five footprint datasets; an empty set matches none. GHS-OBAT holds centroids and is refused.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(DatasetKeys))]
        public HashSet<string>? DatasetKeys { get; set; } = null;

        /// <summary>
        /// Gets or sets the parameters of the matching rule - the sensitivity axes. Null uses the defaults of <see cref="Classes.OpenBuildingMatchParameters"/>.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(OpenBuildingMatchParameters))]
        public OpenBuildingMatchParameters? OpenBuildingMatchParameters { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the links replace the stored snapshot of every county group in scope. True by default.
        /// <para>A sensitivity run with other parameters sets it to false: the run computes the links and returns their summaries without touching <c>building_dataset_match</c>, so the stored snapshot stays the one the paper cites.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Persist))]
        public bool Persist { get; set; } = true;
    }
}
