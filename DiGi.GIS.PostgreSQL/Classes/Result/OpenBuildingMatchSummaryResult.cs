using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The completeness and geometry measures of one open building dataset against BDOT10k over one scope, read from <c>building_dataset_match</c> (DiGi.GIS.PostgreSQL#127; paper 01, result blocks 2–3; endpoint E4, DiGi.GIS.WebAPI#108).
    /// <para>One total row per dataset (<see cref="PrimarySource"/> null) and one row per normalised primary source of the dataset's footprints. A primary source is an attribute of the dataset side, so a per-source row carries the dataset-side figures only: its BDOT10k count, omission count and omission rate are null, because an omission is a BDOT10k footprint no dataset footprint reached and belongs to no source. The footprint counts of the per-source rows (<see cref="DatasetCount"/>, <see cref="DatasetExcludedCount"/>, <see cref="CommissionCount"/>) sum to the total row's; the class counts need not, because a component whose dataset footprints come from two sources is counted under each of them.</para>
    /// <para>Counts: <see cref="BDOTCount"/> and <see cref="DatasetCount"/> are distinct footprints matched or not (the excluded ones are counted apart); the class counts are components, so a one-to-many component of one dataset footprint and three BDOT10k footprints counts once. The geometry measures are over the one-to-one rows only, the weak ones left out; medians and the tenth percentile are linear-interpolated percentiles (<c>percentile_cont</c>).</para>
    /// </summary>
    public class OpenBuildingMatchSummaryResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AreaRatioMedian))]
        private readonly double? areaRatioMedian;

        [JsonInclude, JsonPropertyName(nameof(BDOTCount))]
        private readonly long? bDOTCount;

        [JsonInclude, JsonPropertyName(nameof(BDOTExcludedCount))]
        private readonly long? bDOTExcludedCount;

        [JsonInclude, JsonPropertyName(nameof(CentroidDistanceMedian))]
        private readonly double? centroidDistanceMedian;

        [JsonInclude, JsonPropertyName(nameof(CommissionCount))]
        private readonly long commissionCount;

        [JsonInclude, JsonPropertyName(nameof(CommissionRate))]
        private readonly double? commissionRate;

        [JsonInclude, JsonPropertyName(nameof(ComputedAt))]
        private readonly DateTimeOffset? computedAt;

        [JsonInclude, JsonPropertyName(nameof(DatasetCount))]
        private readonly long datasetCount;

        [JsonInclude, JsonPropertyName(nameof(DatasetExcludedCount))]
        private readonly long datasetExcludedCount;

        [JsonInclude, JsonPropertyName(nameof(DatasetId))]
        private readonly int datasetId;

        [JsonInclude, JsonPropertyName(nameof(DatasetKey))]
        private readonly string? datasetKey;

        [JsonInclude, JsonPropertyName(nameof(DxMean))]
        private readonly double? dxMean;

        [JsonInclude, JsonPropertyName(nameof(DxMedian))]
        private readonly double? dxMedian;

        [JsonInclude, JsonPropertyName(nameof(DyMean))]
        private readonly double? dyMean;

        [JsonInclude, JsonPropertyName(nameof(DyMedian))]
        private readonly double? dyMedian;

        [JsonInclude, JsonPropertyName(nameof(IoUMean))]
        private readonly double? ioUMean;

        [JsonInclude, JsonPropertyName(nameof(IoUMedian))]
        private readonly double? ioUMedian;

        [JsonInclude, JsonPropertyName(nameof(IoUP10))]
        private readonly double? ioUP10;

        [JsonInclude, JsonPropertyName(nameof(ManyToManyCount))]
        private readonly long manyToManyCount;

        [JsonInclude, JsonPropertyName(nameof(ManyToOneCount))]
        private readonly long manyToOneCount;

        [JsonInclude, JsonPropertyName(nameof(OmissionCount))]
        private readonly long? omissionCount;

        [JsonInclude, JsonPropertyName(nameof(OmissionRate))]
        private readonly double? omissionRate;

        [JsonInclude, JsonPropertyName(nameof(OneToManyCount))]
        private readonly long oneToManyCount;

        [JsonInclude, JsonPropertyName(nameof(OneToOneCount))]
        private readonly long oneToOneCount;

        [JsonInclude, JsonPropertyName(nameof(OneToOneWeakCount))]
        private readonly long oneToOneWeakCount;

        [JsonInclude, JsonPropertyName(nameof(OpenBuildingMatchParameters))]
        private readonly OpenBuildingMatchParameters? openBuildingMatchParameters;

        [JsonInclude, JsonPropertyName(nameof(PrimarySource))]
        private readonly string? primarySource;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchSummaryResult"/> class.
        /// </summary>
        /// <param name="datasetKey">The dataset key (<c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c>).</param>
        /// <param name="datasetId">The <c>building_dataset</c> register row the links were computed for.</param>
        /// <param name="primarySource">The normalised primary source the row is restricted to, or null for the dataset's total row.</param>
        /// <param name="bDOTCount">The distinct BDOT10k footprints matched or omitted, or null on a per-source row.</param>
        /// <param name="datasetCount">The distinct dataset footprints matched or committed.</param>
        /// <param name="bDOTExcludedCount">The BDOT10k footprints below the minimum area, or null on a per-source row.</param>
        /// <param name="datasetExcludedCount">The dataset footprints below the minimum area.</param>
        /// <param name="oneToOneCount">The one-to-one components.</param>
        /// <param name="oneToOneWeakCount">The one-to-one components below the IoU threshold.</param>
        /// <param name="oneToManyCount">The components of one dataset footprint and several BDOT10k footprints.</param>
        /// <param name="manyToOneCount">The components of several dataset footprints and one BDOT10k footprint.</param>
        /// <param name="manyToManyCount">The components of several footprints on both sides.</param>
        /// <param name="omissionCount">The BDOT10k footprints no dataset footprint reached, or null on a per-source row.</param>
        /// <param name="commissionCount">The dataset footprints no BDOT10k footprint reached.</param>
        /// <param name="omissionRate">The omission count over the BDOT10k count, or null on a per-source row or when there is no BDOT10k footprint.</param>
        /// <param name="commissionRate">The commission count over the dataset count, or null when there is no dataset footprint.</param>
        /// <param name="ioUMean">The mean intersection over union of the one-to-one pairs, or null when there is none.</param>
        /// <param name="ioUMedian">The median intersection over union of the one-to-one pairs, or null when there is none.</param>
        /// <param name="ioUP10">The tenth percentile of the intersection over union of the one-to-one pairs, or null when there is none.</param>
        /// <param name="areaRatioMedian">The median of the dataset-to-BDOT10k area ratio of the one-to-one pairs, or null when there is none.</param>
        /// <param name="centroidDistanceMedian">The median centroid distance of the one-to-one pairs in metres, or null when there is none.</param>
        /// <param name="dxMean">The mean X offset of the one-to-one pairs in metres (the systematic offset), or null when there is none.</param>
        /// <param name="dxMedian">The median X offset of the one-to-one pairs in metres, or null when there is none.</param>
        /// <param name="dyMean">The mean Y offset of the one-to-one pairs in metres (the systematic offset), or null when there is none.</param>
        /// <param name="dyMedian">The median Y offset of the one-to-one pairs in metres, or null when there is none.</param>
        /// <param name="openBuildingMatchParameters">The rule parameters the links were computed with, or null when the scope mixes snapshots computed with different parameters.</param>
        /// <param name="computedAt">When the oldest snapshot in scope was computed, or null when not known.</param>
        public OpenBuildingMatchSummaryResult(
            string? datasetKey,
            int datasetId,
            string? primarySource,
            long? bDOTCount,
            long datasetCount,
            long? bDOTExcludedCount,
            long datasetExcludedCount,
            long oneToOneCount,
            long oneToOneWeakCount,
            long oneToManyCount,
            long manyToOneCount,
            long manyToManyCount,
            long? omissionCount,
            long commissionCount,
            double? omissionRate,
            double? commissionRate,
            double? ioUMean,
            double? ioUMedian,
            double? ioUP10,
            double? areaRatioMedian,
            double? centroidDistanceMedian,
            double? dxMean,
            double? dxMedian,
            double? dyMean,
            double? dyMedian,
            OpenBuildingMatchParameters? openBuildingMatchParameters,
            DateTimeOffset? computedAt)
        {
            this.datasetKey = datasetKey;
            this.datasetId = datasetId;
            this.primarySource = primarySource;
            this.bDOTCount = bDOTCount;
            this.datasetCount = datasetCount;
            this.bDOTExcludedCount = bDOTExcludedCount;
            this.datasetExcludedCount = datasetExcludedCount;
            this.oneToOneCount = oneToOneCount;
            this.oneToOneWeakCount = oneToOneWeakCount;
            this.oneToManyCount = oneToManyCount;
            this.manyToOneCount = manyToOneCount;
            this.manyToManyCount = manyToManyCount;
            this.omissionCount = omissionCount;
            this.commissionCount = commissionCount;
            this.omissionRate = omissionRate;
            this.commissionRate = commissionRate;
            this.ioUMean = ioUMean;
            this.ioUMedian = ioUMedian;
            this.ioUP10 = ioUP10;
            this.areaRatioMedian = areaRatioMedian;
            this.centroidDistanceMedian = centroidDistanceMedian;
            this.dxMean = dxMean;
            this.dxMedian = dxMedian;
            this.dyMean = dyMean;
            this.dyMedian = dyMedian;
            this.openBuildingMatchParameters = openBuildingMatchParameters is null ? null : new OpenBuildingMatchParameters(openBuildingMatchParameters);
            this.computedAt = computedAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchSummaryResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="openBuildingMatchSummaryResult">The <see cref="OpenBuildingMatchSummaryResult"/> to copy from.</param>
        public OpenBuildingMatchSummaryResult(OpenBuildingMatchSummaryResult? openBuildingMatchSummaryResult)
            : base(openBuildingMatchSummaryResult)
        {
            if (openBuildingMatchSummaryResult is not null)
            {
                datasetKey = openBuildingMatchSummaryResult.datasetKey;
                datasetId = openBuildingMatchSummaryResult.datasetId;
                primarySource = openBuildingMatchSummaryResult.primarySource;
                bDOTCount = openBuildingMatchSummaryResult.bDOTCount;
                datasetCount = openBuildingMatchSummaryResult.datasetCount;
                bDOTExcludedCount = openBuildingMatchSummaryResult.bDOTExcludedCount;
                datasetExcludedCount = openBuildingMatchSummaryResult.datasetExcludedCount;
                oneToOneCount = openBuildingMatchSummaryResult.oneToOneCount;
                oneToOneWeakCount = openBuildingMatchSummaryResult.oneToOneWeakCount;
                oneToManyCount = openBuildingMatchSummaryResult.oneToManyCount;
                manyToOneCount = openBuildingMatchSummaryResult.manyToOneCount;
                manyToManyCount = openBuildingMatchSummaryResult.manyToManyCount;
                omissionCount = openBuildingMatchSummaryResult.omissionCount;
                commissionCount = openBuildingMatchSummaryResult.commissionCount;
                omissionRate = openBuildingMatchSummaryResult.omissionRate;
                commissionRate = openBuildingMatchSummaryResult.commissionRate;
                ioUMean = openBuildingMatchSummaryResult.ioUMean;
                ioUMedian = openBuildingMatchSummaryResult.ioUMedian;
                ioUP10 = openBuildingMatchSummaryResult.ioUP10;
                areaRatioMedian = openBuildingMatchSummaryResult.areaRatioMedian;
                centroidDistanceMedian = openBuildingMatchSummaryResult.centroidDistanceMedian;
                dxMean = openBuildingMatchSummaryResult.dxMean;
                dxMedian = openBuildingMatchSummaryResult.dxMedian;
                dyMean = openBuildingMatchSummaryResult.dyMean;
                dyMedian = openBuildingMatchSummaryResult.dyMedian;
                openBuildingMatchParameters = Core.Query.Clone(openBuildingMatchSummaryResult.openBuildingMatchParameters);
                computedAt = openBuildingMatchSummaryResult.computedAt;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchSummaryResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OpenBuildingMatchSummaryResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the median of the dataset-to-BDOT10k area ratio of the one-to-one pairs, or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? AreaRatioMedian => areaRatioMedian;

        /// <summary>
        /// Gets the distinct BDOT10k footprints matched or omitted, or null on a per-source row.
        /// </summary>
        [JsonIgnore]
        public long? BDOTCount => bDOTCount;

        /// <summary>
        /// Gets the BDOT10k footprints below the minimum area, or null on a per-source row.
        /// </summary>
        [JsonIgnore]
        public long? BDOTExcludedCount => bDOTExcludedCount;

        /// <summary>
        /// Gets the median centroid distance of the one-to-one pairs in metres, or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? CentroidDistanceMedian => centroidDistanceMedian;

        /// <summary>
        /// Gets the dataset footprints no BDOT10k footprint reached.
        /// </summary>
        [JsonIgnore]
        public long CommissionCount => commissionCount;

        /// <summary>
        /// Gets the commission count over the dataset count, or null when there is no dataset footprint.
        /// </summary>
        [JsonIgnore]
        public double? CommissionRate => commissionRate;

        /// <summary>
        /// Gets when the oldest snapshot in scope was computed, or null when not known. Compare it with the BDOT10k import: a BDOT10k re-import on the Main database cannot invalidate the links on Storage in the same transaction.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? ComputedAt => computedAt;

        /// <summary>
        /// Gets the distinct dataset footprints matched or committed.
        /// </summary>
        [JsonIgnore]
        public long DatasetCount => datasetCount;

        /// <summary>
        /// Gets the dataset footprints below the minimum area.
        /// </summary>
        [JsonIgnore]
        public long DatasetExcludedCount => datasetExcludedCount;

        /// <summary>
        /// Gets the <c>building_dataset</c> register row the links were computed for.
        /// </summary>
        [JsonIgnore]
        public int DatasetId => datasetId;

        /// <summary>
        /// Gets the dataset key.
        /// </summary>
        [JsonIgnore]
        public string? DatasetKey => datasetKey;

        /// <summary>
        /// Gets the mean X offset of the one-to-one pairs in metres - the systematic offset, where the ETRS89 versus WGS84 residual (DiGi.GIS#21) shows - or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? DxMean => dxMean;

        /// <summary>
        /// Gets the median X offset of the one-to-one pairs in metres, or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? DxMedian => dxMedian;

        /// <summary>
        /// Gets the mean Y offset of the one-to-one pairs in metres - the systematic offset - or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? DyMean => dyMean;

        /// <summary>
        /// Gets the median Y offset of the one-to-one pairs in metres, or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? DyMedian => dyMedian;

        /// <summary>
        /// Gets the mean intersection over union of the one-to-one pairs, or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? IoUMean => ioUMean;

        /// <summary>
        /// Gets the median intersection over union of the one-to-one pairs, or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? IoUMedian => ioUMedian;

        /// <summary>
        /// Gets the tenth percentile of the intersection over union of the one-to-one pairs, or null when there is none.
        /// </summary>
        [JsonIgnore]
        public double? IoUP10 => ioUP10;

        /// <summary>
        /// Gets the components of several footprints on both sides.
        /// </summary>
        [JsonIgnore]
        public long ManyToManyCount => manyToManyCount;

        /// <summary>
        /// Gets the components of several dataset footprints and one BDOT10k footprint.
        /// </summary>
        [JsonIgnore]
        public long ManyToOneCount => manyToOneCount;

        /// <summary>
        /// Gets the BDOT10k footprints no dataset footprint reached, or null on a per-source row.
        /// </summary>
        [JsonIgnore]
        public long? OmissionCount => omissionCount;

        /// <summary>
        /// Gets the omission count over the BDOT10k count, or null on a per-source row or when there is no BDOT10k footprint.
        /// </summary>
        [JsonIgnore]
        public double? OmissionRate => omissionRate;

        /// <summary>
        /// Gets the components of one dataset footprint and several BDOT10k footprints.
        /// </summary>
        [JsonIgnore]
        public long OneToManyCount => oneToManyCount;

        /// <summary>
        /// Gets the one-to-one components.
        /// </summary>
        [JsonIgnore]
        public long OneToOneCount => oneToOneCount;

        /// <summary>
        /// Gets the one-to-one components below the IoU threshold.
        /// </summary>
        [JsonIgnore]
        public long OneToOneWeakCount => oneToOneWeakCount;

        /// <summary>
        /// Gets the rule parameters the links were computed with, or null when the scope mixes snapshots computed with different parameters.
        /// </summary>
        [JsonIgnore]
        public OpenBuildingMatchParameters? OpenBuildingMatchParameters => openBuildingMatchParameters;

        /// <summary>
        /// Gets the normalised primary source the row is restricted to, or null for the dataset's total row.
        /// </summary>
        [JsonIgnore]
        public string? PrimarySource => primarySource;
    }
}
