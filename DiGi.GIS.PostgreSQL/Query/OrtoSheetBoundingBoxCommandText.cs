namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Builds the statement of <see cref="Classes.OrtoSheetPostgreSQLConverter.GetOrtoSheetsByBoundingBoxAsync(Npgsql.NpgsqlConnection?, Geometry.Planar.Classes.BoundingBox2D?, double, int, System.Threading.CancellationToken)"/>: the sheets whose footprint box intersects a box, boundaries included.
        /// <para>An intersection test alone (<c>min_x &lt;= @max_x AND max_x &gt;= @min_x</c>) gives a btree nothing to prune: <c>min_x &lt;= @max_x</c> holds for everything west of the box. A sheet's footprint is never wider or taller than <c>@extent</c>, so its minimum corner lies within <c>@extent</c> of the box, and both axes become a bounded range (<c>min_x BETWEEN @min_x - @extent AND @max_x</c>) served by the single-column indexes on <c>min_x</c> and <c>min_y</c>, which the planner can combine. The largest footprint of the national index of 2026-11-03 is 4.9 km (DiGi.GIS#17).</para>
        /// <para>Parameters: <c>@min_x</c>, <c>@min_y</c>, <c>@max_x</c>, <c>@max_y</c> and <c>@extent</c>, all double precision.</para>
        /// </summary>
        /// <returns>The statement.</returns>
        public static string OrtoSheetBoundingBoxCommandText()
        {
            return $@"
                SELECT object
                FROM {Constants.TableName.OrtoSheet}
                WHERE min_x BETWEEN @min_x - @extent AND @max_x AND max_x >= @min_x
                  AND min_y BETWEEN @min_y - @extent AND @max_y AND max_y >= @min_y;";
        }
    }
}
