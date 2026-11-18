using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Energy.Classes;
using DiGi.PostgreSQL.Classes;

namespace DiGi.GIS.PostgreSQL.Energy
{
    public static partial class Modify
    {
        /// <summary>
        /// Registers a <see cref="BuildingHeatDemandPostgreSQLConverter"/> in a converter manager, on the Storage database (DiGi.GIS.PostgreSQL#143).
        /// <para><c>DiGi.GIS.PostgreSQL</c> cannot reference this project, so <c>Create.GISPostgreSQLConverterManager()</c> does not register the converter. It goes on the connection and configuration file of the registered <see cref="BuildingSolarRadiationPostgreSQLConverter"/>: <c>building_heat_demand</c> lives beside <c>building_solar_radiation</c> and <c>building_data</c> on the Storage database. A manager that already holds one keeps it.</para>
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The converter manager. This value can be null.</param>
        /// <returns>The registered converter, or null when the manager is null or has no Storage connection to take.</returns>
        public static BuildingHeatDemandPostgreSQLConverter? AddBuildingHeatDemandPostgreSQLConverter(this GISPostgreSQLConverterManager? gISPostgreSQLConverterManager)
        {
            if (gISPostgreSQLConverterManager is null)
            {
                return null;
            }

            if (gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingHeatDemandPostgreSQLConverter>() is BuildingHeatDemandPostgreSQLConverter buildingHeatDemandPostgreSQLConverter_Existing)
            {
                return buildingHeatDemandPostgreSQLConverter_Existing;
            }

            ConnectionData? connectionData = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingSolarRadiationPostgreSQLConverter>()?.ConnectionData;
            if (connectionData is null)
            {
                return null;
            }

            BuildingHeatDemandPostgreSQLConverter result = new(connectionData);
            return gISPostgreSQLConverterManager.Add(result, gISPostgreSQLConverterManager.GetPostgreSQLConfigurationFile<BuildingSolarRadiationPostgreSQLConverter>()) ? result : null;
        }
    }
}
