using DiGi.Analytical.Building.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.PostgreSQL.Classes;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Creates a <see cref="GISPostgreSQLConverterManager"/> with all PostgreSQL converters registered.
        /// Reads connection configuration from <c>PostgreSQL_Main</c> and <c>PostgreSQL_Storage</c> files
        /// in the executing assembly's directory.
        /// <para>
        /// IMPORTANT: Every converter consumed by a GIS WebAPI controller (e.g. <c>BuildingController</c>,
        /// <c>AdministrativeAreal2DController</c>) MUST be registered here. The WebAPI <c>InitializeAsync</c>
        /// reads converters from the returned manager and adds them to the DI container. A missing
        /// registration causes the controller's converter dependency to be <see langword="null"/>,
        /// resulting in a 500 Internal Server Error at runtime.
        /// </para>
        /// </summary>
        /// <returns>A configured <see cref="GISPostgreSQLConverterManager"/> if successful; otherwise, null.</returns>
        public static GISPostgreSQLConverterManager? GISPostgreSQLConverterManager()
        {
            string? directory_ExecutingAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrWhiteSpace(directory_ExecutingAssembly) || !Directory.Exists(directory_ExecutingAssembly))
            {
                return null;
            }

            GISPostgreSQLConverterManager result = new();

            string path;

            path = Path.Combine(directory_ExecutingAssembly, Constants.FileName.PostgreSQL_Main);
            if (!string.IsNullOrWhiteSpace(path) && Path.Exists(path) && DiGi.PostgreSQL.Create.PostgreSQLConfigurationFile(path) is PostgreSQLConfigurationFile postgreSQLConfigurationFile_Main)
            {
                ConnectionData? connectionData = DiGi.PostgreSQL.Create.ConnectionData(postgreSQLConfigurationFile_Main);
                if (connectionData is not null)
                {
                    result.Add(new AdministrativeAreal2DPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new Building2DPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);

                    result.Add(new YearBuiltDataPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new Building2DOccupancyDataPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new AdministrativeAreal2DOccupancyDataPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new EPWFilePostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new UnitPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new StatisticalDataCollectionPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new AddressPointPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                }
            }

            path = Path.Combine(directory_ExecutingAssembly, Constants.FileName.PostgreSQL_Storage);
            if (!string.IsNullOrWhiteSpace(path) && Path.Exists(path) && DiGi.PostgreSQL.Create.PostgreSQLConfigurationFile(path) is PostgreSQLConfigurationFile postgreSQLConfigurationFile_Storage)
            {
                ConnectionData? connectionData = DiGi.PostgreSQL.Create.ConnectionData(postgreSQLConfigurationFile_Storage);
                if (connectionData is not null)
                {
                    result.Add(new OrtoDatasPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);
                    result.Add(new BuildingDataPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);
                    result.Add(new BuildingModelPostgreSQLConverter(connectionData, BuildingModelDetailLevel.Component), postgreSQLConfigurationFile_Storage);
                    result.Add(new BuildingSolarRadiationPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);
                    result.Add(new BuildingPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);
                    result.Add(new TerrainPointPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);
                }
            }

            return result;
        }
    }
}