namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Builds the statement of <see cref="Classes.OrtoAreaLabelPostgreSQLConverter.GetOrtoAreaLabelsAsync(Npgsql.NpgsqlConnection?, string, int, System.Threading.CancellationToken)"/>: the label rows filed for one test area.
        /// <para>The rows are read from their columns - the label table has no <c>object</c> column - in the order the upsert writes them, ordered by epoch and then interpreter so a re-read of an unchanged area answers in the same order every time. Every identity column is selected, so the caller can tell a footprint label (<c>county_id</c> with <c>reference</c>) from a drawn-object label (<c>object_id</c>) and one interpreter's row from another's. An area that holds no labels answers no rows.</para>
        /// <para>Parameters: <c>@area_key</c>, text.</para>
        /// </summary>
        /// <returns>The statement.</returns>
        public static string OrtoAreaLabelsByAreaKeyCommandText()
        {
            return $@"
                SELECT area_key, county_id, reference, object_id, epoch_year, state, observation_date_min, observation_date_max, interpreter, note, updated_at
                FROM {Constants.TableName.OrtoAreaLabel}
                WHERE area_key = @area_key
                ORDER BY epoch_year, interpreter;";
        }
    }
}
