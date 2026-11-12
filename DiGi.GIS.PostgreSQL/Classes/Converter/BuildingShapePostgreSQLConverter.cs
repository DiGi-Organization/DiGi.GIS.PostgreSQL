using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Stores and reads the fixed, ordered building shape labelling samples (<see cref="Constants.TableName.BuildingShapeSample"/>) and their manual labels (<see cref="Constants.TableName.BuildingShapeLabel"/>) (DiGi.GIS.PostgreSQL#135).
    /// <para>A sample is written once and never overwritten: every annotator labels the same items in the same <see cref="BuildingShapeSampleItem.Ordinal"/> order, which inter-annotator agreement needs. One label is kept per sample item and interpreter; a re-label replaces the interpreter's own row, while a second interpreter keeps a parallel one. A label is accepted only for an item of its sample. Both tables live on the Main database.</para>
    /// </summary>
    public class BuildingShapePostgreSQLConverter : PostgreSQLConverter<BuildingShapeLabel>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingShapePostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData"/> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be null.</param>
        public BuildingShapePostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table holding the labelling samples.
        /// </summary>
        public static string TableName_Sample => Constants.TableName.BuildingShapeSample;

        /// <summary>
        /// Gets the name of the database table holding the labels.
        /// </summary>
        public static string TableName_Label => Constants.TableName.BuildingShapeLabel;

        /// <summary>
        /// Asynchronously creates both tables (and their indexes) and analyzes them, so the query-planner statistics are populated from the first query.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if both tables exist afterwards; otherwise, false.</returns>
        public async Task<bool> CreateTableAsync(int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return false;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            bool result = await Create.TableAsync_BuildingShapeSample(npgsqlConnection, commandTimeout, cancellationToken) && await Create.TableAsync_BuildingShapeLabel(npgsqlConnection, commandTimeout, cancellationToken);
            if (result)
            {
                await DiGi.PostgreSQL.Modify.Analyze(npgsqlConnection, Constants.TableName.BuildingShapeSample, commandTimeout, cancellationToken);
                await DiGi.PostgreSQL.Modify.Analyze(npgsqlConnection, Constants.TableName.BuildingShapeLabel, commandTimeout, cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads the number of labels one interpreter has filed for the items of a sample.
        /// <para>Only labels of items that belong to the sample are counted. A missing table answers 0.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="interpreter">The interpreter whose labels are counted.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or null when the connection, the key or the interpreter is null or blank.</returns>
        public static async Task<int?> GetLabelCountAsync(NpgsqlConnection? npgsqlConnection, string? sampleKey, string? interpreter, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(sampleKey) || string.IsNullOrWhiteSpace(interpreter))
            {
                return null;
            }

            string commandText = $@"
                SELECT count(*)
                FROM {Constants.TableName.BuildingShapeLabel} l
                JOIN {Constants.TableName.BuildingShapeSample} s
                    ON s.sample_key = l.sample_key AND s.county_id = l.county_id AND s.reference = l.reference
                WHERE l.sample_key = @sample_key AND l.interpreter = @interpreter;";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = sampleKey });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("interpreter", NpgsqlDbType.Text) { Value = interpreter });

                object? value = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
                return value is long count ? (int)count : 0;
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return 0;
            }
        }

        /// <summary>
        /// Asynchronously reads the number of labels one interpreter has filed for the items of a sample, automatically managing the connection.
        /// </summary>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="interpreter">The interpreter whose labels are counted.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or null when nothing could be read.</returns>
        public async Task<int?> GetLabelCountAsync(string? sampleKey, string? interpreter, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetLabelCountAsync(npgsqlConnection, sampleKey, interpreter, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads every label filed for the items of a sample, by every interpreter, ordered by item ordinal and interpreter.
        /// <para>Labels of references that are not in the sample are not returned. A missing table answers an empty list.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the labels, or null when the connection or the key is null or blank.</returns>
        public static async Task<List<BuildingShapeLabel>?> GetLabelsAsync(NpgsqlConnection? npgsqlConnection, string? sampleKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(sampleKey))
            {
                return null;
            }

            string commandText = $@"
                SELECT l.sample_key, l.county_id, l.reference, l.shape, l.interpreter, l.note, l.updated_at
                FROM {Constants.TableName.BuildingShapeLabel} l
                JOIN {Constants.TableName.BuildingShapeSample} s
                    ON s.sample_key = l.sample_key AND s.county_id = l.county_id AND s.reference = l.reference
                WHERE l.sample_key = @sample_key
                ORDER BY s.ordinal, l.interpreter;";

            List<BuildingShapeLabel> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = sampleKey });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    result.Add(new BuildingShapeLabel(
                        npgsqlDataReader.GetString(0),
                        npgsqlDataReader.GetInt32(1),
                        npgsqlDataReader.GetString(2),
                        (BuildingShape)npgsqlDataReader.GetInt16(3),
                        npgsqlDataReader.GetString(4),
                        npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetString(5),
                        npgsqlDataReader.GetFieldValue<DateTimeOffset>(6)));
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads every label filed for the items of a sample, automatically managing the connection.
        /// </summary>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the labels, or null when nothing could be read.</returns>
        public async Task<List<BuildingShapeLabel>?> GetLabelsAsync(string? sampleKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetLabelsAsync(npgsqlConnection, sampleKey, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the next item of a sample that one interpreter has not labelled yet.
        /// <para>Without <paramref name="afterOrdinal"/> the unlabelled item with the lowest ordinal is returned. With it, the search starts after that ordinal and wraps around to the start, so a skipped item comes back only once the interpreter has passed every other unlabelled item. A missing table answers null.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="interpreter">The interpreter whose labels are excluded.</param>
        /// <param name="afterOrdinal">The ordinal of the item the interpreter skipped or last saw, or null to start from the beginning.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the next unlabelled item, or null when the interpreter has labelled every item, the sample does not exist, or the connection, the key or the interpreter is null or blank.</returns>
        public static async Task<BuildingShapeSampleItem?> GetNextSampleItemAsync(NpgsqlConnection? npgsqlConnection, string? sampleKey, string? interpreter, int? afterOrdinal = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(sampleKey) || string.IsNullOrWhiteSpace(interpreter))
            {
                return null;
            }

            string orderBy = afterOrdinal is null ? "s.ordinal" : "CASE WHEN s.ordinal <= @after_ordinal THEN 1 ELSE 0 END, s.ordinal";

            string commandText = $@"
                SELECT s.sample_key, s.ordinal, s.county_id, s.reference, s.solver_shape
                FROM {Constants.TableName.BuildingShapeSample} s
                WHERE s.sample_key = @sample_key
                    AND NOT EXISTS (
                        SELECT 1
                        FROM {Constants.TableName.BuildingShapeLabel} l
                        WHERE l.sample_key = s.sample_key AND l.county_id = s.county_id AND l.reference = s.reference AND l.interpreter = @interpreter)
                ORDER BY {orderBy}
                LIMIT 1;";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = sampleKey });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("interpreter", NpgsqlDbType.Text) { Value = interpreter });
                if (afterOrdinal is int afterOrdinal_Value)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("after_ordinal", NpgsqlDbType.Integer) { Value = afterOrdinal_Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                if (!await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                return new BuildingShapeSampleItem(
                    npgsqlDataReader.GetString(0),
                    npgsqlDataReader.GetInt32(1),
                    npgsqlDataReader.GetInt32(2),
                    npgsqlDataReader.GetString(3),
                    (BuildingShape)npgsqlDataReader.GetInt16(4));
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously reads the next item of a sample that one interpreter has not labelled yet, automatically managing the connection.
        /// </summary>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="interpreter">The interpreter whose labels are excluded.</param>
        /// <param name="afterOrdinal">The ordinal of the item the interpreter skipped or last saw, or null to start from the beginning.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the next unlabelled item, or null when there is none or nothing could be read.</returns>
        public async Task<BuildingShapeSampleItem?> GetNextSampleItemAsync(string? sampleKey, string? interpreter, int? afterOrdinal = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetNextSampleItemAsync(npgsqlConnection, sampleKey, interpreter, afterOrdinal, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the number of items of a sample.
        /// <para>A missing table answers 0.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or null when the connection or the key is null or blank.</returns>
        public static async Task<int?> GetSampleItemCountAsync(NpgsqlConnection? npgsqlConnection, string? sampleKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(sampleKey))
            {
                return null;
            }

            try
            {
                await using NpgsqlCommand npgsqlCommand = new($"SELECT count(*) FROM {Constants.TableName.BuildingShapeSample} WHERE sample_key = @sample_key;", npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = sampleKey });

                object? value = await npgsqlCommand.ExecuteScalarAsync(cancellationToken);
                return value is long count ? (int)count : 0;
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return 0;
            }
        }

        /// <summary>
        /// Asynchronously reads the number of items of a sample, automatically managing the connection.
        /// </summary>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count, or null when nothing could be read.</returns>
        public async Task<int?> GetSampleItemCountAsync(string? sampleKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetSampleItemCountAsync(npgsqlConnection, sampleKey, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads every item of a sample, ordered by ordinal.
        /// <para>A missing table answers an empty list.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the items, or null when the connection or the key is null or blank.</returns>
        public static async Task<List<BuildingShapeSampleItem>?> GetSampleItemsAsync(NpgsqlConnection? npgsqlConnection, string? sampleKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(sampleKey))
            {
                return null;
            }

            string commandText = $@"
                SELECT sample_key, ordinal, county_id, reference, solver_shape
                FROM {Constants.TableName.BuildingShapeSample}
                WHERE sample_key = @sample_key
                ORDER BY ordinal;";

            List<BuildingShapeSampleItem> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = sampleKey });

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    result.Add(new BuildingShapeSampleItem(
                        npgsqlDataReader.GetString(0),
                        npgsqlDataReader.GetInt32(1),
                        npgsqlDataReader.GetInt32(2),
                        npgsqlDataReader.GetString(3),
                        (BuildingShape)npgsqlDataReader.GetInt16(4)));
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads every item of a sample, automatically managing the connection.
        /// </summary>
        /// <param name="sampleKey">The identity of the sample.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the items, or null when nothing could be read.</returns>
        public async Task<List<BuildingShapeSampleItem>?> GetSampleItemsAsync(string? sampleKey, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await GetSampleItemsAsync(npgsqlConnection, sampleKey, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a new sample, refusing to touch a sample key that already holds rows.
        /// <para>Every item must carry the same non-blank sample key and a non-blank reference; otherwise nothing is written. The rows are written in one transaction, <paramref name="batchSize"/> at a time, one statement per batch carrying one array per column. A key that already holds rows, two items sharing a (county, reference) or an ordinal, and a concurrent insert of the same key are all refused, so a sample is never overwritten or mixed.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="buildingShapeSampleItems">The items of the sample.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true when the sample was written, false when it was refused (empty, inconsistent or already existing), or null when the connection or the items are null or the tables could not be created.</returns>
        public static async Task<bool?> InsertSampleAsync(NpgsqlConnection? npgsqlConnection, IEnumerable<BuildingShapeSampleItem?>? buildingShapeSampleItems, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || buildingShapeSampleItems is null)
            {
                return null;
            }

            List<BuildingShapeSampleItem> buildingShapeSampleItems_Valid = [];
            string? sampleKey = null;
            foreach (BuildingShapeSampleItem? buildingShapeSampleItem in buildingShapeSampleItems)
            {
                if (buildingShapeSampleItem is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(buildingShapeSampleItem.SampleKey) || string.IsNullOrWhiteSpace(buildingShapeSampleItem.Reference))
                {
                    return false;
                }

                sampleKey ??= buildingShapeSampleItem.SampleKey;
                if (!string.Equals(sampleKey, buildingShapeSampleItem.SampleKey, StringComparison.Ordinal))
                {
                    return false;
                }

                buildingShapeSampleItems_Valid.Add(buildingShapeSampleItem);
            }

            if (sampleKey is null || buildingShapeSampleItems_Valid.Count == 0)
            {
                return false;
            }

            if (!await Create.TableAsync_BuildingShapeSample(npgsqlConnection, commandTimeout, cancellationToken) || !await Create.TableAsync_BuildingShapeLabel(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            batchSize = Math.Max(1, batchSize);

            string commandText = $@"
                INSERT INTO {Constants.TableName.BuildingShapeSample} (sample_key, ordinal, county_id, reference, solver_shape)
                SELECT @sample_key, t.ordinal, t.county_id, t.reference, t.solver_shape
                FROM unnest(@ordinals, @county_ids, @references, @solver_shapes) AS t(ordinal, county_id, reference, solver_shape);";

            await using NpgsqlTransaction npgsqlTransaction = await npgsqlConnection.BeginTransactionAsync(cancellationToken);
            try
            {
                await using (NpgsqlCommand npgsqlCommand = new($"SELECT EXISTS (SELECT 1 FROM {Constants.TableName.BuildingShapeSample} WHERE sample_key = @sample_key);", npgsqlConnection, npgsqlTransaction))
                {
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = sampleKey });

                    if (await npgsqlCommand.ExecuteScalarAsync(cancellationToken) is true)
                    {
                        await npgsqlTransaction.RollbackAsync(cancellationToken);
                        return false;
                    }
                }

                for (int i = 0; i < buildingShapeSampleItems_Valid.Count; i += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<BuildingShapeSampleItem> buildingShapeSampleItems_Batch = buildingShapeSampleItems_Valid.GetRange(i, Math.Min(batchSize, buildingShapeSampleItems_Valid.Count - i));
                    int count = buildingShapeSampleItems_Batch.Count;

                    int[] ordinals = new int[count];
                    int[] countyIds = new int[count];
                    string[] references = new string[count];
                    short[] solverShapes = new short[count];

                    for (int j = 0; j < count; j++)
                    {
                        BuildingShapeSampleItem buildingShapeSampleItem = buildingShapeSampleItems_Batch[j];

                        ordinals[j] = buildingShapeSampleItem.Ordinal;
                        countyIds[j] = buildingShapeSampleItem.CountyId;
                        references[j] = buildingShapeSampleItem.Reference!;
                        solverShapes[j] = (short)buildingShapeSampleItem.SolverShape;
                    }

                    await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection, npgsqlTransaction);
                    npgsqlCommand.CommandTimeout = commandTimeout;
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = sampleKey });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("ordinals", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = ordinals });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = countyIds });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references });
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("solver_shapes", NpgsqlDbType.Array | NpgsqlDbType.Smallint) { Value = solverShapes });

                    await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await npgsqlTransaction.CommitAsync(cancellationToken);
                return true;
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                await npgsqlTransaction.RollbackAsync(CancellationToken.None);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously writes a new sample, automatically managing the connection.
        /// </summary>
        /// <param name="buildingShapeSampleItems">The items of the sample.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true when the sample was written, false when it was refused, or null when nothing could be written.</returns>
        public async Task<bool?> InsertSampleAsync(IEnumerable<BuildingShapeSampleItem?>? buildingShapeSampleItems, int batchSize = 1000, int commandTimeout = 120, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await InsertSampleAsync(npgsqlConnection, buildingShapeSampleItems, batchSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously stores one label, replacing the interpreter's earlier label of the same item.
        /// <para>The label is written only when its (sample key, county, reference) is an item of the sample; the conflict target is <c>(sample_key, county_id, reference, interpreter)</c>, so the same interpreter's re-label updates <c>shape</c>, <c>note</c> and <c>updated_at</c> in place while a different interpreter inserts a row of her or his own.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to connect to the database.</param>
        /// <param name="buildingShapeLabel">The label to store.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true when the label was stored, false when its item is not in the sample, or null when the connection or the label is null, the label lacks a sample key, reference or interpreter, or the tables could not be created.</returns>
        public static async Task<bool?> UpsertLabelAsync(NpgsqlConnection? npgsqlConnection, BuildingShapeLabel? buildingShapeLabel, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || buildingShapeLabel is null || string.IsNullOrWhiteSpace(buildingShapeLabel.SampleKey) || string.IsNullOrWhiteSpace(buildingShapeLabel.Reference) || string.IsNullOrWhiteSpace(buildingShapeLabel.Interpreter))
            {
                return null;
            }

            if (!await Create.TableAsync_BuildingShapeSample(npgsqlConnection, commandTimeout, cancellationToken) || !await Create.TableAsync_BuildingShapeLabel(npgsqlConnection, commandTimeout, cancellationToken))
            {
                return null;
            }

            string commandText = $@"
                INSERT INTO {Constants.TableName.BuildingShapeLabel} (sample_key, county_id, reference, shape, interpreter, note, updated_at)
                SELECT s.sample_key, s.county_id, s.reference, @shape, @interpreter, @note, @updated_at
                FROM {Constants.TableName.BuildingShapeSample} s
                WHERE s.sample_key = @sample_key AND s.county_id = @county_id AND s.reference = @reference
                ON CONFLICT (sample_key, county_id, reference, interpreter) DO UPDATE SET
                    shape = EXCLUDED.shape,
                    note = EXCLUDED.note,
                    updated_at = EXCLUDED.updated_at;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("sample_key", NpgsqlDbType.Text) { Value = buildingShapeLabel.SampleKey });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_id", NpgsqlDbType.Integer) { Value = buildingShapeLabel.CountyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = buildingShapeLabel.Reference });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("shape", NpgsqlDbType.Smallint) { Value = (short)buildingShapeLabel.BuildingShape });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("interpreter", NpgsqlDbType.Text) { Value = buildingShapeLabel.Interpreter });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("note", NpgsqlDbType.Text) { Value = (object?)buildingShapeLabel.Note ?? DBNull.Value });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("updated_at", NpgsqlDbType.TimestampTz) { Value = buildingShapeLabel.DateTime.UtcDateTime });

            return await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken) > 0;
        }

        /// <summary>
        /// Asynchronously stores one label, automatically managing the connection.
        /// </summary>
        /// <param name="buildingShapeLabel">The label to store.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true when the label was stored, false when its item is not in the sample, or null when nothing could be stored.</returns>
        public async Task<bool?> UpsertLabelAsync(BuildingShapeLabel? buildingShapeLabel, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);
            return await UpsertLabelAsync(npgsqlConnection, buildingShapeLabel, commandTimeout, cancellationToken);
        }
    }
}
