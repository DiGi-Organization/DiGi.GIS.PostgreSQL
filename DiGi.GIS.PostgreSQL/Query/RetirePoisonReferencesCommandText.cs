namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Builds the poison-row retirement statement of an update queue: deletes every entry that reached the attempt ceiling and returns what was discarded, so the caller can log each retired entry at Error.
        /// <para>The predicate is a plain <c>attempts &gt;= @maxAttempts</c> rather than <c>COALESCE(attempts, 0) &gt;= @maxAttempts</c>. The column is <c>INT NOT NULL DEFAULT 0</c> and <c>ADD COLUMN IF NOT EXISTS attempts INT NOT NULL DEFAULT 0</c> backfills the rows that predate it, so it can never be null - and wrapping it in <c>COALESCE</c> is what made the predicate unsargable: the statement walked every row of the queue at the start of every claim, until the walk outlasted the command timeout and the claim returned nothing at all (DiGi.GIS.PostgreSQL#145). The table carries a plain btree on <c>attempts</c> for it, because a partial index cannot follow a parameterized ceiling (see <see cref="Create.TableAsync_Building2DReference"/>).</para>
        /// <para>Parameters: <c>@maxAttempts</c>, integer.</para>
        /// </summary>
        /// <param name="tableName">The queue table to retire from. An identifier, interpolated at the call site from the whitelisted constants - a parameter cannot name a table.</param>
        /// <returns>The statement.</returns>
        public static string RetirePoisonReferencesCommandText(string tableName)
        {
            return $@"
                DELETE FROM {tableName}
                WHERE attempts >= @maxAttempts
                RETURNING id, county_id, reference;";
        }
    }
}
