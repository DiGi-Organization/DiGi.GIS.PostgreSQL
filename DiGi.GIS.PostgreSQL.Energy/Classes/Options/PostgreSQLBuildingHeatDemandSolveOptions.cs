using DiGi.Analytical.Building.Energy.Classes;
using DiGi.Analytical.Building.Regulation.Enums;
using DiGi.Core.Classes;
using DiGi.GIS.Classes;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Energy.Classes
{
    /// <summary>
    /// Provides the options of the county heat-demand solve, <see cref="BuildingHeatDemandSolveTask"/> (DiGi.GIS.PostgreSQL#143).
    /// </summary>
    public class PostgreSQLBuildingHeatDemandSolveOptions : SerializableOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingHeatDemandSolveOptions"/> class with the defaults.
        /// </summary>
        public PostgreSQLBuildingHeatDemandSolveOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingHeatDemandSolveOptions"/> class by copying an existing options instance.
        /// </summary>
        /// <param name="postgreSQLBuildingHeatDemandSolveOptions">The options to copy. This value can be null.</param>
        public PostgreSQLBuildingHeatDemandSolveOptions(PostgreSQLBuildingHeatDemandSolveOptions? postgreSQLBuildingHeatDemandSolveOptions)
            : base(postgreSQLBuildingHeatDemandSolveOptions)
        {
            if (postgreSQLBuildingHeatDemandSolveOptions is not null)
            {
                BatchSize = postgreSQLBuildingHeatDemandSolveOptions.BatchSize;
                BuildingCountMax = postgreSQLBuildingHeatDemandSolveOptions.BuildingCountMax;
                CommandTimeout = postgreSQLBuildingHeatDemandSolveOptions.CommandTimeout;
                ConstructionFlavour = postgreSQLBuildingHeatDemandSolveOptions.ConstructionFlavour;
                CountyIds = postgreSQLBuildingHeatDemandSolveOptions.CountyIds is null ? null : [.. postgreSQLBuildingHeatDemandSolveOptions.CountyIds];
                HeatDemandOptions = postgreSQLBuildingHeatDemandSolveOptions.HeatDemandOptions is null ? null : new HeatDemandOptions(postgreSQLBuildingHeatDemandSolveOptions.HeatDemandOptions);
                MaxDegreeOfParallelism = postgreSQLBuildingHeatDemandSolveOptions.MaxDegreeOfParallelism;
                NeighbourRadius = postgreSQLBuildingHeatDemandSolveOptions.NeighbourRadius;
                SkipCompleted = postgreSQLBuildingHeatDemandSolveOptions.SkipCompleted;
                SolarRadius = postgreSQLBuildingHeatDemandSolveOptions.SolarRadius;
                TileSize = postgreSQLBuildingHeatDemandSolveOptions.TileSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingHeatDemandSolveOptions"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the options. This value can be null.</param>
        public PostgreSQLBuildingHeatDemandSolveOptions(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets or sets the number of buildings read, calculated and written together.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BatchSize))]
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the number of buildings of each county part after which its walk stops, for a pilot run; null for every building.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(BuildingCountMax))]
        public int? BuildingCountMax { get; set; } = null;

        /// <summary>
        /// Gets or sets the timeout in seconds of every database command; 0 disables it.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CommandTimeout))]
        public int CommandTimeout { get; set; } = 600;

        /// <summary>
        /// Gets or sets the construction flavour of the envelope and the openings: the TABULA constructions of the year built, or the regulated limits of its regime. The typology data is the TypicalAsBuilt set's either way.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(ConstructionFlavour))]
        public ConstructionFlavour ConstructionFlavour { get; set; } = ConstructionFlavour.TypicalAsBuilt;

        /// <summary>
        /// Gets or sets the ids of the county parts to calculate; null for every part. A county with several parts is calculated part by part, each under its own id (<c>Coding - GIS Administrative Data.md</c>).
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(CountyIds))]
        public HashSet<int>? CountyIds { get; set; } = null;

        /// <summary>
        /// Gets or sets the options of the heat-demand engine; null for its defaults.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(HeatDemandOptions))]
        public HeatDemandOptions? HeatDemandOptions { get; set; } = null;

        /// <summary>
        /// Gets or sets the largest number of buildings calculated at once; 0 or less for half the processors.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(MaxDegreeOfParallelism))]
        public int MaxDegreeOfParallelism { get; set; } = 0;

        /// <summary>
        /// Gets or sets the radius, in m, around a building within which other buildings are its neighbours: for the party walls (within 0.2 m) and for the fire-separation and obstruction rules of the opening generation. The default is <see cref="OpeningGenerationOptions.ObstructionMaxDistance"/>, the farthest a neighbour counts there.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(NeighbourRadius))]
        public double NeighbourRadius { get; set; } = new OpeningGenerationOptions().ObstructionMaxDistance;

        /// <summary>
        /// Gets or sets a value indicating whether a building that already has a result of the current method version is stepped over, so a stopped run resumes where it stopped.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SkipCompleted))]
        public bool SkipCompleted { get; set; } = true;

        /// <summary>
        /// Gets or sets the neighbour radius, in m, of the stored solar result the irradiation is taken from. A building without a result of this radius takes its latest result of any radius, and a building without any result the unshaded irradiation; the radius used is recorded in each result.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(SolarRadius))]
        public double SolarRadius { get; set; } = 100;

        /// <summary>
        /// Gets or sets the side, in m, of the square tiles a county part is walked in, by the centroid of each building, so the buildings of a batch lie close together and share their neighbours.
        /// </summary>
        [JsonInclude, JsonPropertyName(nameof(TileSize))]
        public double TileSize { get; set; } = 250;
    }
}
