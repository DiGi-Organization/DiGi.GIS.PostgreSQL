using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// How much of one area's buildings the orthophoto store holds, measured over the buildings inside that area's polygon rather than over its county's.
    /// <para>The figure the estimated county-level counts cannot give. <c>orto_datas</c> and <c>building_2d</c> are partitioned by <c>county_id</c>, so a partition estimate describes a whole county and says nothing about any area inside it; this is counted, not estimated.</para>
    /// <para>Counted from the building side alone, by geometry. A building belongs to an area when its centre lies inside the area's polygon - not when its stored <c>subdivision_id</c> names it, which files a building under one subdivision only and so cannot say which buildings a district holds where the subdivision layer nests (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>). <c>orto_datas</c> carries a <c>subdivision_id</c> column of its own, but it has never been written - not one of the 8 384 055 rows stored across 225 counties carries a value - so grouping the orthophoto side by it answers zero for every subdivision in the country.</para>
    /// </summary>
    public class OrtoDatasCoverageResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(AdministrativeAreal2DId))]
        private readonly int? administrativeAreal2DId;

        [JsonInclude, JsonPropertyName(nameof(Building2DCount))]
        private readonly long building2DCount;

        [JsonInclude, JsonPropertyName(nameof(OrtoDatasCount))]
        private readonly long ortoDatasCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDatasCoverageResult"/> class.
        /// </summary>
        /// <param name="countyId">The identifier of the county the coverage was measured in.</param>
        /// <param name="administrativeAreal2DId">The identifier of the area the coverage describes, or null for the county's buildings that lie inside none of the areas measured.</param>
        /// <param name="building2DCount">The number of buildings the area holds.</param>
        /// <param name="ortoDatasCount">The number of those buildings that have an orthophoto row.</param>
        public OrtoDatasCoverageResult(int countyId, int? administrativeAreal2DId, long building2DCount, long ortoDatasCount)
        {
            this.countyId = countyId;
            this.administrativeAreal2DId = administrativeAreal2DId;
            this.building2DCount = building2DCount;
            this.ortoDatasCount = ortoDatasCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDatasCoverageResult"/> class by copying an existing one.
        /// </summary>
        /// <param name="ortoDatasCoverageResult">The <see cref="OrtoDatasCoverageResult"/> to copy from.</param>
        public OrtoDatasCoverageResult(OrtoDatasCoverageResult? ortoDatasCoverageResult)
            : base(ortoDatasCoverageResult)
        {
            if (ortoDatasCoverageResult is not null)
            {
                countyId = ortoDatasCoverageResult.countyId;
                administrativeAreal2DId = ortoDatasCoverageResult.administrativeAreal2DId;
                building2DCount = ortoDatasCoverageResult.building2DCount;
                ortoDatasCount = ortoDatasCoverageResult.ortoDatasCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoDatasCoverageResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public OrtoDatasCoverageResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the county the coverage was measured in.
        /// <para>One polygon part, not a code - a multi-part county is measured a part at a time, because that is how both tables are partitioned.</para>
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the identifier of the area the coverage describes - whichever polygon the caller asked to be measured: a subdivision, or a municipality measured over its own polygon rather than as a sum of subdivisions.
        /// <para>Null is not a missing value: it is the county's buildings whose centre lies inside none of the areas measured. Where the areas are the county's subdivisions those buildings belong to no subdivision and to no municipality, so nothing below county level should ever count them, and they are kept apart rather than folded into a neighbour.</para>
        /// <para>Where the areas nest, a building inside a neighbourhood is counted for the neighbourhood, its district and its city alike - the results of nested areas are not disjoint and must not be summed.</para>
        /// </summary>
        [JsonIgnore]
        public int? AdministrativeAreal2DId => administrativeAreal2DId;

        /// <summary>
        /// Gets the number of buildings the area holds. The denominator of the coverage.
        /// </summary>
        [JsonIgnore]
        public long Building2DCount => building2DCount;

        /// <summary>
        /// Gets the number of the area's buildings that have an orthophoto row. The numerator of the coverage.
        /// <para>Counted on references present in <c>orto_datas</c> for the same county, not on that table's own subdivision column, which has never been written.</para>
        /// </summary>
        [JsonIgnore]
        public long OrtoDatasCount => ortoDatasCount;
    }
}
