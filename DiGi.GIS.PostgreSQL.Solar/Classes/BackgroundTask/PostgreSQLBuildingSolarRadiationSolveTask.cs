using DiGi.EPW.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Solar.Classes
{
    /// <summary>
    /// Represents a background task that solves the annual solar radiation of every building of a county part for one or more neighbour radii and stores it in the building solar radiation table, reading and writing the storage database directly through its converters (DiGi.GIS.PostgreSQL#136).
    /// <para>The run itself - scope, tiling, batching, parallel solve, counting and logging - is <see cref="BuildingSolarRadiationSolveTask"/>'s; this class only supplies its reads and its write. Batch rows carry no user email: no user asked for them.</para>
    /// </summary>
    public class PostgreSQLBuildingSolarRadiationSolveTask : BuildingSolarRadiationSolveTask, IGISPostgreSQLObject
    {
        /// <summary>
        /// The GIS PostgreSQL converter manager used to retrieve converters and execute operations.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        private AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter;
        private Building2DPostgreSQLConverter? building2DPostgreSQLConverter;
        private BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter;
        private BuildingSolarRadiationPostgreSQLConverter? buildingSolarRadiationPostgreSQLConverter;
        private EPWFilePostgreSQLConverter? ePWFilePostgreSQLConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingSolarRadiationSolveTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager used to retrieve converters and execute operations.</param>
        public PostgreSQLBuildingSolarRadiationSolveTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <inheritdoc />
        protected override async Task<List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadBuildingModelsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingModelPostgreSQLConverter is null)
            {
                return null;
            }

            List<BuildingModel>? buildingModels_Stored = await buildingModelPostgreSQLConverter.GetItemsByReferencesAsync(references, countyId, null, true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (buildingModels_Stored is null)
            {
                return null;
            }

            List<DiGi.Analytical.Building.Classes.BuildingModel?> result = [];
            foreach (BuildingModel? buildingModel_Stored in buildingModels_Stored)
            {
                result.Add(buildingModel_Stored?.ToDiGi());
            }

            return result;
        }

        /// <inheritdoc />
        protected override async Task<List<Building2DCentroid>?> ReadBuilding2DCentroidsAsync(int countyId, int commandTimeout, CancellationToken cancellationToken)
        {
            if (building2DPostgreSQLConverter is null)
            {
                return null;
            }

            return await building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<List<AdministrativeAreal2DReference>?> ReadCountyReferencesAsync(int commandTimeout, CancellationToken cancellationToken)
        {
            if (administrativeAreal2DPostgreSQLConverter is null)
            {
                return null;
            }

            return await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<EPWFile?> ReadEPWFileAsync(int id, int commandTimeout, CancellationToken cancellationToken)
        {
            if (ePWFilePostgreSQLConverter is null)
            {
                return null;
            }

            return await ePWFilePostgreSQLConverter.GetEPWFileByIdAsync(id, commandTimeout, cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<List<(int Id, double X, double Y)>?> ReadEPWFileLocationsAsync(int commandTimeout, CancellationToken cancellationToken)
        {
            if (ePWFilePostgreSQLConverter is null)
            {
                return null;
            }

            return await ePWFilePostgreSQLConverter.GetEPWFileLocationsAsync(commandTimeout, cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<List<DiGi.Analytical.Building.Classes.BuildingModel?>?> ReadNeighbourBuildingModelsAsync(Circle2D circle2D, int commandTimeout, CancellationToken cancellationToken)
        {
            if (building2DPostgreSQLConverter is null || buildingModelPostgreSQLConverter is null)
            {
                return null;
            }

            List<(int CountyId, DiGi.Analytical.Building.Classes.BuildingModel BuildingModel)>? buildingModels = await building2DPostgreSQLConverter.BuildingModelsByCircle2DAsync(buildingModelPostgreSQLConverter, circle2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (buildingModels is null)
            {
                return null;
            }

            List<DiGi.Analytical.Building.Classes.BuildingModel?> result = [];
            foreach ((int _, DiGi.Analytical.Building.Classes.BuildingModel buildingModel) in buildingModels)
            {
                result.Add(buildingModel);
            }

            return result;
        }

        /// <inheritdoc />
        protected override async Task<HashSet<string>?> ReadSolvedReferencesAsync(int countyId, double radius, int solverVersion, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingSolarRadiationPostgreSQLConverter is null)
            {
                return null;
            }

            return await buildingSolarRadiationPostgreSQLConverter.GetReferencesAsync(countyId, commandTimeout: commandTimeout, radius: radius, solverVersion: solverVersion, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        protected override bool TryInitialize()
        {
            building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            buildingSolarRadiationPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingSolarRadiationPostgreSQLConverter>();
            ePWFilePostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<EPWFilePostgreSQLConverter>();
            administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            if (building2DPostgreSQLConverter is null || buildingModelPostgreSQLConverter is null || buildingSolarRadiationPostgreSQLConverter is null || ePWFilePostgreSQLConverter is null || administrativeAreal2DPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: a converter is missing - building 2D {Building2D}, building model {BuildingModel}, solar radiation {SolarRadiation}, EPW {EPW}, counties {Counties}", GetType().Name, building2DPostgreSQLConverter is not null, buildingModelPostgreSQLConverter is not null, buildingSolarRadiationPostgreSQLConverter is not null, ePWFilePostgreSQLConverter is not null, administrativeAreal2DPostgreSQLConverter is not null);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override async Task<int?> WriteAsync(int countyId, List<GIS.Classes.BuildingSolarRadiationResult> buildingSolarRadiationResults, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingSolarRadiationPostgreSQLConverter is null)
            {
                return null;
            }

            return await buildingSolarRadiationPostgreSQLConverter.UpdateAsync(countyId, buildingSolarRadiationResults, commandTimeout, cancellationToken);
        }
    }
}
