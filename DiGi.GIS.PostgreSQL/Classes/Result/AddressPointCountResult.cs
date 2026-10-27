using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The number of rows of <c>address_point</c>, by how each point was matched to a building footprint, for one county part or for the whole table. See <see cref="AddressPointPostgreSQLConverter.GetCountResultAsync(Npgsql.NpgsqlConnection?, int?, int, System.Threading.CancellationToken)"/>.
    /// <para><see cref="Total"/> counts every row, including any whose match type is none of <see cref="Inside"/>, <see cref="Nearest"/> and <see cref="Unmatched"/>, so it can exceed their sum. The figures describe the database the converter was built for, and nothing else.</para>
    /// </summary>
    public class AddressPointCountResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int? countyId;

        [JsonInclude, JsonPropertyName(nameof(Inside))]
        private readonly long inside;

        [JsonInclude, JsonPropertyName(nameof(Nearest))]
        private readonly long nearest;

        [JsonInclude, JsonPropertyName(nameof(Unmatched))]
        private readonly long unmatched;

        [JsonInclude, JsonPropertyName(nameof(Total))]
        private readonly long total;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressPointCountResult"/> class.
        /// </summary>
        /// <param name="countyId">The county part id the rows were counted under, or null for the whole table.</param>
        /// <param name="inside">The number of points that lie inside a building footprint.</param>
        /// <param name="nearest">The number of points assigned to the nearest building footprint within the maximum distance.</param>
        /// <param name="unmatched">The number of points assigned to a county part but to no building.</param>
        /// <param name="total">The number of rows counted, whatever their match type.</param>
        public AddressPointCountResult(int? countyId, long inside, long nearest, long unmatched, long total)
        {
            this.countyId = countyId;
            this.inside = inside;
            this.nearest = nearest;
            this.unmatched = unmatched;
            this.total = total;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressPointCountResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="addressPointCountResult">The <see cref="AddressPointCountResult"/> to copy from.</param>
        public AddressPointCountResult(AddressPointCountResult? addressPointCountResult)
            : base(addressPointCountResult)
        {
            if (addressPointCountResult is not null)
            {
                countyId = addressPointCountResult.countyId;
                inside = addressPointCountResult.inside;
                nearest = addressPointCountResult.nearest;
                unmatched = addressPointCountResult.unmatched;
                total = addressPointCountResult.total;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressPointCountResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public AddressPointCountResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the county part id the rows were counted under, or null for the whole table.
        /// </summary>
        [JsonIgnore]
        public int? CountyId => countyId;

        /// <summary>
        /// Gets the number of points that lie inside a building footprint.
        /// </summary>
        [JsonIgnore]
        public long Inside => inside;

        /// <summary>
        /// Gets the number of points assigned to the nearest building footprint within the maximum distance.
        /// </summary>
        [JsonIgnore]
        public long Nearest => nearest;

        /// <summary>
        /// Gets the number of points assigned to a county part but to no building.
        /// </summary>
        [JsonIgnore]
        public long Unmatched => unmatched;

        /// <summary>
        /// Gets the number of rows counted, whatever their match type.
        /// </summary>
        [JsonIgnore]
        public long Total => total;
    }
}
