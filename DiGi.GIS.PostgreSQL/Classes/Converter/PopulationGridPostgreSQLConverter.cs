using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using GeoAPI.CoordinateSystems.Transformations;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// The grid resolution, in metres, of the cells this table holds: the NSP 1 km export.
        /// </summary>
        private const int CellResolution = 1000;

        /// <summary>
        /// Reads every cell of a downloaded GeoJSON export of the census grid into memory.
        /// <para>The export is a FeatureCollection on EPSG:3035 (checked when the file declares a <c>crs</c> member), and each feature's <c>code</c> names its cell on that lattice - <c>CRS3035RES1000m&lt;northing&gt;E&lt;easting&gt;</c> - together with the cell's south-west corner. <b>The stored outline is the whole 1 km square the code names, not the outline the export carries:</b> the export clips about 4 100 border cells to the national boundary and splits 100 of them into several parts, while the code always describes the complete lattice cell, which is what a point-in-cell assignment needs. The square is built on the lattice and reprojected to EPSG:2180 with <c>GIS.Create.EPSG2180TransformationFromEPSG3035()</c>.</para>
        /// <para><c>tot</c> is the population: a missing <c>tot</c> property or a value that is not an integer fails the run loudly (the export's schema changed), while a <c>null</c> <c>tot</c> is a value hidden by statistical secrecy and is stored as a null population - never as a zero. The sex and age breakdowns (<c>tot_male</c>, <c>tot_0_14</c>, ...) carry nulls in about a fifth of the cells of the real export and are not read at all.</para>
        /// <para>Everything is materialised: the 2021 export is ~190 MB of JSON and ~316 000 cells, so a run needs a few hundred MB of working set. <see cref="PopulateAsync(NpgsqlConnection?, string?, int?, bool, int, IProgress{long}?, int, CancellationToken)"/> is the path for the import; this method is the parse that facts and small exports use.</para>
        /// </summary>
        /// <param name="path">The path of the GeoJSON export.</param>
        /// <param name="censusYear">The census year the grid was counted in (for example 2021), stored on every cell.</param>
        /// <param name="cancellationToken">The cancellation token, observed every 4096 features.</param>
        /// <returns>The cells in file order, or null when the path is missing or the census year was not given. A malformed file throws with the reason and the offending feature.</returns>
        public static List<PopulationGridCell>? ReadCells(string? path, int? censusYear, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path) || censusYear is not int censusYear_Value)
            {
                return null;
            }

            if (!File.Exists(path))
            {
                return null;
            }

            ICoordinateTransformation? coordinateTransformation = GIS.Create.EPSG2180TransformationFromEPSG3035();
            if (coordinateTransformation is null)
            {
                return null;
            }

            using FileStream fileStream = File.OpenRead(path);
            using JsonDocument jsonDocument = JsonDocument.Parse(fileStream);

            JsonElement root = jsonDocument.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("type", out JsonElement typeElement)
                || !string.Equals(typeElement.GetString(), "FeatureCollection", StringComparison.Ordinal))
            {
                throw new InvalidDataException($"The file is not a GeoJSON FeatureCollection: {path}");
            }

            if (root.TryGetProperty("crs", out JsonElement crsElement)
                && crsElement.ValueKind == JsonValueKind.Object
                && crsElement.TryGetProperty("properties", out JsonElement crsProperties)
                && crsProperties.ValueKind == JsonValueKind.Object
                && crsProperties.TryGetProperty("name", out JsonElement crsNameElement)
                && crsNameElement.ValueKind == JsonValueKind.String
                && crsNameElement.GetString() is string crsName
                && !crsName.EndsWith("3035", StringComparison.Ordinal))
            {
                throw new InvalidDataException($"The file declares '{crsName}' while the census grid is published on EPSG:3035: {path}");
            }

            if (!root.TryGetProperty("features", out JsonElement featuresElement) || featuresElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException($"The FeatureCollection carries no features array: {path}");
            }

            List<PopulationGridCell> populationGridCells = [];
            int index = 0;
            foreach (JsonElement feature in featuresElement.EnumerateArray())
            {
                if ((index++ & 0xFFF) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                populationGridCells.Add(Cell(feature, censusYear_Value, coordinateTransformation));
            }

            return populationGridCells;
        }

        /// <summary>
        /// Asynchronously imports a downloaded GeoJSON export of the census grid into the table, automatically managing the connection.
        /// </summary>
        /// <param name="path">The path of the GeoJSON export.</param>
        /// <param name="censusYear">The census year the grid was counted in (for example 2021); required.</param>
        /// <param name="clear">Whether the census year's stored rows are deleted after the file has been read and validated, before the import writes.</param>
        /// <param name="batchSize">The number of cells sent per insert command.</param>
        /// <param name="progress">Progress reporter carrying the number of cells written.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of cells written, 0 for an empty export, or null when the connection, the path or the census year is missing.</returns>
        public async Task<long?> PopulateAsync(string? path, int? censusYear, bool clear = false, int batchSize = 1000, IProgress<long>? progress = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await PopulateAsync(npgsqlConnection, path, censusYear, clear, batchSize, progress, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously imports a downloaded GeoJSON export of the census grid into the table.
        /// <para>The file is read and validated in full first, so a malformed download cannot clear or overwrite anything; <paramref name="clear"/> then deletes the census year's rows and the upsert writes the new ones in batches. A run that fails midway leaves the cells it wrote, and a re-run converges on them because the write is keyed on <c>(cell_id, census_year)</c>.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="path">The path of the GeoJSON export.</param>
        /// <param name="censusYear">The census year the grid was counted in (for example 2021); required.</param>
        /// <param name="clear">Whether the census year's stored rows are deleted after the file has been read and validated, before the import writes.</param>
        /// <param name="batchSize">The number of cells sent per insert command.</param>
        /// <param name="progress">Progress reporter carrying the number of cells written.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of cells written, 0 for an empty export, or null when the connection, the path or the census year is missing.</returns>
        public static async Task<long?> PopulateAsync(
            NpgsqlConnection? npgsqlConnection,
            string? path,
            int? censusYear,
            bool clear = false,
            int batchSize = 1000,
            IProgress<long>? progress = null,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            List<PopulationGridCell>? populationGridCells = ReadCells(path, censusYear, cancellationToken);
            if (populationGridCells is null)
            {
                return null;
            }

            if (clear && censusYear is int censusYear_Value)
            {
                long? clearedCount = await ClearAsync(npgsqlConnection, censusYear_Value, commandTimeout, cancellationToken);
                if (clearedCount is null)
                {
                    return null;
                }
            }

            long? writtenCount = await UpsertAsync(npgsqlConnection, populationGridCells, batchSize, commandTimeout, cancellationToken);
            if (writtenCount is long writtenCount_Value)
            {
                progress?.Report(writtenCount_Value);
            }

            return writtenCount;
        }

        /// <summary>
        /// Asynchronously deletes the stored rows of one census year, automatically managing the connection.
        /// </summary>
        /// <param name="censusYear">The census year whose rows are deleted.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows deleted, or null when the connection is null; a table that does not exist yet deletes nothing.</returns>
        public async Task<long?> ClearAsync(int censusYear, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await ClearAsync(npgsqlConnection, censusYear, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the stored rows of one census year.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="censusYear">The census year whose rows are deleted.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows deleted, or null when the connection is null; a table that does not exist yet deletes nothing.</returns>
        public static async Task<long?> ClearAsync(NpgsqlConnection? npgsqlConnection, int censusYear, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"DELETE FROM {TableName} WHERE census_year = @census_year;";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("census_year", NpgsqlDbType.Smallint) { Value = (short)censusYear });

                return await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts one GeoJSON feature of the census grid export into a <see cref="PopulationGridCell"/> whose outline is the full lattice square the cell's <c>code</c> names, reprojected to EPSG:2180.
        /// <para>Everything the reader does not recognise fails with the feature's identifier and the reason: a changed export schema must stop the import rather than be guessed at. The <c>crs</c> of the collection is checked before any feature is read; the lattice of the code decides the geometry, so the export's own coordinates - jittered, and clipped at the border - are never parsed.</para>
        /// </summary>
        /// <param name="feature">The feature to convert.</param>
        /// <param name="censusYear">The census year stored on the cell.</param>
        /// <param name="coordinateTransformation">The EPSG:3035 to EPSG:2180 transformation to project the lattice square with.</param>
        /// <returns>The cell.</returns>
        /// <exception cref="InvalidDataException">The feature is not a grid cell this import can read.</exception>
        private static PopulationGridCell Cell(JsonElement feature, int censusYear, ICoordinateTransformation coordinateTransformation)
        {
            string context = "<unknown>";
            if (feature.ValueKind == JsonValueKind.Object)
            {
                if (feature.TryGetProperty("id", out JsonElement idElement))
                {
                    context = idElement.ToString();
                }
                else if (feature.TryGetProperty("oid", out JsonElement oidElement))
                {
                    context = oidElement.ToString();
                }
            }
            else
            {
                throw new InvalidDataException($"Feature {context}: expected a GeoJSON feature object, found {feature.ValueKind}.");
            }

            if (!feature.TryGetProperty("properties", out JsonElement properties) || properties.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"Feature {context}: carries no properties object.");
            }

            if (!properties.TryGetProperty("code", out JsonElement codeElement) || codeElement.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(codeElement.GetString()))
            {
                throw new InvalidDataException($"Feature {context}: carries no grid cell 'code'.");
            }

            string code = codeElement.GetString()!;

            Match match = Regex.Match(code, @"^CRS(?<epsg>\d+)RES(?<resolution>\d+)mN(?<northing>\d+)E(?<easting>\d+)$", RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                throw new InvalidDataException($"Feature {context}: cell code '{code}' is not of the expected 'CRS3035RES1000m<northing>E<easting>' form.");
            }

            if (!string.Equals(match.Groups["epsg"].Value, "3035", StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Feature {context}: cell code '{code}' is not on the EPSG:3035 lattice the census grid is published on.");
            }

            if (!int.TryParse(match.Groups["resolution"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int resolution) || resolution != CellResolution)
            {
                throw new InvalidDataException($"Feature {context}: cell code '{code}' is not a 1000 m cell of the 1 km grid this table holds.");
            }

            if (!int.TryParse(match.Groups["northing"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int northing)
                || !int.TryParse(match.Groups["easting"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int easting))
            {
                throw new InvalidDataException($"Feature {context}: cell code '{code}' carries a corner outside the integer range of the lattice.");
            }

            int? population = null;
            if (!properties.TryGetProperty("tot", out JsonElement totalElement))
            {
                throw new InvalidDataException($"Feature {context}: carries no 'tot' population property.");
            }

            switch (totalElement.ValueKind)
            {
                case JsonValueKind.Null:
                    // Hidden by statistical secrecy: no value, which is stored as a null population and never as a zero.
                    break;

                case JsonValueKind.Number:
                    if (!totalElement.TryGetInt32(out int total) || total < 0)
                    {
                        throw new InvalidDataException($"Feature {context}: 'tot' ({totalElement.GetRawText()}) is not a population count.");
                    }

                    population = total;
                    break;

                default:
                    throw new InvalidDataException($"Feature {context}: 'tot' is {totalElement.ValueKind}, expected a number or null.");
            }

            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(
                new Point2D(easting, northing),
                new Point2D(easting + resolution, northing),
                new Point2D(easting + resolution, northing + resolution),
                new Point2D(easting, northing + resolution),
                new Point2D(easting, northing));

            if (polygonalFace2D is null)
            {
                throw new InvalidDataException($"Feature {context}: cell '{code}' could not be turned into an outline.");
            }

            PolygonalFace2D? polygonalFace2D_2180 = polygonalFace2D.ToEPSG2180(coordinateTransformation);
            if (polygonalFace2D_2180 is null)
            {
                throw new InvalidDataException($"Feature {context}: cell '{code}' does not fall inside the EPSG:2180 domain.");
            }

            return new PopulationGridCell(code, censusYear, population, polygonalFace2D_2180);
        }
    }
}
