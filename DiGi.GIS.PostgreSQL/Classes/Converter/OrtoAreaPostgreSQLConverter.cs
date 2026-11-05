using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and reads full-interpretation test areas in the <see cref="Constants.TableName.OrtoArea"/> table: one row per test area, keyed by the invariant location key of its bounding box (DiGi.GIS.PostgreSQL#129).
    /// <para>The <c>object</c> column carries the whole <see cref="OrtoArea"/>, sampled cells included, so one read returns the area with its grid in index order; the envelope columns (<c>min_x</c> ... <c>max_y</c>) duplicate the box for keyed and ranged lookup. The table lives on the Main database, beside <c>building_2d</c>. The area's imagery is a Storage read through <see cref="OrtoAreaImagePostgreSQLConverter"/>, and no statement ever reads the two databases at once.</para>
    /// </summary>
    public class OrtoAreaPostgreSQLConverter : PostgreSQLConverter<OrtoArea>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public OrtoAreaPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with full-interpretation test areas.
        /// </summary>
        public static string TableName => Constants.TableName.OrtoArea;

        /// <summary>
        /// Asynchronously reads one test area by the invariant location key of its bounding box.
        /// <para>A key that is not stored, and a table that does not exist yet, both answer null (<see cref="Query.OrtoAreaByKeyCommandText"/>).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="areaKey">The invariant location key of the area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the area with its sampled cells, or null when the connection or the key is null or the area is not stored.</returns>
        public static async Task<OrtoArea?> GetOrtoAreaAsync(NpgsqlConnection? npgsqlConnection, string? areaKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(areaKey))
            {
                return null;
            }

            string commandText = Query.OrtoAreaByKeyCommandText();

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                if (!await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                return JsonNode.Parse(npgsqlDataReader.GetString(0)) is JsonObject jsonObject ? new OrtoArea(jsonObject) : null;
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously stores one test area, replacing the row with the same invariant location key.
        /// <para>The conflict target is <c>area_key</c>: an area is re-sampled as a whole, so a later write replaces the previous row, sampled cells included. The table and its box indexes are created when missing, and an area without a key or without a bounding box is not written.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="ortoArea">The area to store.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when the connection or the area is null or the table could not be created.</returns>
        public static async Task<long?> UpdateAsync(NpgsqlConnection? npgsqlConnection, OrtoArea? ortoArea, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ortoArea is null)
            {
                return null;
            }

            if (!await Create.TableAsync_OrtoArea(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(ortoArea.AreaKey) || ortoArea.BoundingBox2D is not BoundingBox2D boundingBox2D)
            {
                return 0;
            }

            string commandText = $@"
                INSERT INTO {TableName} (area_key, min_x, min_y, max_x, max_y, name, stratum, cell_size, cell_count, seed, object)
                VALUES (@area_key, @min_x, @min_y, @max_x, @max_y, @name, @stratum, @cell_size, @cell_count, @seed, @object)
                ON CONFLICT (area_key) DO UPDATE SET
                    min_x = EXCLUDED.min_x,
                    min_y = EXCLUDED.min_y,
                    max_x = EXCLUDED.max_x,
                    max_y = EXCLUDED.max_y,
                    name = EXCLUDED.name,
                    stratum = EXCLUDED.stratum,
                    cell_size = EXCLUDED.cell_size,
                    cell_count = EXCLUDED.cell_count,
                    seed = EXCLUDED.seed,
                    object = EXCLUDED.object;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = ortoArea.AreaKey });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox2D.Min.X });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox2D.Min.Y });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox2D.Max.X });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox2D.Max.Y });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = (object?)ortoArea.Name ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("stratum", NpgsqlDbType.Text) { Value = (object?)ortoArea.Stratum ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("cell_size", NpgsqlDbType.Double) { Value = ortoArea.CellSize });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("cell_count", NpgsqlDbType.Integer) { Value = ortoArea.CellCount });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("seed", NpgsqlDbType.Bigint) { Value = ortoArea.Seed });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb) { Value = ortoArea.ToJsonObject()?.ToJsonString() ?? "{}" });

            return await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads one test area by the invariant location key of its bounding box, automatically managing the connection.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the area with its sampled cells, or null when nothing could be read.</returns>
        public async Task<OrtoArea?> GetOrtoAreaAsync(string? areaKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetOrtoAreaAsync(npgsqlConnection, areaKey, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores one test area, replacing the row with the same invariant location key, automatically managing the connection.
        /// </summary>
        /// <param name="ortoArea">The area to store.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when nothing could be stored.</returns>
        public async Task<long?> UpdateAsync(OrtoArea? ortoArea, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, ortoArea, commandTimeout, cancellationToken);
        }
    }
}
