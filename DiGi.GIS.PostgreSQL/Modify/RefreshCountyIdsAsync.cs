using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Asynchronously moves the rows keyed on the given building references onto <paramref name="countyId"/>, in a table partitioned by county.
        /// <para>A building belongs to the county polygon part its footprint lies in, and <c>building_2d</c> is where that is recorded. Everything else keyed on a building has to follow it: every read of these tables filters on <c>county_id</c> first, so a row left under the part the building came from answers nothing, for anyone, ever again. This is the move that keeps them together, and it is meant to be called for the same references immediately after <c>Building2DPostgreSQLConverter.RefreshCountyIdsAsync</c> has moved the buildings themselves.</para>
        /// <para><c>county_id</c> is the <b>partition key</b>, so this is a row movement between partitions rather than an ordinary column update. The destination partition is created first, and the identifiers of the rows are preserved.</para>
        /// <para><b>Nothing is deleted.</b> A row cannot move onto a destination that already holds the same key, and two rows carrying one key under two different parts cannot both arrive; such a row stays where it is and its reference is not reported, so a caller comparing the result against what it passed in learns what is left to settle by hand.</para>
        /// <para><b>Only three tables are accepted</b> - <c>building</c>, <c>building_data</c> and <c>orto_datas</c> - and their key columns are named here rather than taken from the caller, so no identifier in the statement below comes from outside this method. Every one of them is checked against the columns the table actually has before it reaches the database. The tables holding referenced objects are keyed on <c>unique_id</c> instead and are moved by <c>Building2DReferencedObjectPostgreSQLConverter.RefreshCountyIdsAsync</c>; <c>building_2d</c> has its own method on its own converter.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="tableName">The table to move rows in. One of <c>building</c>, <c>building_data</c> or <c>orto_datas</c>; anything else is refused.</param>
        /// <param name="references">The building references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county polygon part the rows should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched, which cannot be pruned to a partition and reads the whole table.</param>
        /// <param name="batchSize">The number of references sent in one statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references that had at least one row moved, an empty set when the table does not exist, or null when the connection was null, no references were given, or the table is not one of the three this handles.</returns>
        public static async Task<HashSet<string>?> RefreshCountyIdsAsync(
            this NpgsqlConnection? npgsqlConnection,
            string? tableName,
            IEnumerable<string>? references,
            int countyId,
            IEnumerable<int>? countyIds_Source = null,
            int batchSize = 1000,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null || string.IsNullOrWhiteSpace(tableName))
            {
                return null;
            }

            // The key columns of each table, named here rather than passed in. They decide two things: what
            // counts as a collision on arrival, and how rows carrying one key are ranked so that only one of
            // them moves. For building that is (reference, lod, year) - the same tuple its unique index
            // carries - so a model of another level of detail at the destination does not block the move.
            //
            // Each is paired with whether it is nullable, because that decides how it may be compared. A
            // column declared NOT NULL is compared with =, which an index can serve; IS NOT DISTINCT FROM
            // means the same thing for it and cannot be indexed at all. Getting this wrong does not give a
            // wrong answer, it gives a sequential scan: every one of these tables indexes
            // (county_id, reference), so a comparison the index cannot serve leaves nothing to seek on, and
            // a batch carrying a single reference measured 31 seconds against production.
            List<(string ColumnName, bool Nullable)> columnNames_Key;
            if (tableName == Constants.TableName.Building)
            {
                columnNames_Key = [("reference", false), ("lod", true), ("year", true)];
            }
            else if (tableName == Constants.TableName.OrtoDatas)
            {
                columnNames_Key = [("reference", false)];
            }
            else if (tableName == Constants.TableName.BuildingData)
            {
                columnNames_Key = [(Core.IO.Query.UniqueId(IO.Constants.Column.Reference) ?? "reference", false)];
            }
            else
            {
                return null;
            }

            HashSet<string> references_Set = [];
            foreach (string reference in references)
            {
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    references_Set.Add(reference);
                }
            }

            HashSet<string> result = [];

            if (references_Set.Count == 0)
            {
                return result;
            }

            bool exists = await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, tableName);
            if (!exists)
            {
                return result;
            }

            // Every identifier the statement will carry is resolved against the columns the table actually
            // has. They are constants a few lines above rather than caller input, but a column list is the
            // only guard that can be relied on for an identifier, which cannot be a parameter.
            List<string>? columnNames = await DiGi.PostgreSQL.Query.ColumnNamesAsync(npgsqlConnection, tableName, cancellationToken: cancellationToken);
            if (columnNames is null)
            {
                return null;
            }

            HashSet<string> columnNames_Existing = [];
            foreach (string columnName in columnNames)
            {
                columnNames_Existing.Add(columnName);
            }

            if (!columnNames_Existing.Contains("county_id"))
            {
                return null;
            }

            foreach ((string ColumnName, bool Nullable) columnName_Key in columnNames_Key)
            {
                if (!columnNames_Existing.Contains(columnName_Key.ColumnName))
                {
                    return null;
                }
            }

            string columnName_Reference = columnNames_Key[0].ColumnName;

            // A row cannot move into a partition that does not exist, and the part a whole county belongs to
            // may never have held one of these rows either. Idempotent once the partition is there.
            await DiGi.PostgreSQL.Create.TableAsync_Partition(npgsqlConnection, tableName, countyId.ToString(), [countyId]);

            NpgsqlCommandBuilder npgsqlCommandBuilder = new();

            string tableName_Quoted = npgsqlCommandBuilder.QuoteIdentifier(tableName);

            // Every table here is unique on its county together with these columns, so the key is what
            // addresses a row: it identifies exactly one, it is what the destination collides on, and it is
            // indexed. ctid would address a row too, but only within its own partition and only through a
            // scan - and building_data has no identifier column at all, its primary key being
            // (county_id, reference).
            //
            // IS NOT DISTINCT FROM rather than =, because these keys are indexed NULLS NOT DISTINCT: a
            // building with no level of detail recorded is one key, not a key that never matches.
            List<string> columnNames_Key_Quoted = [];
            List<string> conditions_Target = [];
            List<string> conditions_Stray = [];
            foreach ((string ColumnName, bool Nullable) columnName_Key in columnNames_Key)
            {
                string columnName_Quoted = npgsqlCommandBuilder.QuoteIdentifier(columnName_Key.ColumnName);

                // = where the column cannot be null, so the index on (county_id, reference) can serve it.
                // IS NOT DISTINCT FROM only where a null is a value the key carries, as lod and year do on
                // building, whose index is declared NULLS NOT DISTINCT for exactly that reason.
                string @operator = columnName_Key.Nullable ? "IS NOT DISTINCT FROM" : "=";

                columnNames_Key_Quoted.Add($"t.{columnName_Quoted}");
                conditions_Target.Add($"t_Target.{columnName_Quoted} {@operator} t.{columnName_Quoted}");
                conditions_Stray.Add($"t.{columnName_Quoted} {@operator} s.{columnName_Quoted}");
            }

            // The parts the rows can be sitting under are written into the statement rather than carried as
            // a nullable parameter tested with OR. An OR against a parameter is not something the planner
            // can prune a partitioned table on, so the version that had one read all 406 partitions of
            // building_data on every batch - which is how a national run spent ten minutes per county and
            // then timed out (DiGi-Organization/DiGi.GIS.PostgreSQL#68).
            string condition_Source = countyIds_Source is null ? string.Empty : "\r\n                      AND t.county_id = ANY(@countyIds)";

            // NOT EXISTS is the collision guard for the unique key. It is cheap despite the surrounding
            // scan: county_id is fixed, so it prunes to the destination partition and probes the index the
            // constraint is already backed by.
            //
            // ROW_NUMBER guards the collision NOT EXISTS cannot see - two rows carrying one key under two
            // different wrong parts both pass it and would then collide with each other on arrival. The
            // lowest part moves, the rest stay where they are and are not reported. The ordering is total,
            // because the key is unique within a part.
            string commandText = $@"
                WITH stray AS MATERIALIZED (
                    SELECT t.county_id AS county_id,
                           {string.Join(", ", columnNames_Key_Quoted)},
                           ROW_NUMBER() OVER (PARTITION BY {string.Join(", ", columnNames_Key_Quoted)} ORDER BY t.county_id) AS move_rank
                    FROM {tableName_Quoted} t
                    WHERE t.{npgsqlCommandBuilder.QuoteIdentifier(columnName_Reference)} = ANY(@references)
                      AND t.county_id <> @countyId{condition_Source}
                      AND NOT EXISTS (
                              SELECT 1
                              FROM {tableName_Quoted} t_Target
                              WHERE t_Target.county_id = @countyId
                                AND {string.Join(" AND ", conditions_Target)})
                )
                UPDATE {tableName_Quoted} t
                SET county_id = @countyId
                FROM stray s
                WHERE t.county_id = s.county_id
                  AND {string.Join(" AND ", conditions_Stray)}
                  AND s.move_rank = 1
                RETURNING t.{npgsqlCommandBuilder.QuoteIdentifier(columnName_Reference)};";

            List<string> references_Unique = [.. references_Set];
            int[]? countyIds_Array = countyIds_Source is null ? null : [.. new HashSet<int>(countyIds_Source)];

            int batchSize_Effective = batchSize < 1 ? 1 : batchSize;

            for (int i = 0; i < references_Unique.Count; i += batchSize_Effective)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string[] references_Batch = [.. references_Unique.GetRange(i, Math.Min(batchSize_Effective, references_Unique.Count - i))];

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.AddWithValue("countyId", countyId);
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Batch });
                if (countyIds_Array is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    if (!npgsqlDataReader.IsDBNull(0))
                    {
                        result.Add(npgsqlDataReader.GetString(0));
                    }
                }
            }

            return result;
        }
    }
}
