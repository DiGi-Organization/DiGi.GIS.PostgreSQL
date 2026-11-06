using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and reads the materialised area tiles of full-interpretation test areas in the <see cref="Constants.TableName.OrtoAreaImage"/> table: one row per test area and imagery epoch (DiGi.GIS.PostgreSQL#129).
    /// <para>The row is keyed by <c>(area_key, epoch_year)</c>, so a re-fetch replaces it, never appends. Negative rows - status 1 (no imagery) and status 2 (fetch error) - are stored with a null <c>bytes</c> and cache the answer, so the source WMS is not asked again for an epoch that has nothing to give. The table lives on the Storage database beside <c>orto_datas</c>; the area registry is a Main read through <see cref="OrtoAreaPostgreSQLConverter"/>, and no statement ever reads the two databases at once.</para>
    /// </summary>
    public class OrtoAreaImagePostgreSQLConverter : PostgreSQLConverter<OrtoAreaImage>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoAreaImagePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public OrtoAreaImagePostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with materialised test area tiles.
        /// </summary>
        public static string TableName => Constants.TableName.OrtoAreaImage;

        /// <summary>
        /// Asynchronously reads the materialised tile of one test area for one imagery epoch.
        /// <para>A pair that was never fetched, and a table that does not exist yet, both answer null: a missing row is a different answer from a stored negative row of status 1 or 2 (<see cref="Query.OrtoAreaImageByAreaKeyAndEpochYearCommandText"/>).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="epochYear">The imagery epoch the tile was fetched for.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the tile, or null when the connection or the key is null or the pair is not stored.</returns>
        public static async Task<OrtoAreaImage?> GetOrtoAreaImageAsync(NpgsqlConnection? npgsqlConnection, string? areaKey, short epochYear, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(areaKey))
            {
                return null;
            }

            string commandText = Query.OrtoAreaImageByAreaKeyAndEpochYearCommandText();

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = areaKey });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("epoch_year", NpgsqlDbType.Smallint) { Value = epochYear });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                if (!await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                double minX = npgsqlDataReader.GetDouble(1);
                double minY = npgsqlDataReader.GetDouble(2);
                double maxX = npgsqlDataReader.GetDouble(3);
                double maxY = npgsqlDataReader.GetDouble(4);

                return new OrtoAreaImage(
                    npgsqlDataReader.GetString(0),
                    new BoundingBox2D(new Point2D(minX, minY), new Point2D(maxX, maxY)),
                    npgsqlDataReader.GetInt16(5),
                    npgsqlDataReader.GetInt32(6),
                    npgsqlDataReader.GetInt32(7),
                    npgsqlDataReader.GetInt16(8),
                    npgsqlDataReader.IsDBNull(9) ? null : npgsqlDataReader.GetString(9),
                    npgsqlDataReader.GetFieldValue<DateTimeOffset>(10),
                    npgsqlDataReader.IsDBNull(11) ? null : npgsqlDataReader.GetFieldValue<byte[]>(11));
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously stores one materialised area tile, replacing the row of the same area and epoch.
        /// <para>The conflict target is <c>(area_key, epoch_year)</c>: one area and one epoch hold one row, and a re-fetch - negative rows of status 1 and 2 included - replaces it, never appends. <see cref="OrtoAreaImage.Bytes"/> is stored as NULL when it is null, which is the shape of those negative rows. The table is created when missing, and an image without an area key or without a bounding box is not written.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="ortoAreaImage">The tile to store.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when the connection or the tile is null or the table could not be created.</returns>
        public static async Task<long?> UpdateAsync(NpgsqlConnection? npgsqlConnection, OrtoAreaImage? ortoAreaImage, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ortoAreaImage is null)
            {
                return null;
            }

            if (!await Create.TableAsync_OrtoAreaImage(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(ortoAreaImage.AreaKey) || ortoAreaImage.BoundingBox2D is not BoundingBox2D boundingBox2D)
            {
                return 0;
            }

            string commandText = $@"
                INSERT INTO {TableName} (area_key, min_x, min_y, max_x, max_y, epoch_year, width, height, status, source_url, fetched_at, bytes)
                VALUES (@area_key, @min_x, @min_y, @max_x, @max_y, @epoch_year, @width, @height, @status, @source_url, @fetched_at, @bytes)
                ON CONFLICT (area_key, epoch_year) DO UPDATE SET
                    min_x = EXCLUDED.min_x,
                    min_y = EXCLUDED.min_y,
                    max_x = EXCLUDED.max_x,
                    max_y = EXCLUDED.max_y,
                    width = EXCLUDED.width,
                    height = EXCLUDED.height,
                    status = EXCLUDED.status,
                    source_url = EXCLUDED.source_url,
                    fetched_at = EXCLUDED.fetched_at,
                    bytes = EXCLUDED.bytes;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("area_key", NpgsqlDbType.Text) { Value = ortoAreaImage.AreaKey });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox2D.Min.X });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox2D.Min.Y });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox2D.Max.X });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox2D.Max.Y });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("epoch_year", NpgsqlDbType.Smallint) { Value = ortoAreaImage.EpochYear });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("width", NpgsqlDbType.Integer) { Value = ortoAreaImage.Width });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("height", NpgsqlDbType.Integer) { Value = ortoAreaImage.Height });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Smallint) { Value = ortoAreaImage.Status });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("source_url", NpgsqlDbType.Text) { Value = (object?)ortoAreaImage.SourceUrl ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("fetched_at", NpgsqlDbType.TimestampTz) { Value = ortoAreaImage.FetchedAt.UtcDateTime });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("bytes", NpgsqlDbType.Bytea) { Value = (object?)ortoAreaImage.Bytes ?? DBNull.Value });

            return await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the materialised tile of one test area for one imagery epoch, automatically managing the connection.
        /// </summary>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="epochYear">The imagery epoch the tile was fetched for.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the tile, or null when nothing could be read.</returns>
        public async Task<OrtoAreaImage?> GetOrtoAreaImageAsync(string? areaKey, short epochYear, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetOrtoAreaImageAsync(npgsqlConnection, areaKey, epochYear, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores one materialised area tile, automatically managing the connection.
        /// </summary>
        /// <param name="ortoAreaImage">The tile to store.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when nothing could be stored.</returns>
        public async Task<long?> UpdateAsync(OrtoAreaImage? ortoAreaImage, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, ortoAreaImage, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously creates the OrtoAreaImage table (and its indexes) in the database, and analyzes it so the query-planner statistics are populated from the first query.
        /// <para>This is the explicit, repeatable materialisation path for the test-area tables (DiGi.GIS.PostgreSQL#130): it runs the same <see cref="Create.TableAsync_OrtoAreaImage(NpgsqlConnection, int, CancellationToken)"/> DDL the converters run at the top of every <c>UpdateAsync</c>, so nothing here diverges from the schema. Every statement is idempotent.</para>
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public async Task<bool> CreateTableAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool result = await Create.TableAsync_OrtoAreaImage(npgsqlConnection, commandTimeout, cancellationToken);
            if (result)
            {
                await DiGi.PostgreSQL.Modify.Analyze(npgsqlConnection, Constants.TableName.OrtoAreaImage, commandTimeout, cancellationToken);
            }

            return result;
        }
    }
}
