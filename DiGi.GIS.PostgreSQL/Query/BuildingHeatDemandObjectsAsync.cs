using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Asynchronously reads every stored heat demand of each of the given references - every method, method version and input variant - as the JSON objects of the stored <c>BuildingHeatDemandResult</c>s, for comparing them (DiGi-Organization/DiGi.GIS.WebAPI#104, Heat Demand Method Identity DiGi-Organization/DiGi.GIS.Analytical#19).
        /// <para>Plain SQL over <see cref="Constants.TableName.BuildingHeatDemand"/>, returning JSON rather than results, so a host that serves it needs no energy engine - the API host, like <see cref="BuildingHeatDemandCountyResultsAsync(NpgsqlConnection?, IEnumerable{int}?, int?, IEnumerable{short}?, int, int, int, CancellationToken)"/>. <c>DiGi.GIS.PostgreSQL.Energy.Classes.BuildingHeatDemandPostgreSQLConverter.GetAllItemsByReferencesAsync</c> is this read made into results.</para>
        /// <para>Each object is labelled by its row's key: the <c>method</c>, <c>method_version</c> and <c>input_variant</c> columns are written into it as <c>HeatDemandResult.HeatDemandMethod</c>, <c>HeatDemandResult.MethodVersion</c> and <c>HeatDemandInputVariant</c> (integers), so the label is the key the row is stored under even where the stored JSON predates the method and the input variant. An object without a nested <c>HeatDemandResult</c> keeps only the input variant. The objects come ordered by reference, method, method version and input variant.</para>
        /// <para>The read is scoped to <paramref name="countyId"/> first; with <paramref name="fallbackByReference"/>, a reference with no row there is looked for under every county part, in a separate statement (<c>Coding - GIS Administrative Data.md</c>, <c>Coding - PostgreSQL.md</c> §3). References are read 1000 at a time.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> of the Storage database. This value can be null.</param>
        /// <param name="references">The references to read. This value can be null.</param>
        /// <param name="countyId">The county part to scope the read.</param>
        /// <param name="fallbackByReference">True to look for a reference not found under <paramref name="countyId"/> under every county part.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds the JSON object of every stored result of the references found, or null when the connection or the references are null or the table is missing.</returns>
        public static async Task<List<JsonObject>?> BuildingHeatDemandObjectsAsync(this NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, int countyId, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
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

            // The key columns written into the object, so the label is always the key the row is stored under. jsonb_set adds a
            // missing last key but never a missing parent, so an object without a nested HeatDemandResult keeps only the input variant.
            const string objectExpression = @"jsonb_set(jsonb_set(jsonb_set(object, '{HeatDemandInputVariant}', to_jsonb(input_variant)), '{HeatDemandResult,HeatDemandMethod}', to_jsonb(method)), '{HeatDemandResult,MethodVersion}', to_jsonb(method_version))";

            string commandText_Scoped = $@"
                SELECT {objectExpression}, reference
                FROM {Constants.TableName.BuildingHeatDemand}
                WHERE county_id = @county_id
                  AND reference = ANY(@references)
                ORDER BY reference, method, method_version, input_variant, created_at;";
            string commandText_Unscoped = $@"
                SELECT {objectExpression}, reference
                FROM {Constants.TableName.BuildingHeatDemand}
                WHERE reference = ANY(@references)
                ORDER BY reference, method, method_version, input_variant, created_at;";

            List<JsonObject> result = [];
            HashSet<string> references_Found = [];
            try
            {
                for (int offset = 0; offset < references_List.Count; offset += 1000)
                {
                    List<string> references_Batch = references_List.GetRange(offset, Math.Min(1000, references_List.Count - offset));
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
                    result.Add(jsonObject);
                }
            }
        }
    }
}
