using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The outcome of a yearly solar radiation summary run: how many buildings it wrote and how many of their surfaces it aggregated, plus the ones it had to step over.
    /// <para><see cref="ProcessedReferenceCount"/> is the buildings that got a row - the latest stored result of each county and reference.</para>
    /// <para><see cref="SurfaceCount"/> is the usable surfaces aggregated - a finite area greater than zero. <see cref="SurfaceWithoutSectorCount"/> is the usable surfaces with no classifiable facing direction (a missing or vertical outward normal, e.g. a flat roof): they count toward the building totals but toward no direction sector. <see cref="SkippedSurfaceCount"/> is the surfaces with no usable area, which contribute nothing and are counted only so their absence is visible.</para>
    /// </summary>
    public class SolarRadiationSummaryResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(ProcessedReferenceCount))]
        private readonly long processedReferenceCount;

        [JsonInclude, JsonPropertyName(nameof(SkippedSurfaceCount))]
        private readonly long skippedSurfaceCount;

        [JsonInclude, JsonPropertyName(nameof(SurfaceCount))]
        private readonly long surfaceCount;

        [JsonInclude, JsonPropertyName(nameof(SurfaceWithoutSectorCount))]
        private readonly long surfaceWithoutSectorCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolarRadiationSummaryResult"/> class.
        /// </summary>
        /// <param name="processedReferenceCount">The number of buildings that got a row written.</param>
        /// <param name="surfaceCount">The number of usable surfaces aggregated.</param>
        /// <param name="surfaceWithoutSectorCount">The number of usable surfaces with no classifiable facing direction.</param>
        /// <param name="skippedSurfaceCount">The number of surfaces skipped because they carry no usable area.</param>
        public SolarRadiationSummaryResult(long processedReferenceCount = 0, long surfaceCount = 0, long surfaceWithoutSectorCount = 0, long skippedSurfaceCount = 0)
        {
            this.processedReferenceCount = processedReferenceCount;
            this.surfaceCount = surfaceCount;
            this.surfaceWithoutSectorCount = surfaceWithoutSectorCount;
            this.skippedSurfaceCount = skippedSurfaceCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolarRadiationSummaryResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="solarRadiationSummaryResult">The <see cref="SolarRadiationSummaryResult"/> to copy from.</param>
        public SolarRadiationSummaryResult(SolarRadiationSummaryResult? solarRadiationSummaryResult)
            : base(solarRadiationSummaryResult)
        {
            if (solarRadiationSummaryResult is not null)
            {
                processedReferenceCount = solarRadiationSummaryResult.processedReferenceCount;
                skippedSurfaceCount = solarRadiationSummaryResult.skippedSurfaceCount;
                surfaceCount = solarRadiationSummaryResult.surfaceCount;
                surfaceWithoutSectorCount = solarRadiationSummaryResult.surfaceWithoutSectorCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolarRadiationSummaryResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public SolarRadiationSummaryResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the number of buildings that got a row written during the last run.
        /// </summary>
        public long ProcessedReferenceCount
        {
            get
            {
                return processedReferenceCount;
            }
        }

        /// <summary>
        /// Gets the number of usable surfaces aggregated during the last run.
        /// </summary>
        public long SurfaceCount
        {
            get
            {
                return surfaceCount;
            }
        }

        /// <summary>
        /// Gets the number of usable surfaces with no classifiable facing direction during the last run - they count toward the building totals but toward no direction sector.
        /// </summary>
        public long SurfaceWithoutSectorCount
        {
            get
            {
                return surfaceWithoutSectorCount;
            }
        }

        /// <summary>
        /// Gets the number of surfaces skipped because they carry no usable area during the last run.
        /// </summary>
        public long SkippedSurfaceCount
        {
            get
            {
                return skippedSurfaceCount;
            }
        }
    }
}
