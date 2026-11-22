using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and reads the GUS 1 km census population grid in the <see cref="Constants.TableName.PopulationGrid"/> table: one row per cell of one census year, holding the cell identifier, the population (null where statistical secrecy hides it) and the outline reprojected to EPSG:2180 (DiGi.GIS.PostgreSQL#106).
    /// <para>The table lives on the Main database, beside <c>building_2d</c>, because a comparison statement reads the grid, the buildings, the occupancy and the county outline on one database. Cells are read by bounding box - the GiST box index narrows the read, the exact geometry tests run in C#, the way every other geometry read in this repository works.</para>
    /// </summary>
    public class PopulationGridPostgreSQLConverter : PostgreSQLConverter<PopulationGridCell>, IGISPostgreSQLConverter<PopulationGridCell>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationGridPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public PopulationGridPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with the census population grid.
        /// </summary>
        public static string TableName => Constants.TableName.PopulationGrid;

        /// <summary>
        /// Asynchronously reads the grid cells of one census year whose stored box reaches the given bounding box, automatically managing the connection.
        /// </summary>
        /// <param name="boundingBox2D">The bounding box in EPSG:2180 (easting in X) the cells have to reach.</param>
        /// <param name="censusYear">The census year to read (for example 2021).</param>
        /// <param name="tolerance">The distance the box is widened by on every side before the overlap test.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the cells, empty when none reaches the box, or null when the connection, the box or the table is missing.</returns>
        public async Task<List<PopulationGridCell>?> GetCellsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, int censusYear, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetCellsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, censusYear, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the grid cells of one census year whose stored box reaches the given bounding box.
        /// <para>The GiST index on the box columns answers which rows reach the box; the box is widened by <paramref name="tolerance"/> on every side, so a cell touching the box is never lost to rounding. A row whose <c>object</c> does not parse as a polygon is skipped with a warning naming the cell - the comparison would place its buildings by that outline, and a wrong outline would place them silently wrong.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="boundingBox2D">The bounding box in EPSG:2180 (easting in X) the cells have to reach.</param>
        /// <param name="censusYear">The census year to read (for example 2021).</param>
        /// <param name="tolerance">The distance the box is widened by on every side before the overlap test.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the cells, empty when none reaches the box, or null when the connection or the box is null or the table does not exist yet.</returns>
        public static async Task<List<PopulationGridCell>?> GetCellsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, int censusYear, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT cell_id, census_year, population, object
                FROM {TableName}
                WHERE census_year = @census_year
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@min_x, @min_y), point(@max_x, @max_y));";

            List<PopulationGridCell> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("census_year", NpgsqlDbType.Smallint) { Value = (short)censusYear });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox2D.Min.X - tolerance });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox2D.Min.Y - tolerance });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox2D.Max.X + tolerance });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox2D.Max.Y + tolerance });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    string? cellId = npgsqlDataReader.IsDBNull(0) ? null : npgsqlDataReader.GetString(0);
                    int censusYear_Row = npgsqlDataReader.GetInt16(1);
                    int? population = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetInt32(2);

                    PolygonalFace2D? polygonalFace2D = null;
                    string? reason = null;
                    if (npgsqlDataReader.IsDBNull(3))
                    {
                        reason = "carries no object";
                    }
                    else
                    {
                        try
                        {
                            polygonalFace2D = JsonNode.Parse(npgsqlDataReader.GetString(3)) as JsonObject is JsonObject jsonObject ? new PolygonalFace2D(jsonObject) : null;
                            if (polygonalFace2D is null)
                            {
                                reason = "holds no polygon";
                            }
                            else if (polygonalFace2D.GetBoundingBox() is null)
                            {
                                reason = "holds a polygon without a bounding box";
                                polygonalFace2D = null;
                            }
                        }
                        catch (JsonException jsonException)
                        {
                            reason = $"holds an object that is not JSON ({jsonException.Message})";
                            polygonalFace2D = null;
                        }
                    }

                    if (polygonalFace2D is null)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "{Method}: cell {CellId} of census year {CensusYear} was skipped: {Reason}", nameof(GetCellsByBoundingBox2DAsync), cellId ?? "<no identifier>", censusYear_Row, reason ?? "no reason recorded");
                        continue;
                    }

                    result.Add(new PopulationGridCell(cellId, censusYear_Row, population, polygonalFace2D));
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Asynchronously inserts or replaces the given grid cells, automatically managing the connection.
        /// </summary>
        /// <param name="populationGridCells">The cells to write; rows without an identifier or without a usable outline are skipped.</param>
        /// <param name="batchSize">The number of cells sent per command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows written, or null when the connection or the cells are null or the table could not be created.</returns>
        public async Task<long?> UpsertAsync(IEnumerable<PopulationGridCell>? populationGridCells, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpsertAsync(npgsqlConnection, populationGridCells, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts or replaces the given grid cells, creating the table when it does not exist yet.
        /// <para>An upsert on <c>(cell_id, census_year)</c> makes a re-run replace a cell's row rather than append a duplicate, so importing the same download twice leaves the table in the same state. The box columns are worked out from the outline here, because the box is the index the read prefilters on and deriving it anywhere else would let the two disagree.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="populationGridCells">The cells to write; rows without an identifier or without a usable outline are skipped.</param>
        /// <param name="batchSize">The number of cells sent per command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows written, or null when the connection or the cells are null or the table could not be created.</returns>
        public static async Task<long?> UpsertAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<PopulationGridCell>? populationGridCells, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || populationGridCells is null)
            {
                return null;
            }

            if (!await Create.TableAsync_PopulationGrid(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            List<PopulationGridCell> populationGridCells_Valid = [];
            foreach (PopulationGridCell? populationGridCell in populationGridCells)
            {
                if (populationGridCell is null || string.IsNullOrWhiteSpace(populationGridCell.CellId) || populationGridCell.PolygonalFace2D is not PolygonalFace2D polygonalFace2D || polygonalFace2D.GetBoundingBox() is not BoundingBox2D boundingBox2D)
                {
                    continue;
                }

                populationGridCells_Valid.Add(populationGridCell);
            }

            if (populationGridCells_Valid.Count == 0)
            {
                return 0;
            }

            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                INSERT INTO {TableName} (cell_id, census_year, population, min_x, min_y, max_x, max_y, object)
                SELECT t.cell_id, t.census_year, t.population, t.min_x, t.min_y, t.max_x, t.max_y, t.object
                FROM unnest(@cell_ids, @census_years, @populations, @min_xs, @min_ys, @max_xs, @max_ys, @objects)
                    AS t(cell_id, census_year, population, min_x, min_y, max_x, max_y, object)
                ON CONFLICT (cell_id, census_year) DO UPDATE SET
                    population = EXCLUDED.population,
                    min_x = EXCLUDED.min_x,
                    min_y = EXCLUDED.min_y,
                    max_x = EXCLUDED.max_x,
                    max_y = EXCLUDED.max_y,
                    object = EXCLUDED.object,
                    created_at = now();";

            long result = 0;
            for (int i = 0; i < populationGridCells_Valid.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<PopulationGridCell> populationGridCells_Batch = populationGridCells_Valid.GetRange(i, Math.Min(batchSize, populationGridCells_Valid.Count - i));
                int count = populationGridCells_Batch.Count;

                string[] cellIds = new string[count];
                short[] censusYears = new short[count];
                int?[] populations = new int?[count];
                double[] min_Xs = new double[count];
                double[] min_Ys = new double[count];
                double[] max_Xs = new double[count];
                double[] max_Ys = new double[count];
                string[] objects = new string[count];

                for (int j = 0; j < count; j++)
                {
                    PopulationGridCell populationGridCell = populationGridCells_Batch[j];
                    PolygonalFace2D polygonalFace2D = populationGridCell.PolygonalFace2D!;
                    BoundingBox2D boundingBox2D = polygonalFace2D.GetBoundingBox()!;

                    cellIds[j] = populationGridCell.CellId!;
                    censusYears[j] = (short)populationGridCell.CensusYear;
                    populations[j] = populationGridCell.Population;
                    min_Xs[j] = boundingBox2D.Min.X;
                    min_Ys[j] = boundingBox2D.Min.Y;
                    max_Xs[j] = boundingBox2D.Max.X;
                    max_Ys[j] = boundingBox2D.Max.Y;
                    objects[j] = polygonalFace2D.ToJsonObject()?.ToJsonString() ?? "{}";
                }

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("cell_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = cellIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("census_years", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = censusYears });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("populations", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = populations });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_xs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = min_Xs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_ys", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = min_Ys });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_xs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = max_Xs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_ys", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = max_Ys });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("objects", NpgsqlDbType.Array | NpgsqlDbType.Jsonb) { Value = objects });

                result += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            return result;
        }
    }
}
