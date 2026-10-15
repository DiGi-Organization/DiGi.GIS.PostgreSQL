namespace DiGi.GIS.PostgreSQL.Constants
{
    /// <summary>
    /// Provides constant values for database table names used in the PostgreSQL GIS system.
    /// </summary>
    public static class TableName
    {
        /// <summary>
        /// The name of the administrative areal 2D table.
        /// </summary>
        public const string AdministrativeAreal2D = "administrative_areal_2d";

        /// <summary>
        /// The name of the building 2D table.
        /// </summary>
        public const string Building2D = "building_2d";

        /// <summary>
        /// The name of the ortho-datas table.
        /// </summary>
        public const string OrtoDatas = "orto_datas";

        /// <summary>
        /// The name of the building table.
        /// </summary>
        public const string Building = "building";

        /// <summary>
        /// The name of the ortho-datas building 2D reference update table.
        /// </summary>
        public const string OrtoDatas_Building2DReference_Update = "orto_datas_building_2d_reference_update";

        /// <summary>
        /// The name of the year built data table.
        /// </summary>
        public const string YearBuiltData = "year_built_data";

        /// <summary>
        /// The name of the typology model table.
        /// </summary>
        public const string TypologyModel = "typology_model";

        /// <summary>
        /// The name of the occupancy data building 2D table.
        /// </summary>
        public const string OccupancyData_Building2D = "occupancy_data_building_2d";

        /// <summary>
        /// The name of the occupancy data administrative areal 2D table.
        /// </summary>
        public const string OccupancyData_AdministrativeAreal2D = "occupancy_data_administrative_areal_2d";

        /// <summary>
        /// The name of the building data table.
        /// </summary>
        public const string BuildingData = "building_data";

        /// <summary>
        /// The name of the EPW file table.
        /// </summary>
        public const string EPWFile = "epw_file";

        /// <summary>
        /// The name of the building model table.
        /// </summary>
        public const string BuildingModel = "building_model";

        /// <summary>
        /// The name of the building solar radiation table. One row is the stored solar radiation of one building model for one neighbour radius and solver version, partitioned by county part.
        /// </summary>
        public const string BuildingSolarRadiation = "building_solar_radiation";

        /// <summary>
        /// The name of the terrain point table. One row is a single terrain elevation point.
        /// </summary>
        public const string TerrainPoint = "terrain_point";

        /// <summary>
        /// The name of the territorial unit table.
        /// </summary>
        public const string Unit = "unit";

        /// <summary>
        /// The name of the statistical data collection table.
        /// </summary>
        public const string StatisticalDataCollection = "statistical_data_collection";
    }
}