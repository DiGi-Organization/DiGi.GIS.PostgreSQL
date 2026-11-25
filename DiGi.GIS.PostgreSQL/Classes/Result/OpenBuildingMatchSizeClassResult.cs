using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The omission and commission of one footprint size class within an <see cref="OpenBuildingMatchSummaryResult"/> (DiGi.GIS.PostgreSQL#127).
    /// <para>A footprint belongs to the class of its own area: a BDOT10k footprint counts towards <see cref="BDOTCount"/> and, when no dataset footprint reached it, towards <see cref="OmissionCount"/>; a dataset footprint towards <see cref="DatasetCount"/> and <see cref="CommissionCount"/>. Excluded footprints (below the run's minimum area) are not counted. On a per-source row the BDOT10k figures are null, as on the row itself. The classes follow <see cref="Constants.OpenBuildingMatchSizeClass.Bounds"/>.</para>
    /// </summary>
    public class OpenBuildingMatchSizeClassResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(BDOTCount))]
        private readonly long? bDOTCount;

        [JsonInclude, JsonPropertyName(nameof(CommissionCount))]
        private readonly long commissionCount;

        [JsonInclude, JsonPropertyName(nameof(DatasetCount))]
        private readonly long datasetCount;

        [JsonInclude, JsonPropertyName(nameof(MaxArea))]
        private readonly double? maxArea;

        [JsonInclude, JsonPropertyName(nameof(MinArea))]
        private readonly double minArea;

        [JsonInclude, JsonPropertyName(nameof(OmissionCount))]
        private readonly long? omissionCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchSizeClassResult"/> class.
        /// </summary>
        /// <param name="minArea">The lower bound of the class in square metres, inclusive.</param>
        /// <param name="maxArea">The upper bound of the class in square metres, exclusive, or null for the open-ended last class.</param>
        /// <param name="bDOTCount">The BDOT10k footprints of the class, or null on a per-source row.</param>
        /// <param name="omissionCount">The BDOT10k footprints of the class no dataset footprint reached, or null on a per-source row.</param>
        /// <param name="datasetCount">The dataset footprints of the class.</param>
        /// <param name="commissionCount">The dataset footprints of the class no BDOT10k footprint reached.</param>
        public OpenBuildingMatchSizeClassResult(double minArea, double? maxArea, long? bDOTCount, long? omissionCount, long datasetCount, long commissionCount)
        {
            this.minArea = minArea;
            this.maxArea = maxArea;
            this.bDOTCount = bDOTCount;
            this.omissionCount = omissionCount;
            this.datasetCount = datasetCount;
            this.commissionCount = commissionCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchSizeClassResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="openBuildingMatchSizeClassResult">The <see cref="OpenBuildingMatchSizeClassResult"/> to copy from.</param>
        public OpenBuildingMatchSizeClassResult(OpenBuildingMatchSizeClassResult? openBuildingMatchSizeClassResult)
            : base(openBuildingMatchSizeClassResult)
        {
            if (openBuildingMatchSizeClassResult is not null)
            {
                minArea = openBuildingMatchSizeClassResult.minArea;
                maxArea = openBuildingMatchSizeClassResult.maxArea;
                bDOTCount = openBuildingMatchSizeClassResult.bDOTCount;
                omissionCount = openBuildingMatchSizeClassResult.omissionCount;
                datasetCount = openBuildingMatchSizeClassResult.datasetCount;
                commissionCount = openBuildingMatchSizeClassResult.commissionCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchSizeClassResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OpenBuildingMatchSizeClassResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the BDOT10k footprints of the class, or null on a per-source row.
        /// </summary>
        [JsonIgnore]
        public long? BDOTCount => bDOTCount;

        /// <summary>
        /// Gets the dataset footprints of the class no BDOT10k footprint reached.
        /// </summary>
        [JsonIgnore]
        public long CommissionCount => commissionCount;

        /// <summary>
        /// Gets the commission count over the dataset count of the class, or null when the class holds no dataset footprint.
        /// </summary>
        [JsonIgnore]
        public double? CommissionRate => datasetCount > 0 ? (double)commissionCount / datasetCount : null;

        /// <summary>
        /// Gets the dataset footprints of the class.
        /// </summary>
        [JsonIgnore]
        public long DatasetCount => datasetCount;

        /// <summary>
        /// Gets the upper bound of the class in square metres, exclusive, or null for the open-ended last class.
        /// </summary>
        [JsonIgnore]
        public double? MaxArea => maxArea;

        /// <summary>
        /// Gets the lower bound of the class in square metres, inclusive.
        /// </summary>
        [JsonIgnore]
        public double MinArea => minArea;

        /// <summary>
        /// Gets the BDOT10k footprints of the class no dataset footprint reached, or null on a per-source row.
        /// </summary>
        [JsonIgnore]
        public long? OmissionCount => omissionCount;

        /// <summary>
        /// Gets the omission count over the BDOT10k count of the class, or null on a per-source row or when the class holds no BDOT10k footprint.
        /// </summary>
        [JsonIgnore]
        public double? OmissionRate => bDOTCount is long count && count > 0 && omissionCount is long omission ? (double)omission / count : null;
    }
}
