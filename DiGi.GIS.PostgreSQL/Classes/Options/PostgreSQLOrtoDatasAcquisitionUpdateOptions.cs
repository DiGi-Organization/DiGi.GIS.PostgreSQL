using DiGi.Core.Classes;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Options of an acquisition-date backfill of <c>orto_datas</c> (<see cref="PostgreSQLOrtoDatasAcquisitionUpdateTask"/>).
    /// </summary>
    public class PostgreSQLOrtoDatasAcquisitionUpdateOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoDatasAcquisitionUpdateOptions" /> class using a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object used to initialize the options.</param>
        public PostgreSQLOrtoDatasAcquisitionUpdateOptions(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoDatasAcquisitionUpdateOptions" /> class.
        /// </summary>
        public PostgreSQLOrtoDatasAcquisitionUpdateOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLOrtoDatasAcquisitionUpdateOptions" /> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLOrtoDatasAcquisitionUpdateOptions">The source options to copy from.</param>
        public PostgreSQLOrtoDatasAcquisitionUpdateOptions(PostgreSQLOrtoDatasAcquisitionUpdateOptions postgreSQLOrtoDatasAcquisitionUpdateOptions)
            : base(postgreSQLOrtoDatasAcquisitionUpdateOptions)
        {
            if (postgreSQLOrtoDatasAcquisitionUpdateOptions is not null)
            {
                CountyIds = postgreSQLOrtoDatasAcquisitionUpdateOptions.CountyIds is null ? null : [.. postgreSQLOrtoDatasAcquisitionUpdateOptions.CountyIds];
                LastYear = postgreSQLOrtoDatasAcquisitionUpdateOptions.LastYear;
                PageSize = postgreSQLOrtoDatasAcquisitionUpdateOptions.PageSize;
                CommandTimeout = postgreSQLOrtoDatasAcquisitionUpdateOptions.CommandTimeout;
            }
        }

        /// <summary>
        /// Gets or sets the timeout in seconds of each command.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CommandTimeout))]
        public int CommandTimeout { get; set; } = 300;

        /// <summary>
        /// Gets or sets the county polygon part identifiers to backfill. A county code is not a key - a multi-part county has one identifier per polygon part - so name every part that is wanted. Null or empty backfills nothing.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the last year requested when the stored series were fetched (<c>OrtoDatasOptions.Years</c>, by default up to the current year): the run end of every series' last image.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(LastYear))]
        public int LastYear { get; set; } = DateTime.Now.Year;

        /// <summary>
        /// Gets or sets the number of rows read per page; a stored series is several hundred kilobytes.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(PageSize))]
        public int PageSize { get; set; } = 100;
    }
}
