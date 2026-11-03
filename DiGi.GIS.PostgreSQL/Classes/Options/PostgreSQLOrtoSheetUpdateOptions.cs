using DiGi.Core.Classes;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Options of an orthophoto sheet index import into <c>orto_sheet</c> (<see cref="PostgreSQLOrtoSheetUpdateTask"/>).
    /// </summary>
    public class PostgreSQLOrtoSheetUpdateOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoSheetUpdateOptions" /> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object used to initialize the options.</param>
        public PostgreSQLOrtoSheetUpdateOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoSheetUpdateOptions" /> class.
        /// </summary>
        public PostgreSQLOrtoSheetUpdateOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoSheetUpdateOptions" /> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLOrtoSheetUpdateOptions">The source options to copy from.</param>
        public PostgreSQLOrtoSheetUpdateOptions(PostgreSQLOrtoSheetUpdateOptions postgreSQLOrtoSheetUpdateOptions)
            : base(postgreSQLOrtoSheetUpdateOptions)
        {
            if (postgreSQLOrtoSheetUpdateOptions is not null)
            {
                FirstYear = postgreSQLOrtoSheetUpdateOptions.FirstYear;
                LastYear = postgreSQLOrtoSheetUpdateOptions.LastYear;
                PageSize = postgreSQLOrtoSheetUpdateOptions.PageSize;
                RetryCount = postgreSQLOrtoSheetUpdateOptions.RetryCount;
                RetryDelaySeconds = postgreSQLOrtoSheetUpdateOptions.RetryDelaySeconds;
            }
        }

        /// <summary>
        /// Gets or sets the first index year to import. The oldest typename of 2026-11-03 is 1957; a year the service does not publish is reported as missing and skipped.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(FirstYear))]
        public int FirstYear { get; set; } = 1950;

        /// <summary>
        /// Gets or sets the last index year to import.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(LastYear))]
        public int LastYear { get; set; } = DateTime.Now.Year;

        /// <summary>
        /// Gets or sets the number of rows per WFS page.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(PageSize))]
        public int PageSize { get; set; } = GIS.Constants.WebService.OrtoSheetPageSize;

        /// <summary>
        /// Gets or sets the number of times a transient failure of the index service is retried.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(RetryCount))]
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets the delay, in seconds, before the first retry.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(RetryDelaySeconds))]
        public double RetryDelaySeconds { get; set; } = 10;
    }
}
