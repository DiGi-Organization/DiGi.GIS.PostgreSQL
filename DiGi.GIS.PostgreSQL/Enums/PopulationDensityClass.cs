using System.ComponentModel;

namespace DiGi.GIS.PostgreSQL.Enums
{
    /// <summary>
    /// The population density class a census grid cell falls into, by its population per 1 km² cell (DiGi.GIS.PostgreSQL#106).
    /// <para><see cref="Masked"/> is a cell whose value statistical secrecy hides - a missing value, not a zero - and <see cref="Zero"/> is a cell the census reports as empty. The two are kept apart so a comparison can exclude the masked cells from its metrics and say how many it excluded, while an empty cell stays a real observation of an error of exactly the modelled sum.</para>
    /// <para>The band boundaries are <see cref="Constants.PopulationDensity"/>; they are a reporting classification of the census population, never of the modelled one.</para>
    /// </summary>
    public enum PopulationDensityClass
    {
        /// <summary>
        /// The class is not known - a value outside every band, for example a negative population no census publishes.
        /// </summary>
        [Description("undefined")]
        Undefined,

        /// <summary>
        /// The cell's population is hidden by statistical secrecy; the stored value is null.
        /// </summary>
        [Description("masked")]
        Masked,

        /// <summary>
        /// The census reports the cell as empty.
        /// </summary>
        [Description("zero")]
        Zero,

        /// <summary>
        /// Up to and including 50 persons per km².
        /// </summary>
        [Description("1-50")]
        VeryLow,

        /// <summary>
        /// 51 to 200 persons per km².
        /// </summary>
        [Description("51-200")]
        Low,

        /// <summary>
        /// 201 to 1000 persons per km².
        /// </summary>
        [Description("201-1000")]
        Medium,

        /// <summary>
        /// 1001 to 5000 persons per km².
        /// </summary>
        [Description("1001-5000")]
        High,

        /// <summary>
        /// Above 5000 persons per km².
        /// </summary>
        [Description("5001+")]
        VeryHigh,
    }
}
