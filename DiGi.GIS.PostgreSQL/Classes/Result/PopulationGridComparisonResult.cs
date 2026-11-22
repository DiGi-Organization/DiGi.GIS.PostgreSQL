using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The result of comparing the modelled building occupancy of one county against one census year's 1 km population grid, cell by cell (DiGi.GIS.PostgreSQL#106).
    /// <para>The metrics are taken over the cells whose census value is present: <see cref="Mae"/> is the mean absolute error, <see cref="Bias"/> the mean error (positive = the model over-counts) and <see cref="R2"/> the predictive coefficient of determination <c>1 − Σe² / Σ(census − mean)²</c>, which may be negative and is null when fewer than two cells carry a census value or that value has no variance. Cells hidden by statistical secrecy are counted in <see cref="MaskedCellCount"/> and left out of all three - comparing against a zero the census never claimed would manufacture an error of exactly the modelled sum.</para>
    /// <para>The per-cell rows are ordered by cell identifier, so the same inputs always serialise to the same document.</para>
    /// </summary>
    public class PopulationGridComparisonResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(CensusYear))]
        private readonly int censusYear;

        [JsonInclude, JsonPropertyName(nameof(Mae))]
        private readonly double? mae;

        [JsonInclude, JsonPropertyName(nameof(Bias))]
        private readonly double? bias;

        [JsonInclude, JsonPropertyName(nameof(R2))]
        private readonly double? r2;

        [JsonInclude, JsonPropertyName(nameof(ComparedCellCount))]
        private readonly int comparedCellCount;

        [JsonInclude, JsonPropertyName(nameof(MaskedCellCount))]
        private readonly int maskedCellCount;

        [JsonInclude, JsonPropertyName(nameof(BuildingOutsideCellCount))]
        private readonly int buildingOutsideCellCount;

        [JsonInclude, JsonPropertyName(nameof(Items))]
        private readonly List<PopulationGridComparisonItem>? items;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridComparisonResult"/> class.
        /// <para>The metrics are parameters rather than calculations because a constructor sits on the path of every clone and every deserialization; <see cref="Query.Mae(System.Collections.Generic.IReadOnlyList{PopulationGridComparisonItem}?)"/>, <see cref="Query.Bias(System.Collections.Generic.IReadOnlyList{PopulationGridComparisonItem}?)"/> and <see cref="Query.R2(System.Collections.Generic.IReadOnlyList{PopulationGridComparisonItem}?)"/> are what work them out.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county partition the comparison describes.</param>
        /// <param name="censusYear">The census year of the grid compared against (for example 2021).</param>
        /// <param name="mae">The mean absolute error over the cells with a census value, or null when there are none.</param>
        /// <param name="bias">The mean error over the cells with a census value, or null when there are none.</param>
        /// <param name="r2">The predictive coefficient of determination over the cells with a census value, or null when it cannot be derived.</param>
        /// <param name="comparedCellCount">The number of compared cells whose census value is present.</param>
        /// <param name="maskedCellCount">The number of compared cells whose census value is hidden by statistical secrecy.</param>
        /// <param name="buildingOutsideCellCount">The number of county buildings whose centre lies in no stored cell - a masked or absent cell, or outside the grid's coverage.</param>
        /// <param name="items">The per-cell comparison rows, ordered by cell identifier.</param>
        public PopulationGridComparisonResult(
            int countyId,
            int censusYear,
            double? mae,
            double? bias,
            double? r2,
            int comparedCellCount,
            int maskedCellCount,
            int buildingOutsideCellCount,
            IEnumerable<PopulationGridComparisonItem>? items)
        {
            this.countyId = countyId;
            this.censusYear = censusYear;
            this.mae = mae;
            this.bias = bias;
            this.r2 = r2;
            this.comparedCellCount = comparedCellCount;
            this.maskedCellCount = maskedCellCount;
            this.buildingOutsideCellCount = buildingOutsideCellCount;
            this.items = items is null ? null : [.. items];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridComparisonResult"/> class by copying another instance.
        /// </summary>
        /// <param name="populationGridComparisonResult">The source instance to copy from.</param>
        public PopulationGridComparisonResult(PopulationGridComparisonResult? populationGridComparisonResult)
            : base(populationGridComparisonResult)
        {
            if (populationGridComparisonResult is not null)
            {
                countyId = populationGridComparisonResult.countyId;
                censusYear = populationGridComparisonResult.censusYear;
                mae = populationGridComparisonResult.mae;
                bias = populationGridComparisonResult.bias;
                r2 = populationGridComparisonResult.r2;
                comparedCellCount = populationGridComparisonResult.comparedCellCount;
                maskedCellCount = populationGridComparisonResult.maskedCellCount;
                buildingOutsideCellCount = populationGridComparisonResult.buildingOutsideCellCount;

                if (populationGridComparisonResult.items is not null)
                {
                    items = [];
                    foreach (PopulationGridComparisonItem? populationGridComparisonItem in populationGridComparisonResult.items)
                    {
                        if (Core.Query.Clone(populationGridComparisonItem) is PopulationGridComparisonItem populationGridComparisonItem_Clone)
                        {
                            items.Add(populationGridComparisonItem_Clone);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridComparisonResult"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public PopulationGridComparisonResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier of the county partition the comparison describes.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the census year of the grid compared against (for example 2021).
        /// </summary>
        [JsonIgnore]
        public int CensusYear => censusYear;

        /// <summary>
        /// Gets the mean absolute error over the cells with a census value, or null when there are none.
        /// </summary>
        [JsonIgnore]
        public double? Mae => mae;

        /// <summary>
        /// Gets the mean error over the cells with a census value, or null when there are none.
        /// <para>Positive means the model puts more people into the county's cells than the census counted there, negative means fewer.</para>
        /// </summary>
        [JsonIgnore]
        public double? Bias => bias;

        /// <summary>
        /// Gets the predictive coefficient of determination over the cells with a census value, or null when it cannot be derived.
        /// <para>Defined as <c>1 − Σe² / Σ(census − mean)²</c>; it may be negative, and null stands for fewer than two compared cells or a census value with no variance. A squared Pearson correlation would be a different number - the definition here is the one to cite.</para>
        /// </summary>
        [JsonIgnore]
        public double? R2 => r2;

        /// <summary>
        /// Gets the number of compared cells whose census value is present.
        /// </summary>
        [JsonIgnore]
        public int ComparedCellCount => comparedCellCount;

        /// <summary>
        /// Gets the number of compared cells whose census value is hidden by statistical secrecy.
        /// </summary>
        [JsonIgnore]
        public int MaskedCellCount => maskedCellCount;

        /// <summary>
        /// Gets the number of county buildings whose centre lies in no stored cell - a masked or absent cell, or outside the grid's coverage.
        /// </summary>
        [JsonIgnore]
        public int BuildingOutsideCellCount => buildingOutsideCellCount;

        /// <summary>
        /// Gets the per-cell comparison rows, ordered by cell identifier.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<PopulationGridComparisonItem>? Items => items;
    }
}
