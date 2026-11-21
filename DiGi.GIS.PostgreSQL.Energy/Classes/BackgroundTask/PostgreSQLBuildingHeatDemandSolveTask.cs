using DiGi.Core.IO.Table.Classes;
using DiGi.EPW.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Analytical.Energy.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Building2D = DiGi.GIS.Classes.Building2D;
using BuildingModel = DiGi.Analytical.Building.Classes.BuildingModel;

namespace DiGi.GIS.PostgreSQL.Energy.Classes
{
    /// <summary>
    /// Represents a background task that calculates the monthly heat demand of every building of the county parts in scope and stores it, reading and writing the databases directly through their converters (DiGi.GIS.PostgreSQL#143).
    /// <para>The run itself - scope, tiling, batching, parallel calculation, counting and logging - is <see cref="BuildingHeatDemandSolveTask"/>'s; this class only supplies its reads and its write. The write stores each batch in <c>building_heat_demand</c> (<see cref="BuildingHeatDemandPostgreSQLConverter"/>) and then its four summary columns in <c>building_data</c> (<see cref="Modify.Update_HeatDemand(Table?, IEnumerable{BuildingHeatDemandResult?}?)"/>); both live on the Storage database. The heat-demand converter is registered in the manager here when the host has not (<see cref="Modify.AddBuildingHeatDemandPostgreSQLConverter(GISPostgreSQLConverterManager?)"/>).</para>
    /// </summary>
    public class PostgreSQLBuildingHeatDemandSolveTask : BuildingHeatDemandSolveTask, IGISPostgreSQLObject
    {
        /// <summary>
        /// The GIS PostgreSQL converter manager used to retrieve converters and execute operations.
        /// </summary>
        protected readonly GISPostgreSQLConverterManager gISPostgreSQLConverterManager;

        private AddressPointPostgreSQLConverter? addressPointPostgreSQLConverter;
        private AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter;
        private Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter;
        private Building2DPostgreSQLConverter? building2DPostgreSQLConverter;
        private BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter;
        private BuildingHeatDemandPostgreSQLConverter? buildingHeatDemandPostgreSQLConverter;
        private BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter;
        private BuildingSolarRadiationPostgreSQLConverter? buildingSolarRadiationPostgreSQLConverter;
        private EPWFilePostgreSQLConverter? ePWFilePostgreSQLConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLBuildingHeatDemandSolveTask"/> class.
        /// </summary>
        /// <param name="gISPostgreSQLConverterManager">The GIS PostgreSQL converter manager used to retrieve converters and execute operations.</param>
        public PostgreSQLBuildingHeatDemandSolveTask(GISPostgreSQLConverterManager gISPostgreSQLConverterManager)
        {
            this.gISPostgreSQLConverterManager = gISPostgreSQLConverterManager ?? throw new ArgumentNullException(nameof(gISPostgreSQLConverterManager));
        }

        /// <inheritdoc />
        protected override async Task<Dictionary<string, Point2D>?> ReadAddressPointsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken)
        {
            if (addressPointPostgreSQLConverter is null)
            {
                return null;
            }

            List<AddressPointMatch>? addressPointMatches = await addressPointPostgreSQLConverter.GetAddressPointMatchesByReferencesAsync(references, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (addressPointMatches is null)
            {
                return null;
            }

            // A reference is unique only per county part, so a match filed under another part is not this building's.
            Dictionary<string, Point2D> result = [];
            foreach (AddressPointMatch? addressPointMatch in addressPointMatches)
            {
                if (addressPointMatch?.Reference is string reference && (addressPointMatch.CountyId is null || addressPointMatch.CountyId == countyId) && addressPointMatch.AddressPoint?.Location is Point2D location && !result.ContainsKey(reference))
                {
                    result[reference] = location;
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override async Task<List<Building2D?>?> ReadBuilding2DsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken)
        {
            if (building2DPostgreSQLConverter is null)
            {
                return null;
            }

            List<Building2DReference> building2DReferences = references.ConvertAll(x => new Building2DReference() { CountyId = countyId, Reference = x });
            List<PostgreSQL.Classes.Building2D>? building2Ds_Stored = await building2DPostgreSQLConverter.GetBuilding2DsByBuilding2DReferencesAsync(building2DReferences, fallbackByReference: true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (building2Ds_Stored is null)
            {
                return null;
            }

            List<Building2D?> result = [];
            foreach (PostgreSQL.Classes.Building2D? building2D_Stored in building2Ds_Stored)
            {
                result.Add(building2D_Stored?.ToDiGi());
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
        protected override async Task<List<BuildingModel?>?> ReadBuildingModelsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingModelPostgreSQLConverter is null)
            {
                return null;
            }

            List<PostgreSQL.Classes.BuildingModel>? buildingModels_Stored = await buildingModelPostgreSQLConverter.GetItemsByReferencesAsync(references, countyId, null, true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (buildingModels_Stored is null)
            {
                return null;
            }

            List<BuildingModel?> result = [];
            foreach (PostgreSQL.Classes.BuildingModel? buildingModel_Stored in buildingModels_Stored)
            {
                result.Add(buildingModel_Stored?.ToDiGi());
            }

            return result;
        }

        /// <inheritdoc />
        protected override async Task<List<BuildingSolarRadiationResult>?> ReadBuildingSolarRadiationResultsAsync(int countyId, List<string> references, double? radius, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingSolarRadiationPostgreSQLConverter is null)
            {
                return null;
            }

            return await buildingSolarRadiationPostgreSQLConverter.GetItemsByReferencesAsync(references, countyId, commandTimeout: commandTimeout, radius: radius, includeShadingFactors: true, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<HashSet<string>?> ReadCompletedReferencesAsync(int countyId, DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod heatDemandMethod, int methodVersion, Analytical.Energy.Enums.HeatDemandInputVariant heatDemandInputVariant, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingHeatDemandPostgreSQLConverter is null)
            {
                return null;
            }

            return await buildingHeatDemandPostgreSQLConverter.GetReferencesAsync(countyId, methodVersion, heatDemandMethod, heatDemandInputVariant, commandTimeout, cancellationToken);
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
        protected override async Task<List<BuildingModel?>?> ReadNeighbourBuildingModelsAsync(Circle2D circle2D, int commandTimeout, CancellationToken cancellationToken)
        {
            if (building2DPostgreSQLConverter is null || buildingModelPostgreSQLConverter is null)
            {
                return null;
            }

            List<(int CountyId, BuildingModel BuildingModel)>? buildingModels = await building2DPostgreSQLConverter.BuildingModelsByCircle2DAsync(buildingModelPostgreSQLConverter, circle2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (buildingModels is null)
            {
                return null;
            }

            List<BuildingModel?> result = [];
            foreach ((int _, BuildingModel buildingModel) in buildingModels)
            {
                result.Add(buildingModel);
            }

            return result;
        }

        /// <inheritdoc />
        protected override async Task<Dictionary<string, OccupancyData>?> ReadOccupancyDatasAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken)
        {
            if (building2DOccupancyDataPostgreSQLConverter is null)
            {
                return null;
            }

            List<Building2DOccupancyData>? building2DOccupancyDatas = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferencesAsync(references, countyId, fallbackByReference: true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (building2DOccupancyDatas is null)
            {
                return null;
            }

            Dictionary<string, OccupancyData> result = [];
            foreach (Building2DOccupancyData? building2DOccupancyData in building2DOccupancyDatas)
            {
                if (building2DOccupancyData?.Reference is string reference && building2DOccupancyData.ToDiGi() is OccupancyData occupancyData && !result.ContainsKey(reference))
                {
                    result[reference] = occupancyData;
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override async Task<Dictionary<string, (short? YearBuilt, YearBuiltSource? YearBuiltSource, short? CensusYearBuilt)>?> ReadYearBuiltsAsync(int countyId, List<string> references, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingDataPostgreSQLConverter is null)
            {
                return null;
            }

            string? uniqueId_CountyId = Core.IO.Query.UniqueId(IO.Constants.Column.CountyId);
            string? uniqueId_Reference = Core.IO.Query.UniqueId(IO.Constants.Column.Reference);
            string? uniqueId_UserYearBuilt = Core.IO.Query.UniqueId(IO.Constants.Column.UserYearBuilt);
            string? uniqueId_PredictedYearBuilt = Core.IO.Query.UniqueId(IO.Constants.Column.PredictedYearBuilt);
            string? uniqueId_CensusYearBuilt = Core.IO.Query.UniqueId(IO.Constants.Column.CensusYearBuilt);
            if (uniqueId_CountyId is null || uniqueId_Reference is null || uniqueId_UserYearBuilt is null || uniqueId_PredictedYearBuilt is null || uniqueId_CensusYearBuilt is null)
            {
                return null;
            }

            Table? table = await buildingDataPostgreSQLConverter.PullAsync(references, countyId, [uniqueId_CountyId, uniqueId_Reference, uniqueId_UserYearBuilt, uniqueId_PredictedYearBuilt, uniqueId_CensusYearBuilt], fallbackByReference: true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (table is null)
            {
                return null;
            }

            Column? column_Reference = null;
            Column? column_UserYearBuilt = null;
            Column? column_PredictedYearBuilt = null;
            Column? column_CensusYearBuilt = null;
            foreach (Column column in table.Columns)
            {
                string? uniqueId = Core.IO.Query.UniqueId(column);
                if (uniqueId == uniqueId_Reference)
                {
                    column_Reference = column;
                }
                else if (uniqueId == uniqueId_UserYearBuilt)
                {
                    column_UserYearBuilt = column;
                }
                else if (uniqueId == uniqueId_PredictedYearBuilt)
                {
                    column_PredictedYearBuilt = column;
                }
                else if (uniqueId == uniqueId_CensusYearBuilt)
                {
                    column_CensusYearBuilt = column;
                }
            }

            Dictionary<string, (short? YearBuilt, YearBuiltSource? YearBuiltSource, short? CensusYearBuilt)> result = [];
            if (column_Reference is null)
            {
                return result;
            }

            for (int i = 0; i < table.RowCount; i++)
            {
                Row? row = table.GetRow(i);
                if (row is null || !row.TryGetValue(column_Reference.Index, out string? reference) || string.IsNullOrWhiteSpace(reference) || result.ContainsKey(reference!))
                {
                    continue;
                }

                // The census year built of the building's area, read for every building: it replaces a prediction that only bounds the year (DiGi-Organization/DiGi.GIS.Analytical#17).
                short? censusYearBuilt = column_CensusYearBuilt is not null && row.TryGetValue(column_CensusYearBuilt.Index, out object? value_Census) && Core.Query.TryConvert(value_Census, out short censusYearBuilt_Value) && censusYearBuilt_Value > 0 ? censusYearBuilt_Value : null;

                // The user's year when there is one, else the predicted one: the rule of building_data.CalculatedYearBuilt.
                if (column_UserYearBuilt is not null && row.TryGetValue(column_UserYearBuilt.Index, out object? value_User) && Core.Query.TryConvert(value_User, out short yearBuilt_User) && yearBuilt_User > 0)
                {
                    result[reference!] = (yearBuilt_User, YearBuiltSource.User, censusYearBuilt);
                }
                else if (column_PredictedYearBuilt is not null && row.TryGetValue(column_PredictedYearBuilt.Index, out object? value_Predicted) && Core.Query.TryConvert(value_Predicted, out short yearBuilt_Predicted) && yearBuilt_Predicted > 0)
                {
                    result[reference!] = (yearBuilt_Predicted, YearBuiltSource.Prediction, censusYearBuilt);
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override bool TryInitialize()
        {
            addressPointPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AddressPointPostgreSQLConverter>();
            administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            building2DOccupancyDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DOccupancyDataPostgreSQLConverter>();
            building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            buildingModelPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingModelPostgreSQLConverter>();
            buildingSolarRadiationPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingSolarRadiationPostgreSQLConverter>();
            ePWFilePostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<EPWFilePostgreSQLConverter>();
            buildingHeatDemandPostgreSQLConverter = gISPostgreSQLConverterManager.AddBuildingHeatDemandPostgreSQLConverter();

            if (addressPointPostgreSQLConverter is null || administrativeAreal2DPostgreSQLConverter is null || building2DOccupancyDataPostgreSQLConverter is null || building2DPostgreSQLConverter is null || buildingDataPostgreSQLConverter is null || buildingModelPostgreSQLConverter is null || buildingSolarRadiationPostgreSQLConverter is null || ePWFilePostgreSQLConverter is null || buildingHeatDemandPostgreSQLConverter is null)
            {
                Serilog.Modify.Log(
                    Serilog.Enums.LogEventLevel.Error,
                    "{Type}: a converter is missing - address points {AddressPoint}, counties {Counties}, occupancy {Occupancy}, building 2D {Building2D}, building data {BuildingData}, building model {BuildingModel}, solar radiation {SolarRadiation}, EPW {EPW}, heat demand {HeatDemand}",
                    GetType().Name,
                    addressPointPostgreSQLConverter is not null,
                    administrativeAreal2DPostgreSQLConverter is not null,
                    building2DOccupancyDataPostgreSQLConverter is not null,
                    building2DPostgreSQLConverter is not null,
                    buildingDataPostgreSQLConverter is not null,
                    buildingModelPostgreSQLConverter is not null,
                    buildingSolarRadiationPostgreSQLConverter is not null,
                    ePWFilePostgreSQLConverter is not null,
                    buildingHeatDemandPostgreSQLConverter is not null);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override async Task<int?> WriteAsync(int countyId, List<BuildingHeatDemandResult> buildingHeatDemandResults, int commandTimeout, CancellationToken cancellationToken)
        {
            if (buildingHeatDemandPostgreSQLConverter is null || buildingDataPostgreSQLConverter is null)
            {
                return null;
            }

            int? written = await buildingHeatDemandPostgreSQLConverter.UpdateAsync(countyId, buildingHeatDemandResults, commandTimeout, cancellationToken);
            if (written is null)
            {
                return null;
            }

            Table table = new();
            if (Modify.Update_HeatDemand(table, buildingHeatDemandResults) > 0 && !await buildingDataPostgreSQLConverter.PushAsync(table, commandTimeout: commandTimeout, cancellationToken: cancellationToken))
            {
                return null;
            }

            return written;
        }
    }
}
