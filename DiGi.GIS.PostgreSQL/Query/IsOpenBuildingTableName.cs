using System.Diagnostics.CodeAnalysis;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Reports whether a name is one of the six tables of the open building datasets import (DiGi.GIS.PostgreSQL#125).
        /// <para>The list is the guard: a table name is an identifier and cannot be a parameter, so every statement that interpolates one resolves it here first and rejects anything that does not survive (Coding - PostgreSQL.md section 5). The register (<c>building_dataset</c>) is deliberately not on the list - it is read and written only through statements that name it directly.</para>
        /// </summary>
        /// <param name="tableName">The name to check.</param>
        /// <returns>True when the name is one of the five footprint tables or the GHS-OBAT centroid table; otherwise, false.</returns>
        public static bool IsOpenBuildingTableName([NotNullWhen(true)] string? tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return false;
            }

            return string.Equals(tableName, Constants.TableName.EUBUCCO_Building2D, System.StringComparison.Ordinal)
                || string.Equals(tableName, Constants.TableName.Overture_Building2D, System.StringComparison.Ordinal)
                || string.Equals(tableName, Constants.TableName.OSM_Building2D, System.StringComparison.Ordinal)
                || string.Equals(tableName, Constants.TableName.DBSM_Building2D, System.StringComparison.Ordinal)
                || string.Equals(tableName, Constants.TableName.Microsoft_Building2D, System.StringComparison.Ordinal)
                || string.Equals(tableName, Constants.TableName.GHS_OBAT_Building, System.StringComparison.Ordinal);
        }
    }
}
