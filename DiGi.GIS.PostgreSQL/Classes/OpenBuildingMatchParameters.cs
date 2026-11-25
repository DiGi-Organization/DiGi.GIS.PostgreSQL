using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The parameters of the rule that matches open building footprints to BDOT10k (DiGi.GIS.PostgreSQL#127). They are the sensitivity axes of paper 01 §4.9, so every one is a parameter rather than a constant, and the set a snapshot was computed with is stored on every link row (<c>building_dataset_match.parameters</c>).
    /// <para>The rule: a dataset footprint <i>d</i> and a BDOT10k footprint <i>b</i> are joined by an edge when their overlap covers at least <see cref="MinOverlapShare"/> of the smaller of the two (<c>area(d ∩ b) / min(area(d), area(b))</c>, with a positive overlap), or - only when <see cref="MaxCentroidDistance"/> is positive - when they do not overlap and their centroids are at most that far apart. The connected components of the edges are the matches: one <i>d</i> and one <i>b</i> is one-to-one when their intersection over union reaches <see cref="IoUThreshold"/> and one-to-one weak below it; one and several, several and one, several and several are the other classes; a lone <i>b</i> is an omission and a lone <i>d</i> a commission. Footprints smaller than <see cref="MinArea"/> are left out on both sides. Footprints are read <see cref="BoundaryMargin"/> beyond the county group, so a building on a county edge finds its partner across it.</para>
    /// <para>The defaults are the contract the rule was proposed with: 0.5, 0.3, 0 m², 0 m (off), 100 m.</para>
    /// </summary>
    public class OpenBuildingMatchParameters : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(BoundaryMargin))]
        private readonly double boundaryMargin = 100;

        [JsonInclude, JsonPropertyName(nameof(IoUThreshold))]
        private readonly double ioUThreshold = 0.5;

        [JsonInclude, JsonPropertyName(nameof(MaxCentroidDistance))]
        private readonly double maxCentroidDistance = 0;

        [JsonInclude, JsonPropertyName(nameof(MinArea))]
        private readonly double minArea = 0;

        [JsonInclude, JsonPropertyName(nameof(MinOverlapShare))]
        private readonly double minOverlapShare = 0.3;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchParameters"/> class.
        /// </summary>
        /// <param name="ioUThreshold">The intersection over union a one-to-one component needs to count as one-to-one rather than one-to-one weak.</param>
        /// <param name="minOverlapShare">The share of the smaller footprint the overlap must cover for the pair to be joined by an edge.</param>
        /// <param name="minArea">The area in square metres below which a footprint is left out of the matching, on both sides.</param>
        /// <param name="maxCentroidDistance">The centroid distance in metres within which two footprints that do not overlap are still joined by an edge; zero switches the rule off.</param>
        /// <param name="boundaryMargin">The distance in metres beyond the county group the footprints are read, so a building on a county edge finds its partner across it.</param>
        public OpenBuildingMatchParameters(double ioUThreshold = 0.5, double minOverlapShare = 0.3, double minArea = 0, double maxCentroidDistance = 0, double boundaryMargin = 100)
        {
            this.ioUThreshold = ioUThreshold;
            this.minOverlapShare = minOverlapShare;
            this.minArea = minArea;
            this.maxCentroidDistance = maxCentroidDistance;
            this.boundaryMargin = boundaryMargin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchParameters"/> class by copying an existing one.
        /// </summary>
        /// <param name="openBuildingMatchParameters">The <see cref="OpenBuildingMatchParameters"/> to copy from.</param>
        public OpenBuildingMatchParameters(OpenBuildingMatchParameters? openBuildingMatchParameters)
            : base(openBuildingMatchParameters)
        {
            if (openBuildingMatchParameters is not null)
            {
                ioUThreshold = openBuildingMatchParameters.ioUThreshold;
                minOverlapShare = openBuildingMatchParameters.minOverlapShare;
                minArea = openBuildingMatchParameters.minArea;
                maxCentroidDistance = openBuildingMatchParameters.maxCentroidDistance;
                boundaryMargin = openBuildingMatchParameters.boundaryMargin;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingMatchParameters"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized parameters.</param>
        public OpenBuildingMatchParameters(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the distance in metres beyond the county group the footprints are read, so a building on a county edge finds its partner across it.
        /// </summary>
        [JsonIgnore]
        public double BoundaryMargin => boundaryMargin;

        /// <summary>
        /// Gets the intersection over union a one-to-one component needs to count as one-to-one rather than one-to-one weak.
        /// </summary>
        [JsonIgnore]
        public double IoUThreshold => ioUThreshold;

        /// <summary>
        /// Gets the centroid distance in metres within which two footprints that do not overlap are still joined by an edge; zero switches the rule off.
        /// </summary>
        [JsonIgnore]
        public double MaxCentroidDistance => maxCentroidDistance;

        /// <summary>
        /// Gets the area in square metres below which a footprint is left out of the matching, on both sides.
        /// </summary>
        [JsonIgnore]
        public double MinArea => minArea;

        /// <summary>
        /// Gets the share of the smaller footprint the overlap must cover for the pair to be joined by an edge.
        /// </summary>
        [JsonIgnore]
        public double MinOverlapShare => minOverlapShare;
    }
}
