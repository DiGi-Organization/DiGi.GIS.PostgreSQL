using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the mean absolute error of a census grid comparison: the average of <c>|modelled − census|</c> over the cells whose census value is present (DiGi.GIS.PostgreSQL#106).
        /// <para>Cells hidden by statistical secrecy carry no census value and are skipped - they are counted by <see cref="PopulationGridComparisonResult.MaskedCellCount"/>, not measured against a zero. The result is null when no cell carries a census value, which is the absence of a comparison rather than a perfect one.</para>
        /// </summary>
        /// <param name="populationGridComparisonItems">The per-cell rows of the comparison.</param>
        /// <returns>The mean absolute error in persons, or null when no cell carries a census value.</returns>
        public static double? Mae(this IReadOnlyList<PopulationGridComparisonItem>? populationGridComparisonItems)
        {
            if (populationGridComparisonItems is null || populationGridComparisonItems.Count == 0)
            {
                return null;
            }

            double sum = 0;
            int count = 0;
            foreach (PopulationGridComparisonItem? populationGridComparisonItem in populationGridComparisonItems)
            {
                if (populationGridComparisonItem?.CensusPopulation is not int censusPopulation)
                {
                    continue;
                }

                double error = populationGridComparisonItem.ModelledPopulation - censusPopulation;
                sum += System.Math.Abs(error);
                count++;
            }

            return count == 0 ? null : sum / count;
        }
    }
}
