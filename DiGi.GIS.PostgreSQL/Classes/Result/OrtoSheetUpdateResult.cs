using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The outcome of an orthophoto sheet index import (<see cref="Modify.UpdateOrtoSheetsAsync"/>): per requested index year, whether the service published it and whether every row was read, and how many rows were stored.
    /// </summary>
    public class OrtoSheetUpdateResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(Canonical))]
        private readonly long canonical;

        [JsonInclude, JsonPropertyName(nameof(Hits))]
        private readonly long hits;

        [JsonInclude, JsonPropertyName(nameof(MismatchedYears))]
        private readonly List<int>? mismatchedYears;

        [JsonInclude, JsonPropertyName(nameof(MissingYears))]
        private readonly List<int>? missingYears;

        [JsonInclude, JsonPropertyName(nameof(Rows))]
        private readonly long rows;

        [JsonInclude, JsonPropertyName(nameof(Written))]
        private readonly long written;

        [JsonInclude, JsonPropertyName(nameof(Years))]
        private readonly List<int>? years;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoSheetUpdateResult"/> class.
        /// </summary>
        /// <param name="years">The index years requested.</param>
        /// <param name="missingYears">The requested years the service answered no count for: not published, or the count failed.</param>
        /// <param name="mismatchedYears">The years whose rows read differ from the service's count, or whose rows could not be read.</param>
        /// <param name="hits">The sum of the service's counts (<c>RESULTTYPE=hits</c>) over the years read.</param>
        /// <param name="rows">The rows read.</param>
        /// <param name="canonical">The rows left after removing duplicates within each year (<see cref="GIS.Query.OrtoSheetDuplicateGroups"/>).</param>
        /// <param name="written">The rows inserted or replaced.</param>
        public OrtoSheetUpdateResult(IEnumerable<int>? years, IEnumerable<int>? missingYears, IEnumerable<int>? mismatchedYears, long hits, long rows, long canonical, long written)
        {
            this.years = years is null ? null : [.. years];
            this.missingYears = missingYears is null ? null : [.. missingYears];
            this.mismatchedYears = mismatchedYears is null ? null : [.. mismatchedYears];
            this.hits = hits;
            this.rows = rows;
            this.canonical = canonical;
            this.written = written;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoSheetUpdateResult"/> class as a copy of another instance.
        /// </summary>
        /// <param name="ortoSheetUpdateResult">The instance to copy.</param>
        public OrtoSheetUpdateResult(OrtoSheetUpdateResult? ortoSheetUpdateResult)
            : base(ortoSheetUpdateResult)
        {
            if (ortoSheetUpdateResult is not null)
            {
                years = ortoSheetUpdateResult.years is null ? null : new List<int>(ortoSheetUpdateResult.years);
                missingYears = ortoSheetUpdateResult.missingYears is null ? null : new List<int>(ortoSheetUpdateResult.missingYears);
                mismatchedYears = ortoSheetUpdateResult.mismatchedYears is null ? null : new List<int>(ortoSheetUpdateResult.mismatchedYears);
                hits = ortoSheetUpdateResult.hits;
                rows = ortoSheetUpdateResult.rows;
                canonical = ortoSheetUpdateResult.canonical;
                written = ortoSheetUpdateResult.written;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoSheetUpdateResult"/> class from its JSON representation.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        public OrtoSheetUpdateResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the rows left after removing duplicates within each year.
        /// </summary>
        [JsonIgnore]
        public long Canonical => canonical;

        /// <summary>
        /// Gets the sum of the service's counts over the years read.
        /// </summary>
        [JsonIgnore]
        public long Hits => hits;

        /// <summary>
        /// Gets the years whose rows read differ from the service's count, or whose rows could not be read.
        /// </summary>
        [JsonIgnore]
        public List<int>? MismatchedYears => mismatchedYears is null ? null : new List<int>(mismatchedYears);

        /// <summary>
        /// Gets the requested years the service answered no count for.
        /// </summary>
        [JsonIgnore]
        public List<int>? MissingYears => missingYears is null ? null : new List<int>(missingYears);

        /// <summary>
        /// Gets the rows read.
        /// </summary>
        [JsonIgnore]
        public long Rows => rows;

        /// <summary>
        /// Gets the rows inserted or replaced.
        /// </summary>
        [JsonIgnore]
        public long Written => written;

        /// <summary>
        /// Gets the index years requested.
        /// </summary>
        [JsonIgnore]
        public List<int>? Years => years is null ? null : new List<int>(years);
    }
}
