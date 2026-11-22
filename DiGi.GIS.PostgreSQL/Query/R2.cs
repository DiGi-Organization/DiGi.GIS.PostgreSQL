using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the predictive coefficient of determination of a census grid comparison: <c>1 − Σe² / Σ(census − mean)²</c> over the cells whose census value is present (DiGi.GIS.PostgreSQL#106).
        /// <para>The definition is the predictive one - the residual sum of squares against the census values' own variance - so the figure may be negative: a model that is worse than the mean of the census values scores below zero, and it is reported that way rather than clamped. It is a different number from a squared Pearson correlation, and the paper has to say which one it cites.</para>
        /// <para>The result is null when fewer than two cells carry a census value (the variance of one value is zero by definition) or when the census values carry no variance at all - a county of identical cells has nothing for a model to explain.</para>
        /// </summary>
        /// <param name="populationGridComparisonItems">The per-cell rows of the comparison.</param>
        /// <returns>The coefficient of determination, which may be negative, or null when it cannot be derived.</returns>
        public static double? R2(this IReadOnlyList<PopulationGridComparisonItem>? populationGridComparisonItems)
        {
            if (populationGridComparisonItems is null || populationGridComparisonItems.Count < 2)
            {
                return null;
            }

            double sum_ErrorSquared = 0;
            double sum_Census = 0;
            int count = 0;
            foreach (PopulationGridComparisonItem? populationGridComparisonItem in populationGridComparisonItems)
            {
                if (populationGridComparisonItem?.CensusPopulation is not int censusPopulation)
                {
                    continue;
                }

                double error = populationGridComparisonItem.ModelledPopulation - censusPopulation;
                sum_ErrorSquared += error * error;
                sum_Census += censusPopulation;
                count++;
            }

            if (count < 2)
            {
                return null;
            }

            double mean = sum_Census / count;

            double sum_Total = 0;
            foreach (PopulationGridComparisonItem? populationGridComparisonItem in populationGridComparisonItems)
            {
                if (populationGridComparisonItem?.CensusPopulation is not int censusPopulation)
                {
                    continue;
                }

                double difference = censusPopulation - mean;
                sum_Total += difference * difference;
            }

            if (!(sum_Total > 0) || double.IsInfinity(sum_ErrorSquared))
            {
                return null;
            }

            return 1 - (sum_ErrorSquared / sum_Total);
        }
    }
}
