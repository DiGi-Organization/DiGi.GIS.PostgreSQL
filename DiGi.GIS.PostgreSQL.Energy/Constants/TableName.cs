namespace DiGi.GIS.PostgreSQL.Energy.Constants
{
    /// <summary>
    /// The names of the tables of the monthly heat demand (DiGi.GIS.PostgreSQL#143).
    /// </summary>
    public static class TableName
    {
        /// <summary>
        /// The table of the stored monthly heat demand of every building, one row per county part, building model and method version, on the Storage database beside <c>building_solar_radiation</c>.
        /// </summary>
        public const string BuildingHeatDemand = "building_heat_demand";
    }
}
