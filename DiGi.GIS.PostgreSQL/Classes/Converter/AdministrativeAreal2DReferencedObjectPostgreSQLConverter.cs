using DiGi.Core.Interfaces;
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
    /// Provides an abstract base implementation for a PostgreSQL converter specifically designed for administrative areal 2D referenced objects.
    /// </summary>
    /// <typeparam name="TAdministrativeAreal2DReferencedObject">The type of the administrative areal 2D referenced object.</typeparam>
    /// <typeparam name="TUniqueObject">The type of the unique object used for identification, which must implement the <see cref="IUniqueObject"/> interface.</typeparam>
    public abstract class AdministrativeAreal2DReferencedObjectPostgreSQLConverter<TAdministrativeAreal2DReferencedObject, TUniqueObject> : ReferencedObjectPostgreSQLConverter<TAdministrativeAreal2DReferencedObject, TUniqueObject> where TAdministrativeAreal2DReferencedObject : AdministrativeAreal2DReferencedObject<TUniqueObject> where TUniqueObject : IUniqueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrativeAreal2DReferencedObjectPostgreSQLConverter{TAdministrativeAreal2DReferencedObject, TUniqueObject}"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public AdministrativeAreal2DReferencedObjectPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Asynchronously retrieves the total count of records from the database.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count as a long integer.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Public member called on the closed converter type across the workspace; moving it is a breaking change.")]
        public static async Task<long> GetCountAsync(NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return -1;
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, Constants.TableName.AdministrativeAreal2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the total count of records.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds applied to the count command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total record count as a long integer.</returns>
        public async Task<long> GetCountAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, Constants.TableName.AdministrativeAreal2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an estimated count of records from the database.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="analyze">A boolean value indicating whether to perform an ANALYZE operation to update statistics before retrieving the estimate.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token used to propagate notification that the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated number of records as a nullable long, -1 when the table exists but has not been analysed, or null if the table does not exist or connection is null.</returns>
        public async Task<long?> GetEstimatedCountAsync(NpgsqlConnection? npgsqlConnection, bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            return await DiGi.PostgreSQL.Query.EstimatedCountAsync(npgsqlConnection, TableName, analyze, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously gets an estimated row count.
        /// </summary>
        /// <param name="analyze">A boolean indicating whether to run VACUUM ANALYZE before fetching the count.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to every command executed. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated number of rows as a nullable long, -1 when the table exists but has not been analysed, or null if an error occurs or the table does not exist.</returns>
        public async Task<long?> GetEstimatedCountAsync(bool analyze = false, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await DiGi.PostgreSQL.Query.EstimatedCountAsync(npgsqlConnection, TableName, analyze, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a <seeref name="TAdministrativeAreal2DReferencedObject"/> by its unique identifier.
        /// </summary>
        /// <param name="id">The integer identifier of the item to retrieve.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <seeref name="TAdministrativeAreal2DReferencedObject"/> if found; otherwise, null.</returns>
        public async Task<TAdministrativeAreal2DReferencedObject?> GetItemByIdAsync(int id, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            return await GetItemsByIdsAsync([id], commandTimeout: commandTimeout, cancellationToken: cancellationToken).ContinueWith(t => t.Result?.FirstOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an item of type <seeref name="TAdministrativeAreal2DReferencedObject"/> using the specified reference.
        /// <para>A reference can hold several rows, one per stored object, so this returns the most recently stored of them. Use <see cref="GetItemsByReferenceAsync"/> when the whole set is wanted.</para>
        /// </summary>
        /// <param name="reference">The string reference used to locate the object.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <seeref name="TAdministrativeAreal2DReferencedObject"/> if an item with the specified reference is found; otherwise, null.</returns>
        public async Task<TAdministrativeAreal2DReferencedObject?> GetItemByReferenceAsync(string reference, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return await GetItemsByReferencesAsync([reference], 1, commandTimeout, cancellationToken).ContinueWith(t => t.Result?.FirstOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D referenced objects based on the provided identifiers.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection" /> used to connect to the database.</param>
        /// <param name="ids">A collection of integers representing the unique identifiers of the objects to retrieve.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <seeref name="TAdministrativeAreal2DReferencedObject"/> matching the provided identifiers, or null if the connection or the collection of identifiers is null.</returns>
        public async Task<List<TAdministrativeAreal2DReferencedObject>?> GetItemsByIdsAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<int>? ids, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || ids is null)
            {
                return null;
            }

            if (!ids.Any())
            {
                return [];
            }

            // Base query using ANY operator for the array of IDs
            string commandText = $@"
                SELECT id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE id = ANY(@ids);";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Explicitly adding parameters to the command
            npgsqlCommand.Parameters.AddWithValue("ids", ids.ToArray());

            return await ReadAsync(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of administrative areal 2D referenced objects based on the specified identifiers.
        /// </summary>
        /// <param name="ids">A collection of <see cref="System.Int32"/> representing the unique identifiers of the items to retrieve. This parameter can be null.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{TAdministrativeAreal2DReferencedObject}"/> of matching objects, or null if no items are found or the provided identifiers collection is null.</returns>
        public async Task<List<TAdministrativeAreal2DReferencedObject>?> GetItemsByIdsAsync(IEnumerable<int>? ids, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (ids is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetItemsByIdsAsync(npgsqlConnection, ids, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of items that match the specified reference.
        /// </summary>
        /// <param name="reference">The string reference used to identify the items.</param>
        /// <param name="limit">An optional long integer specifying the maximum number of items to return.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <typeparamref name="TAdministrativeAreal2DReferencedObject"/> objects, or null if no items are found.</returns>
        public async Task<List<TAdministrativeAreal2DReferencedObject>?> GetItemsByReferenceAsync(string reference, long? limit = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return await GetItemsByReferencesAsync([reference], limit, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves items of type <typeparamref name="TAdministrativeAreal2DReferencedObject"/> based on the provided references.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection to use for the query.</param>
        /// <param name="references">A collection of strings representing the references used to identify the items to retrieve.</param>
        /// <param name="limit">An optional maximum number of items to retrieve. If null, no limit is applied.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching <typeparamref name="TAdministrativeAreal2DReferencedObject"/> items, or null if the connection or references are null.</returns>
        public async Task<List<TAdministrativeAreal2DReferencedObject>?> GetItemsByReferencesAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<string>? references, long? limit = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null)
            {
                return null;
            }

;

            if (!references.Any())
            {
                return [];
            }

            // Base query.
            // The ordering is not cosmetic: a reference can hold several rows - one per stored object -
            // so without it a limited read returns whichever row the plan happened to reach first, and
            // the answer changes with a vacuum or the heap ordering. created_at alone does not settle it
            // either, because now() is transaction start and a bulk write stamps every row of the batch
            // identically; id is what actually decides between them.
            string commandText = $@"
                SELECT id, unique_id, reference, object, created_at
                FROM {TableName}
                WHERE reference = ANY(@references)
                ORDER BY created_at DESC, id DESC";

            // Append LIMIT clause if limit has a value
            if (limit.HasValue)
            {
                commandText += " LIMIT @limit";
            }

            // Standard PostgreSQL statement termination
            commandText += ";";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            // Adding parameters
            npgsqlCommand.Parameters.AddWithValue("references", references.ToArray());

            if (limit.HasValue)
            {
                npgsqlCommand.Parameters.AddWithValue("limit", limit.Value);
            }

            return await ReadAsync(npgsqlCommand, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of items that match the specified references.
        /// </summary>
        /// <param name="references">An optional collection of strings representing the references of the objects to retrieve.</param>
        /// <param name="limit">An optional long integer specifying the maximum number of items to be returned.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <seeref name="TAdministrativeAreal2DReferencedObject"/> objects, or null if no matching items are found or the references collection is null.</returns>
        public async Task<List<TAdministrativeAreal2DReferencedObject>?> GetItemsByReferencesAsync(IEnumerable<string>? references, long? limit = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection == null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetItemsByReferencesAsync(npgsqlConnection, references, limit, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the specified collection of administrative areal 2D referenced objects.
        /// </summary>
        /// <param name="administrativeAreal2DReferencedObjects">The collection of <seeref name="TAdministrativeAreal2DReferencedObject"/> objects to be updated.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the batch. A value of 0 disables the timeout.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}" /> of integers representing the IDs of the updated objects, or <c>null</c> if no objects were updated.</returns>
        public async Task<HashSet<int>?> UpdateAsync(IEnumerable<TAdministrativeAreal2DReferencedObject>? administrativeAreal2DReferencedObjects, int commandTimeout = 30)
        {
            if (administrativeAreal2DReferencedObjects is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync();

            bool succeded = await PostgreSQL.Create.TableAsync_AdministrativeArea2DReferencedObject(npgsqlConnection, TableName);
            if (!succeded)
            {
                return null;
            }

            HashSet<int> result = [];

            if (!administrativeAreal2DReferencedObjects.Any())
            {
                return result;
            }

            await using NpgsqlBatch npgsqlBatch = new(npgsqlConnection);
            npgsqlBatch.Timeout = commandTimeout;

            foreach (TAdministrativeAreal2DReferencedObject administrativeAreal2DReferencedObject in administrativeAreal2DReferencedObjects)
            {
                if (administrativeAreal2DReferencedObject is null)
                {
                    continue;
                }

                // SQL with full update on conflict (excluding ID)
                NpgsqlBatchCommand npgsqlBatchCommand = new($@"
                    INSERT INTO {TableName} (unique_id, reference, object)
                    VALUES (@unique_id, @reference, @object)
                    ON CONFLICT (unique_id)
                    DO UPDATE SET
                        object = EXCLUDED.object,
                        reference = EXCLUDED.reference
                    RETURNING id;");

                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("unique_id", NpgsqlDbType.Text)
                {
                    Value = administrativeAreal2DReferencedObject.UniqueId
                });

                // Adding parameters with explicit NpgsqlDbType
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = administrativeAreal2DReferencedObject.Reference });

                // Handling potential null for JSONB object
                npgsqlBatchCommand.Parameters.Add(new NpgsqlParameter("object", NpgsqlDbType.Jsonb)
                {
                    Value = (object?)administrativeAreal2DReferencedObject.Object?.ToJsonString() ?? DBNull.Value
                });

                npgsqlBatch.BatchCommands.Add(npgsqlBatchCommand);
            }

            // Execute batch and collect IDs
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlBatch.ExecuteReaderAsync();

            do
            {
                while (await npgsqlDataReader.ReadAsync())
                {
                    // The RETURNING id works for both INSERT and UPDATE cases
                    int id = npgsqlDataReader.GetInt32(0);
                    result.Add(id);
                }
            }
            while (await npgsqlDataReader.NextResultAsync());

            return result;
        }

        /// <summary>
        /// Deletes single stored objects from the set held for one administrative area, naming each of them by its own unique identifier.
        /// <para>An area can hold several rows in this table - one per stored object - so this is the delete half of updating a single object: read the set with <see cref="GetItemsByReferenceAsync"/>, pick the one to change, remove it here, then write the replacement.</para>
        /// <para><c>unique_id</c> already identifies the row on its own - the table declares it <c>UNIQUE</c>. The reference is required as well and is matched as a guard, so a unique identifier belonging to a different area cannot silently take out that area's object.</para>
        /// <para>It removes data and has no undo - read <c>AI Guidelines/Coding - GIS Administrative Data.md</c> before calling it.</para>
        /// </summary>
        /// <param name="uniqueIds">The unique identifiers of the stored objects to delete.</param>
        /// <param name="reference">The reference of the administrative area the objects belong to.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the identifiers of the rows actually deleted, which is how many of the unique identifiers were really there.</returns>
        public async Task<HashSet<int>?> RemoveByUniqueIdsAsync(IEnumerable<string>? uniqueIds, string reference, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (uniqueIds is null || string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            string[] uniqueIds_Array = [.. uniqueIds];
            if (uniqueIds_Array.Length == 0)
            {
                return [];
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            // ANY keeps this one statement rather than one per identifier, and RETURNING reports what was
            // really removed - the count is the only evidence that the delete matched what was intended.
            string commandText = $@"
                DELETE FROM {TableName}
                WHERE reference = @reference
                  AND unique_id = ANY(@uniqueIds)
                RETURNING id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.AddWithValue("reference", reference);
            npgsqlCommand.Parameters.AddWithValue("uniqueIds", uniqueIds_Array);

            HashSet<int> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                result.Add(npgsqlDataReader.GetInt32(0));
            }

            return result;
        }

        /// <summary>
        /// Creates a new instance of a <seeref name="TAdministrativeAreal2DReferencedObject"/>.
        /// </summary>
        /// <param name="id">The integer identifier for the object.</param>
        /// <param name="uniqueId">The optional unique string identifier for the object.</param>
        /// <param name="reference">The optional reference string associated with the object.</param>
        /// <param name="object">The optional <see cref="JsonObject"/> containing additional data for the object.</param>
        /// <param name="createdAt">The optional <see cref="DateTime"/> indicating when the object was created.</param>
        /// <returns>A new instance of <seeref name="TAdministrativeAreal2DReferencedObject"/>.</returns>
        protected abstract TAdministrativeAreal2DReferencedObject Create(int id, string? uniqueId, string? reference, JsonObject? @object, DateTime? createdAt);

        /// <summary>
        /// Creates an instance of <seeref name="TAdministrativeAreal2DReferencedObject"/> using the data provided by the <see cref="NpgsqlDataReader"/>.
        /// </summary>
        /// <param name="npgsqlDataReader">The <see cref="NpgsqlDataReader"/> used to read the record from the database.</param>
        /// <returns>An instance of <seeref name="TAdministrativeAreal2DReferencedObject"/> populated with data from the reader.</returns>
        protected override TAdministrativeAreal2DReferencedObject Create(NpgsqlDataReader npgsqlDataReader)
        {
            return Create(npgsqlDataReader.GetInt32(0), npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetString(1), npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetString(2), JsonNode.Parse(npgsqlDataReader.GetString(3)) as JsonObject, npgsqlDataReader.IsDBNull(4) ? null : npgsqlDataReader.GetDateTime(4));
        }
    }
}