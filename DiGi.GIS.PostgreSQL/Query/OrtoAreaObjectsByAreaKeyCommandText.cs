namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Builds the statement of <see cref="Classes.OrtoAreaObjectPostgreSQLConverter.GetOrtoAreaObjectsAsync(Npgsql.NpgsqlConnection?, string, int, System.Threading.CancellationToken)"/>: the polygons drawn in one test area.
        /// <para>Filtered by <c>area_key</c>, the leading column of <c>idx_orto_area_object_area_key</c>, and ordered by <c>object_id</c> so a read of an unchanged area answers in the same order every time. The <c>object</c> column is read whole, polygon and metadata included; an area that holds no polygons answers no rows.</para>
        /// <para>Parameters: <c>@area_key</c>, text.</para>
        /// </summary>
        /// <returns>The statement.</returns>
        public static string OrtoAreaObjectsByAreaKeyCommandText()
        {
            return $@"
                SELECT object
                FROM {Constants.TableName.OrtoAreaObject}
                WHERE area_key = @area_key
                ORDER BY object_id;";
        }
    }
}
