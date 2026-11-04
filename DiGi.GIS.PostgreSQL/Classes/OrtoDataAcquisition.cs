using DiGi.Core.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The acquisition of one stored orthophoto image: a row of <see cref="Constants.TableName.OrtoDataAcquisition"/>, naming the image by its county part, building reference and requested year (DiGi.GIS.PostgreSQL#123).
    /// <para>Built from a re-dated series by <see cref="Create.OrtoDataAcquisitions"/>. The constructor only assigns.</para>
    /// </summary>
    public class OrtoDataAcquisition : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AcquisitionDate))]
        private readonly DateTimeOffset? acquisitionDate;

        [JsonInclude, JsonPropertyName(nameof(AcquisitionDatePrecision))]
        private readonly OrtoDatePrecision acquisitionDatePrecision = OrtoDatePrecision.Undefined;

        [JsonInclude, JsonPropertyName(nameof(ContentHash))]
        private readonly string? contentHash;

        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(Reference))]
        private readonly string? reference;

        [JsonInclude, JsonPropertyName(nameof(SheetId))]
        private readonly string? sheetId;

        [JsonInclude, JsonPropertyName(nameof(Year))]
        private readonly int year;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDataAcquisition"/> class.
        /// </summary>
        /// <param name="countyId">The county part the image is filed under.</param>
        /// <param name="reference">The building reference of the stored series.</param>
        /// <param name="year">The year requested from the time-enabled WMS (<see cref="GIS.Classes.OrtoData.DateTime"/>).</param>
        /// <param name="acquisitionDate">The date of the photograph, or null when no index sheet dates it.</param>
        /// <param name="acquisitionDatePrecision">The precision of <paramref name="acquisitionDate"/>; <see cref="OrtoDatePrecision.Undefined"/> when undated.</param>
        /// <param name="sheetId">The identifier of the index sheet the date was read from.</param>
        /// <param name="contentHash">The SHA-256 hash of the image bytes.</param>
        public OrtoDataAcquisition(int countyId, string? reference, int year, DateTimeOffset? acquisitionDate, OrtoDatePrecision acquisitionDatePrecision, string? sheetId, string? contentHash)
        {
            this.countyId = countyId;
            this.reference = reference;
            this.year = year;
            this.acquisitionDate = acquisitionDate;
            this.acquisitionDatePrecision = acquisitionDatePrecision;
            this.sheetId = sheetId;
            this.contentHash = contentHash;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDataAcquisition"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoDataAcquisition">The instance to copy.</param>
        public OrtoDataAcquisition(OrtoDataAcquisition? ortoDataAcquisition)
            : base(ortoDataAcquisition)
        {
            if (ortoDataAcquisition is not null)
            {
                countyId = ortoDataAcquisition.countyId;
                reference = ortoDataAcquisition.reference;
                year = ortoDataAcquisition.year;
                acquisitionDate = ortoDataAcquisition.acquisitionDate;
                acquisitionDatePrecision = ortoDataAcquisition.acquisitionDatePrecision;
                sheetId = ortoDataAcquisition.sheetId;
                contentHash = ortoDataAcquisition.contentHash;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDataAcquisition"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoDataAcquisition(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the date of the photograph, or null when no index sheet dates it.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? AcquisitionDate => acquisitionDate;

        /// <summary>
        /// Gets the precision of <see cref="AcquisitionDate"/>; <see cref="OrtoDatePrecision.Undefined"/> when undated.
        /// </summary>
        [JsonIgnore]
        public OrtoDatePrecision AcquisitionDatePrecision => acquisitionDatePrecision;

        /// <summary>
        /// Gets the SHA-256 hash of the image bytes; identical images share it.
        /// </summary>
        [JsonIgnore]
        public string? ContentHash => contentHash;

        /// <summary>
        /// Gets the county part the image is filed under.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the building reference of the stored series.
        /// </summary>
        [JsonIgnore]
        public string? Reference => reference;

        /// <summary>
        /// Gets the identifier of the index sheet the date was read from.
        /// </summary>
        [JsonIgnore]
        public string? SheetId => sheetId;

        /// <summary>
        /// Gets the year requested from the time-enabled WMS.
        /// </summary>
        [JsonIgnore]
        public int Year => year;
    }
}
