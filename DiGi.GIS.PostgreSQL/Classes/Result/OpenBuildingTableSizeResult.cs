using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The disk use of one storage unit of an open building table, read from the system catalog: one county partition of a footprint table, or the whole unpartitioned GHS-OBAT table (DiGi.GIS.PostgreSQL#150).
    /// <para>The measured disk use of the Main database after the pilot imports (DiGi.GIS.PostgreSQL#126), and the per-county sizes the full-country roll-out is extrapolated from. A partitioned parent stores nothing itself - its indexes are inherited by every partition - so the sizes of a table's partitions sum to the table's size.</para>
    /// </summary>
    public class OpenBuildingTableSizeResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int? countyId;

        [JsonInclude, JsonPropertyName(nameof(DatasetKey))]
        private readonly string? datasetKey;

        [JsonInclude, JsonPropertyName(nameof(EstimatedCount))]
        private readonly long? estimatedCount;

        [JsonInclude, JsonPropertyName(nameof(TableName))]
        private readonly string? tableName;

        [JsonInclude, JsonPropertyName(nameof(TotalSize))]
        private readonly long totalSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingTableSizeResult"/> class.
        /// </summary>
        /// <param name="datasetKey">The dataset key of the table (<c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c>, <c>ghs_obat</c>).</param>
        /// <param name="tableName">The name of the table the storage unit belongs to.</param>
        /// <param name="countyId">The identifier of the county part the partition holds, or null for a table that is not partitioned.</param>
        /// <param name="totalSize">The total size in bytes - table, TOAST and indexes.</param>
        /// <param name="estimatedCount">The planner's row estimate, or null when the relation has never been analysed.</param>
        public OpenBuildingTableSizeResult(string? datasetKey, string? tableName, int? countyId, long totalSize, long? estimatedCount)
        {
            this.datasetKey = datasetKey;
            this.tableName = tableName;
            this.countyId = countyId;
            this.totalSize = totalSize;
            this.estimatedCount = estimatedCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingTableSizeResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="openBuildingTableSizeResult">The <see cref="OpenBuildingTableSizeResult"/> to copy from.</param>
        public OpenBuildingTableSizeResult(OpenBuildingTableSizeResult? openBuildingTableSizeResult)
            : base(openBuildingTableSizeResult)
        {
            if (openBuildingTableSizeResult is not null)
            {
                datasetKey = openBuildingTableSizeResult.datasetKey;
                tableName = openBuildingTableSizeResult.tableName;
                countyId = openBuildingTableSizeResult.countyId;
                totalSize = openBuildingTableSizeResult.totalSize;
                estimatedCount = openBuildingTableSizeResult.estimatedCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingTableSizeResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OpenBuildingTableSizeResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the county part the partition holds, or null for a table that is not partitioned (GHS-OBAT), whose size covers every county it holds.
        /// </summary>
        [JsonIgnore]
        public int? CountyId => countyId;

        /// <summary>
        /// Gets the dataset key of the table (<c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c>, <c>ghs_obat</c>).
        /// </summary>
        [JsonIgnore]
        public string? DatasetKey => datasetKey;

        /// <summary>
        /// Gets the planner's row estimate (<c>pg_class.reltuples</c>), or null when the relation has never been analysed.
        /// <para>An estimate, refreshed by <c>ANALYZE</c> and autovacuum: right after an import it may still describe the partition before the run. The exact count is the register's <c>row_count</c> or the open building summaries.</para>
        /// </summary>
        [JsonIgnore]
        public long? EstimatedCount => estimatedCount;

        /// <summary>
        /// Gets the name of the table the storage unit belongs to - the partitioned parent for a partition, never the partition's own name.
        /// </summary>
        [JsonIgnore]
        public string? TableName => tableName;

        /// <summary>
        /// Gets the total size in bytes (<c>pg_total_relation_size</c>): the table, its TOAST table and every index.
        /// </summary>
        [JsonIgnore]
        public long TotalSize => totalSize;
    }
}
