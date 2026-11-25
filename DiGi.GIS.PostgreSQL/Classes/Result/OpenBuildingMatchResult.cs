using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// What one run of the open building matching did for one dataset and one county group (DiGi.GIS.PostgreSQL#127): how many footprints it read on each side, how many it could not use, how many link rows it produced and whether it stored them.
    /// <para>The counts describe the footprints filed under the group's own county parts; the margin read across the group's edge serves as candidates only. <see cref="MarginReachCount"/> counts the owned components that reach a footprint touching the edge of what was read: such a component may continue beyond the margin and be classified differently by the neighbouring group, so a non-zero count says the margin should grow.</para>
    /// </summary>
    public class OpenBuildingMatchResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(BDOTCount))]
        private readonly long bDOTCount;

        [JsonInclude, JsonPropertyName(nameof(BDOTRejectedCount))]
        private readonly long bDOTRejectedCount;

        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        private readonly List<int>? countyIds;

        [JsonInclude, JsonPropertyName(nameof(DatasetCount))]
        private readonly long datasetCount;

        [JsonInclude, JsonPropertyName(nameof(DatasetId))]
        private readonly int? datasetId;

        [JsonInclude, JsonPropertyName(nameof(DatasetKey))]
        private readonly string? datasetKey;

        [JsonInclude, JsonPropertyName(nameof(DatasetRejectedCount))]
        private readonly long datasetRejectedCount;

        [JsonInclude, JsonPropertyName(nameof(MarginReachCount))]
        private readonly long marginReachCount;

        [JsonInclude, JsonPropertyName(nameof(Persisted))]
        private readonly bool persisted;

        [JsonInclude, JsonPropertyName(nameof(RepairedCount))]
        private readonly long repairedCount;

        [JsonInclude, JsonPropertyName(nameof(RowCount))]
        private readonly long rowCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchResult"/> class.
        /// </summary>
        /// <param name="datasetKey">The dataset key.</param>
        /// <param name="datasetId">The <c>building_dataset</c> register row of the group's dataset footprints, or null when the group holds none.</param>
        /// <param name="countyIds">The county parts of the group.</param>
        /// <param name="bDOTCount">The BDOT10k footprints filed under the group.</param>
        /// <param name="datasetCount">The dataset footprints filed under the group.</param>
        /// <param name="bDOTRejectedCount">The BDOT10k footprints of the group whose geometry could not be used.</param>
        /// <param name="datasetRejectedCount">The dataset footprints of the group whose geometry could not be used.</param>
        /// <param name="repairedCount">The footprints read, on both sides and in the margin, whose invalid geometry was repaired before matching.</param>
        /// <param name="rowCount">The link rows the group produced.</param>
        /// <param name="marginReachCount">The owned components reaching a footprint that touches the edge of what was read.</param>
        /// <param name="persisted">Whether the link rows replaced the group's stored snapshot.</param>
        public OpenBuildingMatchResult(
            string? datasetKey,
            int? datasetId,
            IEnumerable<int>? countyIds,
            long bDOTCount,
            long datasetCount,
            long bDOTRejectedCount,
            long datasetRejectedCount,
            long repairedCount,
            long rowCount,
            long marginReachCount,
            bool persisted)
        {
            this.datasetKey = datasetKey;
            this.datasetId = datasetId;
            this.countyIds = countyIds is null ? null : [.. countyIds];
            this.bDOTCount = bDOTCount;
            this.datasetCount = datasetCount;
            this.bDOTRejectedCount = bDOTRejectedCount;
            this.datasetRejectedCount = datasetRejectedCount;
            this.repairedCount = repairedCount;
            this.rowCount = rowCount;
            this.marginReachCount = marginReachCount;
            this.persisted = persisted;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="openBuildingMatchResult">The <see cref="OpenBuildingMatchResult"/> to copy from.</param>
        public OpenBuildingMatchResult(OpenBuildingMatchResult? openBuildingMatchResult)
            : base(openBuildingMatchResult)
        {
            if (openBuildingMatchResult is not null)
            {
                datasetKey = openBuildingMatchResult.datasetKey;
                datasetId = openBuildingMatchResult.datasetId;
                countyIds = openBuildingMatchResult.countyIds is null ? null : [.. openBuildingMatchResult.countyIds];
                bDOTCount = openBuildingMatchResult.bDOTCount;
                datasetCount = openBuildingMatchResult.datasetCount;
                bDOTRejectedCount = openBuildingMatchResult.bDOTRejectedCount;
                datasetRejectedCount = openBuildingMatchResult.datasetRejectedCount;
                repairedCount = openBuildingMatchResult.repairedCount;
                rowCount = openBuildingMatchResult.rowCount;
                marginReachCount = openBuildingMatchResult.marginReachCount;
                persisted = openBuildingMatchResult.persisted;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OpenBuildingMatchResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the BDOT10k footprints filed under the group.
        /// </summary>
        [JsonIgnore]
        public long BDOTCount => bDOTCount;

        /// <summary>
        /// Gets the BDOT10k footprints of the group whose geometry could not be used.
        /// </summary>
        [JsonIgnore]
        public long BDOTRejectedCount => bDOTRejectedCount;

        /// <summary>
        /// Gets the county parts of the group.
        /// </summary>
        [JsonIgnore]
        public List<int>? CountyIds => countyIds is null ? null : [.. countyIds];

        /// <summary>
        /// Gets the dataset footprints filed under the group.
        /// </summary>
        [JsonIgnore]
        public long DatasetCount => datasetCount;

        /// <summary>
        /// Gets the <c>building_dataset</c> register row of the group's dataset footprints, or null when the group holds none.
        /// </summary>
        [JsonIgnore]
        public int? DatasetId => datasetId;

        /// <summary>
        /// Gets the dataset key.
        /// </summary>
        [JsonIgnore]
        public string? DatasetKey => datasetKey;

        /// <summary>
        /// Gets the dataset footprints of the group whose geometry could not be used.
        /// </summary>
        [JsonIgnore]
        public long DatasetRejectedCount => datasetRejectedCount;

        /// <summary>
        /// Gets the owned components reaching a footprint that touches the edge of what was read; non-zero means the boundary margin should grow.
        /// </summary>
        [JsonIgnore]
        public long MarginReachCount => marginReachCount;

        /// <summary>
        /// Gets whether the link rows replaced the group's stored snapshot.
        /// </summary>
        [JsonIgnore]
        public bool Persisted => persisted;

        /// <summary>
        /// Gets the footprints read, on both sides and in the margin, whose invalid geometry was repaired before matching.
        /// </summary>
        [JsonIgnore]
        public long RepairedCount => repairedCount;

        /// <summary>
        /// Gets the link rows the group produced.
        /// </summary>
        [JsonIgnore]
        public long RowCount => rowCount;
    }
}
