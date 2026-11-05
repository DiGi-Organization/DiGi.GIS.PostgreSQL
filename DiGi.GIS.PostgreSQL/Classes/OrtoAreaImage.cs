using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One materialised area tile of a full-interpretation test area for one epoch: a row of <see cref="Constants.TableName.OrtoAreaImage"/> (DiGi.GIS.PostgreSQL#129).
    /// <para>The row is keyed by <c>(area_key, epoch_year)</c>: a re-fetch replaces it, never appends. <see cref="Status"/> is 0 when <see cref="Bytes"/> holds the imagery, 1 when the source holds no imagery for the area and 2 when the fetch failed - the negative rows of 1 and 2 carry no bytes, and they cache the answer so the WMS is not asked again. The table lives on the Storage database beside <c>orto_datas</c>; the area registry itself lives on the Main database and is never read in the same statement as this row.</para>
    /// </summary>
    public class OrtoAreaImage : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(AreaKey))]
        private readonly string areaKey = string.Empty;

        [JsonInclude, JsonPropertyName(nameof(BoundingBox2D))]
        private readonly BoundingBox2D? boundingBox2D;

        [JsonInclude, JsonPropertyName(nameof(Bytes))]
        private readonly byte[]? bytes;

        [JsonInclude, JsonPropertyName(nameof(EpochYear))]
        private readonly short epochYear;

        [JsonInclude, JsonPropertyName(nameof(FetchedAt))]
        private readonly DateTimeOffset fetchedAt;

        [JsonInclude, JsonPropertyName(nameof(Height))]
        private readonly int height;

        [JsonInclude, JsonPropertyName(nameof(SourceUrl))]
        private readonly string? sourceUrl;

        [JsonInclude, JsonPropertyName(nameof(Status))]
        private readonly short status;

        [JsonInclude, JsonPropertyName(nameof(Width))]
        private readonly int width;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaImage"/> class.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the test area the tile belongs to.</param>
        /// <param name="boundingBox2D">The area extent in EPSG:2180, easting in X.</param>
        /// <param name="epochYear">The imagery epoch the tile was fetched for.</param>
        /// <param name="width">The width of the tile in px.</param>
        /// <param name="height">The height of the tile in px.</param>
        /// <param name="status">What the fetch produced: 0 imagery, 1 no imagery, 2 fetch error.</param>
        /// <param name="sourceUrl">The URL the bytes were fetched from, or null when none.</param>
        /// <param name="fetchedAt">When the fetch produced this row.</param>
        /// <param name="bytes">The encoded tile image, or null for the negative rows of status 1 and 2.</param>
        public OrtoAreaImage(string? areaKey, BoundingBox2D? boundingBox2D, short epochYear, int width, int height, short status, string? sourceUrl, DateTimeOffset fetchedAt, byte[]? bytes)
        {
            this.areaKey = areaKey ?? string.Empty;
            this.boundingBox2D = Core.Query.Clone(boundingBox2D);
            this.epochYear = epochYear;
            this.width = width;
            this.height = height;
            this.status = status;
            this.sourceUrl = sourceUrl;
            this.fetchedAt = fetchedAt;
            this.bytes = bytes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaImage"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoAreaImage">The instance to copy.</param>
        public OrtoAreaImage(OrtoAreaImage? ortoAreaImage)
            : base(ortoAreaImage)
        {
            if (ortoAreaImage is not null)
            {
                areaKey = ortoAreaImage.areaKey;
                boundingBox2D = Core.Query.Clone(ortoAreaImage.boundingBox2D);
                bytes = ortoAreaImage.bytes;
                epochYear = ortoAreaImage.epochYear;
                fetchedAt = ortoAreaImage.fetchedAt;
                height = ortoAreaImage.height;
                sourceUrl = ortoAreaImage.sourceUrl;
                status = ortoAreaImage.status;
                width = ortoAreaImage.width;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaImage"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoAreaImage(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the invariant location key of the test area the tile belongs to.
        /// </summary>
        [JsonIgnore]
        public string AreaKey => areaKey;

        /// <summary>
        /// Gets a clone of the area extent in EPSG:2180, easting in X.
        /// </summary>
        [JsonIgnore]
        public BoundingBox2D? BoundingBox2D => Core.Query.Clone(boundingBox2D);

        /// <summary>
        /// Gets the encoded tile image, or null for the negative rows of status 1 and 2.
        /// </summary>
        [JsonIgnore]
        public byte[]? Bytes => bytes;

        /// <summary>
        /// Gets the imagery epoch the tile was fetched for.
        /// </summary>
        [JsonIgnore]
        public short EpochYear => epochYear;

        /// <summary>
        /// Gets when the fetch produced this row.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset FetchedAt => fetchedAt;

        /// <summary>
        /// Gets the height of the tile in px.
        /// </summary>
        [JsonIgnore]
        public int Height => height;

        /// <summary>
        /// Gets the URL the bytes were fetched from, or null when none.
        /// </summary>
        [JsonIgnore]
        public string? SourceUrl => sourceUrl;

        /// <summary>
        /// Gets what the fetch produced: 0 imagery, 1 no imagery, 2 fetch error.
        /// </summary>
        [JsonIgnore]
        public short Status => status;

        /// <summary>
        /// Gets the width of the tile in px.
        /// </summary>
        [JsonIgnore]
        public int Width => width;
    }
}
