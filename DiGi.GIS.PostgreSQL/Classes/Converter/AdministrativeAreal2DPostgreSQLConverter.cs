using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Constants;
using DiGi.GIS.PostgreSQL.Enums;
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
    /// Provides functionality to convert and manage <see cref="AdministrativeAreal2D"/> entities within a PostgreSQL database, implementing the <see cref="IGISPostgreSQLConverter{T}"/> interface.
    /// <para><b>A code is not a key.</b> The table is loaded from BDOT10k, which stores an administrative unit whose territory is disconnected as one <c>OT_ADJA_A</c> feature per polygon part, and every feature becomes its own row. 18 of Poland's 380 counties are multi-part, so <c>type_id = 2</c> holds 406 rows for 380 codes, up to four rows sharing one code. The row count for a code always equals the feature count in that code's source package - none of this is a re-import artifact, and the extra rows carry real territory (for code <c>2412</c> the largest polygon is only 52% of the county), so they must never be deduplicated away.</para>
    /// <para><b>Every part carries its own ancestor chain.</b> <c>type_id = 0</c> and <c>type_id = 1</c> also hold 406 rows each - one country and one voivodeship per county part - so <c>country_id</c> and <c>voivodeship_id</c> on a county row point into that county's own private chain and differ between two rows of the same county. A county row's own <c>county_id</c> is null; its identity is <c>id</c>.</para>
    /// <para><b>Consequences for callers.</b> Resolve by <c>id</c> wherever possible. <see cref="GetIdByCodeAsync(NpgsqlConnection, string, AdministrativeArealType?, int, CancellationToken)"/> collapses a code to the lowest matching row and reports nothing; <see cref="GetIdsByCodeAsync(NpgsqlConnection, string, int?, AdministrativeArealType?, int, CancellationToken)"/> returns every part and is the one to use when ambiguity matters. Any new <c>LIMIT</c> or <c>FirstOrDefault</c> over this table needs an explicit <c>ORDER BY</c> - without one the row returned changes with the query plan, a vacuum or heap ordering, which is exactly how building models came to be filed under one part while its siblings read back empty.</para>
    /// <para>Full analysis: https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/1</para>
    /// </summary>
    public class AdministrativeAreal2DPostgreSQLConverter : PostgreSQLConverter<AdministrativeAreal2D>, IGISPostgreSQLConverter<AdministrativeAreal2D>
    {
        private static readonly IEnumerable<AdministrativeArealType> administrativeArealTypes = Enum.GetValues<AdministrativeArealType>().Cast<AdministrativeArealType>().Where(t => t != AdministrativeArealType.Undefined).OrderBy(t => t);

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrativeAreal2DPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database.</param>
        public AdministrativeAreal2DPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Asynchronously retrieves an administrative areal 2D by its unique identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="id">The unique identifier of the administrative areal 2D to retrieve.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2D"/> if found; otherwise, null.</returns>
        public static async Task<AdministrativeAreal2D?> GetAdministrativeAreal2DByIdAsync(NpgsqlConnection? npgsqlConnection, int id, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Added LIMIT 1 for optimization, although ID should be unique (PK)
            string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE id = @id
                LIMIT 1;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Using strongly typed parameter to prevent SQL injection and ensure correct DB type mapping
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer) { Value = id });

            return (await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken))?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves an administrative areal 2D reference by its unique identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="id">The unique identifier of the administrative areal 2D reference to retrieve.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2DReference"/> if found; otherwise, null.</returns>
        public static async Task<AdministrativeAreal2DReference?> GetAdministrativeAreal2DReferenceByIdAsync(NpgsqlConnection? npgsqlConnection, int id, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            return (await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, [id], commandTimeout, cancellationToken))?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves the reference path for the specified administrative areal 2D reference.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="administrativeAreal2DReference">The <see cref="AdministrativeAreal2DReference"/> for which the reference path is retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2DReferencePath"/> if found; otherwise, null.</returns>
        public static async Task<AdministrativeAreal2DReferencePath?> GetAdministrativeAreal2DReferencePathAsync(NpgsqlConnection? npgsqlConnection, AdministrativeAreal2DReference administrativeAreal2DReference, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || administrativeAreal2DReference is null)
            {
                return null;
            }

            List<int> ids = administrativeAreal2DReference.GetIds();
            if (ids is null || ids.Count == 0)
            {
                return null;
            }

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, ids, commandTimeout, cancellationToken);
            if (administrativeAreal2DReferences is null || administrativeAreal2DReferences.Count == 0)
            {
                return null;
            }

            return new AdministrativeAreal2DReferencePath(administrativeAreal2DReferences);
        }

        /// <summary>
        /// Asynchronously retrieves the reference path for the specified administrative areal 2D identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="id">The unique identifier of the administrative areal 2D entity.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2DReferencePath"/> if found; otherwise, null.</returns>
        public static async Task<AdministrativeAreal2DReferencePath?> GetAdministrativeAreal2DReferencePathAsync(NpgsqlConnection? npgsqlConnection, int id, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            AdministrativeAreal2DReference? administrativeAreal2DReference = await GetAdministrativeAreal2DReferenceByIdAsync(npgsqlConnection, id, commandTimeout, cancellationToken);
            if (administrativeAreal2DReference is null)
            {
                return null;
            }

            return await GetAdministrativeAreal2DReferencePathAsync(npgsqlConnection, administrativeAreal2DReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of reference paths for the specified collection of administrative areal 2D references.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="administrativeAreal2DReferences">The collection of <see cref="AdministrativeAreal2DReference"/> objects for which paths are retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReferencePath"/> objects if successful; otherwise, null.</returns>
        public static async Task<List<AdministrativeAreal2DReferencePath>?> GetAdministrativeAreal2DReferencePathsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<AdministrativeAreal2DReference> administrativeAreal2DReferences, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || administrativeAreal2DReferences is null)
            {
                return null;
            }

            Dictionary<int, AdministrativeAreal2DReference?> dictionary = [];
            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences)
            {
                if (administrativeAreal2DReference?.GetIds() is not List<int> ids)
                {
                    continue;
                }

                for (int i = 0; i < ids.Count; i++)
                {
                    int id = ids[i];

                    if (!dictionary.ContainsKey(id))
                    {
                        dictionary[id] = null;
                    }
                }

                dictionary[administrativeAreal2DReference.Id] = administrativeAreal2DReference;
            }

            List<int> ids_Temp = [];
            foreach (KeyValuePair<int, AdministrativeAreal2DReference?> keyValuePair in dictionary)
            {
                if (keyValuePair.Value is null)
                {
                    ids_Temp.Add(keyValuePair.Key);
                }
            }

            if (ids_Temp.Count != 0)
            {
                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_Temp = await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, ids_Temp, commandTimeout, cancellationToken);
                if (administrativeAreal2DReferences_Temp is not null)
                {
                    foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences_Temp)
                    {
                        dictionary[administrativeAreal2DReference.Id] = administrativeAreal2DReference;
                    }
                }
            }

            List<AdministrativeAreal2DReferencePath> result = [];
            foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences)
            {
                if (administrativeAreal2DReference is null)
                {
                    continue;
                }

                List<int> ids = administrativeAreal2DReference.GetIds();
                if (ids is null)
                {
                    continue;
                }

                List<AdministrativeAreal2DReference> administrativeAreal2DReferences_Temp = [];

                foreach (int id in ids)
                {
                    if (dictionary.TryGetValue(id, out AdministrativeAreal2DReference? administrativeAreal2DReference_Temp) && administrativeAreal2DReference_Temp is not null)
                    {
                        administrativeAreal2DReferences_Temp.Add(administrativeAreal2DReference_Temp);
                    }
                }

                result.Add(new AdministrativeAreal2DReferencePath(administrativeAreal2DReferences_Temp));
            }

            return result;
        }

        /// <summary>
        /// Searches for administrative area reference paths by name (case-insensitive and diacritic-insensitive) and returns a list of reference paths.
        /// </summary>
        /// <param name="npgsqlConnection">Existing Npgsql connection.</param>
        /// <param name="text">The text to search for within the name column.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of AdministrativeAreal2DReferencePath objects matching the search criteria.</returns>
        public static async Task<List<AdministrativeAreal2DReferencePath>?> GetAdministrativeAreal2DReferencePathsByNameAsync(NpgsqlConnection? npgsqlConnection, string text, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await GetAdministrativeAreal2DReferencesByNameAsync(npgsqlConnection, text, commandTimeout, cancellationToken);
            if (administrativeAreal2DReferences is null)
            {
                return null;
            }

            return await GetAdministrativeAreal2DReferencePathsAsync(npgsqlConnection, administrativeAreal2DReferences, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references based on the specified administrative area type and parent identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="administrativeArealType">The target <see cref="AdministrativeArealType"/> of the references to be retrieved.</param>
        /// <param name="parentIds">A collection of integer identifiers for the parent administrative areas.</param>
        /// <param name="uniqueCode">A boolean value indicating whether a unique code should be used during retrieval.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if successful; otherwise, null.</returns>
        public static async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(NpgsqlConnection? npgsqlConnection, AdministrativeArealType administrativeArealType, IEnumerable<int> parentIds, bool uniqueCode = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || parentIds is null)
            {
                return null;
            }

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, parentIds, commandTimeout, cancellationToken);
            if (administrativeAreal2DReferences is null)
            {
                return null;
            }

            List<AdministrativeAreal2DReference>? result = [];
            HashSet<int> seenIds = [];
            foreach (IGrouping<AdministrativeArealType, AdministrativeAreal2DReference> grouping in administrativeAreal2DReferences.GroupBy(x => x.AdministrativeArealType))
            {
                AdministrativeArealType administrativeArealType_Parent = grouping.Key;
                if (administrativeArealType_Parent >= administrativeArealType)
                {
                    continue;
                }

                string? columnName = administrativeArealType_Parent switch
                {
                    AdministrativeArealType.Country => "country_id",
                    AdministrativeArealType.Voivodeship => "voivodeship_id",
                    AdministrativeArealType.County => "county_id",
                    AdministrativeArealType.Municipality => "municipality_id",
                    _ => null
                };

                if (columnName is null)
                {
                    continue;
                }

                HashSet<int> parentIds_AdministrativeArealType = [];
                List<string> codes = grouping.Select(x => x.Code).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList()!;
                Dictionary<string, HashSet<int>>? siblingIdsByCode = codes.Count > 0
                    ? await GetIdsByCodesAsync(npgsqlConnection, codes, administrativeArealType_Parent, commandTimeout: commandTimeout, cancellationToken: cancellationToken)
                    : null;

                foreach (AdministrativeAreal2DReference administrativeAreal2DReference in grouping)
                {
                    if (!string.IsNullOrWhiteSpace(administrativeAreal2DReference.Code)
                        && siblingIdsByCode is not null
                        && siblingIdsByCode.TryGetValue(administrativeAreal2DReference.Code, out HashSet<int>? siblingIds)
                        && siblingIds.Count > 0)
                    {
                        parentIds_AdministrativeArealType.UnionWith(siblingIds);
                        continue;
                    }

                    parentIds_AdministrativeArealType.Add(administrativeAreal2DReference.Id);
                }

                string distinctClause = uniqueCode ? "DISTINCT ON (code)" : string.Empty;
                string orderByClause = uniqueCode ? "ORDER BY code, id ASC" : "ORDER BY id ASC";

                string commandText = $@"
                SELECT {distinctClause}
                    id,              -- index 0
                    reference,       -- index 1
                    code,            -- index 2
                    name,            -- index 3
                    type_id,         -- index 4
                    country_id,      -- index 5
                    voivodeship_id,  -- index 6
                    county_id,       -- index 7
                    municipality_id  -- index 8
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId AND {columnName} = ANY(@parentIds)
                {orderByClause};";

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("parentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = parentIds_AdministrativeArealType!.ToArray() });

                List<AdministrativeAreal2DReference>? administrativeAreal2DReferences_AdministrativeArealType = await ReadAsync_AdministrativeAreal2DReference(npgsqlCommand, cancellationToken);
                if (administrativeAreal2DReferences_AdministrativeArealType is not null)
                {
                    foreach (AdministrativeAreal2DReference administrativeAreal2DReference in administrativeAreal2DReferences_AdministrativeArealType)
                    {
                        if (seenIds.Add(administrativeAreal2DReference.Id))
                        {
                            result.Add(administrativeAreal2DReference);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references filtered by the specified administrative areal type and optionally by a parent identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> that filters the administrative areal references.</param>
        /// <param name="parentId">The optional unique identifier of the parent administrative area.</param>
        /// <param name="uniqueCode">A value indicating whether to filter by a unique code.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{AdministrativeAreal2DReference}"/> of references if successful; otherwise, null.</returns>
        public static async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(NpgsqlConnection? npgsqlConnection, AdministrativeArealType administrativeArealType, int? parentId = null, bool uniqueCode = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string additionalCondition = string.Empty;
            HashSet<int>? parentIds = null;

            if (parentId.HasValue)
            {
                AdministrativeAreal2DReference? administrativeAreal2DReference = await GetAdministrativeAreal2DReferenceByIdAsync(npgsqlConnection, parentId.Value, commandTimeout, cancellationToken);

                if (administrativeAreal2DReference is null)
                {
                    return null;
                }

                AdministrativeArealType administrativeArealType_Parent = administrativeAreal2DReference.AdministrativeArealType;
                if (administrativeArealType_Parent >= administrativeArealType)
                {
                    return null;
                }

                string? columnName = administrativeArealType_Parent switch
                {
                    AdministrativeArealType.Country => "country_id",
                    AdministrativeArealType.Voivodeship => "voivodeship_id",
                    AdministrativeArealType.County => "county_id",
                    AdministrativeArealType.Municipality => "municipality_id",
                    _ => null
                };

                if (columnName is not null)
                {
                    if (!string.IsNullOrWhiteSpace(administrativeAreal2DReference.Code))
                    {
                        parentIds = await GetIdsByCodeAsync(npgsqlConnection, administrativeAreal2DReference.Code, null, administrativeArealType_Parent, commandTimeout, cancellationToken);
                    }

                    if (parentIds is null || parentIds.Count == 0)
                    {
                        parentIds = [parentId.Value];
                    }

                    additionalCondition = $"AND {columnName} = ANY(@parentIds)";
                }
            }

            string distinctClause = uniqueCode ? "DISTINCT ON (code)" : string.Empty;
            string orderByClause = uniqueCode ? "ORDER BY code, id ASC" : "ORDER BY id ASC";

            string commandText = $@"
                SELECT {distinctClause}
                    id,              -- index 0
                    reference,       -- index 1
                    code,            -- index 2
                    name,            -- index 3
                    type_id,         -- index 4
                    country_id,      -- index 5
                    voivodeship_id,  -- index 6
                    county_id,       -- index 7
                    municipality_id  -- index 8
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId {additionalCondition}
                {orderByClause};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

            if (!string.IsNullOrEmpty(additionalCondition) && parentIds is not null)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("parentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = parentIds.ToArray() });
            }

            return await ReadAsync_AdministrativeAreal2DReference(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references based on the specified code.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="code">The identification code used to search for the administrative areal 2D references.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if found; otherwise, null or an empty list.</returns>
        public static async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByCodeAsync(NpgsqlConnection? npgsqlConnection, string code, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            HashSet<int>? ids = await GetIdsByCodeAsync(npgsqlConnection, code, null, null, commandTimeout, cancellationToken);
            if (ids is null)
            {
                return null;
            }

            if (ids.Count == 0)
            {
                return [];
            }

            return await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, ids, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references based on the specified code and administrative areal type.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="code">The identification code used to search for the administrative areal 2D references.</param>
        /// <param name="administrativeArealType">The type of administrative areal to filter by.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if found; otherwise, null or an empty list.</returns>
        public static async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByCodeAsync(NpgsqlConnection? npgsqlConnection, string code, AdministrativeArealType administrativeArealType, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code) || npgsqlConnection is null)
            {
                return null;
            }

            HashSet<int>? ids = await GetIdsByCodeAsync(npgsqlConnection, code, null, administrativeArealType, commandTimeout, cancellationToken);
            if (ids is null)
            {
                return null;
            }

            if (ids.Count == 0)
            {
                return [];
            }

            return await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, ids, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references belonging to a parent identified by the specified parent code, filtered by the target administrative areal type.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="parentCode">The identification code of the parent administrative areal.</param>
        /// <param name="administrativeArealType">The target child <see cref="AdministrativeArealType"/> to filter the results by.</param>
        /// <param name="parentType">An optional parent <see cref="AdministrativeArealType"/> used to filter the parent administrative area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if found; otherwise, null or an empty list.</returns>
        public static async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByParentCodeAsync(NpgsqlConnection? npgsqlConnection, string parentCode, AdministrativeArealType administrativeArealType, AdministrativeArealType? parentType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(parentCode) || npgsqlConnection is null)
            {
                return null;
            }

            HashSet<int>? parentIds = await GetIdsByCodeAsync(npgsqlConnection, parentCode, null, parentType, commandTimeout, cancellationToken);
            if (parentIds is null)
            {
                return null;
            }

            if (parentIds.Count == 0)
            {
                return [];
            }

            return await GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, administrativeArealType, parentIds, false, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references from the database based on the provided identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="ids">A collection of integer identifiers for the records to be retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if successful; otherwise, null if the connection is null.</returns>
        public static async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int> ids, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT
                    id,              -- index 0
                    reference,       -- index 1
                    code,            -- index 2
                    name,            -- index 3
                    type_id,         -- index 4
                    country_id,      -- index 5
                    voivodeship_id,  -- index 6
                    county_id,       -- index 7
                    municipality_id  -- index 8
                FROM {TableName.AdministrativeAreal2D}
                WHERE id = ANY(@ids)
                ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Using strongly typed parameter to prevent SQL injection and ensure correct DB type mapping
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = ids!.ToArray() });

            return await ReadAsync_AdministrativeAreal2DReference(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Searches for administrative areas by name (case-insensitive and diacritic-insensitive) and returns a list of references.
        /// </summary>
        /// <param name="npgsqlConnection">Existing Npgsql connection.</param>
        /// <param name="text">The text to search for within the name column.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of AdministrativeAreal2DReference objects matching the search criteria.</returns>
        public static async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByNameAsync(NpgsqlConnection? npgsqlConnection, string text, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Using unaccent and ILIKE for diacritic-insensitive and case-insensitive search and % placeholders for "contains" logic
            const string commandText = $@"
                SELECT
                    id,              -- index 0
                    reference,       -- index 1
                    code,            -- index 2
                    name,            -- index 3
                    type_id,         -- index 4
                    country_id,      -- index 5
                    voivodeship_id,  -- index 6
                    county_id,       -- index 7
                    municipality_id  -- index 8
                FROM {TableName.AdministrativeAreal2D}
                WHERE unaccent(name) ILIKE unaccent(@text)
                ORDER BY name ASC, id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Adding wildcards to the parameter value to match text at any position
            string formattedSearch = $"%{text}%";
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("text", NpgsqlDbType.Text) { Value = formattedSearch });

            return await ReadAsync_AdministrativeAreal2DReference(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D entities based on the specified administrative areal type and an optional parent identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> of the entities to be retrieved.</param>
        /// <param name="parentId">The optional unique identifier of the parent administrative areal entity.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{AdministrativeAreal2D}"/> of matching entities, or null if the connection is null or no valid entities are found based on the provided criteria.</returns>
        public static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByAdministrativeArealType(NpgsqlConnection? npgsqlConnection, AdministrativeArealType administrativeArealType, int? parentId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string additionalCondition = string.Empty;
            HashSet<int>? parentIds = null;

            if (parentId.HasValue)
            {
                AdministrativeAreal2DReference? administrativeAreal2DReference = await GetAdministrativeAreal2DReferenceByIdAsync(npgsqlConnection, parentId.Value, commandTimeout, cancellationToken);

                if (administrativeAreal2DReference is null)
                {
                    return null;
                }

                AdministrativeArealType administrativeArealType_Parent = administrativeAreal2DReference.AdministrativeArealType;
                if (administrativeArealType_Parent >= administrativeArealType)
                {
                    return null;
                }

                // Determine column name based on parent type
                string? columnName = administrativeArealType_Parent switch
                {
                    AdministrativeArealType.Country => "country_id",
                    AdministrativeArealType.Voivodeship => "voivodeship_id",
                    AdministrativeArealType.County => "county_id",
                    AdministrativeArealType.Municipality => "municipality_id",
                    _ => null
                };

                if (columnName is not null)
                {
                    if (!string.IsNullOrWhiteSpace(administrativeAreal2DReference.Code))
                    {
                        parentIds = await GetIdsByCodeAsync(npgsqlConnection, administrativeAreal2DReference.Code, null, administrativeArealType_Parent, commandTimeout, cancellationToken);
                    }

                    if (parentIds is null || parentIds.Count == 0)
                    {
                        parentIds = [parentId.Value];
                    }

                    additionalCondition = $"AND {columnName} = ANY(@parentIds)";
                }
            }

            // Fixed query string and removed unused HashSet
            string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId {additionalCondition}
                ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Explicit typing for parameters
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

            if (!string.IsNullOrEmpty(additionalCondition) && parentIds is not null)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("parentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = parentIds.ToArray() });
            }

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Gets AdministrativeAreal2D for given AdministrativeArealType (not iterative way). It will check all records with given AdministrativeArealType
        /// </summary>
        /// <param name="npgsqlConnection">NpgsqlConnection</param>
        /// <param name="boundingBox2D">BoundingBox2D</param>
        /// <param name="administrativeArealType">AdministrativeArealType</param>
        /// <param name="tolerance">Tolerance</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>AdministrativeAreal2D list</returns>
        public static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, AdministrativeArealType administrativeArealType, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return [];
            }

            double searchMinX = boundingBox2D.Min.X - tolerance;
            double searchMinY = boundingBox2D.Min.Y - tolerance;
            double searchMaxX = boundingBox2D.Max.X + tolerance;
            double searchMaxY = boundingBox2D.Max.Y + tolerance;

            // The bounding box is expanded by the tolerance on every side; the GiST index on
            // box(point(min_x, min_y), point(max_x, max_y)) serves the '&&' overlap operator.
            string commandText = new($@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY))");

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Gets AdministrativeAreal2D for given AdministrativeArealTypes (Iterative way). It will iterate in order through Country, Voivodeship, County, Municipality to reduce number of objects. BoundingBox2D in range Country check, then Voivodeship in this specific Country etc..
        /// <para><b>An empty level does not end the search.</b> The levels below it are searched against the last level that did answer, because a level can be missing from the source data rather than from the area: m. Poznan (<c>3064</c>) holds no <c>gmina</c> feature in BDOT10k at all, so its 113 subdivisions hang off the county. Stopping at the first empty level returned nothing whatsoever - not even a subdivision - for every query inside a city of 82 075 buildings, which is also how <see cref="Building2DPostgreSQLConverter"/> came back empty there. See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/15.</para>
        /// <para>The narrowing still assumes a unit sits inside its ancestor, which holds for this data with rare exceptions: the settlement layer (<c>OT_ADMS_A</c>) and the division layer (<c>OT_ADJA_A</c>) are digitised independently, so a handful of settlements fall marginally outside their own municipality.</para>
        /// </summary>
        /// <param name="npgsqlConnection">NpgsqlConnection</param>
        /// <param name="boundingBox2D">BoundingBox2D</param>
        /// <param name="administrativeArealTypes">AdministrativeArealTypes</param>
        /// <param name="tolerance">Tolerance</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>AdministrativeAreal2D list</returns>
        public static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, IEnumerable<AdministrativeArealType>? administrativeArealTypes, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return null;
            }

            HashSet<AdministrativeArealType>? administrativeArealTypes_HashSet = administrativeArealTypes?.ToHashSet();

            int maxIndex = (administrativeArealTypes_HashSet is not null && administrativeArealTypes_HashSet.Count > 0) ? (int)administrativeArealTypes_HashSet.Max() : int.MaxValue;

            double searchMinX = boundingBox2D.Min.X - tolerance;
            double searchMinY = boundingBox2D.Min.Y - tolerance;
            double searchMaxX = boundingBox2D.Max.X + tolerance;
            double searchMaxY = boundingBox2D.Max.Y + tolerance;

            List<AdministrativeAreal2D> candidates = [];

            HashSet<int> excludedIds = [];
            HashSet<int> parentIds = [];

            // The level parentIds were read from, which is not always the level directly above the one
            // being searched. m. Poznan (3064) holds no gmina feature in BDOT10k at all, so a search
            // inside the city finds nothing at Municipality and its subdivisions have to be reached from
            // the county instead. Stopping at the first empty level answered nothing at all for every
            // query inside a city of 82 075 buildings.
            // See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/15.
            AdministrativeArealType administrativeArealType_Parent = AdministrativeArealType.Undefined;

            foreach (AdministrativeArealType administrativeArealType in AdministrativeAreal2DPostgreSQLConverter.administrativeArealTypes)
            {
                if ((int)administrativeArealType > maxIndex)
                {
                    break;
                }

                List<AdministrativeAreal2D>? administrativeAreal2Ds = await GetAdministrativeAreal2DsByBoundingBox2D_NoObjectAsync(
                    npgsqlConnection,
                    searchMinX,
                    searchMinY,
                    searchMaxX,
                    searchMaxY,
                    administrativeArealType,
                    administrativeArealType_Parent,
                    parentIds,
                    excludedIds,
                    commandTimeout,
                    cancellationToken);

                if (administrativeAreal2Ds is null || administrativeAreal2Ds.Count == 0)
                {
                    // An empty level is a gap in the source data, not the end of the search, so the levels
                    // below it are still searched against the last level that did answer. Country is the
                    // exception: with nothing found there, there is no ancestor to search against at all.
                    if (administrativeArealType == AdministrativeArealType.Country)
                    {
                        break;
                    }

                    continue;
                }

                administrativeArealType_Parent = administrativeArealType;

                parentIds.Clear();
                foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
                {
                    if (administrativeAreal2D is null)
                    {
                        continue;
                    }

                    excludedIds.Add(administrativeAreal2D.Id);
                    parentIds.Add(administrativeAreal2D.Id);

                    if (administrativeArealTypes_HashSet is not null && !administrativeArealTypes_HashSet.Contains(administrativeAreal2D.AdministrativeArealType))
                    {
                        continue;
                    }

                    candidates.Add(administrativeAreal2D);
                }
            }

            if (candidates.Count == 0)
            {
                return [];
            }

            await PopulateObjectsAsync(npgsqlConnection, candidates, commandTimeout, cancellationToken);

            List<AdministrativeAreal2D>? result = [];
            foreach (AdministrativeAreal2D candidate in candidates)
            {
                GIS.Classes.AdministrativeAreal2D? administrativeAreal2D_GIS = candidate.ToDiGi();
                if (administrativeAreal2D_GIS is null)
                {
                    continue;
                }

                if (administrativeAreal2D_GIS.PolygonalFace2D is PolygonalFace2D polygonalFace2D && boundingBox2D.InRange(polygonalFace2D.ExternalEdge, tolerance))
                {
                    result.Add(candidate);
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects from the database based on the specified code and optional type.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="code">The string code used to identify the administrative areal 2D objects.</param>
        /// <param name="administrativeArealType">The optional <see cref="AdministrativeArealType"/> used to filter the results by a specific type.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{T}"/> of <see cref="AdministrativeAreal2D"/> objects if successful; otherwise, null.</returns>
        public static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByCodeAsync(NpgsqlConnection? npgsqlConnection, string code, AdministrativeArealType? administrativeArealType, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            HashSet<int>? ids = await GetIdsByCodeAsync(npgsqlConnection, code, null, administrativeArealType, commandTimeout, cancellationToken);
            if (ids is null)
            {
                return null;
            }

            if (ids.Count == 0)
            {
                return [];
            }

            return await GetAdministrativeAreal2DsByIdsAsync(npgsqlConnection, ids, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects from the database based on the provided identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="ids">An <see cref="IEnumerable{T}"/> of integer identifiers for the administrative areal 2D objects to retrieve.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{T}"/> of <see cref="AdministrativeAreal2D"/> objects if successful; otherwise, null.</returns>
        public static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int> ids, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ids is null)
            {
                return null;
            }

            if (!ids.Any())
            {
                return [];
            }

            const string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE id = ANY(@ids)
                ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Using strongly typed parameter to prevent SQL injection and ensure correct DB type mapping
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = ids!.ToArray() });

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D records from the database that encompass or are within a specified tolerance of the provided 2D point.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="point2D">The <see cref="Point2D"/> coordinates used to filter the administrative areals.</param>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> specifying the category of administrative areal to retrieve.</param>
        /// <param name="tolerance">The double value representing the distance tolerance applied to the bounding box check. Defaults to <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects if found; otherwise, an empty list or null.</returns>
        public static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByPoint2DAsync(NpgsqlConnection? npgsqlConnection, Point2D? point2D, AdministrativeArealType administrativeArealType, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || point2D is null)
            {
                return [];
            }

            // Applying tolerance by treating the point as a tolerance-sized search box.
            // The GiST index on box(point(min_x, min_y), point(max_x, max_y)) serves the '&&' overlap operator.
            double searchMinX = point2D.X - tolerance;
            double searchMinY = point2D.Y - tolerance;
            double searchMaxX = point2D.X + tolerance;
            double searchMaxY = point2D.Y + tolerance;

            string commandText = new($@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY))");

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously calculates the overall 2D bounding box enclosing country administrative areal entities in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the calculated <see cref="BoundingBox2D"/> enclosing the country administrative areals, or null if no valid geometries exist or connection is null.</returns>
        public static async Task<BoundingBox2D?> GetBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT MIN(min_x), MIN(min_y), MAX(max_x), MAX(max_y)
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                  AND min_x IS NOT NULL AND min_x <> 'NaN'::float8
                  AND min_y IS NOT NULL AND min_y <> 'NaN'::float8
                  AND max_x IS NOT NULL AND max_x <> 'NaN'::float8
                  AND max_y IS NOT NULL AND max_y <> 'NaN'::float8;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)AdministrativeArealType.Country });

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            if (!await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                return null;
            }

            if (npgsqlDataReader.IsDBNull(0) || npgsqlDataReader.IsDBNull(1) || npgsqlDataReader.IsDBNull(2) || npgsqlDataReader.IsDBNull(3))
            {
                return null;
            }

            double minX = npgsqlDataReader.GetDouble(0);
            double minY = npgsqlDataReader.GetDouble(1);
            double maxX = npgsqlDataReader.GetDouble(2);
            double maxY = npgsqlDataReader.GetDouble(3);

            if (double.IsNaN(minX) || double.IsNaN(minY) || double.IsNaN(maxX) || double.IsNaN(maxY))
            {
                return null;
            }

            return new BoundingBox2D(new Point2D(minX, minY), new Point2D(maxX, maxY));
        }
        /// <summary>
        /// Asynchronously retrieves the total number of administrative areal 2D records from the database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of records, or -1 if the connection is null.</returns>
        public static async Task<long> GetCountAsync(NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, TableName.AdministrativeAreal2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the identifiers of every county polygon part a spatial query over the given bounding box has to read, including parts lying outside the box.
        /// <para>A county code is not a key - a county whose territory is disconnected is stored as one row per polygon part - and a building is filed under one of those parts, not necessarily the part whose polygon covers it. Deriving the partition list from the parts overlapping the box alone therefore leaves out the partition the buildings are actually in, and the read comes back empty with no error at all. The result is widened to every part sharing a code with an overlapping part, which is the <see cref="Query.SiblingCountyGroups(IEnumerable{AdministrativeAreal2DReference})"/> rule expressed in SQL.</para>
        /// <para>Widening is safe: it only adds partitions to prune to, and the bounding box predicate on the rows themselves still decides what comes back. Prune by this rather than by the <c>county_id</c> carried by the subdivisions in the box - that column names one part.</para>
        /// <para>See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/64.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the spatial area to search; may be null.</param>
        /// <param name="tolerance">The double value the bounding box is expanded by on every side.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of every county part to read, an empty set when no county overlaps the box, or null when the connection or the bounding box is null.</returns>
        public static async Task<HashSet<int>?> GetCountyIdsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return null;
            }

            double searchMinX = boundingBox2D.Min.X - tolerance;
            double searchMinY = boundingBox2D.Min.Y - tolerance;
            double searchMaxX = boundingBox2D.Max.X + tolerance;
            double searchMaxY = boundingBox2D.Max.Y + tolerance;

            // Two index paths unioned rather than one join carrying an OR: the overlapping parts come off the
            // GiST index on box(point(min_x, min_y), point(max_x, max_y)), their siblings off
            // idx_administrative_areal_2d_type_code. A join reading "same id OR same code" - needed because a
            // part carrying no code has to stay in its own answer - is what would put an OR in front of the
            // planner, and this query runs once per spatial read.
            string commandText = $@"
                WITH overlapping AS (
                    SELECT id, code
                    FROM {TableName.AdministrativeAreal2D}
                    WHERE type_id = @typeId
                        AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY))
                )
                SELECT id FROM overlapping
                UNION
                SELECT sibling.id
                FROM {TableName.AdministrativeAreal2D} AS sibling
                WHERE sibling.type_id = @typeId
                    AND sibling.code IN (SELECT code FROM overlapping WHERE code IS NOT NULL)";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)AdministrativeArealType.County });

            HashSet<int> countyIds = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (!await npgsqlDataReader.IsDBNullAsync(0, cancellationToken))
                {
                    countyIds.Add(npgsqlDataReader.GetInt32(0));
                }
            }

            return countyIds;
        }

        /// <summary>
        /// Asynchronously retrieves an estimated count of the administrative areal 2D records from the database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="analyze">A boolean value indicating whether to perform a table analysis before retrieving the count.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated number of records, -1 when the table exists but has not been analysed, or null if the table does not exist or connection is null.</returns>
        public static async Task<long?> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            return await DiGi.PostgreSQL.Query.EstimatedCountAsync(npgsqlConnection, TableName.AdministrativeAreal2D, analyze, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the identifier of an administrative areal based on its code and optional type.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="code">The string code identifying the administrative areal.</param>
        /// <param name="administrativeArealType">The optional <see cref="AdministrativeArealType"/> to filter the search.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifier of the administrative areal if found; otherwise, null.</returns>
        /// <remarks>A code can match several rows - a multi-part county holds one row per polygon part - and this method collapses them to the lowest identifier. Callers that need to know a code was ambiguous, or that need every part, must use <see cref="GetIdsByCodeAsync(NpgsqlConnection, string, int?, AdministrativeArealType?, int, CancellationToken)"/> instead.</remarks>
        public static async Task<int?> GetIdByCodeAsync(NpgsqlConnection? npgsqlConnection, string? code, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            HashSet<int>? ids = await GetIdsByCodeAsync(npgsqlConnection, code, 1, administrativeArealType, commandTimeout, cancellationToken);
            if (ids is null || ids.Count == 0)
            {
                return null;
            }

            // The query already orders by id, so this is the lowest matching row. Min() states that
            // outright rather than relying on HashSet enumeration order, which guarantees nothing.
            return ids.Min();
        }

        /// <summary>
        /// Asynchronously retrieves a set of identifiers for administrative areals that match the specified code and optional criteria.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="code">The string code used to filter the administrative areals.</param>
        /// <param name="limit">An optional integer specifying the maximum number of identifiers to retrieve.</param>
        /// <param name="administrativeArealType">An optional <see cref="AdministrativeArealType"/> used to further filter the results by type.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of integers representing the IDs if found; otherwise, null.</returns>
        public static async Task<HashSet<int>?> GetIdsByCodeAsync(NpgsqlConnection? npgsqlConnection, string? code, int? limit = null, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            // A code is not unique: BDOT10k stores a county whose territory is disconnected as one
            // OT_ADJA_A feature per polygon part, and every part becomes its own row (18 of Poland's
            // 380 counties, up to four rows each). ORDER BY is therefore what makes LIMIT meaningful -
            // without it the row returned changes with the query plan, a vacuum or heap ordering, and
            // the same code has already resolved to different ids on different occasions.
            string commandText = $@"
                SELECT id
                FROM {TableName.AdministrativeAreal2D}
                WHERE (@typeId IS NULL OR type_id = @typeId)
                  AND code = @code
                ORDER BY id ASC";

            // Dynamically append LIMIT if provided
            if (limit.HasValue)
            {
                commandText += $" LIMIT {limit.Value}";
            }

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Handling the nullable parameter for the SQL query
            object typeValue = administrativeArealType is not null ? (short)administrativeArealType.Value : DBNull.Value;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint)
            {
                Value = typeValue
            });

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Text)
            {
                Value = (object?)code ?? DBNull.Value
            });

            // If limit is provided, we can pre-allocate the HashSet capacity to improve performance
            HashSet<int> results = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (!await npgsqlDataReader.IsDBNullAsync(0, cancellationToken))
                {
                    results.Add(npgsqlDataReader.GetInt32(0));
                }
            }

            return results;
        }

        /// <summary>
        /// Asynchronously retrieves a mapping of administrative area codes to sets of identifiers matching those codes and administrative area type.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="codes">A collection of code strings to query.</param>
        /// <param name="administrativeArealType">The optional <see cref="AdministrativeArealType"/> used to filter the records.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary mapping code strings to sets of integer identifiers if found; otherwise, null.</returns>
        public static async Task<Dictionary<string, HashSet<int>>?> GetIdsByCodesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? codes, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || codes is null)
            {
                return null;
            }

            List<string> codes_List = codes.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            if (codes_List.Count == 0)
            {
                return [];
            }

            string commandText = $@"
                SELECT id, code
                FROM {TableName.AdministrativeAreal2D}
                WHERE (@typeId IS NULL OR type_id = @typeId)
                  AND code = ANY(@codes)
                ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            object typeValue = administrativeArealType is not null ? (short)administrativeArealType.Value : DBNull.Value;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint)
            {
                Value = typeValue
            });

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text)
            {
                Value = codes_List.ToArray()
            });

            Dictionary<string, HashSet<int>> results = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (!await npgsqlDataReader.IsDBNullAsync(0, cancellationToken) && !await npgsqlDataReader.IsDBNullAsync(1, cancellationToken))
                {
                    int id = npgsqlDataReader.GetInt32(0);
                    string code = npgsqlDataReader.GetString(1);
                    if (!results.TryGetValue(code, out HashSet<int>? idSet))
                    {
                        idSet = [];
                        results[code] = idSet;
                    }

                    idSet.Add(id);
                }
            }

            return results;
        }

        /// <summary>
        /// Asynchronously retrieves a collection of sub-codes that start with the specified code prefix from the database, excluding the exact code match.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="code">The parent code or prefix used to filter and identify the associated sub-codes.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of strings containing the matching sub-codes, or <c>null</c> if the connection is null.</returns>
        public static async Task<HashSet<string>?> GetSubCodesAsync(NpgsqlConnection? npgsqlConnection, string? code, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return [];
            }

            HashSet<string> results = [];

            string query = $@"
                SELECT code
                FROM {TableName.AdministrativeAreal2D}
                WHERE code LIKE @prefix
                  AND code <> @code;";

            await using NpgsqlCommand npgsqlCommand = new(query, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("prefix", NpgsqlDbType.Text)
            {
                Value = $"{code}%"
            });

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Text)
            {
                Value = code
            });

            await using NpgsqlDataReader reader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                if (!await reader.IsDBNullAsync(0, cancellationToken))
                {
                    string code_Temp = reader.GetString(0);
                    results.Add(code_Temp);
                }
            }

            return results;
        }

        /// <summary>
        /// Asynchronously clears all data from the administrative areal 2D table in the PostgreSQL database.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the table was cleared successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> ClearAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await DiGi.PostgreSQL.Modify.ClearAsync(npgsqlConnection, TableName.AdministrativeAreal2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously creates the PostgreSQL table for administrative areal 2D data and performs an analysis on the created table.
        /// </summary>
        /// <param name="commandTimeout">The time interval, in seconds, to wait for the command to complete before timing out.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the table was created successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CreateTableAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool result = await Create.TableAsync_AdministrativeArea2D(npgsqlConnection, commandTimeout, cancellationToken);
            if (result)
            {
                await DiGi.PostgreSQL.Modify.Analyze(npgsqlConnection, TableName.AdministrativeAreal2D, commandTimeout);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves an administrative areal 2D based on the specified code and optional type.
        /// </summary>
        /// <param name="code">The unique string code of the administrative areal 2D.</param>
        /// <param name="administrativeArealType">The optional <see cref="AdministrativeArealType"/> used to filter the search results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2D"/> if found; otherwise, null.</returns>
        public async Task<AdministrativeAreal2D?> GetAdministrativeAreal2DByCodeAsync(string code, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            List<AdministrativeAreal2D>? administrativeAreal2Ds = await GetAdministrativeAreal2DsByCodeAsync(code, administrativeArealType, commandTimeout, cancellationToken);
            if (administrativeAreal2Ds is null || administrativeAreal2Ds.Count == 0)
            {
                return null;
            }

            return administrativeAreal2Ds[0];
        }

        /// <summary>
        /// Asynchronously retrieves an administrative areal 2D based on the specified identifier.
        /// </summary>
        /// <param name="id">The unique integer identifier of the administrative areal 2D.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2D"/> if found; otherwise, null.</returns>
        public async Task<AdministrativeAreal2D?> GetAdministrativeAreal2DByIdAsync(int id, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DByIdAsync(npgsqlConnection, id, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a reference to an administrative areal 2D based on the provided code and optional type.
        /// </summary>
        /// <param name="code">The unique code of the administrative areal 2D.</param>
        /// <param name="administrativeArealType">The optional <see cref="AdministrativeArealType"/> used to filter the search results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2DReference"/> if found; otherwise, null.</returns>
        public async Task<AdministrativeAreal2DReference?> GetAdministrativeAreal2DReferenceByCodeAsync(string code, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            HashSet<int>? ids = await GetIdsByCodeAsync(npgsqlConnection, code, 1, administrativeArealType, commandTimeout, cancellationToken);
            if (ids is null || ids.Count == 0)
            {
                return null;
            }

            return (await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, ids, commandTimeout, cancellationToken))?.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously retrieves an <see cref="AdministrativeAreal2DReference"/> by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the administrative areal 2D reference to retrieve.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2DReference"/> if found; otherwise, null.</returns>
        public async Task<AdministrativeAreal2DReference?> GetAdministrativeAreal2DReferenceByIdAsync(int id, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferenceByIdAsync(npgsqlConnection, id, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the administrative areal 2D reference path for the specified administrative areal 2D reference.
        /// </summary>
        /// <param name="administrativeAreal2DReference">The administrative areal 2D reference used to retrieve the corresponding path.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2DReferencePath"/> if found; otherwise, null.</returns>
        public async Task<AdministrativeAreal2DReferencePath?> GetAdministrativeAreal2DReferencePathAsync(AdministrativeAreal2DReference administrativeAreal2DReference, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (administrativeAreal2DReference is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencePathAsync(npgsqlConnection, administrativeAreal2DReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an administrative areal 2D reference path by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the administrative areal 2D reference path.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AdministrativeAreal2DReferencePath"/> if found; otherwise, null.</returns>
        public async Task<AdministrativeAreal2DReferencePath?> GetAdministrativeAreal2DReferencePathAsync(int id, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencePathAsync(npgsqlConnection, id, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of reference paths for the specified collection of administrative areal 2D references.
        /// </summary>
        /// <param name="administrativeAreal2DReferences">The collection of <see cref="AdministrativeAreal2DReference"/> objects for which paths are retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReferencePath"/> objects if successful; otherwise, null.</returns>
        public async Task<List<AdministrativeAreal2DReferencePath>?> GetAdministrativeAreal2DReferencePathsAsync(IEnumerable<AdministrativeAreal2DReference> administrativeAreal2DReferences, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (administrativeAreal2DReferences is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencePathsAsync(npgsqlConnection, administrativeAreal2DReferences, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Searches for administrative area reference paths by name (case-insensitive and diacritic-insensitive) and returns a list of reference paths.
        /// </summary>
        /// <param name="text">The text to search for within the name column.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of AdministrativeAreal2DReferencePath objects matching the search criteria.</returns>
        public async Task<List<AdministrativeAreal2DReferencePath>?> GetAdministrativeAreal2DReferencePathsByNameAsync(string text, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            List<AdministrativeAreal2DReference>? administrativeAreal2DReferences = await GetAdministrativeAreal2DReferencesByNameAsync(npgsqlConnection, text, commandTimeout, cancellationToken);
            if (administrativeAreal2DReferences is null)
            {
                return null;
            }

            return await GetAdministrativeAreal2DReferencePathsAsync(npgsqlConnection, administrativeAreal2DReferences, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references based on the specified administrative areal type, an optional parent identifier, and a uniqueness flag.
        /// </summary>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> that defines the category of administrative areals to be retrieved.</param>
        /// <param name="parentId">The optional integer identifier of the parent administrative areal used to filter for child elements.</param>
        /// <param name="uniqueCode">A boolean value indicating whether the retrieval should be filtered by unique codes.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if successful; otherwise, null.</returns>
        public async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType administrativeArealType, int? parentId = null, bool uniqueCode = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            if (administrativeArealType == AdministrativeArealType.Country && parentId.HasValue)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(npgsqlConnection, administrativeArealType, parentId, uniqueCode, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references associated with the specified code.
        /// </summary>
        /// <param name="code">The string code used to identify the administrative areals.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if found; otherwise, null or an empty list.</returns>
        public async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByCodeAsync(string code, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            HashSet<int>? ids = await GetIdsByCodeAsync(npgsqlConnection, code, null, null, commandTimeout, cancellationToken);
            if (ids is null)
            {
                return null;
            }

            if (ids.Count == 0)
            {
                return [];
            }

            return await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, ids, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references based on the specified code and administrative areal type.
        /// </summary>
        /// <param name="code">The string code used to identify the administrative areals.</param>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> specifying the category of the administrative areal.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if matches are found; otherwise, null or an empty list.</returns>
        public async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByCodeAsync(string code, AdministrativeArealType administrativeArealType, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencesByCodeAsync(npgsqlConnection, code, administrativeArealType, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references belonging to a parent identified by the specified parent code, filtered by the target administrative areal type.
        /// </summary>
        /// <param name="parentCode">The identification code of the parent administrative areal.</param>
        /// <param name="administrativeArealType">The target child <see cref="AdministrativeArealType"/> to filter the results by.</param>
        /// <param name="parentType">An optional parent <see cref="AdministrativeArealType"/> used to filter the parent administrative area.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if matches are found; otherwise, null or an empty list.</returns>
        public async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByParentCodeAsync(string parentCode, AdministrativeArealType administrativeArealType, AdministrativeArealType? parentType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(parentCode))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencesByParentCodeAsync(npgsqlConnection, parentCode, administrativeArealType, parentType, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references associated with the specified identifiers using the internal connection data.
        /// </summary>
        /// <param name="ids">The collection of integer identifiers used to retrieve the administrative areal 2D references.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if matches are found; otherwise, null.</returns>
        public async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByIdsAsync(IEnumerable<int> ids, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (ids is null)
            {
                return null;
            }

            if (!ids.Any())
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencesByIdsAsync(npgsqlConnection, ids, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D references that match the specified search text (case-insensitive and diacritic-insensitive) using the internal connection data.
        /// </summary>
        /// <param name="text">The search string used to filter administrative areal 2D references by their name.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2DReference"/> objects if matches are found; otherwise, null.</returns>
        public async Task<List<AdministrativeAreal2DReference>?> GetAdministrativeAreal2DReferencesByNameAsync(string text, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DReferencesByNameAsync(npgsqlConnection, text, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects filtered by the specified administrative areal type and an optional parent identifier.
        /// </summary>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> used to filter the administrative areal objects.</param>
        /// <param name="parentId">The optional unique identifier of the parent administrative areal object.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects if successful; otherwise, null.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByAdministrativeArealType(AdministrativeArealType administrativeArealType, int? parentId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            if (administrativeArealType == AdministrativeArealType.Country && parentId.HasValue)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DsByAdministrativeArealType(npgsqlConnection, administrativeArealType, parentId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects filtered by the specified administrative areal type.
        /// </summary>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> used to filter the administrative areal objects.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects if successful; otherwise, null or an empty list.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByAdministrativeArealTypeAsync(AdministrativeArealType administrativeArealType, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            // Creating the connection using the shared PostgreSQL infrastructure
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            // Simple, high-performance query filtering only by type_id
            const string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Adding the mandatory parameter
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects that fall within the area defined by the specified bounding box and match the provided administrative areal type.
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area. If this value is null, the method returns null.</param>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> used to filter the administrative areal objects.</param>
        /// <param name="tolerance">A double value representing the distance tolerance used for the search operation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the bounding box is null or the administrative areal type is undefined.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, AdministrativeArealType administrativeArealType, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (boundingBox2D is null || administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DsByBoundingBox2DAsync(boundingBox2D, [administrativeArealType], tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects that fall within the area defined by the specified bounding box and match any of the provided administrative areal types.
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area. If this value is null, the method returns null.</param>
        /// <param name="administrativeArealTypes">A collection of <see cref="AdministrativeArealType"/> values used to filter the administrative areal objects.</param>
        /// <param name="tolerance">A double value representing the distance tolerance used for the search operation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the bounding box is null.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, IEnumerable<AdministrativeArealType>? administrativeArealTypes, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (boundingBox2D is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, administrativeArealTypes, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects that fall within the area defined by the specified bounding box.
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the search area. If this value is null, an empty list may be returned.</param>
        /// <param name="tolerance">A double value representing the distance tolerance used for the search operation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the retrieval fails.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            return await GetAdministrativeAreal2DsByBoundingBox2DAsync(boundingBox2D, null, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects that fall within the area defined by the specified circle and match any of the provided administrative areal types.
        /// </summary>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area. If this value is null, an empty list is returned.</param>
        /// <param name="administrativeArealTypes">A collection of <see cref="AdministrativeArealType"/> values used to filter the administrative areal objects.</param>
        /// <param name="tolerance">A double value representing the distance tolerance added to the circle's radius for the search operation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the retrieval fails.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByCircle2DAsync(Circle2D? circle2D, IEnumerable<AdministrativeArealType>? administrativeArealTypes, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (circle2D is null)
            {
                return [];
            }

            return await GetAdministrativeAreal2DsByPoint2DAsync(circle2D.Center, administrativeArealTypes, circle2D.Radius + tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects that fall within the area defined by the specified circle and match the given administrative areal type.
        /// </summary>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area. If this value is null, an empty list is returned.</param>
        /// <param name="administrativeArealType">The <see cref="AdministrativeArealType"/> used to filter the administrative areal objects.</param>
        /// <param name="tolerance">A double value representing the distance tolerance added to the circle's radius for the search operation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the retrieval fails.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByCircle2DAsync(Circle2D? circle2D, AdministrativeArealType administrativeArealType, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (circle2D is null)
            {
                return [];
            }

            return await GetAdministrativeAreal2DsByPoint2DAsync(circle2D.Center, administrativeArealType, circle2D.Radius + tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects that fall within the area defined by the specified circle.
        /// </summary>
        /// <param name="circle2D">The <see cref="Circle2D"/> defining the search area. If this value is null, an empty list is returned.</param>
        /// <param name="tolerance">A double value representing the distance tolerance added to the circle's radius for the search operation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the retrieval fails.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByCircle2DAsync(Circle2D? circle2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (circle2D is null)
            {
                return [];
            }

            return await GetAdministrativeAreal2DsByPoint2DAsync(circle2D.Center, circle2D.Radius + tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects based on the specified code and an optional administrative areal type.
        /// </summary>
        /// <param name="code">The string representation of the code used to filter the administrative areal records.</param>
        /// <param name="administrativeArealType">An optional <see cref="AdministrativeArealType"/> used to further refine the search results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the database connection could not be established.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByCodeAsync(string code, AdministrativeArealType? administrativeArealType, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DsByCodeAsync(npgsqlConnection, code, administrativeArealType, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D objects based on the provided collection of codes.
        /// <para>If the codes collection is null or empty, all records from the table are retrieved.</para>
        /// </summary>
        /// <param name="codes">An optional collection of strings representing the codes used to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects, or null if the database connection could not be established.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByCodesAsync(IEnumerable<string>? codes = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            List<AdministrativeAreal2D> result = [];

            // 1. Dynamic SQL construction
            // If ids is null or empty, the WHERE clause is effectively ignored or skipped
            bool noFilter = codes is null || !codes.Any();

            string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                {(noFilter ? "" : "WHERE code = ANY(@codes)")}
                ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            if (!noFilter)
            {
                // Passing the whole collection as a PostgreSQL array
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text)
                {
                    Value = codes!.ToArray()
                });
            }

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areas based on the provided identifiers.
        /// </summary>
        /// <param name="ids">An optional collection of integer identifiers used to filter the results. If this parameter is null or empty, no ID filtering is applied and the whole table is read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="AdministrativeAreal2D"/> objects if successful; otherwise, null.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByIdsAsync(IEnumerable<int>? ids = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            // 1. Dynamic SQL construction
            // If ids is null or empty, the WHERE clause is effectively ignored or skipped
            bool noFilter = ids is null || !ids.Any();

            string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                {(noFilter ? "" : "WHERE id = ANY(@ids)")}
                ORDER BY id ASC;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            if (!noFilter)
            {
                // Passing the whole collection as a PostgreSQL array
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = ids!.ToArray() });
            }

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areas of a specific type that contain or are near the specified 2D point within the given tolerance.
        /// </summary>
        /// <param name="point2D">The 2D point used to search for administrative areas. If this value is null, the method returns null.</param>
        /// <param name="administrativeArealType">The type of administrative area to retrieve. If this value is <see cref="AdministrativeArealType.Undefined"/>, the method returns null.</param>
        /// <param name="tolerance">The distance tolerance used for the spatial query. The default value is <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching <see cref="AdministrativeAreal2D"/> objects, or null if the provided point is null or the administrative area type is undefined.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByPoint2DAsync(Point2D? point2D, AdministrativeArealType administrativeArealType, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (point2D is null || administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetAdministrativeAreal2DsByPoint2DAsync(point2D, [administrativeArealType], tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areas that contain or are near the specified 2D point within the given tolerance.
        /// </summary>
        /// <param name="point2D">The 2D point used to search for administrative areas. If this value is null, the method returns null.</param>
        /// <param name="tolerance">The distance tolerance used for the spatial query. The default value is <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching <see cref="AdministrativeAreal2D"/> objects, or null if the provided point is null.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByPoint2DAsync(Point2D? point2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            return await GetAdministrativeAreal2DsByPoint2DAsync(point2D, null, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areas that contain or are near the specified 2D point, filtered by the provided administrative area types and within the given tolerance.
        /// <para>Levels are searched from Country downwards, each narrowed to the children of the last level that answered. <b>An empty level does not end the search</b> - see <see cref="GetAdministrativeAreal2DsByBoundingBox2DAsync(NpgsqlConnection, BoundingBox2D, IEnumerable{AdministrativeArealType}, double, int, CancellationToken)"/> for why m. Poznan makes that necessary.</para>
        /// </summary>
        /// <param name="point2D">The 2D point used to search for administrative areas. If this value is null, the method returns null.</param>
        /// <param name="administrativeArealTypes">An optional collection of administrative area types to filter the results.</param>
        /// <param name="tolerance">The distance tolerance used for the spatial query. The default value is <see cref="Core.Constants.Tolerance.MacroDistance"/>.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching <see cref="AdministrativeAreal2D"/> objects, or null if the provided point is null.</returns>
        public async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByPoint2DAsync(Point2D? point2D, IEnumerable<AdministrativeArealType>? administrativeArealTypes, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (point2D is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return [];
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            HashSet<AdministrativeArealType>? administrativeArealTypes_HashSet = administrativeArealTypes?.ToHashSet();

            int maxIndex = administrativeArealTypes_HashSet is not null && administrativeArealTypes_HashSet.Count > 0 ? (int)administrativeArealTypes_HashSet.Max() : int.MaxValue;

            double searchMinX = point2D.X - tolerance;
            double searchMinY = point2D.Y - tolerance;
            double searchMaxX = point2D.X + tolerance;
            double searchMaxY = point2D.Y + tolerance;

            List<AdministrativeAreal2D> candidates = [];

            HashSet<int> excludedIds = [];
            HashSet<int> parentIds = [];

            // The level parentIds were read from - see the note on the bounding box search above, which
            // steps over an empty level the same way and for the same reason.
            AdministrativeArealType administrativeArealType_Parent = AdministrativeArealType.Undefined;

            foreach (AdministrativeArealType administrativeArealType in AdministrativeAreal2DPostgreSQLConverter.administrativeArealTypes)
            {
                if ((int)administrativeArealType > maxIndex)
                {
                    break;
                }

                List<AdministrativeAreal2D>? administrativeAreal2Ds = await GetAdministrativeAreal2DsByPoint2D_NoObjectAsync(
                    npgsqlConnection,
                    searchMinX,
                    searchMinY,
                    searchMaxX,
                    searchMaxY,
                    administrativeArealType,
                    administrativeArealType_Parent,
                    parentIds,
                    excludedIds,
                    commandTimeout,
                    cancellationToken);

                if (administrativeAreal2Ds is null || administrativeAreal2Ds.Count == 0)
                {
                    if (administrativeArealType == AdministrativeArealType.Country)
                    {
                        break;
                    }

                    continue;
                }

                administrativeArealType_Parent = administrativeArealType;

                parentIds.Clear();
                foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
                {
                    if (administrativeAreal2D is null)
                    {
                        continue;
                    }

                    excludedIds.Add(administrativeAreal2D.Id);
                    parentIds.Add(administrativeAreal2D.Id);

                    if (administrativeArealTypes_HashSet is not null && !administrativeArealTypes_HashSet.Contains(administrativeAreal2D.AdministrativeArealType))
                    {
                        continue;
                    }

                    candidates.Add(administrativeAreal2D);
                }
            }

            if (candidates.Count == 0)
            {
                return [];
            }

            await PopulateObjectsAsync(npgsqlConnection, candidates, commandTimeout, cancellationToken);

            List<AdministrativeAreal2D> result = [];
            foreach (AdministrativeAreal2D candidate in candidates)
            {
                GIS.Classes.AdministrativeAreal2D? administrativeAreal2D_GIS = candidate.ToDiGi();
                if (administrativeAreal2D_GIS is null)
                {
                    continue;
                }

                if (administrativeAreal2D_GIS.PolygonalFace2D is PolygonalFace2D polygonalFace2D && polygonalFace2D.InRange(point2D, tolerance))
                {
                    result.Add(candidate);
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously calculates the overall 2D bounding box enclosing country administrative areal entities in the PostgreSQL database.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the calculated <see cref="BoundingBox2D"/> enclosing the country administrative areals, or null if no valid geometries exist or connection is null.</returns>
        public async Task<BoundingBox2D?> GetBoundingBox2DAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetBoundingBox2DAsync(npgsqlConnection, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves all codes of administrative areal 2D entities from the database.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of strings representing the codes, or <see langword="null"/> if the database connection cannot be established.</returns>
        public async Task<HashSet<string>?> GetCodesAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            HashSet<string> codes = [];

            // We only select the 'id' column to minimize data transfer
            const string commandText = $"SELECT code FROM {TableName.AdministrativeAreal2D};";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            await using NpgsqlDataReader reader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                // Reading only the first column (index 0)
                string code = reader.GetString(0);
                codes.Add(code);
            }

            return codes;
        }

        /// <summary>
        /// Asynchronously retrieves the total count of administrative areal 2D entities from the database.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a <see cref="long"/>; returns -1 if the database connection cannot be established.</returns>
        public async Task<long> GetCountAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, TableName.AdministrativeAreal2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the identifiers of every county polygon part a spatial query over the given bounding box has to read, including parts lying outside the box.
        /// <para>See <see cref="GetCountyIdsByBoundingBox2DAsync(NpgsqlConnection, BoundingBox2D, double, int, CancellationToken)"/> for why the parts overlapping the box are not the whole answer.</para>
        /// </summary>
        /// <param name="boundingBox2D">The <see cref="BoundingBox2D"/> defining the spatial area to search; may be null.</param>
        /// <param name="tolerance">The double value the bounding box is expanded by on every side.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of every county part to read, an empty set when no county overlaps the box, or null when the bounding box is null or the connection could not be established.</returns>
        public async Task<HashSet<int>?> GetCountyIdsByBoundingBox2DAsync(BoundingBox2D? boundingBox2D, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (boundingBox2D is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetCountyIdsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the identifiers of every county polygon part whose subdivision layer nests - where a subdivision lies inside another subdivision of the same municipality, as a city holds its districts and their neighbourhoods.
        /// <para>Decided on the stored bounding boxes: a subdivision counts as nested when its box lies within the strictly larger box of a sibling, expanded by <paramref name="tolerance"/>. A box inside a box does not prove a polygon inside a polygon, so this over-approximates - which is what a <b>scope</b> wants: every county that might hold a nesting is named, at the cost of the odd county that only looks like one from its boxes. It is not a membership test; <see cref="Query.ContainerIds(IReadOnlyDictionary{int, PolygonalFace2D}, double)"/> decides nesting on the polygons themselves.</para>
        /// <para>Widened to every part sharing a code with a part named, the <see cref="GetCountyIdsByBoundingBox2DAsync(NpgsqlConnection?, BoundingBox2D?, double, int, CancellationToken)"/> rule: a subdivision's <c>county_id</c> names one part, and the buildings it holds may be filed under a sibling. See <see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the PostgreSQL database.</param>
        /// <param name="tolerance">The distance a container's box is expanded by on every side before the inner box is tested against it.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the county parts to scope to, an empty set when no subdivision nests anywhere, or null when the connection is null.</returns>
        public static async Task<HashSet<int>?> GetCountyIdsWithNestedSubdivisionsAsync(NpgsqlConnection? npgsqlConnection, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Pairs are formed within a municipality - or within a county for subdivisions filed directly under
            // one - because a nesting never crosses that boundary, and the pairing is what would otherwise grow
            // with the square of the layer. The inner box is compared against the container's box grown by the
            // tolerance, and the container has to be strictly larger, so two rows sharing one box name neither.
            string commandText = $@"
                WITH nested AS (
                    SELECT DISTINCT inner_row.county_id
                    FROM {TableName.AdministrativeAreal2D} AS inner_row
                    JOIN {TableName.AdministrativeAreal2D} AS outer_row
                        ON outer_row.type_id = inner_row.type_id
                        AND outer_row.id <> inner_row.id
                        AND outer_row.municipality_id IS NOT DISTINCT FROM inner_row.municipality_id
                        AND outer_row.county_id IS NOT DISTINCT FROM inner_row.county_id
                        AND box(point(inner_row.min_x, inner_row.min_y), point(inner_row.max_x, inner_row.max_y)) <@ box(point(outer_row.min_x - @tolerance, outer_row.min_y - @tolerance), point(outer_row.max_x + @tolerance, outer_row.max_y + @tolerance))
                        AND area(box(point(outer_row.min_x, outer_row.min_y), point(outer_row.max_x, outer_row.max_y))) > area(box(point(inner_row.min_x, inner_row.min_y), point(inner_row.max_x, inner_row.max_y)))
                    WHERE inner_row.type_id = @typeId_Subdivision
                        AND inner_row.county_id IS NOT NULL
                ),
                named AS (
                    SELECT county.id, county.code
                    FROM {TableName.AdministrativeAreal2D} AS county
                    WHERE county.type_id = @typeId_County
                        AND county.id IN (SELECT county_id FROM nested)
                )
                SELECT id FROM named
                UNION
                SELECT sibling.id
                FROM {TableName.AdministrativeAreal2D} AS sibling
                WHERE sibling.type_id = @typeId_County
                    AND sibling.code IN (SELECT code FROM named WHERE code IS NOT NULL)";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("tolerance", NpgsqlDbType.Double) { Value = tolerance });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId_Subdivision", NpgsqlDbType.Smallint) { Value = (short)AdministrativeArealType.Subdivision });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId_County", NpgsqlDbType.Smallint) { Value = (short)AdministrativeArealType.County });

            HashSet<int> countyIds = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (!await npgsqlDataReader.IsDBNullAsync(0, cancellationToken))
                {
                    countyIds.Add(npgsqlDataReader.GetInt32(0));
                }
            }

            return countyIds;
        }

        /// <summary>
        /// Asynchronously retrieves the identifiers of every county polygon part whose subdivision layer nests. See <see cref="GetCountyIdsWithNestedSubdivisionsAsync(NpgsqlConnection?, double, int, CancellationToken)"/>.
        /// </summary>
        /// <param name="tolerance">The distance a container's box is expanded by on every side before the inner box is tested against it.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the county parts to scope to, an empty set when no subdivision nests anywhere, or null when the connection could not be established.</returns>
        public async Task<HashSet<int>?> GetCountyIdsWithNestedSubdivisionsAsync(double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetCountyIdsWithNestedSubdivisionsAsync(npgsqlConnection, tolerance, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated count of the administrative areal 2D entities from the database.
        /// </summary>
        /// <param name="analyze">A boolean value indicating whether to analyze the table before retrieving the estimate.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated count as a nullable <see cref="long"/>, -1 when the table exists but has not been analysed, or null if the table does not exist or connection cannot be established.</returns>
        public async Task<long?> GetEstimatedCountAsync(bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await DiGi.PostgreSQL.Query.EstimatedCountAsync(npgsqlConnection, TableName.AdministrativeAreal2D, analyze, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the identifier of an administrative areal 2D entity based on the specified code and type.
        /// </summary>
        /// <param name="code">The identification code of the administrative areal entity.</param>
        /// <param name="administrativeArealType">The type of the administrative areal entity.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifier as an <see cref="int"/> if found; otherwise, <c>null</c>.</returns>
        public async Task<int?> GetIdByCodeAsync(string? code, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (code is null || administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetIdByCodeAsync(npgsqlConnection, code, administrativeArealType, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves all identifiers for administrative areal 2D entities from the database.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of integers containing the IDs, or <c>null</c> if the database connection could not be established.</returns>
        public async Task<HashSet<int>?> GetIdsAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            HashSet<int> ids = [];

            // We only select the 'id' column to minimize data transfer
            string query = $"SELECT id FROM {TableName.AdministrativeAreal2D};";

            await using NpgsqlCommand npgsqlCommand = new(query, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            await using NpgsqlDataReader reader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                // Reading only the first column (index 0)
                int id = reader.GetInt32(0);
                ids.Add(id);
            }

            return ids;
        }

        /// <summary>
        /// Asynchronously retrieves a set of identifiers for administrative areal entities of the specified type from the database.
        /// </summary>
        /// <param name="administrativeArealType">The type of administrative areal used to filter the results.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects to signal that the asynchronous operation should be canceled.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of integers containing the IDs, or <c>null</c> if the database connection could not be established.</returns>
        public async Task<HashSet<int>?> GetIdsAsync(AdministrativeArealType administrativeArealType, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            HashSet<int> ids = [];

            // We only select the 'id' column to minimize data transfer
            string query = $"SELECT id FROM {TableName.AdministrativeAreal2D} WHERE type_id = @typeId;";

            await using NpgsqlCommand npgsqlCommand = new(query, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });
            await using NpgsqlDataReader reader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                // Reading only the first column (index 0)
                int id = reader.GetInt32(0);
                ids.Add(id);
            }

            return ids;
        }

        /// <summary>
        /// Asynchronously retrieves every administrative areal 2D identifier matching the specified code and type.
        /// <para>A county code matches one row per polygon part of a multi-part county, so this returns several identifiers for such a county. Use it wherever an ambiguous code has to be detected or every part has to be visited, rather than <see cref="GetIdByCodeAsync(string, AdministrativeArealType?, int, CancellationToken)"/>, which silently collapses the match to the lowest identifier.</para>
        /// </summary>
        /// <param name="code">The identification code of the administrative areal entity.</param>
        /// <param name="administrativeArealType">The type of the administrative areal entity.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matching identifiers, an empty set when the code matches nothing, or <c>null</c> when the connection could not be established.</returns>
        public async Task<HashSet<int>?> GetIdsByCodeAsync(string? code, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (code is null || administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetIdsByCodeAsync(npgsqlConnection, code, null, administrativeArealType, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a mapping of administrative area codes to sets of identifiers matching those codes and administrative area type.
        /// </summary>
        /// <param name="codes">A collection of code strings to query.</param>
        /// <param name="administrativeArealType">The optional <see cref="AdministrativeArealType"/> used to filter the records.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary mapping code strings to sets of integer identifiers if found; otherwise, null.</returns>
        public async Task<Dictionary<string, HashSet<int>>?> GetIdsByCodesAsync(IEnumerable<string>? codes, AdministrativeArealType? administrativeArealType = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (codes is null || administrativeArealType == AdministrativeArealType.Undefined)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetIdsByCodesAsync(npgsqlConnection, codes, administrativeArealType, commandTimeout, cancellationToken);
        }
        /// <summary>
        /// Asynchronously retrieves a collection of sub-codes that start with the specified code prefix from the database, excluding the exact code match.
        /// </summary>
        /// <param name="code">The parent code or prefix used to filter and identify the associated sub-codes.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of strings containing the matching sub-codes, or <c>null</c> if the database connection could not be established.</returns>
        public async Task<HashSet<string>?> GetSubCodesAsync(string? code, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetSubCodesAsync(npgsqlConnection, code, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously refreshes the administrative areal 2D data within the PostgreSQL database.
        /// <para>Every row's parent chain (<c>country_id</c> / <c>voivodeship_id</c> / <c>county_id</c> / <c>municipality_id</c>) is cleared and rebuilt, so a re-run repairs whatever the previous one got wrong. Parents are searched one administrative level at a time, nearest level first, and the walk stops at the first level that yields a match - a row whose own level directly above holds no parent is filed under the nearest ancestor that does. Matching within a level, including its majority-overlap fallback, is <see cref="Modify.UpdateIds(AdministrativeAreal2D, IEnumerable{AdministrativeAreal2D}, double)"/>.</para>
        /// <para>At each level the candidates are first narrowed to the rows the row's own <c>code</c> names (<see cref="Query.AdministrativeCodeKey(string, AdministrativeArealType)"/>), and geometry only chooses between those - a code names several rows when a unit's territory is disconnected. Geometry alone is not enough: it silently files a row under whichever neighbour's bounding box happens to be the only one holding the sample point.</para>
        /// <para>Real BDOT10k data needs all three. Poznan (<c>3064</c>) has no <c>gmina</c> feature at all, so its subdivisions have no Municipality to match; a handful of settlements sit in a gap between municipality polygons; and before this every one of Poznan's 113 subdivisions was wrong - 87 with a null chain (https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/14) and 26 filed under county <c>3021</c>, along with 10 more rows elsewhere in the country (https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/15).</para>
        /// </summary>
        /// <param name="postgreSQLAdministrativeAreal2DRefreshOptions">The options used to configure the refresh process. If null, a new instance of <see cref="PostgreSQLAdministrativeAreal2DRefreshOptions"/> is initialized.</param>
        /// <param name="progress">The provider for reporting the progress of the refresh operation as a long value.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while carrying out the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the refresh was successful; otherwise, false.</returns>
        public async Task<bool> RefreshAsync(PostgreSQLAdministrativeAreal2DRefreshOptions? postgreSQLAdministrativeAreal2DRefreshOptions = default, IProgress<long>? progress = default, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            postgreSQLAdministrativeAreal2DRefreshOptions ??= new PostgreSQLAdministrativeAreal2DRefreshOptions();

            Dictionary<AdministrativeArealType, List<AdministrativeAreal2D>?> dictionary = [];

            // Each loaded level is also indexed by the code slice naming it, so a row searching for a
            // parent looks up the handful of rows its own code names instead of scanning the level.
            Dictionary<AdministrativeArealType, Dictionary<string, List<AdministrativeAreal2D>>> dictionary_Code = [];

            List<AdministrativeArealType> administrativeArealTypes = [.. Enum.GetValues<AdministrativeArealType>().Cast<AdministrativeArealType>()];
            administrativeArealTypes.Remove(AdministrativeArealType.Undefined);
            administrativeArealTypes.Sort();

            int count = administrativeArealTypes.Count;

            int totalUpdated = 0;

            for (int i = 0; i < count; i++)
            {
                AdministrativeArealType administrativeArealType = administrativeArealTypes[i];

                List<AdministrativeAreal2D>? administrativeAreal2Ds_Current = await GetAdministrativeAreal2DsByAdministrativeArealTypeAsync(administrativeArealType, commandTimeout, cancellationToken);
                administrativeAreal2Ds_Current?.RemoveAll(x => x.BoundingBox2D == null);

                if (administrativeAreal2Ds_Current != null && administrativeAreal2Ds_Current.Count != 0)
                {
                    foreach (AdministrativeAreal2D administrativeAreal2D_Current in administrativeAreal2Ds_Current)
                    {
                        administrativeAreal2D_Current.ResetIds();
                    }

                    if (i != 0)
                    {
                        foreach (AdministrativeAreal2D administrativeAreal2D_Current in administrativeAreal2Ds_Current)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            totalUpdated++;
                            progress?.Report(totalUpdated);

                            // Levels are searched nearest-first and the walk stops at the first one that
                            // yields a parent. Searching only the level directly above is not enough:
                            // Poznan (3064) has no gmina feature in BDOT10k at all, so its subdivisions
                            // have no Municipality to match and would keep every parent id null.
                            // See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/14.
                            for (int j = i - 1; j >= 0; j--)
                            {
                                AdministrativeArealType administrativeArealType_Parent = administrativeArealTypes[j];

                                List<AdministrativeAreal2D>? administrativeAreal2Ds_Parent;

                                if (Query.AdministrativeCodeKey(administrativeAreal2D_Current.Code, administrativeArealType_Parent) is string key)
                                {
                                    // The row's own code names its ancestor at this level, so only the rows
                                    // that code names are eligible - geometry still chooses between them
                                    // when the code names several polygon parts. Without this a row can be
                                    // filed under a neighbour whose bounding box happens to be the only one
                                    // holding the sample point, which is how every one of Poznan's 113
                                    // subdivisions ended up either unparented or under county 3021.
                                    // See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/15.
                                    if (!dictionary_Code[administrativeArealType_Parent].TryGetValue(key, out administrativeAreal2Ds_Parent))
                                    {
                                        // The code names no row at this level - a city with no gmina feature
                                        // of its own - so the answer is at the level above, not here.
                                        continue;
                                    }
                                }
                                else
                                {
                                    // Country carries no code relation, so it stays a plain geometric search.
                                    administrativeAreal2Ds_Parent = dictionary[administrativeArealType_Parent];
                                }

                                if (administrativeAreal2Ds_Parent is null || administrativeAreal2Ds_Parent.Count == 0)
                                {
                                    continue;
                                }

                                if (Modify.UpdateIds(administrativeAreal2D_Current, administrativeAreal2Ds_Parent, postgreSQLAdministrativeAreal2DRefreshOptions.Tolerance))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                dictionary[administrativeArealType] = administrativeAreal2Ds_Current;

                // Indexed by the slice of the row's own code that names this level, which is exactly the
                // slice a row one level down computes from its own code - Query.AdministrativeCodeKey is
                // both sides of the join.
                Dictionary<string, List<AdministrativeAreal2D>> dictionary_Code_Current = [];
                if (administrativeAreal2Ds_Current is not null)
                {
                    foreach (AdministrativeAreal2D administrativeAreal2D_Current in administrativeAreal2Ds_Current)
                    {
                        if (Query.AdministrativeCodeKey(administrativeAreal2D_Current.Code, administrativeArealType) is not string key)
                        {
                            continue;
                        }

                        if (!dictionary_Code_Current.TryGetValue(key, out List<AdministrativeAreal2D>? administrativeAreal2Ds_Key))
                        {
                            administrativeAreal2Ds_Key = [];
                            dictionary_Code_Current[key] = administrativeAreal2Ds_Key;
                        }

                        administrativeAreal2Ds_Key.Add(administrativeAreal2D_Current);
                    }
                }

                dictionary_Code[administrativeArealType] = dictionary_Code_Current;
            }

            List<AdministrativeAreal2D> administrativeAreal2Ds = [];
            foreach (KeyValuePair<AdministrativeArealType, List<AdministrativeAreal2D>?> keyValuePair in dictionary)
            {
                if (keyValuePair.Value is not null && keyValuePair.Value.Count > 0)
                {
                    administrativeAreal2Ds.AddRange(keyValuePair.Value);
                }
            }

            HashSet<int>? indexes = await UpdateAsync(administrativeAreal2Ds, commandTimeout, cancellationToken);

            return indexes is not null && indexes.Count != 0;
        }

        /// <summary>
        /// Asynchronously removes administrative areal 2D records from the database based on the provided identifiers.
        /// </summary>
        /// <param name="ids">An optional collection of integer identifiers for the records to be removed.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of IDs that were successfully deleted, or null if the input was null or a database connection could not be established.</returns>
        public async Task<HashSet<int>?> RemoveAsync(IEnumerable<int>? ids, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (ids is null)
            {
                return null;
            }

            // Creating the connection using the shared PostgreSQL infrastructure
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            HashSet<int> result = [];

            if (!ids.Any())
            {
                return result;
            }

            await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection);
            npgsqlBatch.Timeout = commandTimeout;

            foreach (int id in ids)
            {
                // Using RETURNING id to confirm which record was actually deleted
                NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                    DELETE FROM {TableName.AdministrativeAreal2D}
                    WHERE id = @id
                    RETURNING id;");

                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer) { Value = id });

                npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
            }

            // Execute the batch
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync(cancellationToken);

            do
            {
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    // If the record existed and was deleted, its ID is returned here
                    int id = npgsqlDataReader.GetInt32(0);
                    result.Add(id);
                }
            }
            while (await npgsqlDataReader.NextResultAsync(cancellationToken));

            return result;
        }

        /// <summary>
        /// Asynchronously updates a collection of administrative areal 2D records in the PostgreSQL database.
        /// </summary>
        /// <param name="administrativeAreal2Ds">The collection of <see cref="AdministrativeAreal2D"/> objects to be updated. This value can be null.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of integers representing the IDs of the records that were processed, or null if the update process failed or the input was null.</returns>
        public async Task<HashSet<int>?> UpdateAsync(IEnumerable<AdministrativeAreal2D>? administrativeAreal2Ds, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (administrativeAreal2Ds is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool succeded = await Create.TableAsync_AdministrativeArea2D(npgsqlConnection, commandTimeout, cancellationToken);
            if (!succeded)
            {
                return null;
            }

            HashSet<int> result = [];

            if (!administrativeAreal2Ds.Any())
            {
                return result;
            }

            await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection);
            npgsqlBatch.Timeout = commandTimeout;

            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
            {
                if (administrativeAreal2D is null)
                {
                    continue;
                }

                // SQL with full update on conflict (excluding ID)
                NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                    INSERT INTO {TableName.AdministrativeAreal2D} (reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object)
                    VALUES (@reference, @code, @name, @type_id, @min_x, @min_y, @max_x, @max_y, @country_id, @voivodeship_id, @county_id, @municipality_id, @object)
                    ON CONFLICT (reference)
                    DO UPDATE SET
                        code = EXCLUDED.code,
                        name = EXCLUDED.name,
                        type_id = EXCLUDED.type_id,
                        min_x = EXCLUDED.min_x,
                        min_y = EXCLUDED.min_y,
                        max_x = EXCLUDED.max_x,
                        max_y = EXCLUDED.max_y,
                        country_id = EXCLUDED.country_id,
                        voivodeship_id = EXCLUDED.voivodeship_id,
                        county_id = EXCLUDED.county_id,
                        municipality_id = EXCLUDED.municipality_id,
                        object = EXCLUDED.object
                    RETURNING id;");

                BoundingBox2D? boundingBox2D = administrativeAreal2D.BoundingBox2D;

                // Adding parameters with explicit NpgsqlDbType
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = administrativeAreal2D.Reference });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Text) { Value = (object?)administrativeAreal2D.Code ?? DBNull.Value });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = (object?)administrativeAreal2D.Name ?? DBNull.Value });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("type_id", NpgsqlDbType.Smallint) { Value = (short)administrativeAreal2D.AdministrativeArealType });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_x", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Min.X });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("min_y", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Min.Y });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_x", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Max.X });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("max_y", NpgsqlDbType.Double) { Value = boundingBox2D is null ? double.NaN : boundingBox2D.Max.Y });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("country_id", NpgsqlDbType.Integer) { Value = (object?)administrativeAreal2D.CountryId ?? DBNull.Value });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("voivodeship_id", NpgsqlDbType.Integer) { Value = (object?)administrativeAreal2D.VoivodeshipId ?? DBNull.Value });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = (object?)administrativeAreal2D.CountyId ?? DBNull.Value });
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("municipality_id", NpgsqlDbType.Integer) { Value = (object?)administrativeAreal2D.MunicipalityId ?? DBNull.Value });

                // Handling potential null for JSONB object
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb)
                {
                    Value = (object?)administrativeAreal2D.Object?.ToJsonString() ?? DBNull.Value
                });

                npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
            }

            // Execute batch and collect IDs
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync(cancellationToken);

            do
            {
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    // The RETURNING id works for both INSERT and UPDATE cases
                    int id = npgsqlDataReader.GetInt32(0);
                    result.Add(id);
                }
            }
            while (await npgsqlDataReader.NextResultAsync(cancellationToken));

            return result;
        }
        private static AdministrativeAreal2D Create_AdministrativeAreal2D(NpgsqlDataReader npgsqlDataReader)
        {
            return new AdministrativeAreal2D
            {
                Id = npgsqlDataReader.GetInt32(0),
                Reference = npgsqlDataReader.GetString(1),
                Code = npgsqlDataReader.GetString(2),
                Name = npgsqlDataReader.GetString(3),
                AdministrativeArealType = (AdministrativeArealType)npgsqlDataReader.GetInt32(4),
                BoundingBox2D = new BoundingBox2D(
                        new Point2D(npgsqlDataReader.IsDBNull(5) ? double.NaN : npgsqlDataReader.GetDouble(5), npgsqlDataReader.IsDBNull(6) ? double.NaN : npgsqlDataReader.GetDouble(6)),
                        new Point2D(npgsqlDataReader.IsDBNull(7) ? double.NaN : npgsqlDataReader.GetDouble(7), npgsqlDataReader.IsDBNull(8) ? double.NaN : npgsqlDataReader.GetDouble(8))),
                CountryId = npgsqlDataReader.IsDBNull(9) ? null : npgsqlDataReader.GetInt32(9),
                VoivodeshipId = npgsqlDataReader.IsDBNull(10) ? null : npgsqlDataReader.GetInt32(10),
                CountyId = npgsqlDataReader.IsDBNull(11) ? null : npgsqlDataReader.GetInt32(11),
                MunicipalityId = npgsqlDataReader.IsDBNull(12) ? null : npgsqlDataReader.GetInt32(12),
                Object = JsonNode.Parse(npgsqlDataReader.GetString(13)) as JsonObject,
                CreatedAt = npgsqlDataReader.IsDBNull(14) ? null : npgsqlDataReader.GetDateTime(14),
            };
        }

        private static AdministrativeAreal2D Create_AdministrativeAreal2D_NoObject(NpgsqlDataReader npgsqlDataReader)
        {
            return new AdministrativeAreal2D
            {
                Id = npgsqlDataReader.GetInt32(0),
                Reference = npgsqlDataReader.GetString(1),
                Code = npgsqlDataReader.GetString(2),
                Name = npgsqlDataReader.GetString(3),
                AdministrativeArealType = (AdministrativeArealType)npgsqlDataReader.GetInt32(4),
                BoundingBox2D = new BoundingBox2D(
                        new Point2D(npgsqlDataReader.IsDBNull(5) ? double.NaN : npgsqlDataReader.GetDouble(5), npgsqlDataReader.IsDBNull(6) ? double.NaN : npgsqlDataReader.GetDouble(6)),
                        new Point2D(npgsqlDataReader.IsDBNull(7) ? double.NaN : npgsqlDataReader.GetDouble(7), npgsqlDataReader.IsDBNull(8) ? double.NaN : npgsqlDataReader.GetDouble(8))),
                CountryId = npgsqlDataReader.IsDBNull(9) ? null : npgsqlDataReader.GetInt32(9),
                VoivodeshipId = npgsqlDataReader.IsDBNull(10) ? null : npgsqlDataReader.GetInt32(10),
                CountyId = npgsqlDataReader.IsDBNull(11) ? null : npgsqlDataReader.GetInt32(11),
                MunicipalityId = npgsqlDataReader.IsDBNull(12) ? null : npgsqlDataReader.GetInt32(12),
                CreatedAt = npgsqlDataReader.IsDBNull(13) ? null : npgsqlDataReader.GetDateTime(13),
            };
        }

        private static AdministrativeAreal2DReference Create_AdministrativeAreal2DReference(NpgsqlDataReader npgsqlDataReader)
        {
            return new AdministrativeAreal2DReference
            {
                Id = npgsqlDataReader.GetInt32(0),
                Reference = npgsqlDataReader.GetString(1),
                Code = npgsqlDataReader.GetString(2),
                Name = npgsqlDataReader.GetString(3),
                AdministrativeArealType = (AdministrativeArealType)npgsqlDataReader.GetInt32(4),
                CountryId = npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetInt32(5),
                VoivodeshipId = npgsqlDataReader.IsDBNull(6) ? null : npgsqlDataReader.GetInt32(6),
                CountyId = npgsqlDataReader.IsDBNull(7) ? null : npgsqlDataReader.GetInt32(7),
                MunicipalityId = npgsqlDataReader.IsDBNull(8) ? null : npgsqlDataReader.GetInt32(8),
            };
        }

        private static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByBoundingBox2D_NoObjectAsync(
            NpgsqlConnection? npgsqlConnection,
            double searchMinX,
            double searchMinY,
            double searchMaxX,
            double searchMaxY,
            AdministrativeArealType administrativeArealType,
            AdministrativeArealType administrativeArealType_Parent,
            HashSet<int> parentIds,
            HashSet<int> excludedIds,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Undefined)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Country)
            {
                if (parentIds != null && parentIds.Count != 0)
                {
                    return [];
                }

                string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY));";

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

                return await ReadAsync_AdministrativeAreal2D_NoObject(npgsqlCommand, cancellationToken);
            }

            if (parentIds is null || parentIds.Count == 0)
            {
                return [];
            }

            // Filtered against whichever ancestor the caller actually matched, which is not the level
            // directly above wherever a level is missing from the source data.
            string? parentIdColumnName = Query.IdColumnName(administrativeArealType_Parent);
            if (string.IsNullOrWhiteSpace(parentIdColumnName))
            {
                return [];
            }

            bool hasExclusions = excludedIds != null && excludedIds.Count > 0;
            string excludedFilter = hasExclusions ? "AND id != ALL(@excludedIds)" : "";

            string commandText2 = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    {excludedFilter}
                    AND ({parentIdColumnName} = ANY(@parentIds) OR {parentIdColumnName} IS NULL)
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY));";

            await using NpgsqlCommand npgsqlCommand2 = new(commandText2, npgsqlConnection);
            npgsqlCommand2.CommandTimeout = commandTimeout;

            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("parentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = parentIds.ToArray() });

            if (hasExclusions)
            {
                npgsqlCommand2.Parameters.Add(new NpgsqlParameter("excludedIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = excludedIds!.ToArray() });
            }

            return await ReadAsync_AdministrativeAreal2D_NoObject(npgsqlCommand2, cancellationToken);
        }

        private static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByBoundingBox2DAsync(NpgsqlConnection? npgsqlConnection, BoundingBox2D? boundingBox2D, AdministrativeArealType administrativeArealType, HashSet<int> parentIds, HashSet<int> excludedIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            // Check if point2D or the list of IDs is null/empty
            if (npgsqlConnection is null || boundingBox2D is null)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Undefined)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Country)
            {
                if (parentIds != null && parentIds.Count != 0)
                {
                    return [];
                }

                return await GetAdministrativeAreal2DsByBoundingBox2DAsync(npgsqlConnection, boundingBox2D, administrativeArealType, tolerance, commandTimeout, cancellationToken);
            }

            if (parentIds is null || parentIds.Count == 0)
            {
                return [];
            }

            string? parentIdColumnName = Query.ParentIdColumnName(administrativeArealType);
            if (string.IsNullOrWhiteSpace(parentIdColumnName))
            {
                return [];
            }

            double searchMinX = boundingBox2D.Min.X - tolerance;
            double searchMinY = boundingBox2D.Min.Y - tolerance;
            double searchMaxX = boundingBox2D.Max.X + tolerance;
            double searchMaxY = boundingBox2D.Max.Y + tolerance;

            // The GiST index on box(point(min_x, min_y), point(max_x, max_y)) serves the '&&' overlap operator.
            string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    AND id != ALL(@excludedIds)
                    AND ({parentIdColumnName} = ANY(@parentIds) OR {parentIdColumnName} IS NULL)
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY));";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

            // Passing the IEnumerable<int> as an array parameter
            // Npgsql automatically maps C# arrays/collections to PostgreSQL arrays
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("parentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
            {
                Value = parentIds.ToArray()
            });

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("excludedIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
            {
                Value = excludedIds?.ToArray() ?? []
            });

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        private static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByPoint2D_NoObjectAsync(
            NpgsqlConnection? npgsqlConnection,
            double searchMinX,
            double searchMinY,
            double searchMaxX,
            double searchMaxY,
            AdministrativeArealType administrativeArealType,
            AdministrativeArealType administrativeArealType_Parent,
            HashSet<int> parentIds,
            HashSet<int> excludedIds,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Undefined)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Country)
            {
                if (parentIds != null && parentIds.Count != 0)
                {
                    return [];
                }

                string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY));";

                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

                return await ReadAsync_AdministrativeAreal2D_NoObject(npgsqlCommand, cancellationToken);
            }

            if (parentIds is null || parentIds.Count == 0)
            {
                return [];
            }

            // Filtered against whichever ancestor the caller actually matched, which is not the level
            // directly above wherever a level is missing from the source data.
            string? parentIdColumnName = Query.IdColumnName(administrativeArealType_Parent);
            if (string.IsNullOrWhiteSpace(parentIdColumnName))
            {
                return [];
            }

            bool hasExclusions = excludedIds != null && excludedIds.Count > 0;
            string excludedFilter = hasExclusions ? "AND id != ALL(@excludedIds)" : "";

            string commandText2 = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    {excludedFilter}
                    AND ({parentIdColumnName} = ANY(@parentIds) OR {parentIdColumnName} IS NULL)
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY));";

            await using NpgsqlCommand npgsqlCommand2 = new(commandText2, npgsqlConnection);
            npgsqlCommand2.CommandTimeout = commandTimeout;

            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });
            npgsqlCommand2.Parameters.Add(new NpgsqlParameter("parentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = parentIds.ToArray() });

            if (hasExclusions)
            {
                npgsqlCommand2.Parameters.Add(new NpgsqlParameter("excludedIds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = excludedIds!.ToArray() });
            }

            return await ReadAsync_AdministrativeAreal2D_NoObject(npgsqlCommand2, cancellationToken);
        }

        private static async Task<List<AdministrativeAreal2D>?> GetAdministrativeAreal2DsByPoint2DAsync(NpgsqlConnection? npgsqlConnection, Point2D? point2D, AdministrativeArealType administrativeArealType, HashSet<int> parentIds, HashSet<int> excludedIds, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            // Check if point2D or the list of IDs is null/empty
            if (npgsqlConnection is null || point2D is null)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Undefined)
            {
                return [];
            }

            if (administrativeArealType == AdministrativeArealType.Country)
            {
                if (parentIds != null && parentIds.Count != 0)
                {
                    return [];
                }

                return await GetAdministrativeAreal2DsByPoint2DAsync(npgsqlConnection, point2D, administrativeArealType, tolerance, commandTimeout, cancellationToken);
            }

            if (parentIds is null || parentIds.Count == 0)
            {
                return [];
            }

            string? parentIdColumnName = Query.ParentIdColumnName(administrativeArealType);
            if (string.IsNullOrWhiteSpace(parentIdColumnName))
            {
                return [];
            }

            // 1. Prepare the dynamic part of the query for Excluded IDs
            // We check if we actually have any IDs to exclude to avoid the ALL() trap
            bool hasExclusions = excludedIds != null && excludedIds.Count > 0;
            string excludedFilter = hasExclusions ? "AND id != ALL(@excludedIds)" : "";

            // 2. Build the command text using the safe fragment.
            // The GiST index on box(point(min_x, min_y), point(max_x, max_y)) serves the '&&' overlap operator.
            double searchMinX = point2D.X - tolerance;
            double searchMinY = point2D.Y - tolerance;
            double searchMaxX = point2D.X + tolerance;
            double searchMaxY = point2D.Y + tolerance;

            string commandText = $@"
                SELECT id, reference, code, name, type_id, min_x, min_y, max_x, max_y, country_id, voivodeship_id, county_id, municipality_id, object, created_at
                FROM {TableName.AdministrativeAreal2D}
                WHERE type_id = @typeId
                    {excludedFilter}
                    AND ({parentIdColumnName} = ANY(@parentIds) OR {parentIdColumnName} IS NULL)
                    AND box(point(min_x, min_y), point(max_x, max_y)) && box(point(@searchMinX, @searchMinY), point(@searchMaxX, @searchMaxY));";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // 3. Add parameters - we only add excludedIds if it's actually used in the query
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinX", NpgsqlDbType.Double) { Value = searchMinX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMinY", NpgsqlDbType.Double) { Value = searchMinY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxX", NpgsqlDbType.Double) { Value = searchMaxX });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("searchMaxY", NpgsqlDbType.Double) { Value = searchMaxY });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)administrativeArealType });

            npgsqlCommand.Parameters.Add(new NpgsqlParameter("parentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
            {
                Value = parentIds.ToArray()
            });

            if (hasExclusions)
            {
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("excludedIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                {
                    Value = excludedIds!.ToArray()
                });
            }

            return await ReadAsync_AdministrativeAreal2D(npgsqlCommand, cancellationToken);
        }

        private static async Task PopulateObjectsAsync(NpgsqlConnection npgsqlConnection, IEnumerable<AdministrativeAreal2D> administrativeAreal2Ds, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            int[] ids = [.. administrativeAreal2Ds.Select(x => x.Id)];
            if (ids.Length == 0)
            {
                return;
            }

            Dictionary<int, JsonObject?> map = [];

            string commandText = $@"
                SELECT id, object
                FROM {TableName.AdministrativeAreal2D}
                WHERE id = ANY(@ids);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = ids });

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                int id = npgsqlDataReader.GetInt32(0);
                JsonObject? jsonObject = npgsqlDataReader.IsDBNull(1) ? null : JsonNode.Parse(npgsqlDataReader.GetString(1)) as JsonObject;
                map[id] = jsonObject;
            }

            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
            {
                if (map.TryGetValue(administrativeAreal2D.Id, out JsonObject? jsonObject))
                {
                    administrativeAreal2D.Object = jsonObject;
                }
            }
        }

        private static async Task<List<AdministrativeAreal2D>?> ReadAsync_AdministrativeAreal2D(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_AdministrativeAreal2D(npgsqlDataReader, cancellationToken);
        }

        private static async Task<List<AdministrativeAreal2D>> ReadAsync_AdministrativeAreal2D(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<AdministrativeAreal2D> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_AdministrativeAreal2D(npgsqlDataReader));
            }

            return result;
        }

        private static async Task<List<AdministrativeAreal2D>?> ReadAsync_AdministrativeAreal2D_NoObject(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_AdministrativeAreal2D_NoObject(npgsqlDataReader, cancellationToken);
        }

        private static async Task<List<AdministrativeAreal2D>> ReadAsync_AdministrativeAreal2D_NoObject(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<AdministrativeAreal2D> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_AdministrativeAreal2D_NoObject(npgsqlDataReader));
            }

            return result;
        }

        private static async Task<List<AdministrativeAreal2DReference>?> ReadAsync_AdministrativeAreal2DReference(NpgsqlCommand npgsqlCommand, CancellationToken cancellationToken = default)
        {
            if (npgsqlCommand is null)
            {
                return null;
            }

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);

            return await ReadAsync_AdministrativeAreal2DReference(npgsqlDataReader, cancellationToken);
        }

        private static async Task<List<AdministrativeAreal2DReference>> ReadAsync_AdministrativeAreal2DReference(NpgsqlDataReader npgsqlDataReader, CancellationToken cancellationToken = default)
        {
            List<AdministrativeAreal2DReference> result = [];

            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(Create_AdministrativeAreal2DReference(npgsqlDataReader));
            }

            return result;
        }
    }
}