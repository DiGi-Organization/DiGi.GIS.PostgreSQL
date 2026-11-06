using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and reads the polygons drawn in full-interpretation test areas in the <see cref="Constants.TableName.OrtoAreaObject"/> table: one row per drawn polygon, keyed by its server-assigned identifier (DiGi.GIS.PostgreSQL#129).
    /// <para>The <c>object</c> column carries the whole <see cref="OrtoAreaObject"/>, polygon and metadata included, and <c>area_key</c> carries the owning area with an index of its own, which is what the read by area runs on. <c>created_at</c> stores the drawing moment from the object itself, not the moment the row was inserted. The table lives on the Main database, beside <c>building_2d</c>.</para>
    /// </summary>
    public class OrtoAreaObjectPostgreSQLConverter : PostgreSQLConverter<OrtoAreaObject>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaObjectPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public OrtoAreaObjectPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with polygons drawn in test areas.
        /// </summary>
        public static string TableName => Constants.TableName.OrtoAreaObject;

        /// <summary>
        /// Asynchronously reads the polygons drawn in one test area, ordered by their identifiers.
        /// <para>An area that holds no polygons, and a table that does not exist yet, both answer an empty list (<see cref="Query.OrtoAreaObjectsByAreaKeyCommandText"/>).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the polygons of the area, or null when the connection or the key is null.</returns>
        public static async Task<List<OrtoAreaObject>?> GetOrtoAreaObjectsAsync(NpgsqlConnection? npgsqlConnection, string? areaKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(areaKey))
            {
                return null;
            }

            string commandText = Query.OrtoAreaObjectsByAreaKeyCommandText();

            List<OrtoAreaObject> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    if (JsonNode.Parse(npgsqlDataReader.GetString(0)) is JsonObject jsonObject)
                    {
                        result.Add(new OrtoAreaObject(jsonObject));
                    }
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }

        /// <summary>
        /// Asynchronously stores polygons drawn in test areas, replacing the row of each drawn polygon.
        /// <para>The conflict target is <c>object_id</c>: a server-assigned identifier names one polygon for good, so a revision replaces its row and keeps the identifier stable. A polygon without an identifier or without an area key is skipped, and rows are written <paramref name="batchSize"/> at a time, one statement per batch carrying one array per column. Within one call the last row of a duplicated identifier wins, because one statement cannot update the same row twice. The table is created when missing.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="ortoAreaObjects">The polygons to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when the connection or the polygons are null or the table could not be created.</returns>
        public static async Task<long?> UpdateAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<OrtoAreaObject?>? ortoAreaObjects, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ortoAreaObjects is null)
            {
                return null;
            }

            if (!await Create.TableAsync_OrtoAreaObject(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            Dictionary<string, OrtoAreaObject> dictionary = [];
            foreach (OrtoAreaObject? ortoAreaObject in ortoAreaObjects)
            {
                if (ortoAreaObject is null || string.IsNullOrWhiteSpace(ortoAreaObject.ObjectId) || string.IsNullOrWhiteSpace(ortoAreaObject.AreaKey))
                {
                    continue;
                }

                dictionary[ortoAreaObject.ObjectId] = ortoAreaObject;
            }

            List<OrtoAreaObject> ortoAreaObjects_Valid = [.. dictionary.Values];
            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                INSERT INTO {TableName} (object_id, area_key, cell_index, created_by, created_at, object)
                SELECT t.object_id, t.area_key, t.cell_index, t.created_by, t.created_at, t.object
                FROM unnest(@object_ids, @area_keys, @cell_indexes, @created_bys, @created_ats, @objects)
                    AS t(object_id, area_key, cell_index, created_by, created_at, object)
                ON CONFLICT (object_id) DO UPDATE SET
                    area_key = EXCLUDED.area_key,
                    cell_index = EXCLUDED.cell_index,
                    created_by = EXCLUDED.created_by,
                    created_at = EXCLUDED.created_at,
                    object = EXCLUDED.object;";

            long result = 0;
            for (int i = 0; i < ortoAreaObjects_Valid.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<OrtoAreaObject> ortoAreaObjects_Batch = ortoAreaObjects_Valid.GetRange(i, Math.Min(batchSize, ortoAreaObjects_Valid.Count - i));
                int count = ortoAreaObjects_Batch.Count;

                string[] objectIds = new string[count];
                string[] areaKeys = new string[count];
                int?[] cellIndexes = new int?[count];
                string[] createdBys = new string[count];
                DateTime[] createdAts = new DateTime[count];
                string[] objects = new string[count];

                for (int j = 0; j < count; j++)
                {
                    OrtoAreaObject ortoAreaObject = ortoAreaObjects_Batch[j];

                    objectIds[j] = ortoAreaObject.ObjectId;
                    areaKeys[j] = ortoAreaObject.AreaKey;
                    cellIndexes[j] = ortoAreaObject.CellIndex;
                    createdBys[j] = ortoAreaObject.CreatedBy;
                    createdAts[j] = ortoAreaObject.CreatedAt.UtcDateTime;
                    objects[j] = ortoAreaObject.ToJsonObject()?.ToJsonString() ?? "{}";
                }

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("object_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = objectIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_keys", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = areaKeys });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("cell_indexes", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = cellIndexes });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("created_bys", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = createdBys });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("created_ats", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz) { Value = createdAts });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("objects", NpgsqlDbType.Array | NpgsqlDbType.Jsonb) { Value = objects });

                result += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously removes one drawn polygon together with the remover's own labels on it, provided the remover drew it and no other interpreter has labelled it.
        /// <para>Drawn polygons are shared and their labels feed the agreement between interpreters, so a polygon someone else drew, or one someone else has marked, is never removed (<see cref="Enums.OrtoAreaObjectRemoval"/>). The interpreter is compared case-insensitively, as the label rows are. Everything runs in one transaction that locks the polygon's row: the caller's labels are deleted, then the polygon, and the transaction is rolled back if another interpreter's label exists at that point. A label another interpreter files in the moment between that check and the commit is not seen and stays as a row whose polygon is gone; the label write resolves the polygon first, so the window is one statement wide.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="objectId">The server-assigned identifier of the polygon.</param>
        /// <param name="interpreter">The e-mail of the interpreter asking for the removal, resolved from the session by the caller.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result names the outcome, or null when the connection or an argument is null or blank.</returns>
        public static async Task<Enums.OrtoAreaObjectRemoval?> RemoveAsync(NpgsqlConnection? npgsqlConnection, string? areaKey, string? objectId, string? interpreter, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(areaKey) || string.IsNullOrWhiteSpace(objectId) || string.IsNullOrWhiteSpace(interpreter))
            {
                return null;
            }

            // The label table is read and written below; it is missing only while no label was ever filed.
            if (!await Create.TableAsync_OrtoAreaLabel(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);
            try
            {
                string? createdBy;
                await using (NpgsqlCommand npgsqlCommand = new($"SELECT created_by FROM {TableName} WHERE object_id = @object_id AND area_key = @area_key FOR UPDATE;", npgsqlConnection, npgsqlTransaction))
                {
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("object_id", NpgsqlDbType.Text) { Value = objectId });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });

                    object? value = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
                    if (value is null || value is DBNull)
                    {
                        await npgsqlTransaction.RollbackAsync(cancellationToken);
                        return Enums.OrtoAreaObjectRemoval.NotFound;
                    }

                    createdBy = value as string;
                }

                if (!string.Equals(createdBy?.Trim(), interpreter.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    await npgsqlTransaction.RollbackAsync(cancellationToken);
                    return Enums.OrtoAreaObjectRemoval.NotCreator;
                }

                // The remover's own labels go first; any label left on the polygon then belongs to someone else.
                await using (NpgsqlCommand npgsqlCommand = new($"DELETE FROM {Constants.TableName.OrtoAreaLabel} WHERE area_key = @area_key AND object_id = @object_id AND lower(btrim(interpreter)) = lower(btrim(@interpreter));", npgsqlConnection, npgsqlTransaction))
                {
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("object_id", NpgsqlDbType.Text) { Value = objectId });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("interpreter", NpgsqlDbType.Text) { Value = interpreter });
                    await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                bool labelledByOthers;
                await using (NpgsqlCommand npgsqlCommand = new($"SELECT EXISTS (SELECT 1 FROM {Constants.TableName.OrtoAreaLabel} WHERE area_key = @area_key AND object_id = @object_id);", npgsqlConnection, npgsqlTransaction))
                {
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("object_id", NpgsqlDbType.Text) { Value = objectId });
                    labelledByOthers = await npgsqlCommand.ExecuteScalarAsync(cancellationToken) is true;
                }

                if (labelledByOthers)
                {
                    await npgsqlTransaction.RollbackAsync(cancellationToken);
                    return Enums.OrtoAreaObjectRemoval.LabelledByOthers;
                }

                await using (NpgsqlCommand npgsqlCommand = new($"DELETE FROM {TableName} WHERE object_id = @object_id AND area_key = @area_key;", npgsqlConnection, npgsqlTransaction))
                {
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("object_id", NpgsqlDbType.Text) { Value = objectId });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });
                    await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await npgsqlTransaction.CommitAsync(cancellationToken);
                return Enums.OrtoAreaObjectRemoval.Removed;
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                // The polygon table itself does not exist: no polygon was ever drawn. Disposing the aborted
                // transaction rolls it back.
                return Enums.OrtoAreaObjectRemoval.NotFound;
            }
        }

        /// <summary>
        /// Asynchronously removes one drawn polygon, automatically managing the connection. See <see cref="RemoveAsync(NpgsqlConnection?, string?, string?, string?, int, CancellationToken)"/> for the rules.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="objectId">The server-assigned identifier of the polygon.</param>
        /// <param name="interpreter">The e-mail of the interpreter asking for the removal, resolved from the session by the caller.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result names the outcome, or null when nothing could be attempted.</returns>
        public async Task<Enums.OrtoAreaObjectRemoval?> RemoveAsync(string? areaKey, string? objectId, string? interpreter, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await RemoveAsync(npgsqlConnection, areaKey, objectId, interpreter, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the polygons drawn in one test area, automatically managing the connection.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the polygons of the area, or null when nothing could be read.</returns>
        public async Task<List<OrtoAreaObject>?> GetOrtoAreaObjectsAsync(string? areaKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetOrtoAreaObjectsAsync(npgsqlConnection, areaKey, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores polygons drawn in test areas, automatically managing the connection.
        /// </summary>
        /// <param name="ortoAreaObjects">The polygons to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when nothing could be stored.</returns>
        public async Task<long?> UpdateAsync(IEnumerable<OrtoAreaObject?>? ortoAreaObjects, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, ortoAreaObjects, batchSize, commandTimeout, cancellationToken);
        }
    }
}
