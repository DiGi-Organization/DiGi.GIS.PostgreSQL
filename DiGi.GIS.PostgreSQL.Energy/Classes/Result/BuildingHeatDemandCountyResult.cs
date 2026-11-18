using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Energy.Classes
{
    /// <summary>
    /// Represents what one county part holds in <see cref="Constants.TableName.BuildingHeatDemand"/> for one method version (DiGi.GIS.PostgreSQL#143): how many buildings, their total and mean energy need for space heating, and when they were written.
    /// </summary>
    public class BuildingHeatDemandCountyResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(BuildingCount))]
        private readonly long buildingCount;

        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(FirstCreatedAt))]
        private readonly DateTimeOffset? firstCreatedAt;

        [JsonInclude, JsonPropertyName(nameof(HeatingEnergyNeed))]
        private readonly double heatingEnergyNeed = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(HeatingEnergyNeedPerArea))]
        private readonly double heatingEnergyNeedPerArea = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(LastCreatedAt))]
        private readonly DateTimeOffset? lastCreatedAt;

        [JsonInclude, JsonPropertyName(nameof(MethodVersion))]
        private readonly int methodVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingHeatDemandCountyResult"/> class.
        /// </summary>
        /// <param name="countyId">The id of the county part.</param>
        /// <param name="methodVersion">The method version of the results.</param>
        /// <param name="buildingCount">The number of buildings with a stored result.</param>
        /// <param name="heatingEnergyNeed">The total annual energy need for space heating of those buildings [kWh/a].</param>
        /// <param name="heatingEnergyNeedPerArea">The mean annual energy need for space heating per reference floor area of those buildings [kWh/(m2a)].</param>
        /// <param name="firstCreatedAt">When the first of the results was written.</param>
        /// <param name="lastCreatedAt">When the last of the results was written.</param>
        public BuildingHeatDemandCountyResult(int countyId, int methodVersion, long buildingCount, double heatingEnergyNeed, double heatingEnergyNeedPerArea, DateTimeOffset? firstCreatedAt, DateTimeOffset? lastCreatedAt)
            : base()
        {
            this.countyId = countyId;
            this.methodVersion = methodVersion;
            this.buildingCount = buildingCount;
            this.heatingEnergyNeed = heatingEnergyNeed;
            this.heatingEnergyNeedPerArea = heatingEnergyNeedPerArea;
            this.firstCreatedAt = firstCreatedAt;
            this.lastCreatedAt = lastCreatedAt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingHeatDemandCountyResult"/> class by copying an existing instance.
        /// </summary>
        /// <param name="buildingHeatDemandCountyResult">The <see cref="BuildingHeatDemandCountyResult"/> to copy. This value can be null.</param>
        public BuildingHeatDemandCountyResult(BuildingHeatDemandCountyResult? buildingHeatDemandCountyResult)
            : base(buildingHeatDemandCountyResult)
        {
            if (buildingHeatDemandCountyResult is not null)
            {
                countyId = buildingHeatDemandCountyResult.countyId;
                methodVersion = buildingHeatDemandCountyResult.methodVersion;
                buildingCount = buildingHeatDemandCountyResult.buildingCount;
                heatingEnergyNeed = buildingHeatDemandCountyResult.heatingEnergyNeed;
                heatingEnergyNeedPerArea = buildingHeatDemandCountyResult.heatingEnergyNeedPerArea;
                firstCreatedAt = buildingHeatDemandCountyResult.firstCreatedAt;
                lastCreatedAt = buildingHeatDemandCountyResult.lastCreatedAt;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingHeatDemandCountyResult"/> class from a JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object containing the data. This value can be null.</param>
        public BuildingHeatDemandCountyResult(JsonObject? jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Gets the number of buildings with a stored result.
        /// </summary>
        [JsonIgnore]
        public long BuildingCount => buildingCount;

        /// <summary>
        /// Gets the id of the county part.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets when the first of the results was written.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? FirstCreatedAt => firstCreatedAt;

        /// <summary>
        /// Gets the total annual energy need for space heating of the buildings [kWh/a].
        /// </summary>
        [JsonIgnore]
        public double HeatingEnergyNeed => heatingEnergyNeed;

        /// <summary>
        /// Gets the mean annual energy need for space heating per reference floor area of the buildings [kWh/(m2a)].
        /// </summary>
        [JsonIgnore]
        public double HeatingEnergyNeedPerArea => heatingEnergyNeedPerArea;

        /// <summary>
        /// Gets when the last of the results was written.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? LastCreatedAt => lastCreatedAt;

        /// <summary>
        /// Gets the method version of the results.
        /// </summary>
        [JsonIgnore]
        public int MethodVersion => methodVersion;
    }
}
