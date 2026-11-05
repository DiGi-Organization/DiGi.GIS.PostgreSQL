namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Builds the statement of <see cref="Classes.OrtoAreaImagePostgreSQLConverter.GetOrtoAreaImageAsync(Npgsql.NpgsqlConnection?, string, short, int, System.Threading.CancellationToken)"/>: the materialised tile of one test area for one epoch.
        /// <para>The pair <c>(area_key, epoch_year)</c> is the unique key of <see cref="Constants.TableName.OrtoAreaImage"/>, so the statement is a unique-index lookup answering at most one row, and <c>LIMIT 1</c> states that. The row is read from its columns - the image table has no <c>object</c> column - and a pair that was never fetched answers no row at all: a missing row and a negative row (status 1 or 2) are different answers, and only the second is cached.</para>
        /// <para>Parameters: <c>@area_key</c>, text; <c>@epoch_year</c>, smallint.</para>
        /// </summary>
        /// <returns>The statement.</returns>
        public static string OrtoAreaImageByAreaKeyAndEpochYearCommandText()
        {
            return $@"
                SELECT area_key, min_x, min_y, max_x, max_y, epoch_year, width, height, status, source_url, fetched_at, bytes
                FROM {Constants.TableName.OrtoAreaImage}
                WHERE area_key = @area_key AND epoch_year = @epoch_year
                LIMIT 1;";
        }
    }
}
