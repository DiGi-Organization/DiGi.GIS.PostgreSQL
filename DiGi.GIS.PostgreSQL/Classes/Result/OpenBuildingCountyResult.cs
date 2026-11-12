using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// What one county part of one open building footprint table holds for one release: how many footprints, split by the normalised origin of their geometry (<c>primary_source</c>), and how many carry a licence exception.
    /// <para>The per-source counts are the data of the provenance diagram (paper 01, figure R1; DiGi.GIS.PostgreSQL#126): they show how much of a dataset is a register, OpenStreetMap or a detector, and therefore which datasets are not independent of one another. They always sum to <see cref="Count"/>.</para>
    /// </summary>
    public class OpenBuildingCountyResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(Count))]
        private readonly long count;

        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(DatasetId))]
        private readonly int datasetId;

        [JsonInclude, JsonPropertyName(nameof(EsriCount))]
        private readonly long esriCount;

        [JsonInclude, JsonPropertyName(nameof(LicenceExceptionCount))]
        private readonly long licenceExceptionCount;

        [JsonInclude, JsonPropertyName(nameof(MicrosoftCount))]
        private readonly long microsoftCount;

        [JsonInclude, JsonPropertyName(nameof(OsmCount))]
        private readonly long osmCount;

        [JsonInclude, JsonPropertyName(nameof(OtherCount))]
        private readonly long otherCount;

        [JsonInclude, JsonPropertyName(nameof(RegisterCount))]
        private readonly long registerCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingCountyResult"/> class.
        /// </summary>
        /// <param name="countyId">The identifier of the county part summarised.</param>
        /// <param name="datasetId">The identifier of the <c>building_dataset</c> register row the footprints belong to.</param>
        /// <param name="count">The number of footprints.</param>
        /// <param name="registerCount">How many footprints have a government register as their primary source.</param>
        /// <param name="osmCount">How many footprints have OpenStreetMap as their primary source.</param>
        /// <param name="microsoftCount">How many footprints have the Microsoft detector as their primary source.</param>
        /// <param name="esriCount">How many footprints have Esri Community Maps as their primary source.</param>
        /// <param name="otherCount">How many footprints have any other, or an unrecognised, primary source.</param>
        /// <param name="licenceExceptionCount">How many footprints carry a licence exception (EUBUCCO: Prague CC-BY-SA, Abruzzo CC-BY-NC).</param>
        public OpenBuildingCountyResult(
            int countyId,
            int datasetId,
            long count,
            long registerCount,
            long osmCount,
            long microsoftCount,
            long esriCount,
            long otherCount,
            long licenceExceptionCount)
        {
            this.countyId = countyId;
            this.datasetId = datasetId;
            this.count = count;
            this.registerCount = registerCount;
            this.osmCount = osmCount;
            this.microsoftCount = microsoftCount;
            this.esriCount = esriCount;
            this.otherCount = otherCount;
            this.licenceExceptionCount = licenceExceptionCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingCountyResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="openBuildingCountyResult">The <see cref="OpenBuildingCountyResult"/> to copy from.</param>
        public OpenBuildingCountyResult(OpenBuildingCountyResult? openBuildingCountyResult)
            : base(openBuildingCountyResult)
        {
            if (openBuildingCountyResult is not null)
            {
                countyId = openBuildingCountyResult.countyId;
                datasetId = openBuildingCountyResult.datasetId;
                count = openBuildingCountyResult.count;
                registerCount = openBuildingCountyResult.registerCount;
                osmCount = openBuildingCountyResult.osmCount;
                microsoftCount = openBuildingCountyResult.microsoftCount;
                esriCount = openBuildingCountyResult.esriCount;
                otherCount = openBuildingCountyResult.otherCount;
                licenceExceptionCount = openBuildingCountyResult.licenceExceptionCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingCountyResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OpenBuildingCountyResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the number of footprints the county part holds for the release.
        /// </summary>
        [JsonIgnore]
        public long Count => count;

        /// <summary>
        /// Gets the identifier of the county part summarised.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the identifier of the <c>building_dataset</c> register row the footprints belong to.
        /// </summary>
        [JsonIgnore]
        public int DatasetId => datasetId;

        /// <summary>
        /// Gets how many footprints have Esri Community Maps as their primary source.
        /// </summary>
        [JsonIgnore]
        public long EsriCount => esriCount;

        /// <summary>
        /// Gets how many footprints carry a licence exception (EUBUCCO: Prague CC-BY-SA, Abruzzo CC-BY-NC).
        /// </summary>
        [JsonIgnore]
        public long LicenceExceptionCount => licenceExceptionCount;

        /// <summary>
        /// Gets how many footprints have the Microsoft detector as their primary source.
        /// </summary>
        [JsonIgnore]
        public long MicrosoftCount => microsoftCount;

        /// <summary>
        /// Gets how many footprints have OpenStreetMap as their primary source.
        /// </summary>
        [JsonIgnore]
        public long OsmCount => osmCount;

        /// <summary>
        /// Gets how many footprints have any other, or an unrecognised, primary source.
        /// </summary>
        [JsonIgnore]
        public long OtherCount => otherCount;

        /// <summary>
        /// Gets how many footprints have a government register as their primary source.
        /// <para>For EUBUCCO in Poland this is Budynki3D LoD1 GUGiK, whose footprints come from BDOT10k - so a comparison of these footprints with <c>building_2d</c> is circular, not an independent test.</para>
        /// </summary>
        [JsonIgnore]
        public long RegisterCount => registerCount;
    }
}
