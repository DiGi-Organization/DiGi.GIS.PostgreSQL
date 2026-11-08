using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Recomputes the three derived year built columns of <c>building_data</c> - predicted, user and calculated - from the stored year built history of explicit county parts, writing NULL where the history no longer holds a value.
        /// <para>The columns are derived, so after a prediction run or a user entry is removed they are stale until something derives them again. The building data run cannot clear them: it emits no row for a building with no value left. This names every building in scope through <c>IO.Modify.Update_Building2D_YearBuilt</c>'s references parameter instead, so a building whose history is now empty is written with three unset cells and <c>PushAsync</c> stores NULL in each.</para>
        /// <para>The buildings in scope are <paramref name="references"/>, or, when it is null, every building holding a <c>year_built_data</c> row or a <c>building_data</c> row under the parts. The history is read from the main database and the rows from the storage one - no join crosses them. Each building is written under the part its <c>building_data</c> row is filed under; one with no such row is resolved through <c>building_2d</c> and written only when its history holds a value, so a recompute never adds empty rows.</para>
        /// </summary>
        /// <param name="buildingDataPostgreSQLConverter">The converter of the building data table, in the storage database.</param>
        /// <param name="yearBuiltDataPostgreSQLConverter">The converter of the stored year built history, in the main database.</param>
        /// <param name="building2DPostgreSQLConverter">The converter used to resolve the part of a building that holds no building data row yet.</param>
        /// <param name="countyIds">The county parts in scope. Normally every polygon part of one county.</param>
        /// <param name="references">The references of the buildings to recompute, or null for every building of the parts.</param>
        /// <param name="batchSize">The number of references read per statement and rows written per batch.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>A task that represents the asynchronous operation. The task result counts the buildings considered, written with a value and cleared, or is null when a converter or the parts are missing, or a read or the write could not run.</returns>
        public static async Task<BuildingDataYearBuiltUpdateResult?> UpdateBuildingDataYearBuiltAsync(this BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter, YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter, Building2DPostgreSQLConverter? building2DPostgreSQLConverter, IEnumerable<int>? countyIds, IEnumerable<string>? references, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            int[] countyIds_Array = countyIds is null ? [] : [.. countyIds.Distinct()];
            if (buildingDataPostgreSQLConverter is null || yearBuiltDataPostgreSQLConverter is null || building2DPostgreSQLConverter is null || countyIds_Array.Length == 0 || batchSize <= 0)
            {
                return null;
            }

            string[]? references_Array = references is null ? null : [.. references.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()];
            if (references_Array is not null && references_Array.Length == 0)
            {
                return new BuildingDataYearBuiltUpdateResult(0, 0, 0);
            }

            // reference => the part its building_data row is filed under
            Dictionary<string, int>? countyIds_BuildingData = await BuildingDataCountyIdsAsync(buildingDataPostgreSQLConverter, countyIds_Array, references_Array, batchSize, commandTimeout, cancellationToken);
            if (countyIds_BuildingData is null)
            {
                return null;
            }

            HashSet<string> references_Scope;
            if (references_Array is not null)
            {
                references_Scope = [.. references_Array];
            }
            else
            {
                references_Scope = [.. countyIds_BuildingData.Keys];
                foreach (int countyId in countyIds_Array)
                {
                    HashSet<string>? references_YearBuiltData = await yearBuiltDataPostgreSQLConverter.GetReferencesAsync(countyId, commandTimeout, cancellationToken);
                    if (references_YearBuiltData is null)
                    {
                        return null;
                    }

                    references_Scope.UnionWith(references_YearBuiltData);
                }
            }

            if (references_Scope.Count == 0)
            {
                return new BuildingDataYearBuiltUpdateResult(0, 0, 0);
            }

            // The history of a building may sit under any part of the county, so every part is read.
            List<GIS.Classes.YearBuiltData> yearBuiltDatas = [];
            List<string> references_Scope_List = [.. references_Scope];
            for (int i = 0; i < references_Scope_List.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<string> references_Batch = references_Scope_List.GetRange(i, Math.Min(batchSize, references_Scope_List.Count - i));
                foreach (int countyId in countyIds_Array)
                {
                    List<YearBuiltData>? yearBuiltDatas_PostgreSQL = await yearBuiltDataPostgreSQLConverter.GetItemsByReferencesAsync(references_Batch, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    if (yearBuiltDatas_PostgreSQL is null)
                    {
                        return null;
                    }

                    yearBuiltDatas.AddRange(yearBuiltDatas_PostgreSQL.Select(x => x.ToDiGi()).OfType<GIS.Classes.YearBuiltData>());
                }
            }

            // A building with no building_data row is written only for a value, so only those with history need a part.
            HashSet<string> references_WithHistory = [.. yearBuiltDatas.Select(x => x.Reference).OfType<string>()];
            List<string> references_Unfiled = [.. references_WithHistory.Where(x => references_Scope.Contains(x) && !countyIds_BuildingData.ContainsKey(x))];

            Dictionary<string, int> countyIds_Building2D = [];
            if (references_Unfiled.Count != 0)
            {
                Dictionary<string, int>? countyIds_Resolved = await building2DPostgreSQLConverter.CountyIdsByReferencesAsync(references_Unfiled, countyIds_Array, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (countyIds_Resolved is null)
                {
                    return null;
                }

                countyIds_Building2D = countyIds_Resolved;
            }

            // part => (the history of the buildings written under it, the buildings it holds a building_data row for)
            Dictionary<int, Tuple<List<GIS.Classes.YearBuiltData>, List<string>>> groups = [];
            Tuple<List<GIS.Classes.YearBuiltData>, List<string>> Group(int countyId)
            {
                if (!groups.TryGetValue(countyId, out Tuple<List<GIS.Classes.YearBuiltData>, List<string>>? group))
                {
                    group = new Tuple<List<GIS.Classes.YearBuiltData>, List<string>>([], []);
                    groups[countyId] = group;
                }

                return group;
            }

            foreach (string reference in references_Scope)
            {
                if (countyIds_BuildingData.TryGetValue(reference, out int countyId_BuildingData))
                {
                    Group(countyId_BuildingData).Item2.Add(reference);
                }
            }

            foreach (GIS.Classes.YearBuiltData yearBuiltData in yearBuiltDatas)
            {
                if (yearBuiltData.Reference is not string reference || !references_Scope.Contains(reference))
                {
                    continue;
                }

                if (countyIds_BuildingData.TryGetValue(reference, out int countyId) || countyIds_Building2D.TryGetValue(reference, out countyId))
                {
                    Group(countyId).Item1.Add(yearBuiltData);
                }
            }

            int updated = 0;
            int cleared = 0;
            foreach (KeyValuePair<int, Tuple<List<GIS.Classes.YearBuiltData>, List<string>>> keyValuePair in groups)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Table table = new();

                int count = IO.Modify.Update_Building2D_YearBuilt(table, keyValuePair.Key, keyValuePair.Value.Item1, keyValuePair.Value.Item2);
                if (table.RowCount == 0)
                {
                    continue;
                }

                if (!await buildingDataPostgreSQLConverter.PushAsync(table, batchSize, commandTimeout, cancellationToken))
                {
                    return null;
                }

                updated += count;
                cleared += table.RowCount - count;
            }

            return new BuildingDataYearBuiltUpdateResult(references_Scope.Count, updated, cleared);

            static async Task<Dictionary<string, int>?> BuildingDataCountyIdsAsync(BuildingDataPostgreSQLConverter buildingDataPostgreSQLConverter, int[] countyIds, string[]? references, int batchSize, int commandTimeout, CancellationToken cancellationToken)
            {
                await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(buildingDataPostgreSQLConverter.ConnectionData);
                if (npgsqlConnection is null)
                {
                    return null;
                }

                await npgsqlConnection.OpenAsync(cancellationToken);

                Dictionary<string, int> result = [];
                if (!await DiGi.PostgreSQL.Query.TableExistsAsync(npgsqlConnection, buildingDataPostgreSQLConverter.TableName, cancellationToken: cancellationToken))
                {
                    return result;
                }

                // Ascending part order, so a reference filed under two parts resolves to the same one on every run.
                List<string[]?> references_Batches = [];
                if (references is null)
                {
                    references_Batches.Add(null);
                }
                else
                {
                    for (int i = 0; i < references.Length; i += batchSize)
                    {
                        references_Batches.Add(references[i..Math.Min(i + batchSize, references.Length)]);
                    }
                }

                foreach (string[]? references_Batch in references_Batches)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await using NpgsqlCommand npgsqlCommand = new($@"
                        SELECT reference, county_id
                        FROM ""{buildingDataPostgreSQLConverter.TableName}""
                        WHERE county_id = ANY(@countyIds)
                          AND reference IS NOT NULL
                          {(references_Batch is null ? string.Empty : "AND reference = ANY(@references)")}
                        ORDER BY county_id;", npgsqlConnection);

                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds });
                    if (references_Batch is not null)
                    {
                        npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Batch });
                    }

                    await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                    while (await npgsqlDataReader.ReadAsync(cancellationToken))
                    {
                        result.TryAdd(npgsqlDataReader.GetString(0), npgsqlDataReader.GetInt32(1));
                    }
                }

                return result;
            }
        }
    }
}
