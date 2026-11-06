namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Maps a dataset key of the open building datasets import to the table its footprints are stored in (DiGi.GIS.PostgreSQL#125).
        /// <para>The vocabulary is the one the issue and the readers share: <c>eubucco</c>, <c>overture</c>, <c>osm</c>, <c>dbsm</c>, <c>microsoft</c> for the five partitioned footprint tables, and <c>ghs_obat</c> for the centroid table. The comparison ignores case and surrounding whitespace, so an option written once by a human and once by a tool cannot fetch two different tables.</para>
        /// </summary>
        /// <param name="datasetKey">The dataset key to resolve.</param>
        /// <returns>The table name the dataset's rows live in, or null when the key names no known dataset.</returns>
        public static string? OpenBuildingTableName(string? datasetKey)
        {
            if (string.IsNullOrWhiteSpace(datasetKey))
            {
                return null;
            }

            switch (datasetKey!.Trim().ToUpperInvariant())
            {
                case "EUBUCCO":
                    return Constants.TableName.EUBUCCO_Building2D;

                case "OVERTURE":
                    return Constants.TableName.Overture_Building2D;

                case "OSM":
                    return Constants.TableName.OSM_Building2D;

                case "DBSM":
                    return Constants.TableName.DBSM_Building2D;

                case "MICROSOFT":
                    return Constants.TableName.Microsoft_Building2D;

                case "GHS_OBAT":
                    return Constants.TableName.GHS_OBAT_Building;

                default:
                    return null;
            }
        }
    }
}
