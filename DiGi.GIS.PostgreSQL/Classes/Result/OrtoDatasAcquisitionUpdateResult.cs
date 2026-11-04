using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The outcome of an acquisition-date backfill of <c>orto_datas</c> (<see cref="Modify.UpdateOrtoDatasAcquisitionAsync"/>): the rows and images read, the rows whose acquisition rows (<c>orto_data_acquisition</c>) were written, and how many images an index sheet dated.
    /// </summary>
    public class OrtoDatasAcquisitionUpdateResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(Changed))]
        private readonly long changed;

        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        private readonly List<int>? countyIds;

        [JsonInclude, JsonPropertyName(nameof(Dated))]
        private readonly long dated;

        [JsonInclude, JsonPropertyName(nameof(Images))]
        private readonly long images;

        [JsonInclude, JsonPropertyName(nameof(Rows))]
        private readonly long rows;

        [JsonInclude, JsonPropertyName(nameof(Sheets))]
        private readonly long sheets;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDatasAcquisitionUpdateResult"/> class.
        /// </summary>
        /// <param name="countyIds">The county parts walked.</param>
        /// <param name="sheets">The index sheets read for those parts.</param>
        /// <param name="rows">The <c>orto_datas</c> rows read.</param>
        /// <param name="changed">The rows whose acquisition rows were inserted or changed.</param>
        /// <param name="images">The images of the rows read.</param>
        /// <param name="dated">The images an index sheet dated after the backfill; the rest stay undated.</param>
        public OrtoDatasAcquisitionUpdateResult(IEnumerable<int>? countyIds, long sheets, long rows, long changed, long images, long dated)
        {
            this.countyIds = countyIds is null ? null : [.. countyIds];
            this.sheets = sheets;
            this.rows = rows;
            this.changed = changed;
            this.images = images;
            this.dated = dated;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDatasAcquisitionUpdateResult"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoDatasAcquisitionUpdateResult">The instance to copy.</param>
        public OrtoDatasAcquisitionUpdateResult(OrtoDatasAcquisitionUpdateResult? ortoDatasAcquisitionUpdateResult)
            : base(ortoDatasAcquisitionUpdateResult)
        {
            if (ortoDatasAcquisitionUpdateResult is not null)
            {
                countyIds = ortoDatasAcquisitionUpdateResult.countyIds is null ? null : new List<int>(ortoDatasAcquisitionUpdateResult.countyIds);
                sheets = ortoDatasAcquisitionUpdateResult.sheets;
                rows = ortoDatasAcquisitionUpdateResult.rows;
                changed = ortoDatasAcquisitionUpdateResult.changed;
                images = ortoDatasAcquisitionUpdateResult.images;
                dated = ortoDatasAcquisitionUpdateResult.dated;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDatasAcquisitionUpdateResult"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoDatasAcquisitionUpdateResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the rows whose acquisition rows were inserted or changed; <c>orto_datas</c> itself is never written.
        /// </summary>
        [JsonIgnore]
        public long Changed => changed;

        /// <summary>
        /// Gets the county parts walked.
        /// </summary>
        [JsonIgnore]
        public List<int>? CountyIds => countyIds is null ? null : new List<int>(countyIds);

        /// <summary>
        /// Gets the images an index sheet dated after the backfill.
        /// </summary>
        [JsonIgnore]
        public long Dated => dated;

        /// <summary>
        /// Gets the images of the rows read.
        /// </summary>
        [JsonIgnore]
        public long Images => images;

        /// <summary>
        /// Gets the <c>orto_datas</c> rows read.
        /// </summary>
        [JsonIgnore]
        public long Rows => rows;

        /// <summary>
        /// Gets the index sheets read for the parts walked.
        /// </summary>
        [JsonIgnore]
        public long Sheets => sheets;
    }
}
