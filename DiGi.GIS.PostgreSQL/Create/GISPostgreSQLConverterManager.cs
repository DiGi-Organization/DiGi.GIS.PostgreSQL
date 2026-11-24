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
        /// Creates a <see cref="Classes.GISPostgreSQLConverterManager"/> with all PostgreSQL converters registered.
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
        /// <returns>A configured <see cref="Classes.GISPostgreSQLConverterManager"/> if successful; otherwise, null.</returns>
        public static GISPostgreSQLConverterManager? GISPostgreSQLConverterManager()
        {
            return GISPostgreSQLConverterManager(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Constants.FileName.PostgreSQL_Main, Constants.FileName.PostgreSQL_Storage);
        }

        /// <summary>
        /// Creates a <see cref="Classes.GISPostgreSQLConverterManager"/> with all PostgreSQL converters registered, reading the two connection configuration files from the given directory under the given names.
        /// <para>The parameterless overload passes the executing assembly's directory and the production file names; a test passes its own <c>*_Test.conf</c> names, so the registration - which converter sits on Main and which on Storage - can be asserted without touching a host's database.</para>
        /// </summary>
        /// <param name="directory">The directory holding the configuration files.</param>
        /// <param name="fileName_Main">The name of the Main database configuration file.</param>
        /// <param name="fileName_Storage">The name of the Storage database configuration file.</param>
        /// <returns>A configured <see cref="Classes.GISPostgreSQLConverterManager"/> if successful; otherwise, null.</returns>
        public static GISPostgreSQLConverterManager? GISPostgreSQLConverterManager(string? directory, string? fileName_Main, string? fileName_Storage)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory) || string.IsNullOrWhiteSpace(fileName_Main) || string.IsNullOrWhiteSpace(fileName_Storage))
            {
                return null;
            }

            GISPostgreSQLConverterManager result = new();

            string path;

            path = Path.Combine(directory, fileName_Main);
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
                    result.Add(new PopulationGridPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new OrtoAreaPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new OrtoAreaObjectPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new OrtoAreaLabelPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                    result.Add(new BuildingShapePostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Main);
                }
            }

            path = Path.Combine(directory, fileName_Storage);
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
                    result.Add(new OrtoSheetPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);
                    result.Add(new OrtoAreaImagePostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);

                    // The largest data of the repository (six open building datasets) lives on Storage, where the volume is (DiGi.GIS.PostgreSQL#168).
                    result.Add(new OpenBuildingPostgreSQLConverter(connectionData), postgreSQLConfigurationFile_Storage);
                }
            }

            return result;
        }
    }
}