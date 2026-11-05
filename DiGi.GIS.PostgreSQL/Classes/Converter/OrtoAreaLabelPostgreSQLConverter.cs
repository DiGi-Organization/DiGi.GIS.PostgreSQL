using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and reads the interpretation labels of full-interpretation test areas in the <see cref="Constants.TableName.OrtoAreaLabel"/> table: one row per (area, object identity, epoch, interpreter) (DiGi.GIS.PostgreSQL#129).
    /// <para>The identity columns carry everything a label is filed under: <c>area_key</c> with the footprint pair (<c>county_id</c>, <c>reference</c>) or the drawn-object identifier (<c>object_id</c>), then <c>epoch_year</c> and <c>interpreter</c>. The unique index behind them is <c>NULLS NOT DISTINCT</c> (<see cref="Create.TableAsync_OrtoAreaLabel(NpgsqlConnection?, int, CancellationToken)"/>), so rows that agree only in the NULLs of the identity pair conflict like any others, and one interpreter's re-upsert revises her or his own row while a second interpreter keeps a parallel one. The table lives on the Main database, beside <c>building_2d</c>.</para>
    /// </summary>
    public class OrtoAreaLabelPostgreSQLConverter : PostgreSQLConverter<OrtoAreaLabel>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaLabelPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public OrtoAreaLabelPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with test area labels.
        /// </summary>
        public static string TableName => Constants.TableName.OrtoAreaLabel;

        /// <summary>
        /// Asynchronously reads the label rows filed for one test area, ordered by epoch and interpreter.
        /// <para>An area that holds no labels, and a table that does not exist yet, both answer an empty list (<see cref="Query.OrtoAreaLabelsByAreaKeyCommandText"/>).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the labels of the area, or null when the connection or the key is null.</returns>
        public static async Task<List<OrtoAreaLabel>?> GetOrtoAreaLabelsAsync(NpgsqlConnection? npgsqlConnection, string? areaKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(areaKey))
            {
                return null;
            }

            string commandText = Query.OrtoAreaLabelsByAreaKeyCommandText();

            List<OrtoAreaLabel> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    result.Add(new OrtoAreaLabel(
                        npgsqlDataReader.GetString(0),
                        npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetInt32(1),
                        npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2),
                        npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetString(3),
                        npgsqlDataReader.GetInt16(4),
                        npgsqlDataReader.GetInt16(5),
                        npgsqlDataReader.IsDBNull(6) ? null : npgsqlDataReader.GetFieldValue<DateOnly>(6),
                        npgsqlDataReader.IsDBNull(7) ? null : npgsqlDataReader.GetFieldValue<DateOnly>(7),
                        npgsqlDataReader.GetString(8),
                        npgsqlDataReader.IsDBNull(9) ? null : npgsqlDataReader.GetString(9),
                        npgsqlDataReader.GetFieldValue<DateTimeOffset>(10)));
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }

        /// <summary>
        /// Asynchronously stores interpretation labels, revising the row of each identity rather than appending a second one.
        /// <para>The conflict target is <c>(area_key, county_id, reference, object_id, epoch_year, interpreter)</c>, and the index behind it is <c>NULLS NOT DISTINCT</c>: the object identity is nullable because the footprint pair (<c>county_id</c>, <c>reference</c>) and the drawn-object identifier (<c>object_id</c>) are set in turn, never together, and NULLs that agree must conflict like values do. The same identity under the same interpreter updates <c>state</c>, <c>observation_date_min</c>, <c>observation_date_max</c>, <c>note</c> and <c>updated_at</c> in place; a different interpreter inserts a row of her or his own.</para>
        /// <para>Labels without an area key or without an interpreter are skipped. Rows are written <paramref name="batchSize"/> at a time, one statement per batch carrying one array per column, and the table with its unique index is created before any conflict is inferred, so the first upsert into a fresh database finds the index in place. Within one call the last row of a duplicated identity wins, because one statement cannot update the same row twice.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="ortoAreaLabels">The labels to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when the connection or the labels are null or the table could not be created.</returns>
        public static async Task<long?> UpdateAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<OrtoAreaLabel?>? ortoAreaLabels, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ortoAreaLabels is null)
            {
                return null;
            }

            if (!await Create.TableAsync_OrtoAreaLabel(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            Dictionary<(string AreaKey, int? CountyId, string? Reference, string? ObjectId, short EpochYear, string Interpreter), OrtoAreaLabel> dictionary = [];
            foreach (OrtoAreaLabel? ortoAreaLabel in ortoAreaLabels)
            {
                if (ortoAreaLabel is null || string.IsNullOrWhiteSpace(ortoAreaLabel.AreaKey) || string.IsNullOrWhiteSpace(ortoAreaLabel.Interpreter))
                {
                    continue;
                }

                dictionary[(ortoAreaLabel.AreaKey, ortoAreaLabel.CountyId, ortoAreaLabel.Reference, ortoAreaLabel.ObjectId, ortoAreaLabel.EpochYear, ortoAreaLabel.Interpreter)] = ortoAreaLabel;
            }

            List<OrtoAreaLabel> ortoAreaLabels_Valid = [.. dictionary.Values];
            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                INSERT INTO {TableName} (area_key, county_id, reference, object_id, epoch_year, state, observation_date_min, observation_date_max, interpreter, note, updated_at)
                SELECT t.area_key, t.county_id, t.reference, t.object_id, t.epoch_year, t.state, t.observation_date_min, t.observation_date_max, t.interpreter, t.note, t.updated_at
                FROM unnest(@area_keys, @county_ids, @references, @object_ids, @epoch_years, @states, @observation_date_mins, @observation_date_maxs, @interpreters, @notes, @updated_ats)
                    AS t(area_key, county_id, reference, object_id, epoch_year, state, observation_date_min, observation_date_max, interpreter, note, updated_at)
                ON CONFLICT (area_key, county_id, reference, object_id, epoch_year, interpreter) DO UPDATE SET
                    state = EXCLUDED.state,
                    observation_date_min = EXCLUDED.observation_date_min,
                    observation_date_max = EXCLUDED.observation_date_max,
                    note = EXCLUDED.note,
                    updated_at = EXCLUDED.updated_at;";

            long result = 0;
            for (int i = 0; i < ortoAreaLabels_Valid.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<OrtoAreaLabel> ortoAreaLabels_Batch = ortoAreaLabels_Valid.GetRange(i, Math.Min(batchSize, ortoAreaLabels_Valid.Count - i));
                int count = ortoAreaLabels_Batch.Count;

                string[] areaKeys = new string[count];
                int?[] countyIds = new int?[count];
                string?[] references = new string?[count];
                string?[] objectIds = new string?[count];
                short[] epochYears = new short[count];
                short[] states = new short[count];
                DateOnly?[] observationDateMins = new DateOnly?[count];
                DateOnly?[] observationDateMaxs = new DateOnly?[count];
                string[] interpreters = new string[count];
                string?[] notes = new string?[count];
                DateTime[] updatedAts = new DateTime[count];

                for (int j = 0; j < count; j++)
                {
                    OrtoAreaLabel ortoAreaLabel = ortoAreaLabels_Batch[j];

                    areaKeys[j] = ortoAreaLabel.AreaKey;
                    countyIds[j] = ortoAreaLabel.CountyId;
                    references[j] = ortoAreaLabel.Reference;
                    objectIds[j] = ortoAreaLabel.ObjectId;
                    epochYears[j] = ortoAreaLabel.EpochYear;
                    states[j] = ortoAreaLabel.State;
                    observationDateMins[j] = ortoAreaLabel.ObservationDateMin;
                    observationDateMaxs[j] = ortoAreaLabel.ObservationDateMax;
                    interpreters[j] = ortoAreaLabel.Interpreter;
                    notes[j] = ortoAreaLabel.Note;
                    updatedAts[j] = ortoAreaLabel.UpdatedAt.UtcDateTime;
                }

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_keys", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = areaKeys });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("object_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = objectIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("epoch_years", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = epochYears });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("states", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = states });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("observation_date_mins", NpgsqlDbType.Array | NpgsqlDbType.Date) { Value = observationDateMins });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("observation_date_maxs", NpgsqlDbType.Array | NpgsqlDbType.Date) { Value = observationDateMaxs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("interpreters", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = interpreters });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("notes", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = notes });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("updated_ats", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz) { Value = updatedAts });

                result += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads the label rows filed for one test area, automatically managing the connection.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the labels of the area, or null when nothing could be read.</returns>
        public async Task<List<OrtoAreaLabel>?> GetOrtoAreaLabelsAsync(string? areaKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetOrtoAreaLabelsAsync(npgsqlConnection, areaKey, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores interpretation labels, automatically managing the connection.
        /// </summary>
        /// <param name="ortoAreaLabels">The labels to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when nothing could be stored.</returns>
        public async Task<long?> UpdateAsync(IEnumerable<OrtoAreaLabel?>? ortoAreaLabels, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, ortoAreaLabels, batchSize, commandTimeout, cancellationToken);
        }
    }
}
