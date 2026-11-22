namespace DiGi.GIS.PostgreSQL.Constants
{
    /// <summary>
    /// Provides the population-per-cell boundaries of the density classes of <see cref="Enums.PopulationDensityClass"/> (DiGi.GIS.PostgreSQL#106).
    /// <para>The census grid cell is 1 km², so a cell's population is its population density per square kilometre and the bands read directly against persons/km². The boundaries are upper bounds: a class holds every value up to and including its maximum, and the next class starts one person above it.</para>
    /// </summary>
    public static class PopulationDensity
    {
        /// <summary>
        /// The inclusive upper bound, in persons per km², of the <see cref="Enums.PopulationDensityClass.VeryLow"/> class (50).
        /// </summary>
        public const int VeryLow_Maximum = 50;

        /// <summary>
        /// The inclusive upper bound, in persons per km², of the <see cref="Enums.PopulationDensityClass.Low"/> class (200).
        /// </summary>
        public const int Low_Maximum = 200;

        /// <summary>
        /// The inclusive upper bound, in persons per km², of the <see cref="Enums.PopulationDensityClass.Medium"/> class (1000).
        /// </summary>
        public const int Medium_Maximum = 1000;

        /// <summary>
        /// The inclusive upper bound, in persons per km², of the <see cref="Enums.PopulationDensityClass.High"/> class (5000).
        /// </summary>
        public const int High_Maximum = 5000;
    }
}
