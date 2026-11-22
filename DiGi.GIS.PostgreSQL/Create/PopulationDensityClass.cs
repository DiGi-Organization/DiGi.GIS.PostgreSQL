namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Gets the density class a census grid cell falls into, from the population the census counted in it (DiGi.GIS.PostgreSQL#106).
        /// <para>The class is a property of the census value alone, never of the modelled sum: comparing a model against a classification the model itself would place is circular. A null population is <see cref="Enums.PopulationDensityClass.Masked"/> - statistical secrecy hides the value, which is not the same as the census reporting an empty cell - and a value outside every band is <see cref="Enums.PopulationDensityClass.Undefined"/>. The bands are the upper bounds of <see cref="Constants.PopulationDensity"/>.</para>
        /// </summary>
        /// <param name="population">The population of the cell, or null where statistical secrecy hides it.</param>
        /// <returns>The density class of the cell.</returns>
        public static Enums.PopulationDensityClass PopulationDensityClass(int? population)
        {
            if (population is not int population_Value)
            {
                return Enums.PopulationDensityClass.Masked;
            }

            if (population_Value < 0)
            {
                return Enums.PopulationDensityClass.Undefined;
            }

            if (population_Value == 0)
            {
                return Enums.PopulationDensityClass.Zero;
            }

            if (population_Value <= Constants.PopulationDensity.VeryLow_Maximum)
            {
                return Enums.PopulationDensityClass.VeryLow;
            }

            if (population_Value <= Constants.PopulationDensity.Low_Maximum)
            {
                return Enums.PopulationDensityClass.Low;
            }

            if (population_Value <= Constants.PopulationDensity.Medium_Maximum)
            {
                return Enums.PopulationDensityClass.Medium;
            }

            if (population_Value <= Constants.PopulationDensity.High_Maximum)
            {
                return Enums.PopulationDensityClass.High;
            }

            return Enums.PopulationDensityClass.VeryHigh;
        }
    }
}
