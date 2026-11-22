using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One cell of a census grid comparison (DiGi.GIS.PostgreSQL#106): what the modelled building occupancy summed to in the cell, what the census counted there, the difference between the two, how many county buildings the cell holds, and the cell's census density class.
    /// <para><see cref="CensusPopulation"/> is null where statistical secrecy hides the census value, and <see cref="Error"/> follows it to null - a hidden cell is excluded from the metrics rather than compared against a zero it never claimed. The modelled sum and the building count stay real for such a cell: they are what the model says, whoever is forbidden from saying what the census says.</para>
    /// </summary>
    public class PopulationGridComparisonItem : SerializableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CellId))]
        private readonly string? cellId = null;

        [JsonInclude, JsonPropertyName(nameof(CensusPopulation))]
        private readonly int? censusPopulation = null;

        [JsonInclude, JsonPropertyName(nameof(ModelledPopulation))]
        private readonly long modelledPopulation = 0;

        [JsonInclude, JsonPropertyName(nameof(Error))]
        private readonly long? error = null;

        [JsonInclude, JsonPropertyName(nameof(BuildingCount))]
        private readonly int buildingCount = 0;

        [JsonInclude, JsonPropertyName(nameof(DensityClass))]
        private readonly PopulationDensityClass densityClass = PopulationDensityClass.Undefined;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridComparisonItem"/> class.
        /// <para>The constructor only assigns; the error and the density class are worked out by <see cref="Create.PopulationGridComparisonResultAsync"/>, which holds the census value the error is measured against.</para>
        /// </summary>
        /// <param name="cellId">The identifier the source publishes for the cell.</param>
        /// <param name="censusPopulation">The population the census counted in the cell, or null where statistical secrecy hides it.</param>
        /// <param name="modelledPopulation">The sum of the modelled occupancy of the county buildings whose centre lies in the cell.</param>
        /// <param name="error">The modelled sum minus the census population, or null when the census value is hidden.</param>
        /// <param name="buildingCount">The number of county buildings whose centre lies in the cell, whether or not an occupancy was read for them.</param>
        /// <param name="densityClass">The density class of the census population, <see cref="PopulationDensityClass.Masked"/> where it is hidden.</param>
        public PopulationGridComparisonItem(string? cellId, int? censusPopulation, long modelledPopulation, long? error, int buildingCount, PopulationDensityClass densityClass)
        {
            this.cellId = cellId;
            this.censusPopulation = censusPopulation;
            this.modelledPopulation = modelledPopulation;
            this.error = error;
            this.buildingCount = buildingCount;
            this.densityClass = densityClass;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridComparisonItem"/> class by copying another instance.
        /// </summary>
        /// <param name="populationGridComparisonItem">The source instance to copy data from.</param>
        public PopulationGridComparisonItem(PopulationGridComparisonItem? populationGridComparisonItem)
            : base(populationGridComparisonItem)
        {
            if (populationGridComparisonItem is not null)
            {
                cellId = populationGridComparisonItem.cellId;
                censusPopulation = populationGridComparisonItem.censusPopulation;
                modelledPopulation = populationGridComparisonItem.modelledPopulation;
                error = populationGridComparisonItem.error;
                buildingCount = populationGridComparisonItem.buildingCount;
                densityClass = populationGridComparisonItem.densityClass;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridComparisonItem"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public PopulationGridComparisonItem(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier the source publishes for the cell.
        /// </summary>
        [JsonIgnore]
        public string? CellId => cellId;

        /// <summary>
        /// Gets the population the census counted in the cell, or null where statistical secrecy hides it.
        /// </summary>
        [JsonIgnore]
        public int? CensusPopulation => censusPopulation;

        /// <summary>
        /// Gets the sum of the modelled occupancy of the county buildings whose centre lies in the cell.
        /// </summary>
        [JsonIgnore]
        public long ModelledPopulation => modelledPopulation;

        /// <summary>
        /// Gets the modelled sum minus the census population, or null when the census value is hidden.
        /// <para>Positive means the model puts more people in the cell than the census counted, negative means fewer. The metrics of <see cref="Classes.PopulationGridComparisonResult"/> are taken over the items whose error is present.</para>
        /// </summary>
        [JsonIgnore]
        public long? Error => error;

        /// <summary>
        /// Gets the number of county buildings whose centre lies in the cell, whether or not an occupancy was read for them.
        /// </summary>
        [JsonIgnore]
        public int BuildingCount => buildingCount;

        /// <summary>
        /// Gets the density class of the census population, <see cref="PopulationDensityClass.Masked"/> where it is hidden.
        /// </summary>
        [JsonIgnore]
        public PopulationDensityClass DensityClass => densityClass;
    }
}
