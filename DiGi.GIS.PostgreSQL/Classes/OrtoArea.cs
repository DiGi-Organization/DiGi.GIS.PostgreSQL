using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One full-interpretation test area: a row of <see cref="Constants.TableName.OrtoArea"/> carrying its extent, sampling grid and naming (DiGi.GIS.PostgreSQL#129).
    /// <para>The table's <c>object</c> column stores the whole instance, <see cref="Cells"/> included, so one read returns the area with its sampled cells in index order; the envelope columns (<c>min_x</c> ... <c>max_y</c>) duplicate the box for keyed and ranged lookup. The table lives on the Main database, beside <c>building_2d</c>. The imagery materialised for the area lives on the Storage database (<see cref="Constants.TableName.OrtoAreaImage"/>) and is never read in the same statement as this row.</para>
    /// </summary>
    public class OrtoArea : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AreaKey))]
        private readonly string areaKey = string.Empty;

        [JsonInclude, JsonPropertyName(nameof(BoundingBox2D))]
        private readonly BoundingBox2D? boundingBox2D;

        [JsonInclude, JsonPropertyName(nameof(CellCount))]
        private readonly int cellCount;

        [JsonInclude, JsonPropertyName(nameof(CellSize))]
        private readonly double cellSize;

        [JsonInclude, JsonPropertyName(nameof(Cells))]
        private readonly List<OrtoAreaCell> cells = [];

        [JsonInclude, JsonPropertyName(nameof(Name))]
        private readonly string? name;

        [JsonInclude, JsonPropertyName(nameof(Seed))]
        private readonly long seed;

        [JsonInclude, JsonPropertyName(nameof(Stratum))]
        private readonly string? stratum;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoArea"/> class.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the area's bounding box that names the row.</param>
        /// <param name="boundingBox2D">The area extent in EPSG:2180, easting in X.</param>
        /// <param name="name">The display name of the area, or null when it is unnamed.</param>
        /// <param name="stratum">The named stratum of the area, or null when it is unstratified.</param>
        /// <param name="cellSize">The edge length of one sampled cell, in m.</param>
        /// <param name="cellCount">How many cells the sampling grid holds.</param>
        /// <param name="seed">The seed the sampling grid was drawn with.</param>
        /// <param name="cells">The sampled cells in index order, or null for none.</param>
        public OrtoArea(string? areaKey, BoundingBox2D? boundingBox2D, string? name, string? stratum, double cellSize, int cellCount, long seed, IEnumerable<OrtoAreaCell>? cells)
        {
            this.areaKey = areaKey ?? string.Empty;
            this.boundingBox2D = Core.Query.Clone(boundingBox2D);
            this.name = name;
            this.stratum = stratum;
            this.cellSize = cellSize;
            this.cellCount = cellCount;
            this.seed = seed;

            if (cells is not null)
            {
                this.cells = [.. cells];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoArea"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoArea">The instance to copy.</param>
        public OrtoArea(OrtoArea? ortoArea)
            : base(ortoArea)
        {
            if (ortoArea is not null)
            {
                areaKey = ortoArea.areaKey;
                boundingBox2D = Core.Query.Clone(ortoArea.boundingBox2D);
                cellCount = ortoArea.cellCount;
                cellSize = ortoArea.cellSize;
                cells = new List<OrtoAreaCell>(ortoArea.cells);
                name = ortoArea.name;
                seed = ortoArea.seed;
                stratum = ortoArea.stratum;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoArea"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoArea(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the invariant location key of the area's bounding box that names the row.
        /// </summary>
        [JsonIgnore]
        public string AreaKey => areaKey;

        /// <summary>
        /// Gets a clone of the area extent in EPSG:2180, easting in X.
        /// </summary>
        [JsonIgnore]
        public BoundingBox2D? BoundingBox2D => Core.Query.Clone(boundingBox2D);

        /// <summary>
        /// Gets how many cells the sampling grid holds.
        /// </summary>
        [JsonIgnore]
        public int CellCount => cellCount;

        /// <summary>
        /// Gets the edge length of one sampled cell, in m.
        /// </summary>
        [JsonIgnore]
        public double CellSize => cellSize;

        /// <summary>
        /// Gets the sampled cells in index order.
        /// </summary>
        [JsonIgnore]
        public List<OrtoAreaCell> Cells => cells;

        /// <summary>
        /// Gets the display name of the area, or null when it is unnamed.
        /// </summary>
        [JsonIgnore]
        public string? Name => name;

        /// <summary>
        /// Gets the seed the sampling grid was drawn with.
        /// </summary>
        [JsonIgnore]
        public long Seed => seed;

        /// <summary>
        /// Gets the named stratum of the area, or null when it is unstratified.
        /// </summary>
        [JsonIgnore]
        public string? Stratum => stratum;
    }
}
