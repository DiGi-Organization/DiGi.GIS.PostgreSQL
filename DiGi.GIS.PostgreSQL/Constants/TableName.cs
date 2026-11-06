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
        /// The name of the address point table. One row is one PRG address point, keyed by its PRG local identifier and filed under the county part it lies in, together with the <c>building_2d</c> reference it was matched to.
        /// </summary>
        public const string AddressPoint = "address_point";

        /// <summary>
        /// The name of the open building dataset release register: one row per release of one open building dataset, carrying its version, download date, licence and SHA-256 (DiGi.GIS.PostgreSQL#125). On the Main database, beside <see cref="Building2D"/>.
        /// </summary>
        public const string BuildingDataset = "building_dataset";

        /// <summary>
        /// The name of the EUBUCCO footprint table: one row per footprint of one EUBUCCO release, partitioned by county part (DiGi.GIS.PostgreSQL#125). On the Main database, beside <see cref="Building2D"/>.
        /// </summary>
        public const string EUBUCCO_Building2D = "eubucco_building_2d";

        /// <summary>
        /// The name of the Overture footprint table: one row per footprint of one Overture release, partitioned by county part (DiGi.GIS.PostgreSQL#125). On the Main database, beside <see cref="Building2D"/>.
        /// </summary>
        public const string Overture_Building2D = "overture_building_2d";

        /// <summary>
        /// The name of the OpenStreetMap footprint table: one row per footprint of one Geofabrik extract release, partitioned by county part (DiGi.GIS.PostgreSQL#125). On the Main database, beside <see cref="Building2D"/>.
        /// </summary>
        public const string OSM_Building2D = "osm_building_2d";

        /// <summary>
        /// The name of the DBSM footprint table: one row per footprint of one DBSM release, partitioned by county part (DiGi.GIS.PostgreSQL#125). On the Main database, beside <see cref="Building2D"/>.
        /// </summary>
        public const string DBSM_Building2D = "dbsm_building_2d";

        /// <summary>
        /// The name of the Microsoft footprint table: one row per footprint of one Microsoft release, partitioned by county part (DiGi.GIS.PostgreSQL#125). On the Main database, beside <see cref="Building2D"/>.
        /// </summary>
        public const string Microsoft_Building2D = "microsoft_building_2d";

        /// <summary>
        /// The name of the GHS-OBAT centroid table: one row per centroid of one GHS-OBAT release, keyed by the Overture identifier it links through and not partitioned (DiGi.GIS.PostgreSQL#125). On the Main database, beside <see cref="Building2D"/>.
        /// </summary>
        public const string GHS_OBAT_Building = "ghs_obat_building";

        /// <summary>
        /// The name of the building 2D table.
        /// </summary>
        public const string Building2D = "building_2d";

        /// <summary>
        /// The name of the ortho-datas table.
        /// </summary>
        public const string OrtoDatas = "orto_datas";

        /// <summary>
        /// The name of the orthophoto sheet index table: one row per sheet image of the GUGiK index, on the Storage database beside <see cref="OrtoDatas"/>.
        /// </summary>
        public const string OrtoSheet = "orto_sheet";

        /// <summary>
        /// The name of the orthophoto acquisition table: one row per stored image of <see cref="OrtoDatas"/>, keyed by county part, building reference and requested year, holding the date, sheet and content hash read from <see cref="OrtoSheet"/> (DiGi.GIS.PostgreSQL#123). On the Storage database, partitioned by county part like <see cref="OrtoDatas"/>.
        /// </summary>
        public const string OrtoDataAcquisition = "orto_data_acquisition";

        /// <summary>
        /// The name of the full-interpretation test area table: one row per test area, carrying its bounding box envelope, sampling grid parameters and the sampled cells inside <c>object</c> (DiGi.GIS.PostgreSQL#129). On the Main database.
        /// </summary>
        public const string OrtoArea = "orto_area";

        /// <summary>
        /// The name of the test area object table: one row per polygon drawn in a test area, keyed by its server-assigned identifier and filed under <c>area_key</c> (DiGi.GIS.PostgreSQL#129). On the Main database.
        /// </summary>
        public const string OrtoAreaObject = "orto_area_object";

        /// <summary>
        /// The name of the test area label table: one row per (area, object identity, epoch, interpreter), where the object identity is exactly one of the footprint pair or the drawn-object identifier, unique under a <c>NULLS NOT DISTINCT</c> index (DiGi.GIS.PostgreSQL#129). On the Main database.
        /// </summary>
        public const string OrtoAreaLabel = "orto_area_label";

        /// <summary>
        /// The name of the test area image table: one materialised area tile per test area and imagery epoch, negative rows of missing or failed imagery included (DiGi.GIS.PostgreSQL#129). On the Storage database beside <see cref="OrtoDatas"/>.
        /// </summary>
        public const string OrtoAreaImage = "orto_area_image";

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