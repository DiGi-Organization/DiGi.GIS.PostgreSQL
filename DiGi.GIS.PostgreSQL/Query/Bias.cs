using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the bias of a census grid comparison: the mean of <c>modelled − census</c> over the cells whose census value is present (DiGi.GIS.PostgreSQL#106).
        /// <para>Positive means the model puts more people into the compared cells than the census counted, negative means fewer - the direction a mean absolute error cannot show. Cells hidden by statistical secrecy are skipped, and the result is null when no cell carries a census value.</para>
        /// </summary>
        /// <param name="populationGridComparisonItems">The per-cell rows of the comparison.</param>
        /// <returns>The mean error in persons, or null when no cell carries a census value.</returns>
        public static double? Bias(this IReadOnlyList<PopulationGridComparisonItem>? populationGridComparisonItems)
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

                sum += populationGridComparisonItem.ModelledPopulation - censusPopulation;
                count++;
            }

            return count == 0 ? null : sum / count;
        }
    }
}
