using DiGi.EPW;
using DiGi.EPW.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides functionality for converting and managing <see cref="EPWFile"/> entities within a PostgreSQL database.
    /// </summary>
    public class EPWFilePostgreSQLConverter : PostgreSQLConverter<EPWFile>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EPWFilePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public EPWFilePostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with EPW files.
        /// </summary>
        public static string TableName => Constants.TableName.EPWFile;

        /// <summary>
        /// Asynchronously clears all records from the epw_file table.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the operation succeeded; otherwise, false.</returns>
        public static async Task<bool> ClearAsync(NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            return await DiGi.PostgreSQL.Modify.ClearAsync(npgsqlConnection, TableName, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the closest EPWFile to the given coordinate (x, y).
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="x">The X coordinate (EPSG:2180 easting, in metres).</param>
        /// <param name="y">The Y coordinate (EPSG:2180 northing, in metres).</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the closest <see cref="EPWFile"/>, or null if not found.</returns>
        public static async Task<EPWFile?> GetEPWFileAsync(NpgsqlConnection? npgsqlConnection, double x, double y, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT object
                FROM {TableName}
                ORDER BY point(x, y) <-> point(@x, @y)
                LIMIT 1;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("x", NpgsqlDbType.Double) { Value = x });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("y", NpgsqlDbType.Double) { Value = y });

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            if (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                string jsonString = npgsqlDataReader.GetString(0);
                JsonObject? jsonObject = JsonNode.Parse(jsonString) as JsonObject;
                EPWFile ePWFile = new(jsonObject);
                return ePWFile;
            }

            return null;
        }

        /// <summary>
        /// Asynchronously retrieves the closest EPWFile to the given Point2D.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="point2D">The coordinate point.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the closest <see cref="EPWFile"/>, or null if not found.</returns>
        public static async Task<EPWFile?> GetEPWFileAsync(NpgsqlConnection? npgsqlConnection, Point2D point2D, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (point2D is null)
            {
                return null;
            }

            return await GetEPWFileAsync(npgsqlConnection, point2D.X, point2D.Y, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the closest EPWFile to the given coordinate (x, y) together with the id of its <c>epw_file</c> row.
        /// <para>The same nearest-neighbour lookup as <see cref="GetEPWFileAsync(NpgsqlConnection?, double, double, int, CancellationToken)"/>, which returns the file only, so a caller cannot tell which row it received. The id identifies the weather source of a stored solar radiation summary.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="x">The X coordinate (EPSG:2180 easting, in metres).</param>
        /// <param name="y">The Y coordinate (EPSG:2180 northing, in metres).</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the id of the closest row and its <see cref="EPWFile"/>, or null if not found.</returns>
        public static async Task<(int Id, EPWFile EPWFile)?> GetEPWFileWithIdAsync(NpgsqlConnection? npgsqlConnection, double x, double y, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT id, object
                FROM {TableName}
                ORDER BY point(x, y) <-> point(@x, @y)
                LIMIT 1;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("x", NpgsqlDbType.Double) { Value = x });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("y", NpgsqlDbType.Double) { Value = y });

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            if (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                int id = npgsqlDataReader.GetInt32(0);
                JsonObject? jsonObject = JsonNode.Parse(npgsqlDataReader.GetString(1)) as JsonObject;
                return (id, new EPWFile(jsonObject));
            }

            return null;
        }

        /// <summary>
        /// Asynchronously inserts or updates a collection of EPWFile objects in the database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="ePWFiles">The collection of EPW files to insert or update.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of database identifiers for the inserted or updated records.</returns>
        public static async Task<List<int>> InsertAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<EPWFile> ePWFiles, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ePWFiles is null || !ePWFiles.Any())
            {
                return [];
            }

            bool created = await Create.TableAsync_EPWFile(npgsqlConnection, commandTimeout, cancellationToken);
            if (!created)
            {
                return [];
            }

            List<int> ids = [];
            await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection);
            npgsqlBatch.Timeout = commandTimeout;

            foreach (EPWFile ePWFile in ePWFiles)
            {
                if (ePWFile.Location is not Location location)
                {
                    continue;
                }

                string name = ePWFile.Name();

                // Store coordinates in EPSG:2180 (ETRS89 / Poland CS92) so they share the
                // Building2D coordinate system used by the nearest-neighbour lookup query.
                Point3D point3D = new(location.Longitude, location.Latitude, location.Elevation);
                if (GIS.Convert.ToEPSG2180(point3D) is not Point2D point2D)
                {
                    continue;
                }

                double x = point2D.X;
                double y = point2D.Y;

                NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                    INSERT INTO {TableName} (name, x, y, object)
                    VALUES (@name, @x, @y, @object)
                    ON CONFLICT (name)
                    DO UPDATE SET
                        x = EXCLUDED.x,
                        y = EXCLUDED.y,
                        object = EXCLUDED.object
                    RETURNING id;");

                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("x", NpgsqlDbType.Double) { Value = x });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("y", NpgsqlDbType.Double) { Value = y });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb)
                {
                    Value = (object?)ePWFile.ToJsonObject()?.ToJsonString() ?? DBNull.Value
                });

                npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync(cancellationToken);
            do
            {
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    ids.Add(npgsqlDataReader.GetInt32(0));
                }
            }
            while (await npgsqlDataReader.NextResultAsync(cancellationToken));

            return ids;
        }

        /// <summary>
        /// Asynchronously clears all records from the epw_file table, automatically managing the connection.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the operation succeeded; otherwise, false.</returns>
        public async Task<bool> ClearAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await ClearAsync(npgsqlConnection, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the closest EPWFile to the given coordinate (x, y) together with the id of its <c>epw_file</c> row, automatically managing the connection.
        /// </summary>
        /// <param name="x">The X coordinate (EPSG:2180 easting, in metres).</param>
        /// <param name="y">The Y coordinate (EPSG:2180 northing, in metres).</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the id of the closest row and its <see cref="EPWFile"/>, or null if not found.</returns>
        public async Task<(int Id, EPWFile EPWFile)?> GetEPWFileWithIdAsync(double x, double y, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetEPWFileWithIdAsync(npgsqlConnection, x, y, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the closest EPWFile to the given coordinate (x, y), automatically managing the connection.
        /// </summary>
        /// <param name="x">The X coordinate (EPSG:2180 easting, in metres).</param>
        /// <param name="y">The Y coordinate (EPSG:2180 northing, in metres).</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the closest <see cref="EPWFile"/>, or null if not found.</returns>
        public async Task<EPWFile?> GetEPWFileAsync(double x, double y, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetEPWFileAsync(npgsqlConnection, x, y, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the closest EPWFile to the given Point2D, automatically managing the connection.
        /// </summary>
        /// <param name="point2D">The coordinate point.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the closest <see cref="EPWFile"/>, or null if not found.</returns>
        public async Task<EPWFile?> GetEPWFileAsync(Point2D point2D, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (point2D is null)
            {
                return null;
            }

            return await GetEPWFileAsync(point2D.X, point2D.Y, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts or updates a collection of EPWFile objects in the database, automatically managing the connection.
        /// </summary>
        /// <param name="ePWFiles">The collection of EPW files to insert or update.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of database identifiers for the inserted or updated records.</returns>
        public async Task<List<int>> InsertAsync(IEnumerable<EPWFile> ePWFiles, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await InsertAsync(npgsqlConnection, ePWFiles, commandTimeout, cancellationToken);
        }
    }
}