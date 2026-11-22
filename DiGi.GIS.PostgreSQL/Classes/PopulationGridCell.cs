using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// One row of the <see cref="Constants.TableName.PopulationGrid"/> table: one cell of the GUS 1 km census population grid for one census year (DiGi.GIS.PostgreSQL#106).
    /// <para>The outline is stored reprojected to EPSG:2180 (easting in X), the coordinate reference system everything this repository stores uses; the source publishes the grid in EPSG:3035 and the import performs the conversion. The population is null where statistical secrecy hides the cell's value - a hidden cell is not a zero, and a comparison that read it as one would invent an error of exactly the modelled sum.</para>
    /// </summary>
    public class PopulationGridCell : SerializableObject, ITableObject, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(CellId))]
        private readonly string? cellId = null;

        [JsonInclude, JsonPropertyName(nameof(CensusYear))]
        private readonly int censusYear = 0;

        [JsonInclude, JsonPropertyName(nameof(Population))]
        private readonly int? population = null;

        [JsonInclude, JsonPropertyName(nameof(PolygonalFace2D))]
        private readonly PolygonalFace2D? polygonalFace2D = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridCell"/> class.
        /// <para>The constructor only assigns. What validates the inputs - a cell identifier, a plausible census year, a finite population - belongs to the reader of the source file, which is where a malformed export has to fail loudly rather than be normalised away here.</para>
        /// </summary>
        /// <param name="cellId">The identifier the source publishes for the cell.</param>
        /// <param name="censusYear">The census the population was counted in (for example 2021).</param>
        /// <param name="population">The population of the cell, or null where statistical secrecy hides it.</param>
        /// <param name="polygonalFace2D">The cell outline in EPSG:2180.</param>
        public PopulationGridCell(string? cellId, int censusYear, int? population, PolygonalFace2D? polygonalFace2D)
        {
            this.cellId = cellId;
            this.censusYear = censusYear;
            this.population = population;
            this.polygonalFace2D = polygonalFace2D;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridCell"/> class by copying another instance.
        /// </summary>
        /// <param name="populationGridCell">The source instance to copy data from.</param>
        public PopulationGridCell(PopulationGridCell? populationGridCell)
            : base(populationGridCell)
        {
            if (populationGridCell is not null)
            {
                cellId = populationGridCell.cellId;
                censusYear = populationGridCell.censusYear;
                population = populationGridCell.population;
                polygonalFace2D = Core.Query.Clone(populationGridCell.polygonalFace2D);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridCell"/> class from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> containing the serialized data.</param>
        public PopulationGridCell(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the identifier the source publishes for the cell.
        /// </summary>
        [JsonIgnore]
        public string? CellId => cellId;

        /// <summary>
        /// Gets the census the population was counted in (for example 2021).
        /// </summary>
        [JsonIgnore]
        public int CensusYear => censusYear;

        /// <summary>
        /// Gets the population of the cell, or null where statistical secrecy hides it.
        /// </summary>
        [JsonIgnore]
        public int? Population => population;

        /// <summary>
        /// Gets the cell outline in EPSG:2180, or null when the stored object does not parse as a polygon.
        /// </summary>
        [JsonIgnore]
        public PolygonalFace2D? PolygonalFace2D => polygonalFace2D;
    }
}
