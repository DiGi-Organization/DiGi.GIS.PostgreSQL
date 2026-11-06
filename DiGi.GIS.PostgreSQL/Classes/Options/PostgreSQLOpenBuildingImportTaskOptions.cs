using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides options for importing one release of an open building dataset into its footprint table (DiGi.GIS.PostgreSQL#125).
    /// <para>The scope is exactly one of <see cref="CountyIds"/> or <see cref="BoundingBox2D"/>: the first names county parts outright (the task widens them to every sibling part before the run starts), the second names a pilot area that is not county-shaped (T30). The bounding box is in EPSG:2180 like everything this repository stores, and the task applies it after the reprojection leg - it is never handed to a reader, because every reader interprets a box in its own file's source coordinate reference system and an EPSG:2180 box passed into an EPSG:3035 file would silently filter everything out.</para>
    /// </summary>
    public class PostgreSQLOpenBuildingImportTaskOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOpenBuildingImportTaskOptions"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the options.</param>
        public PostgreSQLOpenBuildingImportTaskOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOpenBuildingImportTaskOptions"/> class.
        /// </summary>
        public PostgreSQLOpenBuildingImportTaskOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOpenBuildingImportTaskOptions"/> class by copying another instance.
        /// </summary>
        /// <param name="postgreSQLOpenBuildingImportTaskOptions">The source options to copy.</param>
        public PostgreSQLOpenBuildingImportTaskOptions(PostgreSQLOpenBuildingImportTaskOptions postgreSQLOpenBuildingImportTaskOptions)
            : base(postgreSQLOpenBuildingImportTaskOptions)
        {
            if (postgreSQLOpenBuildingImportTaskOptions is not null)
            {
                BoundingBox2D = postgreSQLOpenBuildingImportTaskOptions.BoundingBox2D is BoundingBox2D boundingBox2D
                    ? new BoundingBox2D(new Point2D(boundingBox2D.Min.X, boundingBox2D.Min.Y), new Point2D(boundingBox2D.Max.X, boundingBox2D.Max.Y))
                    : null;
                CountyIds = postgreSQLOpenBuildingImportTaskOptions.CountyIds == null ? null : [.. postgreSQLOpenBuildingImportTaskOptions.CountyIds];
                DatasetKey = postgreSQLOpenBuildingImportTaskOptions.DatasetKey;
                DownloadDate = postgreSQLOpenBuildingImportTaskOptions.DownloadDate;
                FileName = postgreSQLOpenBuildingImportTaskOptions.FileName;
                Licence = postgreSQLOpenBuildingImportTaskOptions.Licence;
                Quadkey = postgreSQLOpenBuildingImportTaskOptions.Quadkey;
                Sha256 = postgreSQLOpenBuildingImportTaskOptions.Sha256;
                SourceObservationDate = postgreSQLOpenBuildingImportTaskOptions.SourceObservationDate;
                SourcePaths = postgreSQLOpenBuildingImportTaskOptions.SourcePaths == null ? null : [.. postgreSQLOpenBuildingImportTaskOptions.SourcePaths];
                SourceUrl = postgreSQLOpenBuildingImportTaskOptions.SourceUrl;
                Version = postgreSQLOpenBuildingImportTaskOptions.Version;
            }
        }

        /// <summary>
        /// Gets or sets the scope bounding box in EPSG:2180 (easting in X), for a run scoped to a pilot area that is not county-shaped. Mutually exclusive with <see cref="CountyIds"/>.
        /// <para>Applied after the reprojection leg and used to resolve the scope's counties from the database; never passed to a reader, which would interpret it in the file's own source coordinate reference system.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BoundingBox2D))]
        public BoundingBox2D? BoundingBox2D { get; set; } = null;

        /// <summary>
        /// Gets or sets the county part identifiers to import, for a run scoped to whole counties. Mutually exclusive with <see cref="BoundingBox2D"/>.
        /// <para>Identifiers, never codes: a county held in several pieces is one row per piece, and a code names them all. The task widens each named part to every sibling part sharing its code before the run starts, so a multi-part county is imported as a whole and its partitions are replaced together.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the dataset key of the import (<c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c>, <c>ghs_obat</c>). Required; it selects the reader and the table.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(DatasetKey))]
        public string? DatasetKey { get; set; } = null;

        /// <summary>
        /// Gets or sets the date the release was downloaded; one of the register's three uniqueness columns. Required.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(DownloadDate))]
        public DateTime? DownloadDate { get; set; } = null;

        /// <summary>
        /// Gets or sets the file name the release was downloaded as, stored in the register, or null when not recorded.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(FileName))]
        public string? FileName { get; set; } = null;

        /// <summary>
        /// Gets or sets the licence of the release, stored in the register, or null when not recorded.
        /// <para>Per-row licence exceptions (CC-BY-SA Prague, CC-BY-NC Abruzzo) travel inside each footprint's provenance and are never dropped; this is the release-level licence.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Licence))]
        public string? Licence { get; set; } = null;

        /// <summary>
        /// Gets or sets the quadkey the Microsoft file was fetched under; required when <see cref="DatasetKey"/> is <c>microsoft</c>, because the quadkey is that dataset's source identity.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Quadkey))]
        public string? Quadkey { get; set; } = null;

        /// <summary>
        /// Gets or sets the SHA-256 of the downloaded file, stored in the register, or null when not computed.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Sha256))]
        public string? Sha256 { get; set; } = null;

        /// <summary>
        /// Gets or sets the observation date the source publishes for the release, stored in the register as text because sources state it at different granularities, or null when the source publishes none.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SourceObservationDate))]
        public string? SourceObservationDate { get; set; } = null;

        /// <summary>
        /// Gets or sets the source file paths to import; required, and every path must exist. A dataset released as several files is imported by listing them all, and the run treats them as one release.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SourcePaths))]
        public HashSet<string>? SourcePaths { get; set; } = null;

        /// <summary>
        /// Gets or sets the URL the release was downloaded from, stored in the register, or null when not recorded.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SourceUrl))]
        public string? SourceUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the version string of the release (<c>v0.2</c>, <c>2026-10-21.0</c>, <c>poland-YYMMDD</c>, <c>R2025</c>, <c>links-2026-08-13</c>, <c>R2024A</c>); one of the register's three uniqueness columns. Required.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(Version))]
        public string? Version { get; set; } = null;
    }
}
