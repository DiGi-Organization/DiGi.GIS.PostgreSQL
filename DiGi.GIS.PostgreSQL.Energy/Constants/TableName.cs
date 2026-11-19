namespace DiGi.GIS.PostgreSQL.Energy.Constants
{
    /// <summary>
    /// The names of the tables of the monthly heat demand (DiGi.GIS.PostgreSQL#143).
    /// </summary>
    public static class TableName
    {
        /// <summary>
        /// The table of the stored monthly heat demand of every building, one row per county part, building model and method version, on the Storage database beside <c>building_solar_radiation</c>. The literal lives once, in <see cref="DiGi.GIS.PostgreSQL.Constants.TableName.BuildingHeatDemand"/>, which the summary of the main assembly reads (DiGi.GIS.PostgreSQL#146).
        /// </summary>
        public const string BuildingHeatDemand = DiGi.GIS.PostgreSQL.Constants.TableName.BuildingHeatDemand;
    }
}
