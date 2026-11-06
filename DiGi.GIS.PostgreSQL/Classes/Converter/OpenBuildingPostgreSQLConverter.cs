using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.IO.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using GeoAPI.CoordinateSystems.Transformations;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores the open building datasets: the <c>building_dataset</c> release register and the per-dataset footprint tables (EUBUCCO, Overture, OSM, DBSM, Microsoft, GHS-OBAT), on the Main database beside <c>building_2d</c> (DiGi.GIS.PostgreSQL#125).
    /// <para>The import is one transaction over the whole release: the register row is upserted, the partitions in scope are truncated (the GHS-OBAT table deleted in scope), the footprints stream in through a binary COPY per partition, and the release's row count and attribute list are written before the commit. A run that fails or is cancelled therefore leaves the previous snapshot exactly as it was - the replace is all-or-nothing, and re-running the same release converges on one snapshot.</para>
    /// <para>Partitions and tables are created <b>before</b> the transaction opens, because partition creation takes an exclusive lock on the parent and holding that for the length of a multi-million-row import would block every reader of every county until the import finished (the rule <see cref="TerrainPointPostgreSQLConverter"/> documents). The scope's partitions are created up front too, so the truncation inside the transaction never meets a missing partition.</para>
    /// <para>Every table name this converter interpolates into SQL is resolved through <see cref="Query.IsOpenBuildingTableName(string?)"/> first - an identifier cannot be a parameter, so the stored list of names is the guard.</para>
    /// </summary>
    public class OpenBuildingPostgreSQLConverter : PostgreSQLConverter<BuildingDatasetRelease>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// The number of buffered rows after which the buffer is written. Bounds the memory of a whole-country run while keeping the COPY batches large enough that per-partition importers are opened rarely.
        /// </summary>
        private const int rowCount_Flush = 100000;

        /// <summary>
        /// The most rejected footprints kept in memory for the caller. The count is never capped, only the list.
        /// </summary>
        private const int count_Rejections_Maximum = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenBuildingPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public OpenBuildingPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table this converter is registered against: the release register the footprint tables' <c>dataset_id</c> column points into.
        /// </summary>
        public static string TableName => Constants.TableName.BuildingDataset;

        /// <summary>
        /// Asynchronously creates the release register, the five footprint tables and the GHS-OBAT centroid table, if they do not already exist.
        /// <para>Every statement is idempotent, so the same method is both the up-front schema check of a run and the recovery from a database that lost its tables.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the commands.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the commands. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is true when all seven tables exist afterwards; otherwise, false.</returns>
        public static async Task<bool> CreateTablesAsync(NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            if (!await Create.TableAsync_BuildingDataset(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return false;
            }

            string[] tableNames =
            [
                Constants.TableName.EUBUCCO_Building2D,
                Constants.TableName.Overture_Building2D,
                Constants.TableName.OSM_Building2D,
                Constants.TableName.DBSM_Building2D,
                Constants.TableName.Microsoft_Building2D,
            ];

            foreach (string tableName in tableNames)
            {
                if (!await Create.TableAsync_OpenBuilding2D(npgsqlConnection, tableName, commandTimeout, cancellationToken))
                {
                    return false;
                }
            }

            return await Create.TableAsync_GHS_OBAT_Building(npgsqlConnection, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously creates the release register, the five footprint tables and the GHS-OBAT centroid table, automatically managing the connection.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the commands. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is true when all seven tables exist afterwards; otherwise, false.</returns>
        public async Task<bool> CreateTablesAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await CreateTablesAsync(npgsqlConnection, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts or revises the release's row in <c>building_dataset</c> and returns its identifier.
        /// <para>The conflict target is <c>(dataset_key, version, download_date)</c>, the register's <c>NULLS NOT DISTINCT</c> unique key, so importing the same release twice revises one row and keeps the identifier the footprint rows already point at. The metadata a finished import knows - the row count and the attribute list - is written separately by the run itself, not here.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command. When it has an open transaction, the statement joins it.</param>
        /// <param name="buildingDatasetRelease">The release to insert or revise.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the identifier of the register row, or null when the connection or the release is null, the release does not carry the three columns the register requires, or the statement failed.</returns>
        public static async Task<int?> UpsertReleaseAsync(NpgsqlConnection? npgsqlConnection, BuildingDatasetRelease? buildingDatasetRelease, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || buildingDatasetRelease is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(buildingDatasetRelease.DatasetKey) || string.IsNullOrWhiteSpace(buildingDatasetRelease.Version) || buildingDatasetRelease.DownloadDate is not DateTime downloadDate)
            {
                return null;
            }

            string commandText = $@"
                INSERT INTO {Constants.TableName.BuildingDataset} (dataset_key, version, download_date, source_url, licence, source_observation_date, attribute_names, file_name, sha256, row_count)
                VALUES (@dataset_key, @version, @download_date, @source_url, @licence, @source_observation_date, @attribute_names, @file_name, @sha256, @row_count)
                ON CONFLICT (dataset_key, version, download_date) DO UPDATE SET
                    source_url = EXCLUDED.source_url,
                    licence = EXCLUDED.licence,
                    source_observation_date = EXCLUDED.source_observation_date,
                    file_name = EXCLUDED.file_name,
                    sha256 = EXCLUDED.sha256
                RETURNING id;";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("dataset_key", NpgsqlDbType.Text) { Value = buildingDatasetRelease.DatasetKey });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("version", NpgsqlDbType.Text) { Value = buildingDatasetRelease.Version });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("download_date", NpgsqlDbType.Date) { Value = downloadDate.Date });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("source_url", NpgsqlDbType.Text) { Value = (object?)buildingDatasetRelease.SourceUrl ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("licence", NpgsqlDbType.Text) { Value = (object?)buildingDatasetRelease.Licence ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("source_observation_date", NpgsqlDbType.Text) { Value = (object?)buildingDatasetRelease.SourceObservationDate ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("attribute_names", NpgsqlDbType.Jsonb) { Value = buildingDatasetRelease.AttributeNames is null ? "[]" : JsonSerializer.Serialize(buildingDatasetRelease.AttributeNames) });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("file_name", NpgsqlDbType.Text) { Value = (object?)buildingDatasetRelease.FileName ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sha256", NpgsqlDbType.Text) { Value = (object?)buildingDatasetRelease.Sha256 ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("row_count", NpgsqlDbType.Bigint) { Value = buildingDatasetRelease.RowCount is long rowCount ? rowCount : (object?)DBNull.Value });

                object? result = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
                return result is null || result is DBNull ? null : System.Convert.ToInt32(result);
            }
            catch (Exception exception)
            {
                Serilog.Modify.Log(exception, "{Method}: the release {DatasetKey}/{Version} could not be registered", nameof(UpsertReleaseAsync), buildingDatasetRelease.DatasetKey, buildingDatasetRelease.Version);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously counts the rows currently stored for one dataset, optionally only those filed under the given county parts.
        /// <para>A table that does not exist yet answers null rather than zero, so a count of zero always means the table exists and holds nothing (the worked distinction is on <see cref="Classes.Rejection"/>).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="datasetKey">The dataset key whose table is counted; resolved through <see cref="Query.OpenBuildingTableName(string?)"/>.</param>
        /// <param name="countyIds">The county parts to count, or null to count the whole table.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the number of rows, or null when the connection or dataset key is null, the key is unknown, or the table does not exist.</returns>
        public static async Task<long?> GetRowCountAsync(NpgsqlConnection? npgsqlConnection, string? datasetKey, IEnumerable<int>? countyIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            string? tableName = Query.OpenBuildingTableName(datasetKey);
            if (npgsqlConnection is null || tableName is null)
            {
                return null;
            }

            string commandText = countyIds is null
                ? $"SELECT count(*) FROM {tableName};"
                : $"SELECT count(*) FROM {tableName} WHERE county_id = ANY(@countyIds);";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                if (countyIds is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds.ToArray() });
                }

                object? result = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
                return result is null || result is DBNull ? null : System.Convert.ToInt64(result);
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously counts the register rows matching a release, so a caller can assert that re-importing one release keeps exactly one row.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the command.</param>
        /// <param name="datasetKey">The dataset key to match; required.</param>
        /// <param name="version">The version to match, or null to match every version.</param>
        /// <param name="downloadDate">The download date to match, or null to match every date.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the number of matching register rows, or null when the connection or dataset key is null, or the register does not exist.</returns>
        public static async Task<long?> GetReleaseCountAsync(NpgsqlConnection? npgsqlConnection, string? datasetKey, string? version = null, DateTime? downloadDate = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(datasetKey))
            {
                return null;
            }

            string commandText = $"SELECT count(*) FROM {Constants.TableName.BuildingDataset} WHERE dataset_key = @dataset_key";

            if (version is not null)
            {
                commandText += " AND version = @version";
            }

            if (downloadDate is not null)
            {
                commandText += " AND download_date = @download_date";
            }

            commandText += ";";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("dataset_key", NpgsqlDbType.Text) { Value = datasetKey });

                if (version is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("version", NpgsqlDbType.Text) { Value = version });
                }

                if (downloadDate is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("download_date", NpgsqlDbType.Date) { Value = downloadDate.Value.Date });
                }

                object? result = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
                return result is null || result is DBNull ? null : System.Convert.ToInt64(result);
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously imports one release: files every footprint of the stream under a county part and stores the release's rows, replacing the previous snapshot in scope as one transaction.
        /// <para>The run loads the county parts once, reprojects each footprint with the transformation of its own source coordinate reference system (built lazily, once per run, as locals - never per footprint and never shared across runs), filters it against the scope, files it through <see cref="Query.OpenBuildingCountyId(BoundingBox2D, IPolygonal2D, Point2D, IDictionary{int, IPolygonal2D}, IDictionary{int, BoundingBox2D}, IDictionary{int, PolygonalFace2D}, ISet{int}, BoundingBox2D, double, out bool)"/> and buffers it; the buffer flushes through a binary COPY per partition every <c>rowCount_Flush</c> rows.</para>
        /// <para>A footprint the reprojection rejects (null input, non-finite vertex, short ring, out-of-domain result, unsupported source reference system) is counted with <see cref="UpdateRejectionReason.GeometryRejected"/> and never written; one that files nowhere is counted with <see cref="UpdateRejectionReason.CountyUnresolved"/>; one outside the run's scope is counted as filtered, not as a rejection. The GHS-OBAT table additionally refuses a row without a source identifier, because that identifier is its primary key.</para>
        /// </summary>
        /// <param name="footprints">The footprints to import, in their source coordinate reference system; null imports nothing and answers null.</param>
        /// <param name="buildingDatasetRelease">The release being imported; its dataset key selects the table and its three register columns are required.</param>
        /// <param name="countyIds">The identifiers of the county parts in scope (already widened to the sibling parts of multi-part counties); null or empty answers null.</param>
        /// <param name="boundingBox2D_Scope">The scope bounding box in EPSG:2180, or null for a run scoped by county list alone.</param>
        /// <param name="progress">An optional reporter carrying the running total of rows written, reported at every flush.</param>
        /// <param name="tolerance">The distance a footprint may lie outside a county part and still count as intersecting it.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the commands. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries the register row id, the rows stored, the footprints filtered and the footprints rejected, or null when the run could not start (bad arguments, unreadable schema, no counties to file against).</returns>
        public async Task<OpenBuildingImportResult?> ImportAsync(
            IAsyncEnumerable<Footprint>? footprints,
            BuildingDatasetRelease? buildingDatasetRelease,
            IEnumerable<int>? countyIds,
            BoundingBox2D? boundingBox2D_Scope = null,
            IProgress<long>? progress = null,
            double tolerance = Core.Constants.Tolerance.MacroDistance,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (footprints is null || buildingDatasetRelease is null || countyIds is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the import was not started - no footprint stream, no release or no county scope", nameof(ImportAsync));
                return null;
            }

            HashSet<int> countyIds_Scope = [.. countyIds];
            if (countyIds_Scope.Count == 0)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the import was not started - the scope holds no county part", nameof(ImportAsync));
                return null;
            }

            string? tableName = Query.OpenBuildingTableName(buildingDatasetRelease.DatasetKey);
            if (tableName is null)
            {
                Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the import was not started - {DatasetKey} names no open building table", nameof(ImportAsync), buildingDatasetRelease.DatasetKey!);
                return null;
            }

            bool isGhsObat = string.Equals(tableName, Constants.TableName.GHS_OBAT_Building, StringComparison.Ordinal);

            try
            {
                await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
                if (npgsqlConnection is null)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no connection to the Main database could be built", nameof(ImportAsync));
                    return null;
                }

                await npgsqlConnection.OpenAsync(cancellationToken);

                // The schema first: a database that cannot hold the rows should stop the run now rather than
                // after the register row has been written and a million footprints have been read.
                if (!await CreateTablesAsync(npgsqlConnection, commandTimeout, cancellationToken))
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the open building tables could not be created - the run would have no place to write to", nameof(ImportAsync));
                    return null;
                }

                // The county parts are read once: every footprint of the run is then filed in memory against
                // them, because a query per footprint is the cost this task must not pay (Coding - PostgreSQL.md section 3).
                List<AdministrativeAreal2D>? counties = await AdministrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByAdministrativeArealType(npgsqlConnection, Enums.AdministrativeArealType.County, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (counties is null || counties.Count == 0)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county part could be read - nothing can be filed", nameof(ImportAsync));
                    return null;
                }

                Dictionary<int, IPolygonal2D>? polygonal2Ds_ByCountyId;
                Dictionary<int, PolygonalFace2D>? polygonalFace2Ds_ByCountyId;
                Dictionary<int, BoundingBox2D> boundingBox2Ds_ByCountyId = [];

                if (isGhsObat)
                {
                    // Centroids file against the whole face - holes included - so the faces are what this run derives.
                    polygonalFace2Ds_ByCountyId = counties.PolygonalFace2DsById();
                    polygonal2Ds_ByCountyId = null;

                    foreach (KeyValuePair<int, PolygonalFace2D> pair in polygonalFace2Ds_ByCountyId)
                    {
                        if (pair.Value.GetBoundingBox() is BoundingBox2D boundingBox2D)
                        {
                            boundingBox2Ds_ByCountyId[pair.Key] = boundingBox2D;
                        }
                    }
                }
                else
                {
                    // Footprints file against the exterior edge; the envelope prefilter is derived from the same rings.
                    polygonal2Ds_ByCountyId = counties.Polygonal2DsByCountyId();
                    polygonalFace2Ds_ByCountyId = null;

                    foreach (KeyValuePair<int, IPolygonal2D> pair in polygonal2Ds_ByCountyId)
                    {
                        if (pair.Value.GetBoundingBox() is BoundingBox2D boundingBox2D)
                        {
                            boundingBox2Ds_ByCountyId[pair.Key] = boundingBox2D;
                        }
                    }
                }

                if (boundingBox2Ds_ByCountyId.Count == 0)
                {
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: no county part carries readable geometry - nothing can be filed", nameof(ImportAsync));
                    return null;
                }

                // Partitions are created before the transaction opens: partition creation takes an exclusive lock
                // on the parent, and holding that for the length of the import would block every reader of every
                // county until it finished (the rule TerrainPointPostgreSQLConverter documents).
                List<int> countyIds_Truncate = [];
                List<int> countyIds_PartitionUnavailable = [];

                if (!isGhsObat)
                {
                    foreach (int countyId in countyIds_Scope)
                    {
                        if (await Create.TableAsync_OpenBuilding2D_Partition(npgsqlConnection, tableName, countyId, commandTimeout, cancellationToken))
                        {
                            countyIds_Truncate.Add(countyId);
                        }
                        else
                        {
                            countyIds_PartitionUnavailable.Add(countyId);
                            Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "{Type}: partition of {TableName} for county {CountyId} is unavailable - its rows will be rejected, not written", nameof(ImportAsync), tableName, countyId);
                        }
                    }

                    countyIds_Truncate.Sort();
                }

                // The tallies and the register row id survive the transaction block: they are the result the
                // caller reads after the commit.
                long rowCount = 0;
                long filteredCount = 0;
                long rejectionCount = 0;
                List<Rejection> rejections = [];
                int? datasetId = null;

                await using (NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken))
                {
                    datasetId = await UpsertReleaseAsync(npgsqlConnection, buildingDatasetRelease, commandTimeout, cancellationToken);
                    if (datasetId is null)
                    {
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Error, "{Type}: the release of {DatasetKey}/{Version} could not be registered - nothing was written", nameof(ImportAsync), buildingDatasetRelease.DatasetKey!, buildingDatasetRelease.Version!);
                        return null;
                    }

                    // Replace the previous snapshot in scope. The partitions of this dataset's table are the
                    // snapshot unit: everything filed in them is this run's, and a failure past this point
                    // rolls the truncation back with the rest of the transaction.
                    if (isGhsObat)
                    {
                        string commandText_Delete = $@"DELETE FROM {Constants.TableName.GHS_OBAT_Building} WHERE county_id = ANY(@countyIds);";

                        await using NpgsqlCommand npgsqlCommand_Delete = new(commandText_Delete, npgsqlConnection);
                        npgsqlCommand_Delete.CommandTimeout = commandTimeout;
                        npgsqlCommand_Delete.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Scope.ToArray() });

                        await npgsqlCommand_Delete.ExecuteNonQueryAsync(cancellationToken);
                    }
                    else if (countyIds_Truncate.Count > 0)
                    {
                        string commandText_Truncate = $"TRUNCATE {string.Join(", ", countyIds_Truncate.Select(x => $"{tableName}_{x}"))};";

                        await using NpgsqlCommand npgsqlCommand_Truncate = new(commandText_Truncate, npgsqlConnection);
                        npgsqlCommand_Truncate.CommandTimeout = commandTimeout;

                        await npgsqlCommand_Truncate.ExecuteNonQueryAsync(cancellationToken);
                    }

                    HashSet<string> attributeNames = [];
                    Dictionary<int, List<OpenBuildingRow>> rows_ByCountyId = [];
                    int bufferedCount = 0;

                    // The transformations of this run, built once per source coordinate reference system and
                    // held as locals: per call is the cost DiGi.GIS#21 removed, and a static would share state
                    // across runs (Coding - General.md).
                    ICoordinateTransformation? transformation_4326 = null;
                    ICoordinateTransformation? transformation_3035 = null;

                    void Reject(string? sourceId, UpdateRejectionReason updateRejectionReason)
                    {
                        rejectionCount++;

                        if (rejections.Count < count_Rejections_Maximum)
                        {
                            rejections.Add(new Rejection(sourceId, updateRejectionReason));
                        }
                    }

                    await foreach (Footprint? footprint in footprints.WithCancellation(cancellationToken))
                    {
                        if (footprint is null)
                        {
                            Reject(null, UpdateRejectionReason.Undefined);
                            continue;
                        }

                        ICoordinateTransformation? coordinateTransformation;
                        switch (footprint.SourceSrid)
                        {
                            case 4326:
                                transformation_4326 ??= GIS.Create.EPSG2180TransformationFromEPSG4326();
                                coordinateTransformation = transformation_4326;
                                break;

                            case 3035:
                                transformation_3035 ??= GIS.Create.EPSG2180TransformationFromEPSG3035();
                                coordinateTransformation = transformation_3035;
                                break;

                            default:
                                coordinateTransformation = null;
                                break;
                        }

                        OpenBuildingRow? openBuildingRow = CreateRow(footprint, coordinateTransformation, polygonal2Ds_ByCountyId, boundingBox2Ds_ByCountyId, polygonalFace2Ds_ByCountyId, countyIds_Scope, boundingBox2D_Scope, tolerance, out UpdateRejectionReason? updateRejectionReason);

                        if (openBuildingRow is null)
                        {
                            if (updateRejectionReason is UpdateRejectionReason updateRejectionReason_Temp)
                            {
                                Reject(footprint.SourceId, updateRejectionReason_Temp);
                            }
                            else
                            {
                                // Null row without a reason: the footprint was outside this run's scope, which
                                // is the expected residue of a nationwide file read for pilot areas - not a loss.
                                filteredCount++;
                            }

                            continue;
                        }

                        if (isGhsObat && string.IsNullOrWhiteSpace(openBuildingRow.SourceId))
                        {
                            // The table is keyed by the link identifier, so a row without one cannot be stored
                            // or de-duplicated. Source tables all carry one; this guards the general case.
                            Reject(footprint.SourceId, UpdateRejectionReason.Undefined);
                            continue;
                        }

                        if (countyIds_PartitionUnavailable.Contains(openBuildingRow.CountyId))
                        {
                            Reject(openBuildingRow.SourceId, UpdateRejectionReason.PartitionUnavailable);
                            continue;
                        }

                        // The register's attribute list is the union of the run's rows; the bag is read here as
                        // well as in CreateRow because it is the loop's own tallies, not the row, that need it.
                        if (footprint.Attributes is JsonObject attributes)
                        {
                            foreach (KeyValuePair<string, JsonNode?> pair in attributes)
                            {
                                attributeNames.Add(pair.Key);
                            }
                        }

                        if (!rows_ByCountyId.TryGetValue(openBuildingRow.CountyId, out List<OpenBuildingRow>? rows))
                        {
                            rows = [];
                            rows_ByCountyId[openBuildingRow.CountyId] = rows;
                        }

                        rows.Add(openBuildingRow);
                        bufferedCount++;

                        if (bufferedCount >= rowCount_Flush)
                        {
                            rowCount += await FlushAsync(npgsqlConnection, tableName, isGhsObat, datasetId.Value, rows_ByCountyId, cancellationToken);
                            bufferedCount = 0;
                            progress?.Report(rowCount);
                        }
                    }

                    rowCount += await FlushAsync(npgsqlConnection, tableName, isGhsObat, datasetId.Value, rows_ByCountyId, cancellationToken);
                    bufferedCount = 0;
                    progress?.Report(rowCount);

                    await UpdateReleaseStatisticsAsync(npgsqlConnection, datasetId.Value, rowCount, attributeNames, commandTimeout, cancellationToken);

                    await npgsqlTransaction.CommitAsync(cancellationToken);
                }

                Serilog.Modify.Log(
                    "Open building import of {DatasetKey}/{Version} finished: {RowCount} rows stored, {FilteredCount} out of scope, {RejectionCount} rejected, over {CountyCount} county parts",
                    buildingDatasetRelease.DatasetKey!, buildingDatasetRelease.Version!, rowCount, filteredCount, rejectionCount, countyIds_Scope.Count);

                return new OpenBuildingImportResult(rowCount, filteredCount, rejectionCount, rejections, datasetId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The transaction disposes uncommitted, so an interrupted run leaves the previous snapshot untouched.
                throw;
            }
            catch (Exception exception)
            {
                Serilog.Modify.Log(exception, "{Type}: the import of {DatasetKey}/{Version} failed and was rolled back", nameof(ImportAsync), buildingDatasetRelease.DatasetKey!, buildingDatasetRelease.Version!);
                return null;
            }
        }

        /// <summary>
        /// Turns one footprint into the row its table stores - reprojected, filed and serialized - or reports why it stores nothing.
        /// <para>Three outcomes: a row with a null rejection reason is filed; a null row with a rejection reason was rejected (counted, never written); a null row with a null reason was out of scope (filtered, not a rejection). The reason is the channel that tells the last two apart, because both arrive as a null row.</para>
        /// <para>The reprojection is the rejection unit of DiGi.GIS#21: a null result - null input, non-finite vertex, ring shorter than four ordinates, result outside the EPSG:2180 domain - rejects the whole footprint, and a multi-outer collection is rejected entirely if any of its faces fails, so a partially reprojected ring is never written.</para>
        /// </summary>
        /// <param name="footprint">The footprint in its source coordinate reference system, or null.</param>
        /// <param name="coordinateTransformation">The transformation to EPSG:2180 matching the footprint's source coordinate reference system, or null when the source reference system is unsupported - which rejects the footprint.</param>
        /// <param name="polygonal2Ds_ByCountyId">The exterior ring of every county part loaded for the run; null for a centroid run.</param>
        /// <param name="boundingBox2Ds_ByCountyId">The stored envelope of every county part loaded for the run.</param>
        /// <param name="polygonalFace2Ds_ByCountyId">The face of every county part loaded for the run; required for centroid filing and null for a footprint run.</param>
        /// <param name="countyIds_Scope">The identifiers of the county parts in scope.</param>
        /// <param name="boundingBox2D_Scope">The scope bounding box in EPSG:2180, or null for a county-list scope.</param>
        /// <param name="tolerance">The distance a footprint may lie outside a part and still count as intersecting it.</param>
        /// <param name="updateRejectionReason">Set when the footprint is rejected; null when it is filed or filtered.</param>
        /// <returns>The row to buffer, or null when the footprint is rejected or filtered.</returns>
        public static OpenBuildingRow? CreateRow(
            Footprint? footprint,
            ICoordinateTransformation? coordinateTransformation,
            IDictionary<int, IPolygonal2D>? polygonal2Ds_ByCountyId,
            IDictionary<int, BoundingBox2D>? boundingBox2Ds_ByCountyId,
            IDictionary<int, PolygonalFace2D>? polygonalFace2Ds_ByCountyId,
            ISet<int>? countyIds_Scope,
            BoundingBox2D? boundingBox2D_Scope,
            double tolerance,
            out UpdateRejectionReason? updateRejectionReason)
        {
            updateRejectionReason = null;

            if (footprint is null)
            {
                updateRejectionReason = UpdateRejectionReason.Undefined;
                return null;
            }

            if (coordinateTransformation is null)
            {
                // The readers only emit 4326 and 3035, so this is the factory failing or an unexpected file:
                // either way the footprint cannot be placed in EPSG:2180 and must not be written.
                updateRejectionReason = UpdateRejectionReason.GeometryRejected;
                return null;
            }

            IGeometry2D? geometry_2180 = Reproject(footprint.Geometry, coordinateTransformation);
            if (geometry_2180 is null)
            {
                updateRejectionReason = UpdateRejectionReason.GeometryRejected;
                return null;
            }

            // The filing geometry: a polygon files by its exterior ring, a centroid by its point, and a
            // multi-outer collection (an OSM relation with several outer rings) by its largest face - the
            // other faces travel inside object unchanged, under the county of the dominant one. The envelope
            // is derived in the same pass: GetBoundingBox lives on the concrete shapes, not on IGeometry2D.
            IPolygonal2D? polygonal2D = null;
            Point2D? point2D = null;
            BoundingBox2D? boundingBox2D = null;

            switch (geometry_2180)
            {
                case Point2D point2D_Temp:
                    point2D = point2D_Temp;
                    boundingBox2D = new BoundingBox2D(point2D_Temp, point2D_Temp);
                    break;

                case PolygonalFace2D polygonalFace2D:
                    polygonal2D = polygonalFace2D.ExternalEdge;
                    boundingBox2D = polygonalFace2D.GetBoundingBox();
                    break;

                case GeometryCollection2D geometryCollection2D:
                    PolygonalFace2D? polygonalFace2D_Largest = null;
                    double area_Largest = -1;
                    List<BoundingBox2D> boundingBox2Ds_Children = [];

                    foreach (ICollectable2D collectable in geometryCollection2D)
                    {
                        if (collectable is not PolygonalFace2D polygonalFace2D_Child)
                        {
                            updateRejectionReason = UpdateRejectionReason.GeometryRejected;
                            return null;
                        }

                        if (polygonalFace2D_Child.GetBoundingBox() is BoundingBox2D boundingBox2D_Child)
                        {
                            boundingBox2Ds_Children.Add(boundingBox2D_Child);
                        }

                        double area = polygonalFace2D_Child.GetArea();
                        if (area > area_Largest)
                        {
                            area_Largest = area;
                            polygonalFace2D_Largest = polygonalFace2D_Child;
                        }
                    }

                    if (polygonalFace2D_Largest is null || boundingBox2Ds_Children.Count == 0)
                    {
                        updateRejectionReason = UpdateRejectionReason.GeometryRejected;
                        return null;
                    }

                    polygonal2D = polygonalFace2D_Largest.ExternalEdge;
                    boundingBox2D = new BoundingBox2D(boundingBox2Ds_Children);
                    break;

                default:
                    updateRejectionReason = UpdateRejectionReason.GeometryRejected;
                    return null;
            }

            if (boundingBox2D is null)
            {
                updateRejectionReason = UpdateRejectionReason.MissingGeometry;
                return null;
            }

            int? countyId = Query.OpenBuildingCountyId(boundingBox2D, polygonal2D, point2D, polygonal2Ds_ByCountyId, boundingBox2Ds_ByCountyId, polygonalFace2Ds_ByCountyId, countyIds_Scope, boundingBox2D_Scope, tolerance, out bool rejected);

            if (countyId is null)
            {
                // Rejected when no county reaches the footprint; filtered when it was simply outside this run.
                updateRejectionReason = rejected ? UpdateRejectionReason.CountyUnresolved : null;
                return null;
            }

            JsonObject? provenance = footprint.Provenance;
            JsonObject? attributes = footprint.Attributes;

            string primarySource = Query.PrimarySource(provenance);

            // The stored object is the footprint as the table's coordinate reference system sees it: the
            // reprojected geometry, the source attributes verbatim, the provenance and the licence exception.
            Footprint footprint_2180 = new(footprint.SourceId, 2180, geometry_2180, attributes, provenance, footprint.LicenceException);

            string? objectJson = footprint_2180.ToJsonObject()?.ToJsonString();
            if (objectJson is null)
            {
                updateRejectionReason = UpdateRejectionReason.Undefined;
                return null;
            }

            return new OpenBuildingRow
            {
                CountyId = countyId.Value,
                SourceId = footprint.SourceId,
                Provenance = provenance?.ToJsonString(),
                PrimarySource = primarySource,
                MinX = boundingBox2D.Min.X,
                MinY = boundingBox2D.Min.Y,
                MaxX = boundingBox2D.Max.X,
                MaxY = boundingBox2D.Max.Y,
                Object = objectJson,
            };
        }

        /// <summary>
        /// Reprojects one footprint geometry to EPSG:2180, or returns null when the geometry cannot be transformed as a whole.
        /// </summary>
        /// <param name="geometry">The geometry in its source coordinate reference system, or null.</param>
        /// <param name="coordinateTransformation">The transformation to EPSG:2180, or null.</param>
        /// <returns>The reprojected geometry, or null when any part of the transformation fails - the rejection unit of DiGi.GIS#21.</returns>
        private static IGeometry2D? Reproject(IGeometry2D? geometry, ICoordinateTransformation? coordinateTransformation)
        {
            if (geometry is null || coordinateTransformation is null)
            {
                return null;
            }

            switch (geometry)
            {
                case Point2D point2D:
                    // A centroid goes through the MathTransform directly: the folder's overloads are
                    // polygonal, and the one point overload builds a transformation per call, which is the
                    // cost DiGi.GIS#21 exists to remove. A centroid carries no ring whose failure could be
                    // half-applied, so - like the point convenience in DiGi.GIS - it does not enforce the
                    // Polish domain here; the county filing is what decides whether it lands in the country.
                    if (coordinateTransformation?.MathTransform is not GeoAPI.CoordinateSystems.Transformations.IMathTransform mathTransform)
                    {
                        return null;
                    }

                    if (!double.IsFinite(point2D.X) || !double.IsFinite(point2D.Y))
                    {
                        return null;
                    }

                    double[]? values = mathTransform.Transform([point2D.X, point2D.Y]);
                    if (values is null || values.Length < 2 || !double.IsFinite(values[0]) || !double.IsFinite(values[1]))
                    {
                        return null;
                    }

                    return new Point2D(values[0], values[1]);

                case PolygonalFace2D polygonalFace2D:
                    return polygonalFace2D.ToEPSG2180(coordinateTransformation);

                case GeometryCollection2D geometryCollection2D:
                    // A multi-outer collection is transformed face by face and only kept whole: one face that
                    // fails rejects the footprint, so no row ever holds half a reprojected relation.
                    GeometryCollection2D geometryCollection2D_2180 = [];

                    foreach (ICollectable2D collectable in geometryCollection2D)
                    {
                        if (collectable is not PolygonalFace2D polygonalFace2D_Child)
                        {
                            return null;
                        }

                        if (polygonalFace2D_Child.ToEPSG2180(coordinateTransformation) is not PolygonalFace2D polygonalFace2D_Child_2180)
                        {
                            return null;
                        }

                        geometryCollection2D_2180.Add(polygonalFace2D_Child_2180);
                    }

                    return geometryCollection2D_2180.Count == 0 ? null : geometryCollection2D_2180;

                default:
                    // Lines and shapes no open building table stores are rejected rather than coerced.
                    return null;
            }
        }

        /// <summary>
        /// Writes the buffered rows through a binary COPY - one importer per county partition, or one over the GHS-OBAT table - and empties the buffer.
        /// <para>The caller runs this inside the import transaction, so a flush that fails rolls the whole release back rather than leaving a half-written snapshot.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The connection with the import transaction open.</param>
        /// <param name="tableName">The footprint table, already resolved through <see cref="Query.IsOpenBuildingTableName(string?)"/>.</param>
        /// <param name="isGhsObat">True when the rows go to the unpartitioned GHS-OBAT table.</param>
        /// <param name="datasetId">The identifier of the release in the register.</param>
        /// <param name="rows_ByCountyId">The buffered rows keyed by county part; emptied by this call.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the number of rows written by this flush.</returns>
        private static async Task<long> FlushAsync(NpgsqlConnection npgsqlConnection, string tableName, bool isGhsObat, int datasetId, Dictionary<int, List<OpenBuildingRow>> rows_ByCountyId, CancellationToken cancellationToken)
        {
            if (rows_ByCountyId.Count == 0)
            {
                return 0;
            }

            long count = 0;

            if (isGhsObat)
            {
                // One table, so one importer carries every buffered row regardless of county.
                await using NpgsqlBinaryImporter npgsqlBinaryImporter = await npgsqlConnection.BeginBinaryImportAsync(CopyCommandText_GHSObat(), cancellationToken);

                foreach (List<OpenBuildingRow> rows in rows_ByCountyId.Values)
                {
                    foreach (OpenBuildingRow openBuildingRow in rows)
                    {
                        await WriteRowAsync(npgsqlBinaryImporter, openBuildingRow, datasetId, true, cancellationToken);
                    }
                }

                count = (long)await npgsqlBinaryImporter.CompleteAsync(cancellationToken);
            }
            else
            {
                // One COPY per county: a binary import names a single table, and the rows stream into their
                // county's partition rather than through the partitioned parent (TerrainPointPostgreSQLConverter).
                foreach (KeyValuePair<int, List<OpenBuildingRow>> pair in rows_ByCountyId)
                {
                    await using NpgsqlBinaryImporter npgsqlBinaryImporter = await npgsqlConnection.BeginBinaryImportAsync(CopyCommandText(tableName, pair.Key), cancellationToken);

                    foreach (OpenBuildingRow openBuildingRow in pair.Value)
                    {
                        await WriteRowAsync(npgsqlBinaryImporter, openBuildingRow, datasetId, false, cancellationToken);
                    }

                    count += (long)await npgsqlBinaryImporter.CompleteAsync(cancellationToken);
                }
            }

            rows_ByCountyId.Clear();
            return count;
        }

        /// <summary>
        /// Builds the binary COPY statement for one county partition of a footprint table.
        /// </summary>
        /// <param name="tableName">The footprint table name.</param>
        /// <param name="countyId">The identifier of the county partition; it names the partition.</param>
        /// <returns>The COPY statement text.</returns>
        private static string CopyCommandText(string tableName, int countyId)
        {
            // The identity column is left out: the partition assigns ids, the COPY supplies the rest.
            return $@"
                COPY {tableName}_{countyId} (county_id, dataset_id, source_id, provenance, primary_source, min_x, min_y, max_x, max_y, object)
                FROM STDIN (FORMAT BINARY)";
        }

        /// <summary>
        /// Builds the binary COPY statement for the GHS-OBAT centroid table.
        /// </summary>
        /// <returns>The COPY statement text.</returns>
        private static string CopyCommandText_GHSObat()
        {
            return $@"
                COPY {Constants.TableName.GHS_OBAT_Building} (overture_id, county_id, dataset_id, provenance, primary_source, object)
                FROM STDIN (FORMAT BINARY)";
        }

        /// <summary>
        /// Writes one buffered row into an open binary importer, column by column, in the order the COPY statement lists them.
        /// </summary>
        /// <param name="npgsqlBinaryImporter">The open importer.</param>
        /// <param name="openBuildingRow">The row to write.</param>
        /// <param name="datasetId">The identifier of the release in the register.</param>
        /// <param name="isGhsObat">True when the target is the GHS-OBAT table, whose key is the source identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the row has been written.</returns>
        private static async Task WriteRowAsync(NpgsqlBinaryImporter npgsqlBinaryImporter, OpenBuildingRow openBuildingRow, int datasetId, bool isGhsObat, CancellationToken cancellationToken)
        {
            await npgsqlBinaryImporter.StartRowAsync(cancellationToken);

            if (isGhsObat)
            {
                if (openBuildingRow.SourceId is null)
                {
                    await npgsqlBinaryImporter.WriteNullAsync(cancellationToken);
                }
                else
                {
                    await npgsqlBinaryImporter.WriteAsync(openBuildingRow.SourceId, NpgsqlDbType.Text, cancellationToken);
                }
            }

            await npgsqlBinaryImporter.WriteAsync(openBuildingRow.CountyId, NpgsqlDbType.Integer, cancellationToken);
            await npgsqlBinaryImporter.WriteAsync(datasetId, NpgsqlDbType.Integer, cancellationToken);

            if (!isGhsObat)
            {
                if (openBuildingRow.SourceId is null)
                {
                    await npgsqlBinaryImporter.WriteNullAsync(cancellationToken);
                }
                else
                {
                    await npgsqlBinaryImporter.WriteAsync(openBuildingRow.SourceId, NpgsqlDbType.Text, cancellationToken);
                }
            }

            if (openBuildingRow.Provenance is null)
            {
                await npgsqlBinaryImporter.WriteNullAsync(cancellationToken);
            }
            else
            {
                await npgsqlBinaryImporter.WriteAsync(openBuildingRow.Provenance, NpgsqlDbType.Jsonb, cancellationToken);
            }

            await npgsqlBinaryImporter.WriteAsync(openBuildingRow.PrimarySource, NpgsqlDbType.Text, cancellationToken);

            if (!isGhsObat)
            {
                await npgsqlBinaryImporter.WriteAsync(openBuildingRow.MinX, NpgsqlDbType.Double, cancellationToken);
                await npgsqlBinaryImporter.WriteAsync(openBuildingRow.MinY, NpgsqlDbType.Double, cancellationToken);
                await npgsqlBinaryImporter.WriteAsync(openBuildingRow.MaxX, NpgsqlDbType.Double, cancellationToken);
                await npgsqlBinaryImporter.WriteAsync(openBuildingRow.MaxY, NpgsqlDbType.Double, cancellationToken);
            }

            await npgsqlBinaryImporter.WriteAsync(openBuildingRow.Object, NpgsqlDbType.Jsonb, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the statistics only a finished import knows - the stored row count and the union of the rows' attribute names - into the release's register row.
        /// </summary>
        /// <param name="npgsqlConnection">The connection with the import transaction open.</param>
        /// <param name="datasetId">The identifier of the release in the register.</param>
        /// <param name="rowCount">The number of rows the import stored.</param>
        /// <param name="attributeNames">The distinct attribute names the stored rows carried.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the register row has been updated; a failure propagates, because a release whose statistics never landed should not commit.</returns>
        private static async Task UpdateReleaseStatisticsAsync(NpgsqlConnection npgsqlConnection, int datasetId, long rowCount, HashSet<string> attributeNames, int commandTimeout, CancellationToken cancellationToken)
        {
            List<string> attributeNames_Sorted = [.. attributeNames];
            attributeNames_Sorted.Sort(StringComparer.Ordinal);

            string commandText = $@"
                UPDATE {Constants.TableName.BuildingDataset}
                SET row_count = @row_count, attribute_names = @attribute_names
                WHERE id = @id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("row_count", NpgsqlDbType.Bigint) { Value = rowCount });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("attribute_names", NpgsqlDbType.Jsonb) { Value = JsonSerializer.Serialize(attributeNames_Sorted) });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer) { Value = datasetId });

            await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
