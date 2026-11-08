using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and searches the orthophoto sheet index in the <see cref="Constants.TableName.OrtoSheet"/> table: one row per sheet image of the GUGiK index (<c>SkorowidzOrtofomapy&lt;year&gt;</c>), unique on its download file.
    /// <para>The <c>object</c> column carries the whole <see cref="OrtoSheet"/>, footprint included, so a read returns sheets that <see cref="GIS.Query.AcquisitionOrtoSheet"/> can date images by. The table lives on the Storage database beside <c>orto_datas</c>, which is what the acquisition-date backfill (<see cref="Modify.UpdateOrtoDatasAcquisitionAsync"/>) reads it against (DiGi.GIS.PostgreSQL#122).</para>
    /// </summary>
    public class OrtoSheetPostgreSQLConverter : PostgreSQLConverter<OrtoSheet>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtoSheetPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public OrtoSheetPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with orthophoto sheets.
        /// </summary>
        public static string TableName => Constants.TableName.OrtoSheet;

        /// <summary>
        /// Asynchronously counts the stored sheets, for one index year or for the whole table.
        /// <para>A table that does not exist yet counts zero.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="year">The index year (<see cref="OrtoSheet.Year"/>) to count, or null for every year.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or null when the connection is null.</returns>
        public static async Task<long?> GetCountAsync(NpgsqlConnection? npgsqlConnection, int? year, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $"SELECT count(*) FROM {TableName}{(year is null ? string.Empty : " WHERE year = @year")};";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                if (year is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("year", NpgsqlDbType.Integer) { Value = year.Value });
                }

                return System.Convert.ToInt64(await npgsqlCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.CurrentCulture);
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return 0;
            }
        }

        /// <summary>
        /// Asynchronously reads the sheets whose footprint box intersects a box, boundaries included.
        /// <para>One statement for a whole area: callers read the sheets of a batch of buildings at once and test each building against <see cref="OrtoSheet.Footprint"/> in memory (<see cref="GIS.Query.AcquisitionOrtoSheet"/>). The prefilter is on the stored box, as bounded ranges served by the <c>min_x</c> and <c>min_y</c> indexes (<see cref="Query.OrtoSheetBoundingBoxCommandText"/>); there is no PostGIS in this schema. A table that does not exist yet answers empty.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="boundingBox2D">The area in EPSG:2180, easting in X.</param>
        /// <param name="extent">The largest width or height of a sheet footprint, in m; a sheet larger than this is missed. The largest of the national index of 2026-11-03 is 4.9 km.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the sheets, or null when the connection or the box is null.</returns>
        public static async Task<List<OrtoSheet>?> GetOrtoSheetsByBoundingBoxAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, double extent = 10000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return null;
            }

            string commandText = Query.OrtoSheetBoundingBoxCommandText();

            List<OrtoSheet> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox2D.Min.X });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox2D.Min.Y });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox2D.Max.X });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox2D.Max.Y });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("extent", NpgsqlDbType.Double) { Value = extent });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    if (JsonNode.Parse(npgsqlDataReader.GetString(0)) is JsonObject jsonObject)
                    {
                        result.Add(new OrtoSheet(jsonObject));
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
        /// Asynchronously stores orthophoto sheets, one row per download file.
        /// <para>The conflict target is <c>download_url</c>: one file is one image (<see cref="GIS.Query.OrtoSheetDuplicateGroups"/>, rule A). A stored row is replaced by a row of the same or an earlier index year, so re-running an import refreshes it and a file listed again in a later year's index keeps its first listing, which is the canonical row of its duplicate group. Within one call the earliest year of a duplicated file wins, because one statement cannot update the same row twice.</para>
        /// <para>Sheets without a sheet identifier, a download file or a footprint are skipped. Rows are written <paramref name="batchSize"/> at a time, one statement per batch carrying one array per column. The table is created when missing.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="ortoSheets">The sheets to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when the connection or the sheets are null or the table could not be created.</returns>
        public static async Task<long?> UpdateAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<OrtoSheet?>? ortoSheets, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ortoSheets is null)
            {
                return null;
            }

            if (!await Create.TableAsync_OrtoSheet(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            Dictionary<string, (OrtoSheet OrtoSheet, BoundingBox2D BoundingBox2D)> dictionary = [];
            foreach (OrtoSheet? ortoSheet in ortoSheets)
            {
                if (ortoSheet is null || string.IsNullOrWhiteSpace(ortoSheet.SheetId) || string.IsNullOrWhiteSpace(ortoSheet.DownloadUrl) || ortoSheet.Footprint?.GetBoundingBox() is not BoundingBox2D boundingBox2D)
                {
                    continue;
                }

                if (dictionary.TryGetValue(ortoSheet.DownloadUrl!, out (OrtoSheet OrtoSheet, BoundingBox2D BoundingBox2D) tuple) && tuple.OrtoSheet.Year <= ortoSheet.Year)
                {
                    continue;
                }

                dictionary[ortoSheet.DownloadUrl!] = (ortoSheet, boundingBox2D);
            }

            List<(OrtoSheet OrtoSheet, BoundingBox2D BoundingBox2D)> tuples = [.. dictionary.Values];
            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                INSERT INTO {TableName} (sheet_id, type_name, year, date, date_precision, pixel_size, color_mode, orto_type, sheet_filled, register_date, min_x, min_y, max_x, max_y, download_url, object)
                SELECT t.sheet_id, t.type_name, t.year, t.date, t.date_precision, t.pixel_size, t.color_mode, t.orto_type, t.sheet_filled, t.register_date, t.min_x, t.min_y, t.max_x, t.max_y, t.download_url, t.object
                FROM unnest(@sheet_ids, @type_names, @years, @dates, @date_precisions, @pixel_sizes, @color_modes, @orto_types, @sheet_filleds, @register_dates, @min_xs, @min_ys, @max_xs, @max_ys, @download_urls, @objects)
                    AS t(sheet_id, type_name, year, date, date_precision, pixel_size, color_mode, orto_type, sheet_filled, register_date, min_x, min_y, max_x, max_y, download_url, object)
                ON CONFLICT (download_url) DO UPDATE SET
                    sheet_id = EXCLUDED.sheet_id,
                    type_name = EXCLUDED.type_name,
                    year = EXCLUDED.year,
                    date = EXCLUDED.date,
                    date_precision = EXCLUDED.date_precision,
                    pixel_size = EXCLUDED.pixel_size,
                    color_mode = EXCLUDED.color_mode,
                    orto_type = EXCLUDED.orto_type,
                    sheet_filled = EXCLUDED.sheet_filled,
                    register_date = EXCLUDED.register_date,
                    min_x = EXCLUDED.min_x,
                    min_y = EXCLUDED.min_y,
                    max_x = EXCLUDED.max_x,
                    max_y = EXCLUDED.max_y,
                    object = EXCLUDED.object,
                    created_at = now()
                WHERE EXCLUDED.year <= {TableName}.year;";

            long result = 0;
            for (int i = 0; i < tuples.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<(OrtoSheet OrtoSheet, BoundingBox2D BoundingBox2D)> tuples_Batch = tuples.GetRange(i, Math.Min(batchSize, tuples.Count - i));
                int count = tuples_Batch.Count;

                string[] sheetIds = new string[count];
                string?[] typeNames = new string?[count];
                int[] years = new int[count];
                DateTime?[] dates = new DateTime?[count];
                short[] datePrecisions = new short[count];
                double?[] pixelSizes = new double?[count];
                short[] colorModes = new short[count];
                short[] ortoTypes = new short[count];
                bool?[] sheetFilleds = new bool?[count];
                DateTime?[] registerDates = new DateTime?[count];
                double[] minXs = new double[count];
                double[] minYs = new double[count];
                double[] maxXs = new double[count];
                double[] maxYs = new double[count];
                string[] downloadUrls = new string[count];
                string[] objects = new string[count];

                for (int j = 0; j < count; j++)
                {
                    (OrtoSheet ortoSheet, BoundingBox2D boundingBox2D) = tuples_Batch[j];

                    sheetIds[j] = ortoSheet.SheetId!;
                    typeNames[j] = ortoSheet.TypeName;
                    years[j] = ortoSheet.Year;
                    dates[j] = ortoSheet.Date?.UtcDateTime;
                    datePrecisions[j] = (short)ortoSheet.DatePrecision;
                    pixelSizes[j] = ortoSheet.PixelSize;
                    colorModes[j] = (short)ortoSheet.ColorMode;
                    ortoTypes[j] = (short)ortoSheet.OrtoType;
                    sheetFilleds[j] = ortoSheet.SheetFilled;
                    registerDates[j] = ortoSheet.RegisterDate?.UtcDateTime;
                    minXs[j] = boundingBox2D.Min.X;
                    minYs[j] = boundingBox2D.Min.Y;
                    maxXs[j] = boundingBox2D.Max.X;
                    maxYs[j] = boundingBox2D.Max.Y;
                    downloadUrls[j] = ortoSheet.DownloadUrl!;
                    objects[j] = ortoSheet.ToJsonObject()?.ToJsonString() ?? "{}";
                }

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sheet_ids", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = sheetIds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("type_names", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = typeNames });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("years", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = years });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("dates", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz) { Value = dates });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("date_precisions", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = datePrecisions });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("pixel_sizes", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = pixelSizes });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("color_modes", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = colorModes });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("orto_types", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = ortoTypes });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sheet_filleds", NpgsqlDbType.Array | NpgsqlDbType.Boolean) { Value = sheetFilleds });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("register_dates", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz) { Value = registerDates });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_xs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = minXs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("min_ys", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = minYs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_xs", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = maxXs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("max_ys", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = maxYs });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("download_urls", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = downloadUrls });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("objects", NpgsqlDbType.Array | NpgsqlDbType.Jsonb) { Value = objects });

                result += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously counts the stored sheets, for one index year or for the whole table, automatically managing the connection.
        /// </summary>
        /// <param name="year">The index year to count, or null for every year.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or null when nothing could be counted.</returns>
        public async Task<long?> GetCountAsync(int? year, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetCountAsync(npgsqlConnection, year, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the sheets whose footprint box intersects a box, automatically managing the connection.
        /// </summary>
        /// <param name="boundingBox2D">The area in EPSG:2180, easting in X.</param>
        /// <param name="extent">The largest width or height of a sheet footprint, in m.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the sheets, or null when nothing could be read.</returns>
        public async Task<List<OrtoSheet>?> GetOrtoSheetsByBoundingBoxAsync(BoundingBox2D? boundingBox2D, double extent = 10000, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetOrtoSheetsByBoundingBoxAsync(npgsqlConnection, boundingBox2D, extent, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores orthophoto sheets, one row per download file, automatically managing the connection.
        /// </summary>
        /// <param name="ortoSheets">The sheets to store.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows inserted or replaced, or null when nothing could be stored.</returns>
        public async Task<long?> UpdateAsync(IEnumerable<OrtoSheet?>? ortoSheets, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, ortoSheets, batchSize, commandTimeout, cancellationToken);
        }
    }
}
