using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// How one open building dataset compares with the interpreted reference sample of one test area (paper 01, result blocks 2 and 4.4; the independent check of DiGi.GIS.WebAPI.UI#83).
    /// <para>The reference sample is every object the interpreters saw as present: BDOT10k footprints labelled present and drawn objects (buildings BDOT10k lacks) labelled present. A reference object counts as detected when a dataset footprint contains its internal point or it contains the footprint's internal point. Dataset footprints are counted when the centre of their envelope lies inside the area, and each is classed by the labelled objects it matches: a present one, only absent ones, only indeterminate ones, or none.</para>
    /// </summary>
    public class OrtoAreaDatasetValidationResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(DatasetKey))]
        private readonly string? datasetKey;

        [JsonInclude, JsonPropertyName(nameof(DetectedReferenceCount))]
        private readonly long detectedReferenceCount;

        [JsonInclude, JsonPropertyName(nameof(FootprintCount))]
        private readonly long footprintCount;

        [JsonInclude, JsonPropertyName(nameof(MatchedAbsentFootprintCount))]
        private readonly long matchedAbsentFootprintCount;

        [JsonInclude, JsonPropertyName(nameof(MatchedIndeterminateFootprintCount))]
        private readonly long matchedIndeterminateFootprintCount;

        [JsonInclude, JsonPropertyName(nameof(MatchedPresentFootprintCount))]
        private readonly long matchedPresentFootprintCount;

        [JsonInclude, JsonPropertyName(nameof(ReferenceCount))]
        private readonly long referenceCount;

        [JsonInclude, JsonPropertyName(nameof(UnmatchedFootprintCount))]
        private readonly long unmatchedFootprintCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaDatasetValidationResult"/> class.
        /// </summary>
        /// <param name="datasetKey">The dataset compared (<c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c>).</param>
        /// <param name="referenceCount">The number of reference objects (labelled present) in the area.</param>
        /// <param name="detectedReferenceCount">How many reference objects a dataset footprint matches.</param>
        /// <param name="footprintCount">The number of dataset footprints whose envelope centre lies inside the area.</param>
        /// <param name="matchedPresentFootprintCount">How many of those footprints match at least one reference object.</param>
        /// <param name="matchedAbsentFootprintCount">How many match only objects labelled absent - confirmed commission.</param>
        /// <param name="matchedIndeterminateFootprintCount">How many match only objects labelled indeterminate.</param>
        /// <param name="unmatchedFootprintCount">How many match no labelled object at all - commission, or a building the interpreters did not draw.</param>
        public OrtoAreaDatasetValidationResult(
            string? datasetKey,
            long referenceCount,
            long detectedReferenceCount,
            long footprintCount,
            long matchedPresentFootprintCount,
            long matchedAbsentFootprintCount,
            long matchedIndeterminateFootprintCount,
            long unmatchedFootprintCount)
        {
            this.datasetKey = datasetKey;
            this.referenceCount = referenceCount;
            this.detectedReferenceCount = detectedReferenceCount;
            this.footprintCount = footprintCount;
            this.matchedPresentFootprintCount = matchedPresentFootprintCount;
            this.matchedAbsentFootprintCount = matchedAbsentFootprintCount;
            this.matchedIndeterminateFootprintCount = matchedIndeterminateFootprintCount;
            this.unmatchedFootprintCount = unmatchedFootprintCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaDatasetValidationResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="ortoAreaDatasetValidationResult">The <see cref="OrtoAreaDatasetValidationResult"/> to copy from.</param>
        public OrtoAreaDatasetValidationResult(OrtoAreaDatasetValidationResult? ortoAreaDatasetValidationResult)
            : base(ortoAreaDatasetValidationResult)
        {
            if (ortoAreaDatasetValidationResult is not null)
            {
                datasetKey = ortoAreaDatasetValidationResult.datasetKey;
                referenceCount = ortoAreaDatasetValidationResult.referenceCount;
                detectedReferenceCount = ortoAreaDatasetValidationResult.detectedReferenceCount;
                footprintCount = ortoAreaDatasetValidationResult.footprintCount;
                matchedPresentFootprintCount = ortoAreaDatasetValidationResult.matchedPresentFootprintCount;
                matchedAbsentFootprintCount = ortoAreaDatasetValidationResult.matchedAbsentFootprintCount;
                matchedIndeterminateFootprintCount = ortoAreaDatasetValidationResult.matchedIndeterminateFootprintCount;
                unmatchedFootprintCount = ortoAreaDatasetValidationResult.unmatchedFootprintCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaDatasetValidationResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OrtoAreaDatasetValidationResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the dataset compared.
        /// </summary>
        [JsonIgnore]
        public string? DatasetKey => datasetKey;

        /// <summary>
        /// Gets how many reference objects a dataset footprint matches.
        /// </summary>
        [JsonIgnore]
        public long DetectedReferenceCount => detectedReferenceCount;

        /// <summary>
        /// Gets the number of dataset footprints whose envelope centre lies inside the area.
        /// </summary>
        [JsonIgnore]
        public long FootprintCount => footprintCount;

        /// <summary>
        /// Gets how many dataset footprints match only objects labelled absent - confirmed commission.
        /// </summary>
        [JsonIgnore]
        public long MatchedAbsentFootprintCount => matchedAbsentFootprintCount;

        /// <summary>
        /// Gets how many dataset footprints match only objects labelled indeterminate.
        /// </summary>
        [JsonIgnore]
        public long MatchedIndeterminateFootprintCount => matchedIndeterminateFootprintCount;

        /// <summary>
        /// Gets how many dataset footprints match at least one reference object.
        /// </summary>
        [JsonIgnore]
        public long MatchedPresentFootprintCount => matchedPresentFootprintCount;

        /// <summary>
        /// Gets the share of reference objects the dataset omits, or null when the area has no reference object.
        /// </summary>
        [JsonIgnore]
        public double? OmissionRate => referenceCount == 0 ? null : (double)(referenceCount - detectedReferenceCount) / referenceCount;

        /// <summary>
        /// Gets the number of reference objects (labelled present) in the area.
        /// </summary>
        [JsonIgnore]
        public long ReferenceCount => referenceCount;

        /// <summary>
        /// Gets how many dataset footprints match no labelled object at all.
        /// <para>In a fully interpreted area this is commission; it can also be a building the interpreters missed, which is why it is kept apart from <see cref="MatchedAbsentFootprintCount"/>.</para>
        /// </summary>
        [JsonIgnore]
        public long UnmatchedFootprintCount => unmatchedFootprintCount;
    }
}
