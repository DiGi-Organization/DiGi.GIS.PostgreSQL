namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Builds the statement of <see cref="Classes.OrtoAreaPostgreSQLConverter.GetOrtoAreaAsync(Npgsql.NpgsqlConnection?, string, int, System.Threading.CancellationToken)"/>: one test area by its invariant location key.
        /// <para>The key is the primary key of <see cref="Constants.TableName.OrtoArea"/>, so the statement is a unique-index lookup; <c>LIMIT 1</c> states that at most one row can answer. The <c>object</c> column is read whole, cells included; a key that is not stored answers no row at all.</para>
        /// <para>Parameters: <c>@area_key</c>, text.</para>
        /// </summary>
        /// <returns>The statement.</returns>
        public static string OrtoAreaByKeyCommandText()
        {
            return $@"
                SELECT object
                FROM {Constants.TableName.OrtoArea}
                WHERE area_key = @area_key
                LIMIT 1;";
        }
    }
}
