using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents a read-only snapshot of the occupancy rows stored under one county part, taken to compare the stored occupancies before and after a re-run.
    /// </summary>
    public class Building2DOccupancySnapshotResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(Count))]
        private readonly long count;

        [JsonInclude, JsonPropertyName(nameof(OccupancySum))]
        private readonly double occupancySum;

        [JsonInclude, JsonPropertyName(nameof(Checksum))]
        private readonly string? checksum;

        [JsonInclude, JsonPropertyName(nameof(FiguredSubdivisionCount))]
        private readonly long figuredSubdivisionCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DOccupancySnapshotResult"/> class.
        /// </summary>
        /// <param name="countyId">The identifier of the county part the rows are filed under.</param>
        /// <param name="count">The number of occupancy rows.</param>
        /// <param name="occupancySum">The sum of the stored occupancies.</param>
        /// <param name="checksum">The lowercase hexadecimal SHA-256 of the <c>(reference, occupancy)</c> pairs ordered by reference.</param>
        /// <param name="figuredSubdivisionCount">The number of subdivisions holding at least two buildings with an occupancy row.</param>
        public Building2DOccupancySnapshotResult(int countyId, long count, double occupancySum, string? checksum, long figuredSubdivisionCount)
        {
            this.countyId = countyId;
            this.count = count;
            this.occupancySum = occupancySum;
            this.checksum = checksum;
            this.figuredSubdivisionCount = figuredSubdivisionCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DOccupancySnapshotResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="building2DOccupancySnapshotResult">The <see cref="Building2DOccupancySnapshotResult"/> instance to copy.</param>
        public Building2DOccupancySnapshotResult(Building2DOccupancySnapshotResult? building2DOccupancySnapshotResult)
            : base(building2DOccupancySnapshotResult)
        {
            if (building2DOccupancySnapshotResult is not null)
            {
                countyId = building2DOccupancySnapshotResult.countyId;
                count = building2DOccupancySnapshotResult.count;
                occupancySum = building2DOccupancySnapshotResult.occupancySum;
                checksum = building2DOccupancySnapshotResult.checksum;
                figuredSubdivisionCount = building2DOccupancySnapshotResult.figuredSubdivisionCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DOccupancySnapshotResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the serialized data.</param>
        public Building2DOccupancySnapshotResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the county part the rows are filed under.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the number of occupancy rows.
        /// </summary>
        [JsonIgnore]
        public long Count => count;

        /// <summary>
        /// Gets the sum of the stored occupancies.
        /// </summary>
        [JsonIgnore]
        public double OccupancySum => occupancySum;

        /// <summary>
        /// Gets the lowercase hexadecimal SHA-256 of the <c>(reference, occupancy)</c> pairs ordered by reference. Identical data gives an identical value, whatever the storage order.
        /// </summary>
        [JsonIgnore]
        public string? Checksum => checksum;

        /// <summary>
        /// Gets the number of subdivisions holding at least two buildings with an occupancy row - the ones in which the remainder draw can move occupancy between buildings.
        /// </summary>
        [JsonIgnore]
        public long FiguredSubdivisionCount => figuredSubdivisionCount;
    }
}
