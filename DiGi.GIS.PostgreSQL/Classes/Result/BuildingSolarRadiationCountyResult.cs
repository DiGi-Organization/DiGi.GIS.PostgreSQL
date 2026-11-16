using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// What one county part of the building solar radiation table holds for one neighbour radius and solver version: how many results, how many of the part's residential buildings they cover, when they were written and how large the part's partition is (DiGi.GIS.PostgreSQL#138).
    /// <para>It is the lasting account of a batch solve (<c>PostgreSQLBuildingSolarRadiationSolveTask</c>), whose own counts are discarded when it ends. It describes the database it was read from.</para>
    /// </summary>
    public class BuildingSolarRadiationCountyResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(CreatedAtMax))]
        private readonly DateTimeOffset? createdAtMax;

        [JsonInclude, JsonPropertyName(nameof(CreatedAtMin))]
        private readonly DateTimeOffset? createdAtMin;

        [JsonInclude, JsonPropertyName(nameof(PartitionSize))]
        private readonly long? partitionSize;

        [JsonInclude, JsonPropertyName(nameof(Radius))]
        private readonly double radius;

        [JsonInclude, JsonPropertyName(nameof(ResidentialBuildingCount))]
        private readonly long? residentialBuildingCount;

        [JsonInclude, JsonPropertyName(nameof(ResidentialCoveredCount))]
        private readonly long? residentialCoveredCount;

        [JsonInclude, JsonPropertyName(nameof(RowCount))]
        private readonly long rowCount;

        [JsonInclude, JsonPropertyName(nameof(SolverVersion))]
        private readonly int solverVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingSolarRadiationCountyResult"/> class.
        /// </summary>
        /// <param name="countyId">The identifier of the county part summarised.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored.</param>
        /// <param name="solverVersion">The solver version.</param>
        /// <param name="rowCount">The number of results stored.</param>
        /// <param name="residentialBuildingCount">The number of buildings of the part whose building data row is residential, or null when it cannot be read.</param>
        /// <param name="residentialCoveredCount">The number of residential buildings of the part with a result for this radius and solver version, or null when it cannot be read.</param>
        /// <param name="createdAtMin">The earliest creation time of the results.</param>
        /// <param name="createdAtMax">The latest creation time of the results.</param>
        /// <param name="partitionSize">The total size, in bytes, of the part's partition, or null when it cannot be read.</param>
        public BuildingSolarRadiationCountyResult(
            int countyId,
            double radius,
            int solverVersion,
            long rowCount,
            long? residentialBuildingCount,
            long? residentialCoveredCount,
            DateTimeOffset? createdAtMin,
            DateTimeOffset? createdAtMax,
            long? partitionSize)
        {
            this.countyId = countyId;
            this.radius = radius;
            this.solverVersion = solverVersion;
            this.rowCount = rowCount;
            this.residentialBuildingCount = residentialBuildingCount;
            this.residentialCoveredCount = residentialCoveredCount;
            this.createdAtMin = createdAtMin;
            this.createdAtMax = createdAtMax;
            this.partitionSize = partitionSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingSolarRadiationCountyResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="buildingSolarRadiationCountyResult">The <see cref="BuildingSolarRadiationCountyResult"/> to copy from.</param>
        public BuildingSolarRadiationCountyResult(BuildingSolarRadiationCountyResult? buildingSolarRadiationCountyResult)
            : base(buildingSolarRadiationCountyResult)
        {
            if (buildingSolarRadiationCountyResult is not null)
            {
                countyId = buildingSolarRadiationCountyResult.countyId;
                radius = buildingSolarRadiationCountyResult.radius;
                solverVersion = buildingSolarRadiationCountyResult.solverVersion;
                rowCount = buildingSolarRadiationCountyResult.rowCount;
                residentialBuildingCount = buildingSolarRadiationCountyResult.residentialBuildingCount;
                residentialCoveredCount = buildingSolarRadiationCountyResult.residentialCoveredCount;
                createdAtMin = buildingSolarRadiationCountyResult.createdAtMin;
                createdAtMax = buildingSolarRadiationCountyResult.createdAtMax;
                partitionSize = buildingSolarRadiationCountyResult.partitionSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingSolarRadiationCountyResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public BuildingSolarRadiationCountyResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the county part summarised.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the latest creation time of the results.
        /// <para>A write for an existing key replaces <c>created_at</c>, so with <see cref="CreatedAtMin"/> it gives the wall-clock span of a fresh run, not of the first write ever.</para>
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? CreatedAtMax => createdAtMax;

        /// <summary>
        /// Gets the earliest creation time of the results. A write for an existing key replaces <c>created_at</c>.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? CreatedAtMin => createdAtMin;

        /// <summary>
        /// Gets the total size, in bytes, of the part's partition - table, TOAST and indexes (<c>pg_total_relation_size</c>) - or null when it cannot be read. Every radius and solver version of a part carries the same value.
        /// </summary>
        [JsonIgnore]
        public long? PartitionSize => partitionSize;

        /// <summary>
        /// Gets the neighbour radius, in m, exactly as stored.
        /// </summary>
        [JsonIgnore]
        public double Radius => radius;

        /// <summary>
        /// Gets the number of buildings of the part whose building data row is residential, or null when the database holds no building data or no residential column. Every radius and solver version of a part carries the same value.
        /// </summary>
        [JsonIgnore]
        public long? ResidentialBuildingCount => residentialBuildingCount;

        /// <summary>
        /// Gets the number of residential buildings of the part with a result for this radius and solver version, or null when the database holds no building data or no residential column.
        /// </summary>
        [JsonIgnore]
        public long? ResidentialCoveredCount => residentialCoveredCount;

        /// <summary>
        /// Gets the number of results stored for the part, radius and solver version.
        /// </summary>
        [JsonIgnore]
        public long RowCount => rowCount;

        /// <summary>
        /// Gets the solver version.
        /// </summary>
        [JsonIgnore]
        public int SolverVersion => solverVersion;
    }
}
