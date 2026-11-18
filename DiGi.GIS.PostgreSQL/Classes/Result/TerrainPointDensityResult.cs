using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// How densely one county partition of the terrain point table is sampled: the points it holds divided by the area they were meant to cover.
    /// <para>The cheap way to ask whether a sampling run finished a county. It costs one aggregate over the partition and the county outlines, where deciding the same question point by point costs the generating and the looking up of every node of the lattice - so this is what narrows a country down to the few counties worth that.</para>
    /// <para>It reads as a ratio, which is also its limit: it says how much is missing and nothing about where. A county reported well below one is the input to a coverage query, not a conclusion.</para>
    /// </summary>
    public class TerrainPointDensityResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(Count))]
        private readonly long count;

        [JsonInclude, JsonPropertyName(nameof(Area))]
        private readonly double area;

        [JsonInclude, JsonPropertyName(nameof(Density))]
        private readonly double? density;

        [JsonInclude, JsonPropertyName(nameof(SpacingEquivalent))]
        private readonly double? spacingEquivalent;

        [JsonInclude, JsonPropertyName(nameof(ExpectedDensity))]
        private readonly double? expectedDensity;

        [JsonInclude, JsonPropertyName(nameof(Completeness))]
        private readonly double? completeness;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainPointDensityResult"/> class.
        /// <para>The derived figures are parameters rather than calculations because a constructor sits on the path of every clone and every deserialization. <see cref="Create.TerrainPointDensityResult(int, long, double, double?)"/> is what works them out.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county partition the density describes.</param>
        /// <param name="count">The number of points stored for the county.</param>
        /// <param name="area">The area the points were meant to cover, in square model units.</param>
        /// <param name="density">The points per square model unit, or null when the area is not positive.</param>
        /// <param name="spacingEquivalent">The spacing of a regular lattice of the same density, or null when it cannot be derived.</param>
        /// <param name="expectedDensity">The density a fully sampled county would report, or null when no grid size was given.</param>
        /// <param name="completeness">The share of the expected density actually stored, or null when it cannot be derived.</param>
        public TerrainPointDensityResult(int countyId, long count, double area, double? density, double? spacingEquivalent, double? expectedDensity, double? completeness)
        {
            this.countyId = countyId;
            this.count = count;
            this.area = area;
            this.density = density;
            this.spacingEquivalent = spacingEquivalent;
            this.expectedDensity = expectedDensity;
            this.completeness = completeness;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainPointDensityResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="terrainPointDensityResult">The <see cref="TerrainPointDensityResult"/> to copy from.</param>
        public TerrainPointDensityResult(TerrainPointDensityResult? terrainPointDensityResult)
            : base(terrainPointDensityResult)
        {
            if (terrainPointDensityResult is not null)
            {
                countyId = terrainPointDensityResult.countyId;
                count = terrainPointDensityResult.count;
                area = terrainPointDensityResult.area;
                density = terrainPointDensityResult.density;
                spacingEquivalent = terrainPointDensityResult.spacingEquivalent;
                expectedDensity = terrainPointDensityResult.expectedDensity;
                completeness = terrainPointDensityResult.completeness;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainPointDensityResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public TerrainPointDensityResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the county partition the density describes.
        /// <para>A county whose territory is in several pieces is one partition per piece, each with its own area and its own density. There is no meaningful way to combine two of them into one figure.</para>
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the number of points stored for the county.
        /// </summary>
        [JsonIgnore]
        public long Count => count;

        /// <summary>
        /// Gets the area the points were meant to cover, in square model units.
        /// <para>The partition's own outline, holes excluded, as far as the county's subdivisions cover it. A sampling run keeps a point only inside a subdivision - a point in none of them is discarded as outside the county's land - and files it under the part whose outline contains it, so this is the territory the count belongs to. Summing the subdivisions instead counts nested ones twice and, for a multi-part county, every sibling part; the outline alone counts water and land outside every subdivision, which no run samples (DiGi.GIS.WebAPI#98).</para>
        /// </summary>
        [JsonIgnore]
        public double Area => area;

        /// <summary>
        /// Gets the points per square model unit, or null when the area is not positive.
        /// </summary>
        [JsonIgnore]
        public double? Density => density;

        /// <summary>
        /// Gets the spacing of a regular lattice of the same density, or null when it cannot be derived.
        /// <para>The figure to read when the grid size a run used is not known. A county sampled fully at a hundred metres reports about a hundred; one that received half of its tiles reports about a hundred and forty.</para>
        /// </summary>
        [JsonIgnore]
        public double? SpacingEquivalent => spacingEquivalent;

        /// <summary>
        /// Gets the density a fully sampled county would report, or null when no grid size was given.
        /// </summary>
        [JsonIgnore]
        public double? ExpectedDensity => expectedDensity;

        /// <summary>
        /// Gets the share of the expected density actually stored, or null when it cannot be derived.
        /// <para>One means the county holds what a complete pass would leave. Above one means the county holds more than the grid size accounts for - a finer earlier pass, or points imported from elsewhere.</para>
        /// </summary>
        [JsonIgnore]
        public double? Completeness => completeness;
    }
}
