using DiGi.GIS.Analytical.Energy.Classes;
using DiGi.GIS.Analytical.Energy.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Energy.Classes
{
    /// <summary>
    /// Stores and reads <see cref="BuildingHeatDemandResult"/> objects in the <see cref="Constants.TableName.BuildingHeatDemand"/> table: one row per building model, method, method version and input variant (DiGi-Organization/DiGi.GIS.PostgreSQL#152), partitioned by county part, on the Storage database beside <c>building_solar_radiation</c> (DiGi.GIS.PostgreSQL#143).
    /// <para>The converter lives in <c>DiGi.GIS.PostgreSQL.Energy</c>, which <c>DiGi.GIS.PostgreSQL</c> cannot reference, so <c>Create.GISPostgreSQLConverterManager()</c> does not register it: <see cref="Modify.AddBuildingHeatDemandPostgreSQLConverter(PostgreSQL.Classes.GISPostgreSQLConverterManager?)"/> does, on the Storage connection.</para>
    /// </summary>
    public class BuildingHeatDemandPostgreSQLConverter : PostgreSQLConverter<BuildingHeatDemandResult>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// The number of references or results one statement carries.
        /// </summary>
        private const int batchSize = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingHeatDemandPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> of the Storage database. This value can be null.</param>
        public BuildingHeatDemandPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with stored building heat demand.
        /// </summary>
        public static string TableName => Constants.TableName.BuildingHeatDemand;

        /// <summary>
        /// Asynchronously stores the heat demand of several buildings of one county part, replacing the rows stored for the same keys.
        /// <para>The key is <c>(county_id, building_model_id, method, method_version, input_variant)</c> - the method and the input variant are the integers of the result's <c>HeatDemandResult.HeatDemandMethod</c> and <see cref="BuildingHeatDemandResult.HeatDemandInputVariant"/>, so results of another method or input variant are stored beside, never over, each other; a write for an existing key replaces every other column. The results are written 1000 at a time, one statement per batch, inside one transaction: a batch that fails rolls the whole write back. A result without a reference is stepped over; of two results with one key, the last wins. <paramref name="countyId"/> is written as given whatever the result's own county says. The table and the partition of <paramref name="countyId"/> are created when missing.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> of the Storage database. This value can be null.</param>
        /// <param name="countyId">The county part the buildings are filed under.</param>
        /// <param name="buildingHeatDemandResults">The results to store. This value can be null.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the number of rows written, 0 when there was nothing to write, or null when the connection is null or the table or partition could not be created.</returns>
        public static async Task<int?> UpdateAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<BuildingHeatDemandResult?>? buildingHeatDemandResults, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || buildingHeatDemandResults is null)
            {
                return null;
            }

            // One row per key, the last result winning, each with its JSON.
            Dictionary<(long, short, int, short), (BuildingHeatDemandResult Result, string Json)> rows = [];
            foreach (BuildingHeatDemandResult? buildingHeatDemandResult in buildingHeatDemandResults)
            {
                if (buildingHeatDemandResult is null || string.IsNullOrWhiteSpace(buildingHeatDemandResult.Reference) || buildingHeatDemandResult.ToJsonObject()?.ToJsonString() is not string json)
                {
                    continue;
                }

                rows[(buildingHeatDemandResult.BuildingModelId, Method(buildingHeatDemandResult), buildingHeatDemandResult.HeatDemandResult?.MethodVersion ?? 0, (short)buildingHeatDemandResult.HeatDemandInputVariant)] = (buildingHeatDemandResult, json);
            }

            if (rows.Count == 0)
            {
                return 0;
            }

            if (!await Create.TableAsync_BuildingHeatDemand(npgsqlConnection, commandTimeout, cancellationToken) || !await Create.TableAsync_BuildingHeatDemand_Partition(npgsqlConnection, countyId, commandTimeout, cancellationToken))
            {
                return null;
            }

            List<(BuildingHeatDemandResult Result, string Json)> rows_List = [.. rows.Values];

            // Disposed without a commit, the transaction rolls back - which is what an exception or a cancellation leaves.
            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);

            int result = 0;
            for (int offset = 0; offset < rows_List.Count; offset += batchSize)
            {
                List<(BuildingHeatDemandResult Result, string Json)> rows_Batch = rows_List.GetRange(offset, Math.Min(batchSize, rows_List.Count - offset));

                await using NpgsqlCommand npgsqlCommand = new() { Connection = npgsqlConnection, Transaction = npgsqlTransaction, CommandTimeout = commandTimeout };
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });

                List<string> values = [];
                for (int i = 0; i < rows_Batch.Count; i++)
                {
                    (BuildingHeatDemandResult buildingHeatDemandResult, string json) = rows_Batch[i];
                    DiGi.Analytical.Building.Energy.Classes.HeatDemandResult? heatDemandResult = buildingHeatDemandResult.HeatDemandResult;

                    string suffix = i.ToString(CultureInfo.InvariantCulture);
                    values.Add($"(@county_id, @building_model_id_{suffix}, @reference_{suffix}, @method_{suffix}, @method_version_{suffix}, @input_variant_{suffix}, @weather_source_type_{suffix}, @weather_source_id_{suffix}, @solar_source_{suffix}, @solar_solver_version_{suffix}, @solar_radius_{suffix}, @construction_flavour_{suffix}, @construction_library_version_{suffix}, @heating_energy_need_{suffix}, @heating_energy_need_per_area_{suffix}, @design_heat_load_{suffix}, @object_{suffix}, @created_at_{suffix})");

                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"building_model_id_{suffix}", NpgsqlDbType.Bigint) { Value = buildingHeatDemandResult.BuildingModelId });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"reference_{suffix}", NpgsqlDbType.Text) { Value = buildingHeatDemandResult.Reference! });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"method_{suffix}", NpgsqlDbType.Smallint) { Value = Method(buildingHeatDemandResult) });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"method_version_{suffix}", NpgsqlDbType.Integer) { Value = heatDemandResult?.MethodVersion ?? 0 });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"input_variant_{suffix}", NpgsqlDbType.Smallint) { Value = (short)buildingHeatDemandResult.HeatDemandInputVariant });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"weather_source_type_{suffix}", NpgsqlDbType.Integer) { Value = (int)buildingHeatDemandResult.WeatherSourceType });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"weather_source_id_{suffix}", NpgsqlDbType.Text) { Value = (object?)buildingHeatDemandResult.WeatherSourceId ?? DBNull.Value });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"solar_source_{suffix}", NpgsqlDbType.Integer) { Value = (int)buildingHeatDemandResult.SolarSource });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"solar_solver_version_{suffix}", NpgsqlDbType.Integer) { Value = (object?)buildingHeatDemandResult.SolarSolverVersion ?? DBNull.Value });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"solar_radius_{suffix}", NpgsqlDbType.Double) { Value = (object?)buildingHeatDemandResult.SolarRadius ?? DBNull.Value });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"construction_flavour_{suffix}", NpgsqlDbType.Integer) { Value = (int)buildingHeatDemandResult.ConstructionFlavour });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"construction_library_version_{suffix}", NpgsqlDbType.Text) { Value = (object?)buildingHeatDemandResult.ConstructionLibraryVersion ?? DBNull.Value });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"heating_energy_need_{suffix}", NpgsqlDbType.Double) { Value = Finite(heatDemandResult?.HeatingEnergyNeed) });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"heating_energy_need_per_area_{suffix}", NpgsqlDbType.Double) { Value = Finite(heatDemandResult?.HeatingEnergyNeedPerArea) });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"design_heat_load_{suffix}", NpgsqlDbType.Double) { Value = Finite(heatDemandResult?.DesignHeatLoad) });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"object_{suffix}", NpgsqlDbType.Jsonb) { Value = json });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter($"created_at_{suffix}", NpgsqlDbType.TimestampTz) { Value = (buildingHeatDemandResult.CreatedAt ?? DateTimeOffset.UtcNow).ToUniversalTime() });
                }

                npgsqlCommand.CommandText = $@"
                    INSERT INTO {TableName} (county_id, building_model_id, reference, method, method_version, input_variant, weather_source_type, weather_source_id, solar_source, solar_solver_version, solar_radius, construction_flavour, construction_library_version, heating_energy_need, heating_energy_need_per_area, design_heat_load, object, created_at)
                    VALUES {string.Join(",\n                           ", values)}
                    ON CONFLICT (county_id, building_model_id, method, method_version, input_variant)
                    DO UPDATE SET
                        reference = EXCLUDED.reference,
                        weather_source_type = EXCLUDED.weather_source_type,
                        weather_source_id = EXCLUDED.weather_source_id,
                        solar_source = EXCLUDED.solar_source,
                        solar_solver_version = EXCLUDED.solar_solver_version,
                        solar_radius = EXCLUDED.solar_radius,
                        construction_flavour = EXCLUDED.construction_flavour,
                        construction_library_version = EXCLUDED.construction_library_version,
                        heating_energy_need = EXCLUDED.heating_energy_need,
                        heating_energy_need_per_area = EXCLUDED.heating_energy_need_per_area,
                        design_heat_load = EXCLUDED.design_heat_load,
                        object = EXCLUDED.object,
                        created_at = EXCLUDED.created_at;";

                result += await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await npgsqlTransaction.CommitAsync(cancellationToken);
            return result;

            static object Finite(double? value)
            {
                return value is double value_Double && !double.IsNaN(value_Double) && !double.IsInfinity(value_Double) ? value_Double : DBNull.Value;
            }

            // A result without a heat demand has no method of its own; it is filed under Undefined (0), never under a real method.
            static short Method(BuildingHeatDemandResult buildingHeatDemandResult)
            {
                return (short)(buildingHeatDemandResult.HeatDemandResult?.HeatDemandMethod ?? DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod.Undefined);
            }
        }

        /// <summary>
        /// Asynchronously stores the heat demand of several buildings of one county part, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The county part the buildings are filed under.</param>
        /// <param name="buildingHeatDemandResults">The results to store. This value can be null.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the number of rows written, or null when nothing could be written.</returns>
        public async Task<int?> UpdateAsync(int countyId, IEnumerable<BuildingHeatDemandResult?>? buildingHeatDemandResults, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpdateAsync(npgsqlConnection, countyId, buildingHeatDemandResults, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the stored heat demand of each of the given references, of one method and one input variant.
        /// <para>Results of another method or input variant are never returned, whatever their version: only <see cref="GetAllItemsByReferencesAsync(NpgsqlConnection?, IEnumerable{string}?, int, bool, int, CancellationToken)"/> mixes them (DiGi-Organization/DiGi.GIS.PostgreSQL#152).</para>
        /// <para>The read is scoped to <paramref name="countyId"/> first. With <paramref name="fallbackByReference"/>, a reference not found there is looked for under every county part, because a building of a multi-part county may be filed under a sibling part (<c>Coding - GIS Administrative Data.md</c>); the scoped and the unscoped read are separate statements, so the scoped one prunes to its partition. Of several results of one reference the highest method version wins, a tie broken by the newest. References are read 1000 at a time.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> of the Storage database. This value can be null.</param>
        /// <param name="references">The references to read. This value can be null.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="methodVersion">The method version to read, within <paramref name="heatDemandMethod"/>; null for the highest stored.</param>
        /// <param name="fallbackByReference">True to look for a reference not found under <paramref name="countyId"/> under every county part.</param>
        /// <param name="heatDemandMethod">The method to read.</param>
        /// <param name="heatDemandInputVariant">The input variant to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds one result per reference found, or null when the connection is null or the table is missing.</returns>
        public static async Task<List<BuildingHeatDemandResult>?> GetItemsByReferencesAsync(
            NpgsqlConnection? npgsqlConnection,
            IEnumerable<string>? references,
            int countyId,
            int? methodVersion = null,
            bool fallbackByReference = false,
            DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod heatDemandMethod = DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod.Monthly,
            HeatDemandInputVariant heatDemandInputVariant = HeatDemandInputVariant.Generated,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            List<string> references_List = [];
            HashSet<string> references_Unique = [];
            foreach (string? reference in references)
            {
                if (!string.IsNullOrWhiteSpace(reference) && references_Unique.Add(reference!))
                {
                    references_List.Add(reference!);
                }
            }

            string methodVersionClause = methodVersion is null ? string.Empty : " AND method_version = @method_version";
            string commandText_Scoped = $@"
                SELECT object, reference
                FROM {TableName}
                WHERE county_id = @county_id
                  AND reference = ANY(@references)
                  AND method = @method
                  AND input_variant = @input_variant{methodVersionClause}
                ORDER BY reference, method_version DESC, created_at DESC;";
            string commandText_Unscoped = $@"
                SELECT object, reference
                FROM {TableName}
                WHERE reference = ANY(@references)
                  AND method = @method
                  AND input_variant = @input_variant{methodVersionClause}
                ORDER BY reference, method_version DESC, created_at DESC;";

            List<BuildingHeatDemandResult> result = [];
            HashSet<string> references_Found = [];
            try
            {
                for (int offset = 0; offset < references_List.Count; offset += batchSize)
                {
                    List<string> references_Batch = references_List.GetRange(offset, Math.Min(batchSize, references_List.Count - offset));
                    await ReadAsync(commandText_Scoped, references_Batch, true);

                    if (fallbackByReference)
                    {
                        List<string> references_Missing = references_Batch.FindAll(x => !references_Found.Contains(x));
                        if (references_Missing.Count != 0)
                        {
                            await ReadAsync(commandText_Unscoped, references_Missing, false);
                        }
                    }
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }

            return result;

            async Task ReadAsync(string commandText, List<string> references_Read, bool scoped)
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection) { CommandTimeout = commandTimeout };
                if (scoped)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                }

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Read.ToArray() });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("method", NpgsqlDbType.Smallint) { Value = (short)heatDemandMethod });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("input_variant", NpgsqlDbType.Smallint) { Value = (short)heatDemandInputVariant });
                if (methodVersion is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("method_version", NpgsqlDbType.Integer) { Value = methodVersion.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    // Ordered best-first per reference, so the first row of a reference is the one returned.
                    string reference = npgsqlDataReader.GetString(1);
                    if (references_Found.Contains(reference) || JsonNode.Parse(npgsqlDataReader.GetString(0)) is not JsonObject jsonObject)
                    {
                        continue;
                    }

                    references_Found.Add(reference);
                    result.Add(new BuildingHeatDemandResult(jsonObject));
                }
            }
        }

        /// <summary>
        /// Asynchronously reads the stored heat demand of each of the given references, automatically managing the connection.
        /// </summary>
        /// <param name="references">The references to read. This value can be null.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="methodVersion">The method version to read, within <paramref name="heatDemandMethod"/>; null for the highest stored.</param>
        /// <param name="fallbackByReference">True to look for a reference not found under <paramref name="countyId"/> under every county part.</param>
        /// <param name="heatDemandMethod">The method to read.</param>
        /// <param name="heatDemandInputVariant">The input variant to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds one result per reference found, or null when the connection cannot be made or the table is missing.</returns>
        public async Task<List<BuildingHeatDemandResult>?> GetItemsByReferencesAsync(
            IEnumerable<string>? references,
            int countyId,
            int? methodVersion = null,
            bool fallbackByReference = false,
            DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod heatDemandMethod = DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod.Monthly,
            HeatDemandInputVariant heatDemandInputVariant = HeatDemandInputVariant.Generated,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetItemsByReferencesAsync(npgsqlConnection, references, countyId, methodVersion, fallbackByReference, heatDemandMethod, heatDemandInputVariant, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads every stored heat demand of each of the given references - every method, method version and input variant - for comparing them (DiGi-Organization/DiGi.GIS.PostgreSQL#152, Heat Demand Method Identity DiGi-Organization/DiGi.GIS.Analytical#19).
        /// <para>The only read that mixes methods and input variants. Each result is labelled by its own <c>HeatDemandResult.HeatDemandMethod</c>, <c>HeatDemandResult.MethodVersion</c> and <see cref="BuildingHeatDemandResult.HeatDemandInputVariant"/>, which the key columns repeat; a result stored before the two were recorded reads as <c>Monthly</c> and <c>Generated</c>, as its backfilled columns say. The results come ordered by reference, method, method version and input variant.</para>
        /// <para>The read is scoped to <paramref name="countyId"/> first; with <paramref name="fallbackByReference"/>, a reference with no row there is looked for under every county part, in a separate statement (<c>Coding - GIS Administrative Data.md</c>). References are read 1000 at a time.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> of the Storage database. This value can be null.</param>
        /// <param name="references">The references to read. This value can be null.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="fallbackByReference">True to look for a reference not found under <paramref name="countyId"/> under every county part.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds every stored result of the references found, or null when the connection is null or the table is missing.</returns>
        public static async Task<List<BuildingHeatDemandResult>?> GetAllItemsByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

            List<string> references_List = [];
            HashSet<string> references_Unique = [];
            foreach (string? reference in references)
            {
                if (!string.IsNullOrWhiteSpace(reference) && references_Unique.Add(reference!))
                {
                    references_List.Add(reference!);
                }
            }

            string commandText_Scoped = $@"
                SELECT object, reference
                FROM {TableName}
                WHERE county_id = @county_id
                  AND reference = ANY(@references)
                ORDER BY reference, method, method_version, input_variant, created_at;";
            string commandText_Unscoped = $@"
                SELECT object, reference
                FROM {TableName}
                WHERE reference = ANY(@references)
                ORDER BY reference, method, method_version, input_variant, created_at;";

            List<BuildingHeatDemandResult> result = [];
            HashSet<string> references_Found = [];
            try
            {
                for (int offset = 0; offset < references_List.Count; offset += batchSize)
                {
                    List<string> references_Batch = references_List.GetRange(offset, Math.Min(batchSize, references_List.Count - offset));
                    await ReadAsync(commandText_Scoped, references_Batch, true);

                    if (fallbackByReference)
                    {
                        List<string> references_Missing = references_Batch.FindAll(x => !references_Found.Contains(x));
                        if (references_Missing.Count != 0)
                        {
                            await ReadAsync(commandText_Unscoped, references_Missing, false);
                        }
                    }
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }

            return result;

            async Task ReadAsync(string commandText, List<string> references_Read, bool scoped)
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection) { CommandTimeout = commandTimeout };
                if (scoped)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                }

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Read.ToArray() });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    if (JsonNode.Parse(npgsqlDataReader.GetString(0)) is not JsonObject jsonObject)
                    {
                        continue;
                    }

                    references_Found.Add(npgsqlDataReader.GetString(1));
                    result.Add(new BuildingHeatDemandResult(jsonObject));
                }
            }
        }

        /// <summary>
        /// Asynchronously reads every stored heat demand of each of the given references - every method, method version and input variant - automatically managing the connection.
        /// </summary>
        /// <param name="references">The references to read. This value can be null.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="fallbackByReference">True to look for a reference not found under <paramref name="countyId"/> under every county part.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds every stored result of the references found, or null when the connection cannot be made or the table is missing.</returns>
        public async Task<List<BuildingHeatDemandResult>?> GetAllItemsByReferencesAsync(IEnumerable<string>? references, int countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetAllItemsByReferencesAsync(npgsqlConnection, references, countyId, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the references that carry a stored heat demand of one method and one input variant in the given county part - what a resumed run steps over. A result of another method or input variant does not count (DiGi-Organization/DiGi.GIS.PostgreSQL#152).
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> of the Storage database. This value can be null.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="methodVersion">The method version, within <paramref name="heatDemandMethod"/>; null for any version.</param>
        /// <param name="heatDemandMethod">The method.</param>
        /// <param name="heatDemandInputVariant">The input variant.</param>
        /// <param name="commandTimeout">The timeout in seconds for the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds the references; empty when the table is missing, or null when the connection is null.</returns>
        public static async Task<HashSet<string>?> GetReferencesAsync(NpgsqlConnection? npgsqlConnection, int countyId, int? methodVersion = null, DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod heatDemandMethod = DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod.Monthly, HeatDemandInputVariant heatDemandInputVariant = HeatDemandInputVariant.Generated, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Two texts rather than "@method_version IS NULL OR …" (Coding - PostgreSQL.md §3).
            string commandText = methodVersion is null
                ? $"SELECT DISTINCT reference FROM {TableName} WHERE county_id = @county_id AND method = @method AND input_variant = @input_variant;"
                : $"SELECT DISTINCT reference FROM {TableName} WHERE county_id = @county_id AND method = @method AND input_variant = @input_variant AND method_version = @method_version;";

            HashSet<string> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection) { CommandTimeout = commandTimeout };
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = countyId });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("method", NpgsqlDbType.Smallint) { Value = (short)heatDemandMethod });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("input_variant", NpgsqlDbType.Smallint) { Value = (short)heatDemandInputVariant });
                if (methodVersion is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("method_version", NpgsqlDbType.Integer) { Value = methodVersion.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    result.Add(npgsqlDataReader.GetString(0));
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads the references that carry a stored heat demand in the given county part, automatically managing the connection.
        /// </summary>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="methodVersion">The method version, within <paramref name="heatDemandMethod"/>; null for any version.</param>
        /// <param name="heatDemandMethod">The method.</param>
        /// <param name="heatDemandInputVariant">The input variant.</param>
        /// <param name="commandTimeout">The timeout in seconds for the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds the references; empty when the table is missing, or null when the connection cannot be made.</returns>
        public async Task<HashSet<string>?> GetReferencesAsync(int countyId, int? methodVersion = null, DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod heatDemandMethod = DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod.Monthly, HeatDemandInputVariant heatDemandInputVariant = HeatDemandInputVariant.Generated, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetReferencesAsync(npgsqlConnection, countyId, methodVersion, heatDemandMethod, heatDemandInputVariant, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously summarises the stored heat demand of the given county parts, of one method and one input variant, one row per county part, method version, building type and year-built band: the buildings, shaded and unshaded, the quality counts of their calculation, their energy need - total, mean and quantiles per area - and design heat load, and when they were written.
        /// <para>Delegates to <see cref="PostgreSQL.Query.BuildingHeatDemandCountyResultsAsync(NpgsqlConnection?, IEnumerable{int}?, int?, IEnumerable{short}?, int, int, int, CancellationToken)"/> of the main assembly, which reads no engine type, so the API host can serve the same summary without this assembly (DiGi.GIS.PostgreSQL#146).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> of the Storage database. This value can be null.</param>
        /// <param name="countyIds">The county parts to summarise; null for every part that holds results.</param>
        /// <param name="methodVersion">The method version to summarise, within <paramref name="heatDemandMethod"/>; null for every version, one row each.</param>
        /// <param name="yearBuiltBounds">The bounds of the year-built bands; null or empty for one band of every year.</param>
        /// <param name="heatDemandMethod">The method to summarise; the summary never mixes methods.</param>
        /// <param name="heatDemandInputVariant">The input variant to summarise; the summary never mixes input variants.</param>
        /// <param name="commandTimeout">The timeout in seconds for the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds the summaries; empty when the table is missing, or null when the connection is null.</returns>
        public static Task<List<BuildingHeatDemandCountyResult>?> GetSummariesByCountyIdsAsync(
            NpgsqlConnection? npgsqlConnection,
            IEnumerable<int>? countyIds,
            int? methodVersion = null,
            IEnumerable<short>? yearBuiltBounds = null,
            DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod heatDemandMethod = DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod.Monthly,
            HeatDemandInputVariant heatDemandInputVariant = HeatDemandInputVariant.Generated,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            return PostgreSQL.Query.BuildingHeatDemandCountyResultsAsync(npgsqlConnection, countyIds, methodVersion, yearBuiltBounds, (int)heatDemandMethod, (int)heatDemandInputVariant, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously summarises the stored heat demand of the given county parts, one row per county part, method version, building type and year-built band, automatically managing the connection.
        /// </summary>
        /// <param name="countyIds">The county parts to summarise; null for every part that holds results.</param>
        /// <param name="methodVersion">The method version to summarise, within <paramref name="heatDemandMethod"/>; null for every version, one row each.</param>
        /// <param name="yearBuiltBounds">The bounds of the year-built bands; null or empty for one band of every year.</param>
        /// <param name="heatDemandMethod">The method to summarise; the summary never mixes methods.</param>
        /// <param name="heatDemandInputVariant">The input variant to summarise; the summary never mixes input variants.</param>
        /// <param name="commandTimeout">The timeout in seconds for the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds the summaries, or null when the connection cannot be made.</returns>
        public async Task<List<BuildingHeatDemandCountyResult>?> GetSummariesByCountyIdsAsync(
            IEnumerable<int>? countyIds,
            int? methodVersion = null,
            IEnumerable<short>? yearBuiltBounds = null,
            DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod heatDemandMethod = DiGi.Analytical.Building.Energy.Enums.HeatDemandMethod.Monthly,
            HeatDemandInputVariant heatDemandInputVariant = HeatDemandInputVariant.Generated,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetSummariesByCountyIdsAsync(npgsqlConnection, countyIds, methodVersion, yearBuiltBounds, heatDemandMethod, heatDemandInputVariant, commandTimeout, cancellationToken);
        }
    }
}
