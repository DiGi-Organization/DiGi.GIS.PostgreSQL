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
    }
}
