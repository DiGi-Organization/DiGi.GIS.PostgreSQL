using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents, for one subdivision, the occupancy figure it carries next to the sum of the occupancy rows stored for the buildings filed under it, so that conservation of the distributed figure can be checked.
    /// </summary>
    public class Building2DOccupancySubdivisionResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(SubdivisionId))]
        private readonly int subdivisionId;

        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int? countyId;

        [JsonInclude, JsonPropertyName(nameof(Figure))]
        private readonly double? figure;

        [JsonInclude, JsonPropertyName(nameof(StoredSum))]
        private readonly double storedSum;

        [JsonInclude, JsonPropertyName(nameof(StoredCount))]
        private readonly long storedCount;

        [JsonInclude, JsonPropertyName(nameof(ContainedSubdivisionCount))]
        private readonly long containedSubdivisionCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DOccupancySubdivisionResult"/> class.
        /// </summary>
        /// <param name="subdivisionId">The identifier of the subdivision the buildings are filed under.</param>
        /// <param name="countyId">The county part identifier carried by the subdivision row, or null when the row is missing.</param>
        /// <param name="figure">The occupancy figure of the subdivision, or null when it carries none.</param>
        /// <param name="storedSum">The sum of the stored occupancies of the buildings filed under the subdivision.</param>
        /// <param name="storedCount">The number of those buildings holding an occupancy row.</param>
        /// <param name="containedSubdivisionCount">The number of other subdivisions of the same municipality whose bounding box lies within this one's - zero means the subdivision holds no other, so its figure should be stored in full.</param>
        public Building2DOccupancySubdivisionResult(int subdivisionId, int? countyId, double? figure, double storedSum, long storedCount, long containedSubdivisionCount)
        {
            this.subdivisionId = subdivisionId;
            this.countyId = countyId;
            this.figure = figure;
            this.storedSum = storedSum;
            this.storedCount = storedCount;
            this.containedSubdivisionCount = containedSubdivisionCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DOccupancySubdivisionResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="building2DOccupancySubdivisionResult">The <see cref="Building2DOccupancySubdivisionResult"/> instance to copy.</param>
        public Building2DOccupancySubdivisionResult(Building2DOccupancySubdivisionResult? building2DOccupancySubdivisionResult)
            : base(building2DOccupancySubdivisionResult)
        {
            if (building2DOccupancySubdivisionResult is not null)
            {
                subdivisionId = building2DOccupancySubdivisionResult.subdivisionId;
                countyId = building2DOccupancySubdivisionResult.countyId;
                figure = building2DOccupancySubdivisionResult.figure;
                storedSum = building2DOccupancySubdivisionResult.storedSum;
                storedCount = building2DOccupancySubdivisionResult.storedCount;
                containedSubdivisionCount = building2DOccupancySubdivisionResult.containedSubdivisionCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DOccupancySubdivisionResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the serialized data.</param>
        public Building2DOccupancySubdivisionResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the subdivision the buildings are filed under.
        /// </summary>
        [JsonIgnore]
        public int SubdivisionId => subdivisionId;

        /// <summary>
        /// Gets the county part identifier carried by the subdivision row, or null when the row is missing.
        /// </summary>
        [JsonIgnore]
        public int? CountyId => countyId;

        /// <summary>
        /// Gets the occupancy figure of the subdivision, or null when it carries none.
        /// </summary>
        [JsonIgnore]
        public double? Figure => figure;

        /// <summary>
        /// Gets the sum of the stored occupancies of the buildings filed under the subdivision.
        /// </summary>
        [JsonIgnore]
        public double StoredSum => storedSum;

        /// <summary>
        /// Gets the number of those buildings holding an occupancy row.
        /// </summary>
        [JsonIgnore]
        public long StoredCount => storedCount;

        /// <summary>
        /// Gets the number of other subdivisions of the same municipality whose bounding box lies within this one's. Zero means the subdivision holds no other, so its figure should be stored in full; a larger count is an over-approximation of nesting, as a box inside a box does not prove a polygon inside a polygon.
        /// </summary>
        [JsonIgnore]
        public long ContainedSubdivisionCount => containedSubdivisionCount;
    }
}
