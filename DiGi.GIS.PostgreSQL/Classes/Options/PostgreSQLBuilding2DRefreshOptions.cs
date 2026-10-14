using DiGi.Core.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Options for refreshing 2D building data in a PostgreSQL database.
    /// </summary>
    public class PostgreSQLBuilding2DRefreshOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuilding2DRefreshOptions" /> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object used to initialize the options.</param>
        public PostgreSQLBuilding2DRefreshOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuilding2DRefreshOptions" /> class.
        /// </summary>
        public PostgreSQLBuilding2DRefreshOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuilding2DRefreshOptions" /> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLBuilding2DRefreshOptions">The source options to copy from.</param>
        public PostgreSQLBuilding2DRefreshOptions(PostgreSQLBuilding2DRefreshOptions postgreSQLBuilding2DRefreshOptions)
            : base(postgreSQLBuilding2DRefreshOptions)
        {
            if (postgreSQLBuilding2DRefreshOptions is not null)
            {
                BatchSize = postgreSQLBuilding2DRefreshOptions.BatchSize;
                Tolerance = postgreSQLBuilding2DRefreshOptions.Tolerance;
                OverrideExistingSubdivisionIds = postgreSQLBuilding2DRefreshOptions.OverrideExistingSubdivisionIds;
                StartId = postgreSQLBuilding2DRefreshOptions.StartId;
                CountyIds = postgreSQLBuilding2DRefreshOptions.CountyIds is null ? null : [.. postgreSQLBuilding2DRefreshOptions.CountyIds];
                NestedSubdivisionsOnly = postgreSQLBuilding2DRefreshOptions.NestedSubdivisionsOnly;
            }
        }

        /// <summary>
        /// Gets or sets the number of records to process in a single batch.
        /// </summary>
        [JsonInclude, JsonPropertyName("BatchSize")]
        public int BatchSize { get; set; } = 500;

        /// <summary>
        /// Gets or sets the county polygon part identifiers the refresh is limited to. <see langword="null"/> means every county.
        /// <para>A county code is not a key - a multi-part county has one identifier per polygon part - so name every part that is wanted. Combined with <see cref="NestedSubdivisionsOnly"/>, only the named parts that also hold a nested subdivision layer are refreshed.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the refresh is limited to the counties whose subdivision layer nests - where one subdivision lies inside another of the same municipality, as a city holds its districts and their neighbourhoods.
        /// <para>Those are the counties where the stored <c>subdivision_id</c> depended on the tie-break rather than on the geometry (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>). Expect it to exclude very little: a village and its named parts nest the same way a city and its districts do, and on the development database 404 of 406 county parts qualified. It is a way of naming the affected counties in the log rather than a saving; the re-derivation with <see cref="OverrideExistingSubdivisionIds"/> is, in practice, national. The scope is resolved when the refresh starts and logged.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(NestedSubdivisionsOnly))]
        public bool NestedSubdivisionsOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether existing subdivision IDs should be overridden during the refresh process.
        /// </summary>
        [JsonInclude, JsonPropertyName("OverrideExistingSubdivisionIds")]
        public bool OverrideExistingSubdivisionIds { get; set; } = false;

        /// <summary>
        /// Gets or sets the starting building identifier anchor for keyset pagination.
        /// <para>The refresh walks one county polygon part after another, so the anchor applies within each part: a part skips the rows at or below the anchor and visits the rest, and a part whose rows all end below the anchor is simply done. The default <c>0</c> skips nothing.</para>
        /// </summary>
        [JsonInclude, JsonPropertyName("StartId")]
        public long StartId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the distance tolerance used for processing building data.
        /// </summary>
        [JsonInclude, JsonPropertyName("Tolerance")]
        public double Tolerance { get; set; } = Core.Constants.Tolerance.MacroDistance;
    }
}