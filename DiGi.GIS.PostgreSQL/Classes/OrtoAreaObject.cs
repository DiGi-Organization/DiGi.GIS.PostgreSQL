using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One polygon drawn in a full-interpretation test area: a row of <see cref="Constants.TableName.OrtoAreaObject"/>, keyed by its server-assigned identifier (DiGi.GIS.PostgreSQL#129).
    /// <para><see cref="ObjectId"/> is a server-assigned GUID and stays stable across updates. <see cref="CellIndex"/> is the sampled cell holding the polygon's centroid, computed and stored by the API, or null when the centroid falls outside every sampled cell. The table's <c>object</c> column carries the whole instance, polygon included, and the table lives on the Main database, beside <c>building_2d</c>.</para>
    /// </summary>
    public class OrtoAreaObject : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AreaKey))]
        private readonly string areaKey = string.Empty;

        [JsonInclude, JsonPropertyName(nameof(CellIndex))]
        private readonly int? cellIndex;

        [JsonInclude, JsonPropertyName(nameof(CreatedAt))]
        private readonly DateTimeOffset createdAt;

        [JsonInclude, JsonPropertyName(nameof(CreatedBy))]
        private readonly string createdBy = string.Empty;

        [JsonInclude, JsonPropertyName(nameof(ObjectId))]
        private readonly string objectId = string.Empty;

        [JsonInclude, JsonPropertyName(nameof(Polygon2D))]
        private readonly Polygon2D? polygon2D;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaObject"/> class.
        /// </summary>
        /// <param name="objectId">The server-assigned identifier of the drawn polygon; a stable GUID.</param>
        /// <param name="areaKey">The invariant location key of the test area the polygon was drawn in.</param>
        /// <param name="cellIndex">The 0-based index of the sampled cell holding the polygon's centroid, or null when the centroid falls outside every sampled cell.</param>
        /// <param name="polygon2D">The drawn polygon in EPSG:2180, easting in X.</param>
        /// <param name="createdBy">The email of the user who drew the polygon.</param>
        /// <param name="createdAt">When the polygon was drawn.</param>
        public OrtoAreaObject(string? objectId, string? areaKey, int? cellIndex, Polygon2D? polygon2D, string? createdBy, DateTimeOffset createdAt)
        {
            this.objectId = objectId ?? string.Empty;
            this.areaKey = areaKey ?? string.Empty;
            this.cellIndex = cellIndex;
            this.polygon2D = Core.Query.Clone(polygon2D);
            this.createdBy = createdBy ?? string.Empty;
            this.createdAt = createdAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaObject"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoAreaObject">The instance to copy.</param>
        public OrtoAreaObject(OrtoAreaObject? ortoAreaObject)
            : base(ortoAreaObject)
        {
            if (ortoAreaObject is not null)
            {
                areaKey = ortoAreaObject.areaKey;
                cellIndex = ortoAreaObject.cellIndex;
                createdAt = ortoAreaObject.createdAt;
                createdBy = ortoAreaObject.createdBy;
                objectId = ortoAreaObject.objectId;
                polygon2D = Core.Query.Clone(ortoAreaObject.polygon2D);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaObject"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoAreaObject(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the invariant location key of the test area the polygon was drawn in.
        /// </summary>
        [JsonIgnore]
        public string AreaKey => areaKey;

        /// <summary>
        /// Gets the 0-based index of the sampled cell holding the polygon's centroid, or null when the centroid falls outside every sampled cell.
        /// </summary>
        [JsonIgnore]
        public int? CellIndex => cellIndex;

        /// <summary>
        /// Gets when the polygon was drawn.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset CreatedAt => createdAt;

        /// <summary>
        /// Gets the email of the user who drew the polygon.
        /// </summary>
        [JsonIgnore]
        public string CreatedBy => createdBy;

        /// <summary>
        /// Gets the server-assigned identifier of the drawn polygon; a stable GUID.
        /// </summary>
        [JsonIgnore]
        public string ObjectId => objectId;

        /// <summary>
        /// Gets a clone of the drawn polygon in EPSG:2180, easting in X.
        /// </summary>
        [JsonIgnore]
        public Polygon2D? Polygon2D => Core.Query.Clone(polygon2D);
    }
}
