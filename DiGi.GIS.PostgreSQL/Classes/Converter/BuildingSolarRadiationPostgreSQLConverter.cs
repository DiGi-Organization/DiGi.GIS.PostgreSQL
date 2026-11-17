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
    /// Stores and reads <see cref="GIS.Classes.BuildingSolarRadiationResult"/> objects in the <see cref="Constants.TableName.BuildingSolarRadiation"/> table: one row per building model, neighbour radius and solver version, partitioned by county part.
    /// <para>The <c>object</c> column holds the result without shading factors; the factors of every surface are packed into the lz4-compressed <c>shading_factors</c> column (<see cref="Convert.ToSystem_Bytes(IEnumerable{byte[]})"/>). The table lives on the Storage database, beside <c>building_model</c>.</para>
    /// </summary>
    public class BuildingSolarRadiationPostgreSQLConverter : PostgreSQLConverter<GIS.Classes.BuildingSolarRadiationResult>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingSolarRadiationPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public BuildingSolarRadiationPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with stored building solar radiation.
        /// </summary>
        public static string TableName => Constants.TableName.BuildingSolarRadiation;

        /// <summary>
        /// Asynchronously reads the stored solar radiation of a building model.
        /// <para>The row is looked for under <paramref name="countyId"/> first. When nothing is filed there and <paramref name="fallbackByReference"/> is true, it is looked for under every county part, because a building model id is unique across them and a building of a multi-part county may be filed under a sibling part (<c>Coding - GIS Administrative Data.md</c>). The weather source is not part of the key: the caller compares <see cref="GIS.Classes.BuildingSolarRadiationResult.WeatherSourceType"/> and <see cref="GIS.Classes.BuildingSolarRadiationResult.WeatherSourceId"/> with its own.</para>
        /// <para>A table that does not exist yet answers null, the same as a missing row. So does a row whose packed factors do not unpack to one entry per surface: the next solve then overwrites it.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The county part the building is filed under.</param>
        /// <param name="buildingModelId">The id of the building model.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored (the producer rounds it to 0.1 m).</param>
        /// <param name="solverVersion">The solver version.</param>
        /// <param name="includeShadingFactors">True to read and unpack the shading factors; false to leave every surface's factors null, which skips reading the <c>bytea</c> column.</param>
        /// <param name="fallbackByReference">True to look under every county part when nothing is filed under <paramref name="countyId"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the stored result, or null when nothing is stored for the key.</returns>
        public static async Task<GIS.Classes.BuildingSolarRadiationResult?> GetAsync(
            NpgsqlConnection? npgsqlConnection,
            int countyId,
            long buildingModelId,
            double radius,
            int solverVersion,
            bool includeShadingFactors = false,
            bool fallbackByReference = true,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string columns = includeShadingFactors ? "object, shading_factors" : "object";

            string commandText = $@"
                SELECT {columns}
                FROM {TableName}
                WHERE county_id = @county_id
                  AND building_model_id = @building_model_id
                  AND radius = @radius
                  AND solver_version = @solver_version
                LIMIT 1;";

            GIS.Classes.BuildingSolarRadiationResult? result;
            try
            {
                result = await ReadAsync(commandText, true);
                if (result is null && fallbackByReference)
                {
                    commandText = $@"
                        SELECT {columns}
                        FROM {TableName}
                        WHERE building_model_id = @building_model_id
                          AND radius = @radius
                          AND solver_version = @solver_version
                        ORDER BY created_at DESC
                        LIMIT 1;";

                    result = await ReadAsync(commandText, false);
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }

            return result;

            async Task<GIS.Classes.BuildingSolarRadiationResult?> ReadAsync(string commandText_Read, bool filterByCountyId)
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText_Read, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                if (filterByCountyId)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                }

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("building_model_id", NpgsqlDbType.Bigint) { Value = buildingModelId });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("radius", NpgsqlDbType.Double) { Value = radius });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("solver_version", NpgsqlDbType.Integer) { Value = solverVersion });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                if (!await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                if (JsonNode.Parse(npgsqlDataReader.GetString(0)) is not JsonObject jsonObject)
                {
                    return null;
                }

                GIS.Classes.BuildingSolarRadiationResult buildingSolarRadiationResult = new(jsonObject);
                if (!includeShadingFactors)
                {
                    return buildingSolarRadiationResult;
                }

                List<byte[]>? shadingFactors = Convert.ToSystem_ByteArrays(npgsqlDataReader.GetFieldValue<byte[]>(1));
                if (shadingFactors is null)
                {
                    Console.WriteLine($"Postgres Error ({nameof(GetAsync)}): the shading factors of building model {buildingModelId} do not unpack.");
                    return null;
                }

                GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult_Factors = Create.BuildingSolarRadiationResult(buildingSolarRadiationResult, buildingSolarRadiationResult.CountyId, [.. shadingFactors]);
                if (buildingSolarRadiationResult_Factors is null)
                {
                    Console.WriteLine($"Postgres Error ({nameof(GetAsync)}): building model {buildingModelId} stores {shadingFactors.Count} factor arrays for {buildingSolarRadiationResult.SurfaceSolarRadiations?.Count ?? 0} surfaces.");
                }

                return buildingSolarRadiationResult_Factors;
            }
        }

        /// <summary>
        /// Asynchronously stores the solar radiation of a building, replacing the row stored for the same key.
        /// <para>The key is <c>(county_id, building_model_id, radius, solver_version)</c>. A write for an existing key replaces the summary, the weather members, the user email, the shading factors and <c>created_at</c>; no history is kept. The table and the partition of <paramref name="countyId"/> are created when missing.</para>
        /// <para><paramref name="countyId"/> is written as given, and the stored object is filed under it whatever its own <see cref="GIS.Classes.BuildingSolarRadiationResult.CountyId"/> says. The caller resolves the county part of the building first (<c>Query.CountyIdsByReferencesWithSiblingFallbackAsync</c>), as every other write endpoint does: a county code maps to one row per polygon part (<c>Coding - GIS Administrative Data.md</c>).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The resolved county part the building is filed under.</param>
        /// <param name="buildingSolarRadiationResult">The result to store. Every surface must carry shading factors of one common length.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the id of the stored row, or null when the result has no reference, a non-finite radius, no surfaces, or factors that cannot be packed, or when the table or partition could not be created.</returns>
        public static async Task<long?> UpdateAsync(NpgsqlConnection? npgsqlConnection, int countyId, GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || buildingSolarRadiationResult is null || string.IsNullOrWhiteSpace(buildingSolarRadiationResult.Reference) || !double.IsFinite(buildingSolarRadiationResult.Radius))
            {
                return null;
            }

            List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations = buildingSolarRadiationResult.SurfaceSolarRadiations;
            if (surfaceSolarRadiations is null || surfaceSolarRadiations.Count == 0)
            {
                return null;
            }

            byte[]? shadingFactors = Convert.ToSystem_Bytes(surfaceSolarRadiations.Select(x => x.ShadingFactors));
            if (shadingFactors is null)
            {
                return null;
            }

            string? json = Create.BuildingSolarRadiationResult(buildingSolarRadiationResult, countyId, null)?.ToJsonObject()?.ToJsonString();
            if (json is null)
            {
                return null;
            }

            if (!await Create.TableAsync_BuildingSolarRadiation(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            if (!await Create.TableAsync_BuildingSolarRadiation_Partition(npgsqlConnection, countyId, commandTimeout, cancellationToken))
            {
                return null;
            }

            string commandText = $@"
                INSERT INTO {TableName} (county_id, reference, building_model_id, radius, solver_version, weather_source_type, weather_source_id, weather_period_start, weather_period_end, user_email, object, shading_factors, created_at)
                VALUES (@county_id, @reference, @building_model_id, @radius, @solver_version, @weather_source_type, @weather_source_id, @weather_period_start, @weather_period_end, @user_email, @object, @shading_factors, @created_at)
                ON CONFLICT (county_id, building_model_id, radius, solver_version)
                DO UPDATE SET
                    reference = EXCLUDED.reference,
                    weather_source_type = EXCLUDED.weather_source_type,
                    weather_source_id = EXCLUDED.weather_source_id,
                    weather_period_start = EXCLUDED.weather_period_start,
                    weather_period_end = EXCLUDED.weather_period_end,
                    user_email = EXCLUDED.user_email,
                    object = EXCLUDED.object,
                    shading_factors = EXCLUDED.shading_factors,
                    created_at = EXCLUDED.created_at
                RETURNING id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = buildingSolarRadiationResult.Reference });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("building_model_id", NpgsqlDbType.Bigint) { Value = buildingSolarRadiationResult.BuildingModelId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("radius", NpgsqlDbType.Double) { Value = buildingSolarRadiationResult.Radius });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("solver_version", NpgsqlDbType.Integer) { Value = buildingSolarRadiationResult.SolverVersion });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("weather_source_type", NpgsqlDbType.Integer) { Value = (int)buildingSolarRadiationResult.WeatherSourceType });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("weather_source_id", NpgsqlDbType.Text) { Value = (object?)buildingSolarRadiationResult.WeatherSourceId ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("weather_period_start", NpgsqlDbType.TimestampTz) { Value = (object?)buildingSolarRadiationResult.WeatherPeriodStart?.ToUniversalTime() ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("weather_period_end", NpgsqlDbType.TimestampTz) { Value = (object?)buildingSolarRadiationResult.WeatherPeriodEnd?.ToUniversalTime() ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("user_email", NpgsqlDbType.Text) { Value = (object?)buildingSolarRadiationResult.UserEmail ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb) { Value = json });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("shading_factors", NpgsqlDbType.Bytea) { Value = shadingFactors });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("created_at", NpgsqlDbType.TimestampTz) { Value = (buildingSolarRadiationResult.CreatedAt ?? DateTimeOffset.UtcNow).ToUniversalTime() });

            object? id = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
            return id is long id_Long ? id_Long : null;
        }

        /// <summary>
        /// Asynchronously stores the solar radiation of several buildings of one county part in one statement, replacing the rows stored for the same keys.
        /// <para>The batch form of <see cref="UpdateAsync(NpgsqlConnection?, int, GIS.Classes.BuildingSolarRadiationResult?, int, CancellationToken)"/>, for a batch solve: the same key <c>(county_id, building_model_id, radius, solver_version)</c>, the same replacement and the same packing, but one multi-row upsert instead of one statement per building (<c>Coding - PostgreSQL.md</c> §3). Every row is filed under <paramref name="countyId"/>; the caller groups its results by resolved county part.</para>
        /// <para>A result that the single write would refuse - no reference, a non-finite radius, no surfaces, factors that cannot be packed - is stepped over, not failed. Two results with one key keep the later, because one statement cannot update the same row twice.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The resolved county part every building is filed under.</param>
        /// <param name="buildingSolarRadiationResults">The results to store. Every surface must carry shading factors of one common length.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows written - 0 when nothing in the batch could be stored - or null when the connection or the results are null, or the table or partition could not be created.</returns>
        public static async Task<int?> UpdateAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<GIS.Classes.BuildingSolarRadiationResult?>? buildingSolarRadiationResults, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || buildingSolarRadiationResults is null)
            {
                return null;
            }

            // The rows to write, by key, the later result replacing an earlier one of the same key.
            Dictionary<(long, double, int), (GIS.Classes.BuildingSolarRadiationResult Result, string Json, byte[] ShadingFactors)> rows = [];
            foreach (GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult in buildingSolarRadiationResults)
            {
                if (buildingSolarRadiationResult is null || string.IsNullOrWhiteSpace(buildingSolarRadiationResult.Reference) || !double.IsFinite(buildingSolarRadiationResult.Radius))
                {
                    continue;
                }

                List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations = buildingSolarRadiationResult.SurfaceSolarRadiations;
                if (surfaceSolarRadiations is null || surfaceSolarRadiations.Count == 0)
                {
                    continue;
                }

                byte[]? shadingFactors = Convert.ToSystem_Bytes(surfaceSolarRadiations.Select(x => x.ShadingFactors));
                if (shadingFactors is null)
                {
                    continue;
                }

                string? json = Create.BuildingSolarRadiationResult(buildingSolarRadiationResult, countyId, null)?.ToJsonObject()?.ToJsonString();
                if (json is null)
                {
                    continue;
                }

                rows[(buildingSolarRadiationResult.BuildingModelId, buildingSolarRadiationResult.Radius, buildingSolarRadiationResult.SolverVersion)] = (buildingSolarRadiationResult, json, shadingFactors);
            }

            if (rows.Count == 0)
            {
                return 0;
            }

            if (!await Create.TableAsync_BuildingSolarRadiation(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            if (!await Create.TableAsync_BuildingSolarRadiation_Partition(npgsqlConnection, countyId, commandTimeout, cancellationToken))
            {
                return null;
            }

            await using NpgsqlCommand npgsqlCommand = new() { Connection = npgsqlConnection, CommandTimeout = commandTimeout };
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });

            List<string> values = [];
            int index = 0;
            foreach ((GIS.Classes.BuildingSolarRadiationResult result, string json, byte[] shadingFactors) in rows.Values)
            {
                string suffix = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                values.Add($"(@county_id, @reference_{suffix}, @building_model_id_{suffix}, @radius_{suffix}, @solver_version_{suffix}, @weather_source_type_{suffix}, @weather_source_id_{suffix}, @weather_period_start_{suffix}, @weather_period_end_{suffix}, @user_email_{suffix}, @object_{suffix}, @shading_factors_{suffix}, @created_at_{suffix})");

                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"reference_{suffix}", NpgsqlDbType.Text) { Value = result.Reference! });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"building_model_id_{suffix}", NpgsqlDbType.Bigint) { Value = result.BuildingModelId });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"radius_{suffix}", NpgsqlDbType.Double) { Value = result.Radius });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"solver_version_{suffix}", NpgsqlDbType.Integer) { Value = result.SolverVersion });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"weather_source_type_{suffix}", NpgsqlDbType.Integer) { Value = (int)result.WeatherSourceType });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"weather_source_id_{suffix}", NpgsqlDbType.Text) { Value = (object?)result.WeatherSourceId ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"weather_period_start_{suffix}", NpgsqlDbType.TimestampTz) { Value = (object?)result.WeatherPeriodStart?.ToUniversalTime() ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"weather_period_end_{suffix}", NpgsqlDbType.TimestampTz) { Value = (object?)result.WeatherPeriodEnd?.ToUniversalTime() ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"user_email_{suffix}", NpgsqlDbType.Text) { Value = (object?)result.UserEmail ?? DBNull.Value });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"object_{suffix}", NpgsqlDbType.Jsonb) { Value = json });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"shading_factors_{suffix}", NpgsqlDbType.Bytea) { Value = shadingFactors });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter($"created_at_{suffix}", NpgsqlDbType.TimestampTz) { Value = (result.CreatedAt ?? DateTimeOffset.UtcNow).ToUniversalTime() });

                index++;
            }

            npgsqlCommand.CommandText = $@"
                INSERT INTO {TableName} (county_id, reference, building_model_id, radius, solver_version, weather_source_type, weather_source_id, weather_period_start, weather_period_end, user_email, object, shading_factors, created_at)
                VALUES {string.Join(",\n                       ", values)}
                ON CONFLICT (county_id, building_model_id, radius, solver_version)
                DO UPDATE SET
                    reference = EXCLUDED.reference,
                    weather_source_type = EXCLUDED.weather_source_type,
                    weather_source_id = EXCLUDED.weather_source_id,
                    weather_period_start = EXCLUDED.weather_period_start,
                    weather_period_end = EXCLUDED.weather_period_end,
                    user_email = EXCLUDED.user_email,
                    object = EXCLUDED.object,
                    shading_factors = EXCLUDED.shading_factors,
                    created_at = EXCLUDED.created_at;";

            return await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores the solar radiation of several buildings of one county part in one statement, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The resolved county part every building is filed under.</param>
        /// <param name="buildingSolarRadiationResults">The results to store. Every surface must carry shading factors of one common length.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows written, or null when nothing could be written.</returns>
        public async Task<int?> UpdateAsync(int countyId, IEnumerable<GIS.Classes.BuildingSolarRadiationResult?>? buildingSolarRadiationResults, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, countyId, buildingSolarRadiationResults, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores the solar radiation of buildings of several county parts in one transaction: every result is stored, or none is.
        /// <para>The all-or-nothing form of <see cref="UpdateAsync(NpgsqlConnection?, int, IEnumerable{GIS.Classes.BuildingSolarRadiationResult?}?, int, CancellationToken)"/>, which stores one part in one multi-row upsert with the same key and the same replacement. That form steps over a result it cannot store and keeps the later of two results with one key; here either case rolls the whole transaction back, so a committed count always equals the count given. A batch write endpoint relies on it: a batch that fails leaves nothing of itself behind (DiGi.GIS.WebAPI#93).</para>
        /// <para>Every result is filed under the county part it is listed under, whatever its own <see cref="GIS.Classes.BuildingSolarRadiationResult.CountyId"/> says; the caller resolves the parts first (<c>Query.CountyIdsByReferencesWithSiblingFallbackAsync</c>). The table and the partitions are created before the transaction opens. One part's upsert carries 13 parameters per result and Npgsql allows 65 535 per statement, so a part holds at most 5 041 results.</para>
        /// <para>A database error propagates after the rollback, and so does a requested cancellation.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> used to connect to the database, with no transaction in progress.</param>
        /// <param name="buildingSolarRadiationResults_ByCountyId">The results to store, by the resolved county part each is filed under. Every surface must carry shading factors of one common length.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows written, which is the number of results given (0 when none is given), or null when nothing was written: the connection or the results are null, the table or a partition could not be created, or a result could not be stored.</returns>
        public static async Task<int?> UpdateAsync(NpgsqlConnection? npgsqlConnection, IReadOnlyDictionary<int, List<GIS.Classes.BuildingSolarRadiationResult>>? buildingSolarRadiationResults_ByCountyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || buildingSolarRadiationResults_ByCountyId is null)
            {
                return null;
            }

            int count = 0;
            foreach (List<GIS.Classes.BuildingSolarRadiationResult>? buildingSolarRadiationResults in buildingSolarRadiationResults_ByCountyId.Values)
            {
                count += buildingSolarRadiationResults?.Count ?? 0;
            }

            if (count == 0)
            {
                return 0;
            }

            // Created outside the transaction: the per-part upsert repeats these statements, and they are then no-ops.
            if (!await Create.TableAsync_BuildingSolarRadiation(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            foreach (KeyValuePair<int, List<GIS.Classes.BuildingSolarRadiationResult>> keyValuePair in buildingSolarRadiationResults_ByCountyId)
            {
                if (keyValuePair.Value is null || keyValuePair.Value.Count == 0)
                {
                    continue;
                }

                if (!await Create.TableAsync_BuildingSolarRadiation_Partition(npgsqlConnection, keyValuePair.Key, commandTimeout, cancellationToken))
                {
                    return null;
                }
            }

            // Disposed without a commit, the transaction rolls back - which is what an exception or a cancellation leaves.
            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

            int written = 0;
            foreach (KeyValuePair<int, List<GIS.Classes.BuildingSolarRadiationResult>> keyValuePair in buildingSolarRadiationResults_ByCountyId)
            {
                if (keyValuePair.Value is null || keyValuePair.Value.Count == 0)
                {
                    continue;
                }

                int? written_CountyId = await UpdateAsync(npgsqlConnection, keyValuePair.Key, keyValuePair.Value, commandTimeout, cancellationToken);
                if (written_CountyId is null || written_CountyId.Value != keyValuePair.Value.Count)
                {
                    await npgsqlTransaction.RollbackAsync(cancellationToken);
                    Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "{Type}: county part {CountyId} stored {Written} of {Count} results, so none of the {Total} results was stored", nameof(BuildingSolarRadiationPostgreSQLConverter), keyValuePair.Key, written_CountyId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "-", keyValuePair.Value.Count, count);
                    return null;
                }

                written += written_CountyId.Value;
            }

            await npgsqlTransaction.CommitAsync(cancellationToken);
            return written;
        }

        /// <summary>
        /// Asynchronously stores the solar radiation of buildings of several county parts in one transaction, automatically managing the connection: every result is stored, or none is.
        /// <para>See <see cref="UpdateAsync(NpgsqlConnection?, IReadOnlyDictionary{int, List{GIS.Classes.BuildingSolarRadiationResult}}?, int, CancellationToken)"/>.</para>
        /// </summary>
        /// <param name="buildingSolarRadiationResults_ByCountyId">The results to store, by the resolved county part each is filed under. Every surface must carry shading factors of one common length.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows written, which is the number of results given, or null when nothing was written.</returns>
        public async Task<int?> UpdateAsync(IReadOnlyDictionary<int, List<GIS.Classes.BuildingSolarRadiationResult>>? buildingSolarRadiationResults_ByCountyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, buildingSolarRadiationResults_ByCountyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the stored solar radiation of a building model, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The county part the building is filed under.</param>
        /// <param name="buildingModelId">The id of the building model.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored (the producer rounds it to 0.1 m).</param>
        /// <param name="solverVersion">The solver version.</param>
        /// <param name="includeShadingFactors">True to read and unpack the shading factors; false to leave every surface's factors null.</param>
        /// <param name="fallbackByReference">True to look under every county part when nothing is filed under <paramref name="countyId"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the stored result, or null when nothing is stored for the key.</returns>
        public async Task<GIS.Classes.BuildingSolarRadiationResult?> GetAsync(
            int countyId,
            long buildingModelId,
            double radius,
            int solverVersion,
            bool includeShadingFactors = false,
            bool fallbackByReference = true,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetAsync(npgsqlConnection, countyId, buildingModelId, radius, solverVersion, includeShadingFactors, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores the solar radiation of a building, replacing the row stored for the same key, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The resolved county part the building is filed under.</param>
        /// <param name="buildingSolarRadiationResult">The result to store. Every surface must carry shading factors of one common length.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the id of the stored row, or null when nothing was stored.</returns>
        public async Task<long?> UpdateAsync(int countyId, GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, countyId, buildingSolarRadiationResult, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the references that carry a stored solar radiation in the given county part.
        /// <para>The reference is the building's <c>building_2d</c> reference, the same key <c>building_data</c> is held under. A table that does not exist yet answers empty, the same as a county with no stored result.</para>
        /// <para><paramref name="radius"/> and <paramref name="solverVersion"/> narrow the read to the references stored for that variant and solve - what a batch solve that resumes steps over.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored; null for any radius.</param>
        /// <param name="solverVersion">The solver version; null for any version.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references, empty when the county holds none or the table is missing, or null when the connection is null.</returns>
        public static async Task<HashSet<string>?> GetReferencesAsync(
            NpgsqlConnection? npgsqlConnection,
            int countyId,
            int commandTimeout = 30,
            double? radius = null,
            int? solverVersion = null,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Each filter adds its own clause rather than "@x IS NULL OR …", which defeats the plan (Coding - PostgreSQL.md §3).
            string commandText = $@"
                SELECT DISTINCT reference
                FROM {TableName}
                WHERE county_id = @county_id
                  AND reference IS NOT NULL
                  AND reference <> ''";
            if (radius is not null)
            {
                commandText += @"
                  AND radius = @radius";
            }

            if (solverVersion is not null)
            {
                commandText += @"
                  AND solver_version = @solver_version";
            }

            HashSet<string> references = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                if (radius is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("radius", NpgsqlDbType.Double) { Value = radius.Value });
                }

                if (solverVersion is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("solver_version", NpgsqlDbType.Integer) { Value = solverVersion.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    if (npgsqlDataReader.GetString(0) is string reference && !string.IsNullOrWhiteSpace(reference))
                    {
                        references.Add(reference);
                    }
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return references;
        }

        /// <summary>
        /// Asynchronously reads the latest stored solar radiation of each of the given references in the given county part.
        /// <para>For a reference with several stored results (several solver versions), the one with the highest solver version wins, a tie broken by the newest creation time. The read is scoped to <paramref name="countyId"/>: a reference the county does not hold is simply absent from the result, not answered out of some other county. The shading factors are not read - the summary never needs them.</para>
        /// <para>With <paramref name="radius"/> the read is limited to the results stored for that neighbour radius - one variant, such as the isolated building (radius 0). Without it every radius competes, so once a building carries several variants the one written last wins, building by building (DiGi.GIS.PostgreSQL#107).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="references">The references to read.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored, to read; null to read the latest result of any radius.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result holds the latest result of each reference, one entry per reference, or null when the connection is null or the table is missing.</returns>
        public static async Task<List<GIS.Classes.BuildingSolarRadiationResult>?> GetItemsByReferencesAsync(
            NpgsqlConnection? npgsqlConnection,
            IEnumerable<string>? references,
            int countyId,
            int commandTimeout = 30,
            double? radius = null,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            List<string> references_List = [];
            foreach (string? reference in references)
            {
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    references_List.Add(reference);
                }
            }

            if (references_List.Count == 0)
            {
                return [];
            }

            // Two texts rather than "@radius IS NULL OR radius = @radius", which defeats the plan (Coding - PostgreSQL.md §3).
            string commandText = radius is null
                ? $@"
                SELECT object, reference
                FROM {TableName}
                WHERE county_id = @county_id
                  AND reference = ANY(@references)
                ORDER BY reference, solver_version DESC, created_at DESC"
                : $@"
                SELECT object, reference
                FROM {TableName}
                WHERE county_id = @county_id
                  AND reference = ANY(@references)
                  AND radius = @radius
                ORDER BY reference, solver_version DESC, created_at DESC";

            List<GIS.Classes.BuildingSolarRadiationResult> results = [];
            HashSet<string> seen = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_List.ToArray() });
                if (radius is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("radius", NpgsqlDbType.Double) { Value = radius.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    // The rows are ordered latest-first per reference, so the first one of a reference is the latest; the rest are stepped over.
                    if (npgsqlDataReader.GetString(1) is not string reference || !seen.Add(reference))
                    {
                        continue;
                    }

                    if (JsonNode.Parse(npgsqlDataReader.GetString(0)) is not JsonObject jsonObject)
                    {
                        continue;
                    }

                    results.Add(new GIS.Classes.BuildingSolarRadiationResult(jsonObject));
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }

            return results;
        }

        /// <summary>
        /// Asynchronously reads the references that carry a stored solar radiation in the given county part, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored; null for any radius.</param>
        /// <param name="solverVersion">The solver version; null for any version.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references, empty when the county holds none or the table is missing, or null when the connection is null.</returns>
        public async Task<HashSet<string>?> GetReferencesAsync(int countyId, int commandTimeout = 30, double? radius = null, int? solverVersion = null, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetReferencesAsync(npgsqlConnection, countyId, commandTimeout, radius, solverVersion, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the latest stored solar radiation of each of the given references in the given county part, automatically managing the connection.
        /// </summary>
        /// <param name="references">The references to read.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="radius">The neighbour radius, in m, exactly as stored, to read; null to read the latest result of any radius.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result holds the latest result of each reference, one entry per reference, or null when the connection is null or the table is missing.</returns>
        public async Task<List<GIS.Classes.BuildingSolarRadiationResult>?> GetItemsByReferencesAsync(IEnumerable<string>? references, int countyId, int commandTimeout = 30, double? radius = null, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetItemsByReferencesAsync(npgsqlConnection, references, countyId, commandTimeout, radius, cancellationToken);
        }

        /// <summary>
        /// Asynchronously summarises what each of the specified county parts holds per neighbour radius and solver version: how many results, how many of the part's residential buildings they cover, when they were written and how large the part's partition is (DiGi.GIS.PostgreSQL#138).
        /// <para>Every statement prunes partitions with <c>county_id = ANY(@county_ids)</c>. An unscoped read first lists the parts that hold results and then reads exactly those, so <c>building_data</c> is read only for parts with results (<c>Coding - PostgreSQL.md</c> §3). The partition size is taken from each row's own partition (<c>tableoid</c>), never from a built name.</para>
        /// <para>The residential counts join <c>building_data</c> on <c>(county_id, reference)</c>; both tables are on the Storage database. The residential column is resolved against the building data column list and quoted (§5); when the building data table or the column is missing the counts are null. The summary describes the database <paramref name="npgsqlConnection"/> points at.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the commands.</param>
        /// <param name="countyIds">The identifiers of the county parts to summarise. When null every part holding results is summarised.</param>
        /// <param name="solverVersion">The solver version to summarise; null summarises every version, one row per version.</param>
        /// <param name="buildingDataPostgreSQLConverter">The building data converter whose table and column list the residential counts are read from; null for <see cref="BuildingDataPostgreSQLConverter"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries one <see cref="BuildingSolarRadiationCountyResult"/> per county part, radius and solver version holding results, ordered by county part, solver version and radius; empty when nothing is stored or the table is missing; null when the connection is null.</returns>
        public static async Task<List<BuildingSolarRadiationCountyResult>?> GetSummariesByCountyIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? countyIds, int? solverVersion = null, BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, TableName, commandTimeout, cancellationToken))
            {
                return [];
            }

            List<BuildingSolarRadiationCountyResult> result = [];
            int[] countyIds_Array = [];
            try
            {
                // The parts to read: the given ones, or every part holding results. Either way the statements below run
                // with a literal ANY(@county_ids), never "@x IS NULL OR ..." (Coding - PostgreSQL.md §3).
                if (countyIds is not null)
                {
                    countyIds_Array = [.. countyIds.Distinct()];
                }
                else
                {
                    List<int> countyIds_Stored = [];
                    await using NpgsqlCommand npgsqlCommand_CountyIds = new($"SELECT DISTINCT county_id FROM {TableName} ORDER BY county_id;", npgsqlConnection);
                    npgsqlCommand_CountyIds.CommandTimeout = commandTimeout;
                    await using (NpgsqlDataReader npgsqlDataReader = await npgsqlCommand_CountyIds.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await npgsqlDataReader.ReadAsync(cancellationToken))
                        {
                            countyIds_Stored.Add(npgsqlDataReader.GetInt32(0));
                        }
                    }

                    countyIds_Array = [.. countyIds_Stored];
                }

                if (countyIds_Array.Length == 0)
                {
                    return result;
                }

                string solverVersionClause = solverVersion is null ? string.Empty : " AND solver_version = @solver_version";

                // One row per part, radius and solver version; a part is one partition, so grouping by tableoid splits nothing.
                List<(int CountyId, double Radius, int SolverVersion, long RowCount, DateTimeOffset? CreatedAtMin, DateTimeOffset? CreatedAtMax, long PartitionSize)> rows = [];
                string commandText = $@"
                    SELECT county_id, radius, solver_version, COUNT(*), MIN(created_at), MAX(created_at), pg_total_relation_size(tableoid)
                    FROM {TableName}
                    WHERE county_id = ANY(@county_ids){solverVersionClause}
                    GROUP BY county_id, radius, solver_version, tableoid
                    ORDER BY county_id, solver_version, radius;";

                await using (NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection))
                {
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    AddParameters(npgsqlCommand);

                    await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                    while (await npgsqlDataReader.ReadAsync(cancellationToken))
                    {
                        rows.Add((
                            npgsqlDataReader.GetInt32(0),
                            npgsqlDataReader.GetDouble(1),
                            npgsqlDataReader.GetInt32(2),
                            npgsqlDataReader.GetInt64(3),
                            npgsqlDataReader.IsDBNull(4) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(4),
                            npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(5),
                            npgsqlDataReader.GetInt64(6)));
                    }
                }

                if (rows.Count == 0)
                {
                    return result;
                }

                // The residential column, resolved against the building data column list and quoted (§5).
                buildingDataPostgreSQLConverter ??= new BuildingDataPostgreSQLConverter(null);
                string tableName_BuildingData = buildingDataPostgreSQLConverter.TableName;
                string? uniqueId_IsResidential = Core.IO.Query.UniqueId(IO.Constants.Column.IsResidential);

                bool residential = false;
                if (!string.IsNullOrWhiteSpace(uniqueId_IsResidential) && await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, tableName_BuildingData, commandTimeout, cancellationToken))
                {
                    List<Core.IO.Table.Classes.Column>? columns = await buildingDataPostgreSQLConverter.GetColumnsByUniqueIdsAsync(npgsqlConnection, [uniqueId_IsResidential!], commandTimeout, cancellationToken);
                    residential = columns is not null && columns.Exists(x => x is not null && Core.IO.Query.UniqueId(x) == uniqueId_IsResidential);
                }

                Dictionary<int, long> residentialBuildingCounts = [];
                Dictionary<(int, double, int), long> residentialCoveredCounts = [];
                if (residential)
                {
                    NpgsqlCommandBuilder npgsqlCommandBuilder = new();
                    string tableName_BuildingData_Quoted = npgsqlCommandBuilder.QuoteIdentifier(tableName_BuildingData);
                    string columnName_IsResidential_Quoted = npgsqlCommandBuilder.QuoteIdentifier(uniqueId_IsResidential!);

                    string commandText_Residential = $@"
                        SELECT county_id, COUNT(*)
                        FROM {tableName_BuildingData_Quoted}
                        WHERE county_id = ANY(@county_ids)
                          AND {columnName_IsResidential_Quoted} IS TRUE
                        GROUP BY county_id;";

                    await using (NpgsqlCommand npgsqlCommand = new(commandText_Residential, npgsqlConnection))
                    {
                        npgsqlCommand.CommandTimeout = commandTimeout;
                        AddParameters(npgsqlCommand);

                        await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                        while (await npgsqlDataReader.ReadAsync(cancellationToken))
                        {
                            residentialBuildingCounts[npgsqlDataReader.GetInt32(0)] = npgsqlDataReader.GetInt64(1);
                        }
                    }

                    string commandText_Covered = $@"
                        SELECT s.county_id, s.radius, s.solver_version, COUNT(DISTINCT s.reference)
                        FROM {TableName} s
                        JOIN {tableName_BuildingData_Quoted} b ON b.county_id = s.county_id AND b.reference = s.reference
                        WHERE s.county_id = ANY(@county_ids)
                          AND b.county_id = ANY(@county_ids)
                          AND b.{columnName_IsResidential_Quoted} IS TRUE{(solverVersion is null ? string.Empty : " AND s.solver_version = @solver_version")}
                        GROUP BY s.county_id, s.radius, s.solver_version;";

                    await using (NpgsqlCommand npgsqlCommand = new(commandText_Covered, npgsqlConnection))
                    {
                        npgsqlCommand.CommandTimeout = commandTimeout;
                        AddParameters(npgsqlCommand);

                        await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                        while (await npgsqlDataReader.ReadAsync(cancellationToken))
                        {
                            residentialCoveredCounts[(npgsqlDataReader.GetInt32(0), npgsqlDataReader.GetDouble(1), npgsqlDataReader.GetInt32(2))] = npgsqlDataReader.GetInt64(3);
                        }
                    }
                }

                foreach ((int countyId, double radius, int solverVersion_Row, long rowCount, DateTimeOffset? createdAtMin, DateTimeOffset? createdAtMax, long partitionSize) in rows)
                {
                    long? residentialBuildingCount = residential ? (residentialBuildingCounts.TryGetValue(countyId, out long count_Building) ? count_Building : 0) : null;
                    long? residentialCoveredCount = residential ? (residentialCoveredCounts.TryGetValue((countyId, radius, solverVersion_Row), out long count_Covered) ? count_Covered : 0) : null;

                    result.Add(new BuildingSolarRadiationCountyResult(countyId, radius, solverVersion_Row, rowCount, residentialBuildingCount, residentialCoveredCount, createdAtMin, createdAtMax, partitionSize));
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;

            void AddParameters(NpgsqlCommand npgsqlCommand_Parameters)
            {
                npgsqlCommand_Parameters.Parameters.Add(new NpgsqlParameter("county_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds_Array });
                if (solverVersion is not null)
                {
                    npgsqlCommand_Parameters.Parameters.Add(new NpgsqlParameter("solver_version", NpgsqlDbType.Integer) { Value = solverVersion.Value });
                }
            }
        }

        /// <summary>
        /// Asynchronously summarises what each of the specified county parts holds per neighbour radius and solver version, automatically managing the connection.
        /// <para>The residential counts are read from <see cref="BuildingDataPostgreSQLConverter"/> on the same database (Storage).</para>
        /// </summary>
        /// <param name="countyIds">The identifiers of the county parts to summarise. When null every part holding results is summarised.</param>
        /// <param name="solverVersion">The solver version to summarise; null summarises every version, one row per version.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result carries one <see cref="BuildingSolarRadiationCountyResult"/> per county part, radius and solver version holding results; empty when nothing is stored or the table is missing; null when no connection could be opened.</returns>
        public async Task<List<BuildingSolarRadiationCountyResult>?> GetSummariesByCountyIdsAsync(IEnumerable<int>? countyIds, int? solverVersion = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetSummariesByCountyIdsAsync(npgsqlConnection, countyIds, solverVersion, buildingDataPostgreSQLConverter: null, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }
    }
}
