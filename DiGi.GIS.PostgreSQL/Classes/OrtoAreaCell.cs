using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One sampled cell of a full-interpretation test area: one element of <see cref="OrtoArea.Cells"/> (DiGi.GIS.PostgreSQL#129).
    /// <para><see cref="Index"/> is the 0-based position of the cell in the sampling grid, and <see cref="BoundingBox2D"/> is the cell extent in EPSG:2180, easting in X. Cells travel inside the <see cref="OrtoArea"/> object in <see cref="Constants.TableName.OrtoArea"/>; there is no cell table of their own.</para>
    /// </summary>
    public class OrtoAreaCell : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(BoundingBox2D))]
        private readonly BoundingBox2D? boundingBox2D;

        [JsonInclude, JsonPropertyName(nameof(Index))]
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaCell"/> class.
        /// </summary>
        /// <param name="index">The 0-based position of the cell in the sampling grid.</param>
        /// <param name="boundingBox2D">The cell extent in EPSG:2180, easting in X.</param>
        public OrtoAreaCell(int index, BoundingBox2D? boundingBox2D)
        {
            this.index = index;
            this.boundingBox2D = Core.Query.Clone(boundingBox2D);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaCell"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoAreaCell">The instance to copy.</param>
        public OrtoAreaCell(OrtoAreaCell? ortoAreaCell)
            : base(ortoAreaCell)
        {
            if (ortoAreaCell is not null)
            {
                index = ortoAreaCell.index;
                boundingBox2D = Core.Query.Clone(ortoAreaCell.boundingBox2D);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaCell"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoAreaCell(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets a clone of the cell extent in EPSG:2180, easting in X.
        /// </summary>
        [JsonIgnore]
        public BoundingBox2D? BoundingBox2D => Core.Query.Clone(boundingBox2D);

        /// <summary>
        /// Gets the 0-based position of the cell in the sampling grid.
        /// </summary>
        [JsonIgnore]
        public int Index => index;
    }
}
