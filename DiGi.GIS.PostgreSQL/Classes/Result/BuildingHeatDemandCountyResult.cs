using DiGi.Analytical.Building.Regulation.Enums;
using DiGi.Core.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Represents what one county part holds in <see cref="Constants.TableName.BuildingHeatDemand"/> for one method version, building type and year-built band (DiGi.GIS.PostgreSQL#143, #146): how many buildings, how many took their irradiation from a stored solar result, the quality counts of their calculation, their energy need for space heating - total, mean and quantiles per area - their design heat load, and when they were written.
    /// <para>The production measure of a heat demand run, read through the API (<c>Coding - PostgreSQL.md</c> §6). The building type and the year built are the ones the calculation used; the band is one of the year-built bounds the summary was asked for, so the rows of a county line up with a typology's construction periods.</para>
    /// </summary>
    public class BuildingHeatDemandCountyResult : SerializableResult, IGISPostgreSQLSerializableObject
    {
        [JsonInclude, JsonPropertyName(nameof(BuildingCount))]
        private readonly long buildingCount;

        [JsonInclude, JsonPropertyName(nameof(BuildingType))]
        private readonly BuildingType buildingType;

        [JsonInclude, JsonPropertyName(nameof(CountyId))]
        private readonly int countyId;

        [JsonInclude, JsonPropertyName(nameof(DesignHeatLoad))]
        private readonly double designHeatLoad = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(ExcludedComponentBuildingCount))]
        private readonly long excludedComponentBuildingCount;

        [JsonInclude, JsonPropertyName(nameof(ExcludedComponentCount))]
        private readonly long excludedComponentCount;

        [JsonInclude, JsonPropertyName(nameof(FirstCreatedAt))]
        private readonly DateTimeOffset? firstCreatedAt;

        [JsonInclude, JsonPropertyName(nameof(GlazingFallbackBuildingCount))]
        private readonly long glazingFallbackBuildingCount;

        [JsonInclude, JsonPropertyName(nameof(GlazingFallbackCount))]
        private readonly long glazingFallbackCount;

        [JsonInclude, JsonPropertyName(nameof(HeatingEnergyNeed))]
        private readonly double heatingEnergyNeed = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(HeatingEnergyNeedPerArea))]
        private readonly double heatingEnergyNeedPerArea = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(HeatingEnergyNeedPerArea_P10))]
        private readonly double heatingEnergyNeedPerArea_P10 = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(HeatingEnergyNeedPerArea_P50))]
        private readonly double heatingEnergyNeedPerArea_P50 = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(HeatingEnergyNeedPerArea_P90))]
        private readonly double heatingEnergyNeedPerArea_P90 = double.NaN;

        [JsonInclude, JsonPropertyName(nameof(LastCreatedAt))]
        private readonly DateTimeOffset? lastCreatedAt;

        [JsonInclude, JsonPropertyName(nameof(MethodVersion))]
        private readonly int methodVersion;

        [JsonInclude, JsonPropertyName(nameof(MissingIrradiationBuildingCount))]
        private readonly long missingIrradiationBuildingCount;

        [JsonInclude, JsonPropertyName(nameof(ShadedCount))]
        private readonly long shadedCount;

        [JsonInclude, JsonPropertyName(nameof(UnshadedCount))]
        private readonly long unshadedCount;

        [JsonInclude, JsonPropertyName(nameof(YearBuiltBandIndex))]
        private readonly int? yearBuiltBandIndex;

        [JsonInclude, JsonPropertyName(nameof(YearBuiltMax))]
        private readonly short? yearBuiltMax;

        [JsonInclude, JsonPropertyName(nameof(YearBuiltMin))]
        private readonly short? yearBuiltMin;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingHeatDemandCountyResult"/> class.
        /// </summary>
        /// <param name="countyId">The id of the county part.</param>
        /// <param name="methodVersion">The method version of the results.</param>
        /// <param name="buildingType">The building type the calculation used.</param>
        /// <param name="yearBuiltBandIndex">The index of the year-built band: 0 below the first bound (or every year when no bound was given), i from the bound i - 1 to the bound i, the bound count from the last bound up; null for the buildings without a year built.</param>
        /// <param name="yearBuiltMin">The first year of the band, inclusive; null when the band is open below or holds the buildings without a year built.</param>
        /// <param name="yearBuiltMax">The last year of the band, inclusive; null when the band is open above or holds the buildings without a year built.</param>
        /// <param name="buildingCount">The number of buildings with a stored result.</param>
        /// <param name="shadedCount">The number of those buildings whose irradiation came from a stored solar result.</param>
        /// <param name="unshadedCount">The number of those buildings calculated with the unshaded irradiation, having no stored solar result.</param>
        /// <param name="heatingEnergyNeed">The total annual energy need for space heating of the buildings [kWh/a]; NaN when none has a value.</param>
        /// <param name="heatingEnergyNeedPerArea">The mean annual energy need for space heating per reference floor area of the buildings [kWh/(m2a)]; NaN when none has a value.</param>
        /// <param name="heatingEnergyNeedPerArea_P10">The 10th percentile of the energy need per reference floor area [kWh/(m2a)]; NaN when none has a value.</param>
        /// <param name="heatingEnergyNeedPerArea_P50">The median of the energy need per reference floor area [kWh/(m2a)]; NaN when none has a value.</param>
        /// <param name="heatingEnergyNeedPerArea_P90">The 90th percentile of the energy need per reference floor area [kWh/(m2a)]; NaN when none has a value.</param>
        /// <param name="designHeatLoad">The total design heat load of the buildings [W]; NaN when none has a value.</param>
        /// <param name="excludedComponentBuildingCount">The number of buildings with at least one component or opening excluded because its thermal transmittance is not known.</param>
        /// <param name="excludedComponentCount">The total number of such excluded components and openings.</param>
        /// <param name="glazingFallbackBuildingCount">The number of buildings with at least one glazed opening whose glazed area was taken from the frame fraction, its construction having no panes.</param>
        /// <param name="glazingFallbackCount">The total number of such glazed openings.</param>
        /// <param name="missingIrradiationBuildingCount">The number of buildings with at least one external surface without monthly irradiation, whose solar gains are 0.</param>
        /// <param name="firstCreatedAt">When the first of the results was written.</param>
        /// <param name="lastCreatedAt">When the last of the results was written.</param>
        public BuildingHeatDemandCountyResult(
            int countyId,
            int methodVersion,
            BuildingType buildingType,
            int? yearBuiltBandIndex,
            short? yearBuiltMin,
            short? yearBuiltMax,
            long buildingCount,
            long shadedCount,
            long unshadedCount,
            double heatingEnergyNeed,
            double heatingEnergyNeedPerArea,
            double heatingEnergyNeedPerArea_P10,
            double heatingEnergyNeedPerArea_P50,
            double heatingEnergyNeedPerArea_P90,
            double designHeatLoad,
            long excludedComponentBuildingCount,
            long excludedComponentCount,
            long glazingFallbackBuildingCount,
            long glazingFallbackCount,
            long missingIrradiationBuildingCount,
            DateTimeOffset? firstCreatedAt,
            DateTimeOffset? lastCreatedAt)
            : base()
        {
            this.countyId = countyId;
            this.methodVersion = methodVersion;
            this.buildingType = buildingType;
            this.yearBuiltBandIndex = yearBuiltBandIndex;
            this.yearBuiltMin = yearBuiltMin;
            this.yearBuiltMax = yearBuiltMax;
            this.buildingCount = buildingCount;
            this.shadedCount = shadedCount;
            this.unshadedCount = unshadedCount;
            this.heatingEnergyNeed = heatingEnergyNeed;
            this.heatingEnergyNeedPerArea = heatingEnergyNeedPerArea;
            this.heatingEnergyNeedPerArea_P10 = heatingEnergyNeedPerArea_P10;
            this.heatingEnergyNeedPerArea_P50 = heatingEnergyNeedPerArea_P50;
            this.heatingEnergyNeedPerArea_P90 = heatingEnergyNeedPerArea_P90;
            this.designHeatLoad = designHeatLoad;
            this.excludedComponentBuildingCount = excludedComponentBuildingCount;
            this.excludedComponentCount = excludedComponentCount;
            this.glazingFallbackBuildingCount = glazingFallbackBuildingCount;
            this.glazingFallbackCount = glazingFallbackCount;
            this.missingIrradiationBuildingCount = missingIrradiationBuildingCount;
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
                buildingType = buildingHeatDemandCountyResult.buildingType;
                yearBuiltBandIndex = buildingHeatDemandCountyResult.yearBuiltBandIndex;
                yearBuiltMin = buildingHeatDemandCountyResult.yearBuiltMin;
                yearBuiltMax = buildingHeatDemandCountyResult.yearBuiltMax;
                buildingCount = buildingHeatDemandCountyResult.buildingCount;
                shadedCount = buildingHeatDemandCountyResult.shadedCount;
                unshadedCount = buildingHeatDemandCountyResult.unshadedCount;
                heatingEnergyNeed = buildingHeatDemandCountyResult.heatingEnergyNeed;
                heatingEnergyNeedPerArea = buildingHeatDemandCountyResult.heatingEnergyNeedPerArea;
                heatingEnergyNeedPerArea_P10 = buildingHeatDemandCountyResult.heatingEnergyNeedPerArea_P10;
                heatingEnergyNeedPerArea_P50 = buildingHeatDemandCountyResult.heatingEnergyNeedPerArea_P50;
                heatingEnergyNeedPerArea_P90 = buildingHeatDemandCountyResult.heatingEnergyNeedPerArea_P90;
                designHeatLoad = buildingHeatDemandCountyResult.designHeatLoad;
                excludedComponentBuildingCount = buildingHeatDemandCountyResult.excludedComponentBuildingCount;
                excludedComponentCount = buildingHeatDemandCountyResult.excludedComponentCount;
                glazingFallbackBuildingCount = buildingHeatDemandCountyResult.glazingFallbackBuildingCount;
                glazingFallbackCount = buildingHeatDemandCountyResult.glazingFallbackCount;
                missingIrradiationBuildingCount = buildingHeatDemandCountyResult.missingIrradiationBuildingCount;
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
        /// Gets the building type the calculation used.
        /// </summary>
        [JsonIgnore]
        public BuildingType BuildingType => buildingType;

        /// <summary>
        /// Gets the id of the county part.
        /// </summary>
        [JsonIgnore]
        public int CountyId => countyId;

        /// <summary>
        /// Gets the total design heat load of the buildings [W]; NaN when none has a value.
        /// </summary>
        [JsonIgnore]
        public double DesignHeatLoad => designHeatLoad;

        /// <summary>
        /// Gets the number of buildings with at least one component or opening excluded because its thermal transmittance is not known.
        /// </summary>
        [JsonIgnore]
        public long ExcludedComponentBuildingCount => excludedComponentBuildingCount;

        /// <summary>
        /// Gets the total number of components and openings excluded because their thermal transmittance is not known.
        /// </summary>
        [JsonIgnore]
        public long ExcludedComponentCount => excludedComponentCount;

        /// <summary>
        /// Gets when the first of the results was written.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? FirstCreatedAt => firstCreatedAt;

        /// <summary>
        /// Gets the number of buildings with at least one glazed opening whose glazed area was taken from the frame fraction, its construction having no panes.
        /// </summary>
        [JsonIgnore]
        public long GlazingFallbackBuildingCount => glazingFallbackBuildingCount;

        /// <summary>
        /// Gets the total number of glazed openings whose glazed area was taken from the frame fraction.
        /// </summary>
        [JsonIgnore]
        public long GlazingFallbackCount => glazingFallbackCount;

        /// <summary>
        /// Gets the total annual energy need for space heating of the buildings [kWh/a]; NaN when none has a value.
        /// </summary>
        [JsonIgnore]
        public double HeatingEnergyNeed => heatingEnergyNeed;

        /// <summary>
        /// Gets the mean annual energy need for space heating per reference floor area of the buildings [kWh/(m2a)]; NaN when none has a value.
        /// </summary>
        [JsonIgnore]
        public double HeatingEnergyNeedPerArea => heatingEnergyNeedPerArea;

        /// <summary>
        /// Gets the 10th percentile of the energy need per reference floor area [kWh/(m2a)]; NaN when none has a value.
        /// </summary>
        [JsonIgnore]
        public double HeatingEnergyNeedPerArea_P10 => heatingEnergyNeedPerArea_P10;

        /// <summary>
        /// Gets the median of the energy need per reference floor area [kWh/(m2a)]; NaN when none has a value.
        /// </summary>
        [JsonIgnore]
        public double HeatingEnergyNeedPerArea_P50 => heatingEnergyNeedPerArea_P50;

        /// <summary>
        /// Gets the 90th percentile of the energy need per reference floor area [kWh/(m2a)]; NaN when none has a value.
        /// </summary>
        [JsonIgnore]
        public double HeatingEnergyNeedPerArea_P90 => heatingEnergyNeedPerArea_P90;

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

        /// <summary>
        /// Gets the number of buildings with at least one external surface without monthly irradiation, whose solar gains are 0.
        /// </summary>
        [JsonIgnore]
        public long MissingIrradiationBuildingCount => missingIrradiationBuildingCount;

        /// <summary>
        /// Gets the number of buildings whose irradiation came from a stored solar result.
        /// </summary>
        [JsonIgnore]
        public long ShadedCount => shadedCount;

        /// <summary>
        /// Gets the number of buildings calculated with the unshaded irradiation, having no stored solar result.
        /// </summary>
        [JsonIgnore]
        public long UnshadedCount => unshadedCount;

        /// <summary>
        /// Gets the index of the year-built band: 0 below the first bound (or every year when no bound was given), i from the bound i - 1 to the bound i, the bound count from the last bound up; null for the buildings without a year built.
        /// </summary>
        [JsonIgnore]
        public int? YearBuiltBandIndex => yearBuiltBandIndex;

        /// <summary>
        /// Gets the last year of the band, inclusive; null when the band is open above or holds the buildings without a year built.
        /// </summary>
        [JsonIgnore]
        public short? YearBuiltMax => yearBuiltMax;

        /// <summary>
        /// Gets the first year of the band, inclusive; null when the band is open below or holds the buildings without a year built.
        /// </summary>
        [JsonIgnore]
        public short? YearBuiltMin => yearBuiltMin;
    }
}
