using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The interpreted reference sample of one test area at one imagery epoch, compared with BDOT10k and, optionally, with the open building datasets (paper 01, sections 4.4 and 5.2; DiGi.GIS.WebAPI.UI#83).
    /// <para>Labels are resolved per object: when every interpreter who did not answer indeterminate agrees, that state stands; when they disagree, the object counts as indeterminate and in <see cref="DisagreementCount"/>. BDOT10k footprints labelled absent are its commission; drawn objects labelled present are buildings it omits.</para>
    /// </summary>
    public class OrtoAreaValidationResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AreaKey))]
        private readonly string? areaKey;

        [JsonInclude, JsonPropertyName(nameof(DatasetValidationResults))]
        private readonly List<OrtoAreaDatasetValidationResult>? datasetValidationResults;

        [JsonInclude, JsonPropertyName(nameof(DisagreementCount))]
        private readonly long disagreementCount;

        [JsonInclude, JsonPropertyName(nameof(DrawnAbsentCount))]
        private readonly long drawnAbsentCount;

        [JsonInclude, JsonPropertyName(nameof(DrawnIndeterminateCount))]
        private readonly long drawnIndeterminateCount;

        [JsonInclude, JsonPropertyName(nameof(DrawnPresentCount))]
        private readonly long drawnPresentCount;

        [JsonInclude, JsonPropertyName(nameof(EpochYear))]
        private readonly short? epochYear;

        [JsonInclude, JsonPropertyName(nameof(FootprintAbsentCount))]
        private readonly long footprintAbsentCount;

        [JsonInclude, JsonPropertyName(nameof(FootprintIndeterminateCount))]
        private readonly long footprintIndeterminateCount;

        [JsonInclude, JsonPropertyName(nameof(FootprintPresentCount))]
        private readonly long footprintPresentCount;

        [JsonInclude, JsonPropertyName(nameof(InterpreterCount))]
        private readonly int interpreterCount;

        [JsonInclude, JsonPropertyName(nameof(Stratum))]
        private readonly string? stratum;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaValidationResult"/> class.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="stratum">The stratum of the test area, or null when unrecorded.</param>
        /// <param name="epochYear">The imagery epoch the labels interpret, or null when the area holds no label.</param>
        /// <param name="interpreterCount">The number of distinct interpreters who labelled the epoch.</param>
        /// <param name="footprintPresentCount">BDOT10k footprints resolved as present.</param>
        /// <param name="footprintAbsentCount">BDOT10k footprints resolved as absent - its commission.</param>
        /// <param name="footprintIndeterminateCount">BDOT10k footprints resolved as indeterminate.</param>
        /// <param name="drawnPresentCount">Drawn objects resolved as present - buildings BDOT10k omits.</param>
        /// <param name="drawnAbsentCount">Drawn objects resolved as absent.</param>
        /// <param name="drawnIndeterminateCount">Drawn objects resolved as indeterminate.</param>
        /// <param name="disagreementCount">Objects whose interpreters disagreed; already counted as indeterminate.</param>
        /// <param name="datasetValidationResults">The comparison with each open building dataset requested, or null when none was.</param>
        public OrtoAreaValidationResult(
            string? areaKey,
            string? stratum,
            short? epochYear,
            int interpreterCount,
            long footprintPresentCount,
            long footprintAbsentCount,
            long footprintIndeterminateCount,
            long drawnPresentCount,
            long drawnAbsentCount,
            long drawnIndeterminateCount,
            long disagreementCount,
            IEnumerable<OrtoAreaDatasetValidationResult>? datasetValidationResults)
        {
            this.areaKey = areaKey;
            this.stratum = stratum;
            this.epochYear = epochYear;
            this.interpreterCount = interpreterCount;
            this.footprintPresentCount = footprintPresentCount;
            this.footprintAbsentCount = footprintAbsentCount;
            this.footprintIndeterminateCount = footprintIndeterminateCount;
            this.drawnPresentCount = drawnPresentCount;
            this.drawnAbsentCount = drawnAbsentCount;
            this.drawnIndeterminateCount = drawnIndeterminateCount;
            this.disagreementCount = disagreementCount;
            this.datasetValidationResults = datasetValidationResults is null ? null : [.. datasetValidationResults.Select(x => new OrtoAreaDatasetValidationResult(x))];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaValidationResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="ortoAreaValidationResult">The <see cref="OrtoAreaValidationResult"/> to copy from.</param>
        public OrtoAreaValidationResult(OrtoAreaValidationResult? ortoAreaValidationResult)
            : base(ortoAreaValidationResult)
        {
            if (ortoAreaValidationResult is not null)
            {
                areaKey = ortoAreaValidationResult.areaKey;
                stratum = ortoAreaValidationResult.stratum;
                epochYear = ortoAreaValidationResult.epochYear;
                interpreterCount = ortoAreaValidationResult.interpreterCount;
                footprintPresentCount = ortoAreaValidationResult.footprintPresentCount;
                footprintAbsentCount = ortoAreaValidationResult.footprintAbsentCount;
                footprintIndeterminateCount = ortoAreaValidationResult.footprintIndeterminateCount;
                drawnPresentCount = ortoAreaValidationResult.drawnPresentCount;
                drawnAbsentCount = ortoAreaValidationResult.drawnAbsentCount;
                drawnIndeterminateCount = ortoAreaValidationResult.drawnIndeterminateCount;
                disagreementCount = ortoAreaValidationResult.disagreementCount;
                datasetValidationResults = ortoAreaValidationResult.datasetValidationResults is null ? null : [.. ortoAreaValidationResult.datasetValidationResults.Select(x => new OrtoAreaDatasetValidationResult(x))];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaValidationResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OrtoAreaValidationResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the invariant location key of the test area.
        /// </summary>
        [JsonIgnore]
        public string? AreaKey => areaKey;

        /// <summary>
        /// Gets the share of BDOT10k footprints with a decided state that were seen as absent, or null when none was decided.
        /// </summary>
        [JsonIgnore]
        public double? BDOT10kCommissionRate => footprintPresentCount + footprintAbsentCount == 0 ? null : (double)footprintAbsentCount / (footprintPresentCount + footprintAbsentCount);

        /// <summary>
        /// Gets the share of reference objects BDOT10k omits (drawn objects seen as present), or null when the area has no reference object.
        /// </summary>
        [JsonIgnore]
        public double? BDOT10kOmissionRate => ReferenceCount == 0 ? null : (double)drawnPresentCount / ReferenceCount;

        /// <summary>
        /// Gets the comparison with each open building dataset requested, or null when none was.
        /// </summary>
        [JsonIgnore]
        public List<OrtoAreaDatasetValidationResult>? DatasetValidationResults => datasetValidationResults is null ? null : [.. datasetValidationResults.Select(x => new OrtoAreaDatasetValidationResult(x))];

        /// <summary>
        /// Gets the number of objects whose interpreters disagreed; they are already counted as indeterminate.
        /// </summary>
        [JsonIgnore]
        public long DisagreementCount => disagreementCount;

        /// <summary>
        /// Gets the number of drawn objects resolved as absent.
        /// </summary>
        [JsonIgnore]
        public long DrawnAbsentCount => drawnAbsentCount;

        /// <summary>
        /// Gets the number of drawn objects resolved as indeterminate.
        /// </summary>
        [JsonIgnore]
        public long DrawnIndeterminateCount => drawnIndeterminateCount;

        /// <summary>
        /// Gets the number of drawn objects resolved as present - buildings BDOT10k omits.
        /// </summary>
        [JsonIgnore]
        public long DrawnPresentCount => drawnPresentCount;

        /// <summary>
        /// Gets the imagery epoch the labels interpret, or null when the area holds no label.
        /// </summary>
        [JsonIgnore]
        public short? EpochYear => epochYear;

        /// <summary>
        /// Gets the number of BDOT10k footprints resolved as absent - its commission.
        /// </summary>
        [JsonIgnore]
        public long FootprintAbsentCount => footprintAbsentCount;

        /// <summary>
        /// Gets the number of BDOT10k footprints resolved as indeterminate.
        /// </summary>
        [JsonIgnore]
        public long FootprintIndeterminateCount => footprintIndeterminateCount;

        /// <summary>
        /// Gets the number of BDOT10k footprints resolved as present.
        /// </summary>
        [JsonIgnore]
        public long FootprintPresentCount => footprintPresentCount;

        /// <summary>
        /// Gets the share of labelled objects resolved as indeterminate, or null when nothing was labelled. Paper 01 reports it as part of the result (section 4.4).
        /// </summary>
        [JsonIgnore]
        public double? IndeterminateRate
        {
            get
            {
                long count = footprintPresentCount + footprintAbsentCount + footprintIndeterminateCount + drawnPresentCount + drawnAbsentCount + drawnIndeterminateCount;
                return count == 0 ? null : (double)(footprintIndeterminateCount + drawnIndeterminateCount) / count;
            }
        }

        /// <summary>
        /// Gets the number of distinct interpreters who labelled the epoch.
        /// </summary>
        [JsonIgnore]
        public int InterpreterCount => interpreterCount;

        /// <summary>
        /// Gets the number of reference objects: BDOT10k footprints and drawn objects resolved as present.
        /// </summary>
        [JsonIgnore]
        public long ReferenceCount => footprintPresentCount + drawnPresentCount;

        /// <summary>
        /// Gets the stratum of the test area, or null when unrecorded.
        /// </summary>
        [JsonIgnore]
        public string? Stratum => stratum;
    }
}
