using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The outcome of importing the PRG address points of one county into <c>address_point</c>. See <c>Modify.UpdateAddressPointsAsync</c>.
    /// <para><see cref="Inside"/>, <see cref="Nearest"/> and <see cref="Unmatched"/> count the points assigned to a county part, by how they were matched to a building. <see cref="Outside"/> counts the points that lie in none of the county's parts and <see cref="Skipped"/> those without a PRG local identifier, a location or a name; neither is written. <see cref="Written"/> is the number of rows inserted or replaced, which is lower than the matched points when the table already holds a newer version of some of them.</para>
    /// </summary>
    public class AddressPointUpdateResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        private readonly List<int>? countyIds;

        [JsonInclude, JsonPropertyName(nameof(Inside))]
        private readonly int inside;

        [JsonInclude, JsonPropertyName(nameof(Nearest))]
        private readonly int nearest;

        [JsonInclude, JsonPropertyName(nameof(Unmatched))]
        private readonly int unmatched;

        [JsonInclude, JsonPropertyName(nameof(Outside))]
        private readonly int outside;

        [JsonInclude, JsonPropertyName(nameof(Skipped))]
        private readonly int skipped;

        [JsonInclude, JsonPropertyName(nameof(Written))]
        private readonly long written;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressPointUpdateResult"/> class.
        /// </summary>
        /// <param name="countyIds">The county part ids the points were assigned to.</param>
        /// <param name="inside">The number of points that lie inside a building footprint.</param>
        /// <param name="nearest">The number of points assigned to the nearest building footprint within the maximum distance.</param>
        /// <param name="unmatched">The number of points assigned to a county part but to no building.</param>
        /// <param name="outside">The number of points that lie in none of the county parts, which were not written.</param>
        /// <param name="skipped">The number of points without a PRG local identifier, a location or a street or locality name, which were not written.</param>
        /// <param name="written">The number of rows inserted or replaced.</param>
        public AddressPointUpdateResult(IEnumerable<int>? countyIds, int inside, int nearest, int unmatched, int outside, int skipped, long written)
        {
            this.countyIds = countyIds is null ? null : [.. countyIds];
            this.inside = inside;
            this.nearest = nearest;
            this.unmatched = unmatched;
            this.outside = outside;
            this.skipped = skipped;
            this.written = written;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressPointUpdateResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="addressPointUpdateResult">The <see cref="AddressPointUpdateResult"/> to copy from.</param>
        public AddressPointUpdateResult(AddressPointUpdateResult? addressPointUpdateResult)
            : base(addressPointUpdateResult)
        {
            if (addressPointUpdateResult is not null)
            {
                countyIds = addressPointUpdateResult.countyIds is null ? null : new List<int>(addressPointUpdateResult.countyIds);
                inside = addressPointUpdateResult.inside;
                nearest = addressPointUpdateResult.nearest;
                unmatched = addressPointUpdateResult.unmatched;
                outside = addressPointUpdateResult.outside;
                skipped = addressPointUpdateResult.skipped;
                written = addressPointUpdateResult.written;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressPointUpdateResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public AddressPointUpdateResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the county part ids the points were assigned to.
        /// </summary>
        [JsonIgnore]
        public List<int>? CountyIds => countyIds is null ? null : new List<int>(countyIds);

        /// <summary>
        /// Gets the number of points that lie inside a building footprint.
        /// </summary>
        [JsonIgnore]
        public int Inside => inside;

        /// <summary>
        /// Gets the number of points assigned to the nearest building footprint within the maximum distance.
        /// </summary>
        [JsonIgnore]
        public int Nearest => nearest;

        /// <summary>
        /// Gets the number of points assigned to a county part but to no building.
        /// </summary>
        [JsonIgnore]
        public int Unmatched => unmatched;

        /// <summary>
        /// Gets the number of points that lie in none of the county parts, which were not written.
        /// </summary>
        [JsonIgnore]
        public int Outside => outside;

        /// <summary>
        /// Gets the number of points without a PRG local identifier, a location or a street or locality name, which were not written.
        /// </summary>
        [JsonIgnore]
        public int Skipped => skipped;

        /// <summary>
        /// Gets the number of rows inserted or replaced.
        /// </summary>
        [JsonIgnore]
        public long Written => written;
    }
}
