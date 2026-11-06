using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One release of one open building dataset as the <c>building_dataset</c> register stores it: the dataset key, version, download date, licence and SHA-256 the release was fetched under (DiGi.GIS.PostgreSQL#125).
    /// <para>One row is one release, unique on <c>(dataset_key, version, download_date)</c> under a <c>NULLS NOT DISTINCT</c> index, so re-importing the same release revises its row instead of adding one. The identifier of the row is what the footprint tables' <c>dataset_id</c> column names, and a run updates <see cref="RowCount"/> and <see cref="AttributeNames"/> once it knows them. The table lives on the Main database, beside <c>building_2d</c>.</para>
    /// </summary>
    public class BuildingDatasetRelease : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AttributeNames))]
        private readonly List<string>? attributeNames;

        [JsonInclude, JsonPropertyName(nameof(CreatedAt))]
        private readonly DateTime? createdAt;

        [JsonInclude, JsonPropertyName(nameof(DatasetKey))]
        private readonly string? datasetKey;

        [JsonInclude, JsonPropertyName(nameof(DownloadDate))]
        private readonly DateTime? downloadDate;

        [JsonInclude, JsonPropertyName(nameof(FileName))]
        private readonly string? fileName;

        [JsonInclude, JsonPropertyName(nameof(Id))]
        private readonly int? id;

        [JsonInclude, JsonPropertyName(nameof(Licence))]
        private readonly string? licence;

        [JsonInclude, JsonPropertyName(nameof(RowCount))]
        private readonly long? rowCount;

        [JsonInclude, JsonPropertyName(nameof(Sha256))]
        private readonly string? sha256;

        [JsonInclude, JsonPropertyName(nameof(SourceObservationDate))]
        private readonly string? sourceObservationDate;

        [JsonInclude, JsonPropertyName(nameof(SourceUrl))]
        private readonly string? sourceUrl;

        [JsonInclude, JsonPropertyName(nameof(Version))]
        private readonly string? version;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingDatasetRelease"/> class.
        /// </summary>
        /// <param name="datasetKey">The dataset the release belongs to (<c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c>, <c>ghs_obat</c>).</param>
        /// <param name="version">The version string of the release (<c>v0.2</c>, <c>2026-10-21.0</c>, <c>poland-YYMMDD</c>, <c>R2025</c>, <c>links-2026-08-13</c>, <c>R2024A</c>).</param>
        /// <param name="downloadDate">The date the release was downloaded.</param>
        /// <param name="sourceUrl">The URL the release was downloaded from, or null when not recorded.</param>
        /// <param name="licence">The licence of the release, or null when not recorded.</param>
        /// <param name="sourceObservationDate">The observation date the source publishes for the release, or null when it publishes none. Held as text because sources state it at different granularities.</param>
        /// <param name="fileName">The file name the release was downloaded as, or null when not recorded.</param>
        /// <param name="sha256">The SHA-256 of the downloaded file, or null when not computed.</param>
        public BuildingDatasetRelease(string? datasetKey, string? version, DateTime? downloadDate, string? sourceUrl = null, string? licence = null, string? sourceObservationDate = null, string? fileName = null, string? sha256 = null)
        {
            this.datasetKey = datasetKey;
            this.version = version;
            this.downloadDate = downloadDate;
            this.sourceUrl = sourceUrl;
            this.licence = licence;
            this.sourceObservationDate = sourceObservationDate;
            this.fileName = fileName;
            this.sha256 = sha256;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingDatasetRelease"/> class by copying another instance.
        /// </summary>
        /// <param name="buildingDatasetRelease">The source release to copy.</param>
        public BuildingDatasetRelease(BuildingDatasetRelease? buildingDatasetRelease)
            : base(buildingDatasetRelease)
        {
            if (buildingDatasetRelease is not null)
            {
                attributeNames = buildingDatasetRelease.attributeNames == null ? null : [.. buildingDatasetRelease.attributeNames];
                createdAt = buildingDatasetRelease.createdAt;
                datasetKey = buildingDatasetRelease.datasetKey;
                downloadDate = buildingDatasetRelease.downloadDate;
                fileName = buildingDatasetRelease.fileName;
                id = buildingDatasetRelease.id;
                licence = buildingDatasetRelease.licence;
                rowCount = buildingDatasetRelease.rowCount;
                sha256 = buildingDatasetRelease.sha256;
                sourceObservationDate = buildingDatasetRelease.sourceObservationDate;
                sourceUrl = buildingDatasetRelease.sourceUrl;
                version = buildingDatasetRelease.version;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingDatasetRelease"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the release.</param>
        public BuildingDatasetRelease(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="BuildingDatasetRelease"/> instance.
        /// </summary>
        /// <returns>A new <see cref="BuildingDatasetRelease"/> instance with the same data, or null when cloning fails.</returns>
        public override ISerializableObject? Clone()
        {
            return new BuildingDatasetRelease(this);
        }

        /// <summary>
        /// Gets the distinct attribute names the release's rows carried, filled by the import once it has read the release.
        /// </summary>
        [JsonIgnore]
        public List<string>? AttributeNames => attributeNames;

        /// <summary>
        /// Gets the moment the register row was created, or null when the row has not been read back from the database.
        /// </summary>
        [JsonIgnore]
        public DateTime? CreatedAt => createdAt;

        /// <summary>
        /// Gets the dataset the release belongs to.
        /// </summary>
        [JsonIgnore]
        public string? DatasetKey => datasetKey;

        /// <summary>
        /// Gets the date the release was downloaded.
        /// </summary>
        [JsonIgnore]
        public DateTime? DownloadDate => downloadDate;

        /// <summary>
        /// Gets the file name the release was downloaded as, or null when not recorded.
        /// </summary>
        [JsonIgnore]
        public string? FileName => fileName;

        /// <summary>
        /// Gets the identifier of the register row, or null when the row has not been read back from the database.
        /// <para>This is the value the footprint tables' <c>dataset_id</c> column carries.</para>
        /// </summary>
        [JsonIgnore]
        public int? Id => id;

        /// <summary>
        /// Gets the licence of the release, or null when not recorded.
        /// </summary>
        [JsonIgnore]
        public string? Licence => licence;

        /// <summary>
        /// Gets the number of rows the release's import stored, or null before the import has finished.
        /// </summary>
        [JsonIgnore]
        public long? RowCount => rowCount;

        /// <summary>
        /// Gets the SHA-256 of the downloaded file, or null when not computed.
        /// </summary>
        [JsonIgnore]
        public string? Sha256 => sha256;

        /// <summary>
        /// Gets the observation date the source publishes for the release, held as text because sources state it at different granularities, or null when the source publishes none.
        /// </summary>
        [JsonIgnore]
        public string? SourceObservationDate => sourceObservationDate;

        /// <summary>
        /// Gets the URL the release was downloaded from, or null when not recorded.
        /// </summary>
        [JsonIgnore]
        public string? SourceUrl => sourceUrl;

        /// <summary>
        /// Gets the version string of the release.
        /// </summary>
        [JsonIgnore]
        public string? Version => version;
    }
}
