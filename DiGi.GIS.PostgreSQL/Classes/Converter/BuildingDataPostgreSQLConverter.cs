using DiGi.GIS.IO;
using DiGi.GIS.PostgreSQL.Interfaces;
using DiGi.PostgreSQL.Classes;
using DiGi.PostgreSQL.Table;
using DiGi.PostgreSQL.Table.Classes;
using DiGi.PostgreSQL.Table.Enums;
using Npgsql;
using NpgsqlTypes;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides a PostgreSQL converter for building data, facilitating the translation between building table representations
    /// using <see cref="Core.IO.Table.Classes.Column"/> and their corresponding PostgreSQL database structures, while implementing GIS-specific conversion functionality.
    /// </summary>
    public class BuildingDataPostgreSQLConverter : TablePostgreSQLConverter<Core.IO.Table.Classes.Column>, IGISPostgreSQLConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingDataPostgreSQLConverter" /> class.
        /// </summary>
        /// <param name="connectionData">The <see cref="ConnectionData" /> containing the connection settings required to establish a connection to the PostgreSQL database. This value can be <see langword="null"/>.</param>
        public BuildingDataPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the database table associated with building data.
        /// </summary>
        public override string TableName => Constants.TableName.BuildingData;

        /// <summary>
        /// Gets the table conversion options specifically configured for <see cref="Core.IO.Table.Classes.Column"/>.
        /// </summary>
        /// <value>
        /// A <see cref="TableConversionOptions{TColumn}"/> instance containing the configuration settings for the table conversion, or <c>null</c>.
        /// </value>
        protected override TableConversionOptions<Core.IO.Table.Classes.Column>? TableConversionOptions => new()
        {
            PrimaryKeyColumns = [IO.Constants.Column.CountyId, IO.Constants.Column.Reference],
            PartitioningOptions = new PartitioningOptions<Core.IO.Table.Classes.Column>()
            {
                Column = IO.Constants.Column.CountyId,
                PartitioningRule = new ValuePartitioningRule()
            }
        };

        /// <summary>
        /// Asynchronously retrieves a collection of unique values based on the specified column unique identifier and county identifier, applying optional dynamic filters.
        /// </summary>
        /// <typeparam name="T">The type of the values to be retrieved.</typeparam>
        /// <param name="columnUniqueId">The unique identifier of the column used for filtering; can be <see langword="null"/>.</param>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="filterGroup">The optional dynamic hierarchical filters to apply prior to retrieving the unique values.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a nullable collection of nullable elements of type <typeparam ref="T"/>, or null if no values are found.</returns>
        public async Task<IEnumerable<T?>?> GetUniqueValuesAsync<T>(string? columnUniqueId, int countyId, FilterGroup? filterGroup = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(columnUniqueId))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetUniqueValuesAsync<T>(npgsqlConnection, columnUniqueId, countyId, filterGroup, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a collection of unique values for a specified column identifier and county from the database, applying optional dynamic filters.
        /// </summary>
        /// <typeparam name="T">The type of the unique values to be retrieved.</typeparam>
        /// <param name="npgsqlConnection">The Npgsql connection instance used to execute the command.</param>
        /// <param name="columnUniqueId">The unique identifier of the column used to filter for unique values.</param>
        /// <param name="countyId">The integer identifier of the county.</param>
        /// <param name="filterGroup">The optional dynamic hierarchical filters to apply prior to retrieving the unique values.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of nullable values of type T, or null if no results are found, the connection is invalid, or <paramref name="columnUniqueId"/> does not name a stored column.</returns>
        public async Task<IEnumerable<T?>?> GetUniqueValuesAsync<T>(NpgsqlConnection? npgsqlConnection, string? columnUniqueId, int countyId, FilterGroup? filterGroup = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(columnUniqueId))
            {
                return null;
            }

            // The county is folded into the filters rather than written into the statement, so that every
            // read goes through the base method: it resolves each identifier against the stored column list
            // and rejects anything not on it before a statement is built. What this replaced pasted
            // columnUniqueId straight into SELECT DISTINCT, the WHERE clause and ORDER BY, and that value
            // arrives from a query string - an identifier cannot be parameterised, so the list is the guard.
            FilterGroup filterGroup_Combined = new()
            {
                LogicalOperator = FilterLogicalOperator.And
            };

            FilterCondition filterCondition_County = new()
            {
                ColumnUniqueId = IO.Constants.Column.CountyId.UniqueId(),
                FilterOperator = FilterOperator.Equals,
                Value = countyId
            };

            filterGroup_Combined.FilterConditions = [filterCondition_County];
            filterGroup_Combined.FilterGroups = filterGroup is null ? [] : [filterGroup];

            return await GetUniqueValuesAsync<T>(npgsqlConnection, columnUniqueId, filterGroup_Combined, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a table based on the specified references, county identifier, and optional column filters.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to communicate with the PostgreSQL database; may be null.</param>
        /// <param name="references">An <see cref="IEnumerable{T}"/> of <see cref="string"/> values representing the references to be pulled.</param>
        /// <param name="countyId">An optional integer identifying the specific county.</param>
        /// <param name="columnUniqueIds">An optional <see cref="IEnumerable{T}"/> of <see cref="string"/> unique identifiers for columns to include in the operation.</param>
        /// <param name="batchSize">The integer number of records to process per batch. Defaults to 1000.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Table"/> object if the data is successfully retrieved; otherwise, null.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync(
            NpgsqlConnection? npgsqlConnection,
            IEnumerable<string> references,
            int? countyId,
            IEnumerable<string>? columnUniqueIds = null,
            int batchSize = 1000,
            bool fallbackByReference = false,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || references is null || !references.Any())
            {
                return null;
            }

            if (countyId is null || !fallbackByReference)
            {
                HashSet<string>? columnUniqueIds_Temp = columnUniqueIds == null ? null : [.. columnUniqueIds];

                List<Core.IO.Table.Classes.Column> columns = await GetColumnsByUniqueIdsAsync(npgsqlConnection, columnUniqueIds_Temp, commandTimeout, cancellationToken) ?? [];

                Core.IO.Table.Classes.Table table = new(columns);

                Core.IO.Table.Classes.Column? column_Reference = table.UpdateColumn<Core.IO.Table.Classes.Column>(IO.Constants.Column.Reference);
                if (column_Reference is null)
                {
                    return null;
                }

                Core.IO.Table.Classes.Column? column_CountyId = countyId is null ? null : table.UpdateColumn<Core.IO.Table.Classes.Column>(IO.Constants.Column.CountyId);

                foreach (string reference in references)
                {
                    Dictionary<int, object?> values = [];
                    values[column_Reference.Index] = reference;
                    if (column_CountyId is not null)
                    {
                        values[column_CountyId.Index] = countyId;
                    }

                    table.AddRow(values);
                }

                await PullAsync(npgsqlConnection, table, batchSize, commandTimeout, cancellationToken);

                return table;
            }

            string[] references_Array = [.. references.Distinct()];

            string checkCommandText = $@"
                SELECT reference
                FROM ""{TableName}""
                WHERE reference = ANY(@references)
                  AND county_id = @countyId;";

            HashSet<string> inCountyReferences = [];
            await using (NpgsqlCommand checkCommand = new(checkCommandText, npgsqlConnection) { CommandTimeout = commandTimeout })
            {
                checkCommand.Parameters.Add(new NpgsqlParameter("references", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = references_Array });
                checkCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });

                await using NpgsqlDataReader checkReader = await checkCommand.ExecuteReaderAsync(cancellationToken);
                while (await checkReader.ReadAsync(cancellationToken))
                {
                    inCountyReferences.Add(checkReader.GetString(0));
                }
            }

            List<string> inCounty = [];
            List<string> missing = [];
            foreach (string reference in references)
            {
                if (inCountyReferences.Contains(reference))
                {
                    inCounty.Add(reference);
                }
                else
                {
                    missing.Add(reference);
                }
            }

            if (missing.Count == 0)
            {
                return await PullAsync(npgsqlConnection, references, countyId, columnUniqueIds, batchSize, fallbackByReference: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            }

            if (inCounty.Count == 0)
            {
                return await PullAsync(npgsqlConnection, references, null, columnUniqueIds, batchSize, fallbackByReference: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            }

            Core.IO.Table.Classes.Table? table_InCounty = await PullAsync(npgsqlConnection, inCounty, countyId, columnUniqueIds, batchSize, fallbackByReference: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            Core.IO.Table.Classes.Table? table_Missing = await PullAsync(npgsqlConnection, missing, null, columnUniqueIds, batchSize, fallbackByReference: false, commandTimeout: commandTimeout, cancellationToken: cancellationToken);

            if (table_InCounty is null)
            {
                return table_Missing;
            }

            if (table_Missing is not null)
            {
                foreach (Core.IO.Table.Classes.Row row in table_Missing.Rows)
                {
                    Dictionary<string, object?> rowValues = [];
                    foreach (Core.IO.Table.Classes.Column column in table_Missing.Columns)
                    {
                        if (column.Name is not null)
                        {
                            rowValues[column.Name] = row[column.Index];
                        }
                    }

                    table_InCounty.AddRow(rowValues);
                }
            }

            return table_InCounty;
        }

        /// <summary>
        /// Asynchronously retrieves data from the database based on specified column identifiers and values.
        /// </summary>
        /// <typeparam name="TObject">The type of the values used for filtering the table data.</typeparam>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance to be used for the database operation. Can be null.</param>
        /// <param name="columnUniqueId">The unique identifier of the column used as the primary filter.</param>
        /// <param name="values">A collection of values of type <typeparam ref="TObject"/> to retrieve from the table. Can be null.</param>
        /// <param name="columnUniqueIds">An optional collection of additional column unique identifiers to include in the retrieval process. Defaults to null.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Core.IO.Table.Classes.Table"/> object if data was successfully retrieved; otherwise, null.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync<TObject>(NpgsqlConnection? npgsqlConnection, string columnUniqueId, IEnumerable<TObject>? values, IEnumerable<string>? columnUniqueIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || values is null || !values.Any() || string.IsNullOrWhiteSpace(columnUniqueId))
            {
                return null;
            }

            HashSet<string>? columnUniqueIds_Temp = columnUniqueIds == null ? null : [.. columnUniqueIds];

            List<Core.IO.Table.Classes.Column> columns = await GetColumnsByUniqueIdsAsync(npgsqlConnection, columnUniqueIds_Temp, commandTimeout, cancellationToken) ?? [];

            Core.IO.Table.Classes.Table table = new(columns);

            await PullAsync(npgsqlConnection, table, columnUniqueId, values, commandTimeout, cancellationToken);

            return table;
        }

        /// <summary>
        /// Asynchronously retrieves a table record based on a specified column unique identifier and value.
        /// </summary>
        /// <typeparam name="TObject">The type of the object used for the retrieval operation.</typeparam>
        /// <param name="columnUniqueId">The unique identifier of the column to be used as the primary filter.</param>
        /// <param name="value">The value to search for in the specified column; can be null.</param>
        /// <param name="columnUniqueIds">An optional collection of string unique identifiers for additional columns to be retrieved.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Core.IO.Table.Classes.Table"/> instance if a matching record is found; otherwise, null.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync<TObject>(string columnUniqueId, object? value, IEnumerable<string>? columnUniqueIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(columnUniqueId))
            {
                return null;
            }

            HashSet<string>? columnUniqueIds_Temp = columnUniqueIds == null ? null : [.. columnUniqueIds];

            List<Core.IO.Table.Classes.Column> columns = await GetColumnsByUniqueIdsAsync(columnUniqueIds_Temp, commandTimeout, cancellationToken) ?? [];

            Core.IO.Table.Classes.Table table = new(columns);

            await PullAsync(table, columnUniqueId, value, commandTimeout, cancellationToken);

            return table;
        }

        /// <summary>
        /// Asynchronously retrieves a table record from the database based on a specified unique identifier and value.
        /// </summary>
        /// <typeparam name="TObject">The type of object associated with the data retrieval operation.</typeparam>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to communicate with the PostgreSQL database. Can be null.</param>
        /// <param name="columnUniqueId">The name of the column used as the unique identifier for filtering the record.</param>
        /// <param name="value">The value to match against the specified unique identifier column. Can be null.</param>
        /// <param name="columnUniqueIds">An optional collection of strings representing additional column identifiers to be processed. Can be null.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Core.IO.Table.Classes.Table"/> object if a matching record is found; otherwise, null.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync<TObject>(NpgsqlConnection? npgsqlConnection, string columnUniqueId, object? value, IEnumerable<string>? columnUniqueIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(columnUniqueId))
            {
                return null;
            }

            HashSet<string>? columnUniqueIds_Temp = columnUniqueIds == null ? null : [.. columnUniqueIds];

            List<Core.IO.Table.Classes.Column> columns = await GetColumnsByUniqueIdsAsync(npgsqlConnection, columnUniqueIds_Temp, commandTimeout, cancellationToken) ?? [];

            Core.IO.Table.Classes.Table table = new(columns);

            await PullAsync(npgsqlConnection, table, columnUniqueId, value, commandTimeout, cancellationToken);

            return table;
        }

        /// <summary>
        /// Asynchronously pulls data from a table based on the specified column unique identifiers and values.
        /// </summary>
        /// <typeparam name="TObject">The type of the objects contained in the values collection.</typeparam>
        /// <param name="columnUniqueId">The unique identifier of the primary column used for the pull operation.</param>
        /// <param name="values">An enumerable collection of <typeparamref name="TObject"/> values to be used as criteria. Can be null.</param>
        /// <param name="columnUniqueIds">An optional enumerable collection of additional unique identifiers for columns. Defaults to null.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Core.IO.Table.Classes.Table"/> instance if data is successfully retrieved; otherwise, null.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync<TObject>(string columnUniqueId, IEnumerable<TObject>? values, IEnumerable<string>? columnUniqueIds = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (values is null || !values.Any() || string.IsNullOrWhiteSpace(columnUniqueId))
            {
                return null;
            }

            HashSet<string>? columnUniqueIds_Temp = columnUniqueIds == null ? null : [.. columnUniqueIds];

            List<Core.IO.Table.Classes.Column> columns = await GetColumnsByUniqueIdsAsync(columnUniqueIds_Temp, commandTimeout, cancellationToken) ?? [];

            Core.IO.Table.Classes.Table table = new(columns);

            await PullAsync(table, columnUniqueId, values, commandTimeout, cancellationToken);

            return table;
        }

        /// <summary>
        /// Asynchronously retrieves a table based on the specified references, county identifier, and optional column filters.
        /// </summary>
        /// <param name="references">An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the references to pull.</param>
        /// <param name="countyId">An optional <see cref="int"/> representing the unique identifier of the county.</param>
        /// <param name="columnUniqueIds">An optional <see cref="IEnumerable{T}"/> of <see cref="string"/> specifying the unique identifiers of the columns to retrieve.</param>
        /// <param name="batchSize">An <see cref="int"/> specifying the number of records to process per batch. Defaults to 1000.</param>
        /// <param name="fallbackByReference">A boolean value indicating whether to perform a fallback query without county filtering for references not found in the specified county.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="Core.IO.Table.Classes.Table"/> object if successful; otherwise, <see langword="null"/>.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync(IEnumerable<string> references, int? countyId, IEnumerable<string>? columnUniqueIds = null, int batchSize = 1000, bool fallbackByReference = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await PullAsync(npgsqlConnection, references, countyId, columnUniqueIds, batchSize, fallbackByReference, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously pulls a keyset-paginated chunk of building data from a partition county over an already open connection.
        /// <para>Prefer this overload when paging a whole partition. The overload opening a connection of its own resolves the column metadata and opens a connection on every call, so paging a county of tens of thousands of rows through it costs one connection and one metadata query per page.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open connection the pull is executed on.</param>
        /// <param name="countyId">The partition key identifying the county.</param>
        /// <param name="columnUniqueIds">The optional list of column unique identifiers to project.</param>
        /// <param name="lastReference">The last reference string from the previous page used as the cursor seek-key.</param>
        /// <param name="pageSize">The page size count limit.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the async operation, returning the populated <see cref="Core.IO.Table.Classes.Table"/> if successful; otherwise, null.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<string>? columnUniqueIds, string? lastReference, int pageSize = 250, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            HashSet<string>? columnUniqueIds_Temp = columnUniqueIds == null ? null : [.. columnUniqueIds];
            List<Core.IO.Table.Classes.Column> columns = await GetColumnsByUniqueIdsAsync(npgsqlConnection, columnUniqueIds_Temp, commandTimeout, cancellationToken) ?? [];

            Core.IO.Table.Classes.Table table_Result = new(columns);
            table_Result.UpdateColumn<Core.IO.Table.Classes.Column>(IO.Constants.Column.Reference);
            table_Result.UpdateColumn<Core.IO.Table.Classes.Column>(IO.Constants.Column.CountyId);

            bool isSuccess = await PullAsync(
                npgsqlConnection,
                table_Result,
                IO.Constants.Column.Reference.UniqueId()!,
                lastReference,
                pageSize,
                countyId,
                commandTimeout,
                cancellationToken);

            return isSuccess ? table_Result : null;
        }

        /// <summary>
        /// Asynchronously pulls a keyset-paginated chunk of building data from a partition county, opening a connection of its own.
        /// <para>Each call opens a connection and resolves the column metadata, so a caller paging a whole partition should open one connection and use the overload taking it.</para>
        /// </summary>
        /// <param name="countyId">The partition key identifying the county.</param>
        /// <param name="columnUniqueIds">The optional list of column unique identifiers to project.</param>
        /// <param name="lastReference">The last reference string from the previous page used as the cursor seek-key.</param>
        /// <param name="pageSize">The page size count limit.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the async operation, returning the populated <see cref="Core.IO.Table.Classes.Table"/> if successful; otherwise, null.</returns>
        public async Task<Core.IO.Table.Classes.Table?> PullAsync(int countyId, IEnumerable<string>? columnUniqueIds, string? lastReference, int pageSize = 250, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection_Db = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection_Db is null)
            {
                return null;
            }
            await npgsqlConnection_Db.OpenAsync(cancellationToken);

            return await PullAsync(npgsqlConnection_Db, countyId, columnUniqueIds, lastReference, pageSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously pulls one page of a county partition in physical (heap) order over an already open connection, continuing after the position the previous page ended at.
        /// <para>This is the read to use for a whole partition. The keyset <see cref="PullAsync(NpgsqlConnection?, int, IEnumerable{string}?, string?, int, int, CancellationToken)"/> fetches every ~230-column row in reference order, one random heap read per row. For part 55417 (155 307 rows) that took 368-654 s on production, against about 15 s for a sequential read of a 100 543-row partition (DiGi.GIS.WebAPI.UI#29). Keep the keyset read for callers that need reference order.</para>
        /// <para>The page carries the requested columns plus <see cref="IO.Constants.Column.Reference"/> and <see cref="IO.Constants.Column.CountyId"/>, so every row carries the primary key. Windowing, sizing and the position format are those of the base <c>TablePostgreSQLConverter.PullByPhysicalOrderAsync</c>.</para>
        /// <para><b>Concurrent writes.</b> A row rewritten during a walk is read twice when its new version lands ahead of the walk, and missed when it lands behind. Callers dedup on <c>(county_id, reference)</c>. A walk on one connection inside one <c>REPEATABLE READ</c> transaction is exact, because the commands join the connection's transaction; <see cref="Create.TypologyAsync"/> does that.</para>
        /// <para>A server older than PostgreSQL 14 cannot serve the read, and the pull answers <c>(null, null)</c> as for any other failure. Test <c>DiGi.PostgreSQL.Table.Query.IsPhysicalOrderSupported(npgsqlConnection)</c> first and fall back to the keyset read.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open connection the pull is executed on.</param>
        /// <param name="countyId">The partition key identifying the county part.</param>
        /// <param name="columnUniqueIds">The optional list of column unique identifiers to project. Null projects every column.</param>
        /// <param name="lastPosition">The position the previous page returned, or <see langword="null"/> to start at the beginning of the partition.</param>
        /// <param name="pageSize">The maximum number of rows to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task whose result pairs the page with the position to continue from:
        /// <list type="bullet">
        /// <item>a position when the page came back full;</item>
        /// <item><see cref="string.Empty"/> when the partition is exhausted (the table may still hold the last rows);</item>
        /// <item><c>(null, null)</c> when the read failed or declined (no connection, a position that is not a tid, a server older than PostgreSQL 14).</item>
        /// </list></returns>
        public async Task<(Core.IO.Table.Classes.Table? Table, string? Position)> PullByPhysicalOrderAsync(NpgsqlConnection? npgsqlConnection, int countyId, IEnumerable<string>? columnUniqueIds, string? lastPosition, int pageSize = 250, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return (null, null);
            }

            HashSet<string>? columnUniqueIds_Temp = columnUniqueIds == null ? null : [.. columnUniqueIds];
            List<Core.IO.Table.Classes.Column> columns = await GetColumnsByUniqueIdsAsync(npgsqlConnection, columnUniqueIds_Temp, commandTimeout, cancellationToken) ?? [];

            Core.IO.Table.Classes.Table table_Result = new(columns);
            table_Result.UpdateColumn<Core.IO.Table.Classes.Column>(IO.Constants.Column.Reference);
            table_Result.UpdateColumn<Core.IO.Table.Classes.Column>(IO.Constants.Column.CountyId);

            string? position = await PullByPhysicalOrderAsync(npgsqlConnection, table_Result, lastPosition, pageSize, countyId, commandTimeout, cancellationToken);

            return position is null ? (null, null) : (table_Result, position);
        }

        /// <summary>
        /// Asynchronously pulls one page of a county partition in physical (heap) order, opening a connection of its own.
        /// <para>Each call opens a connection and resolves the column metadata, so a caller walking a whole partition on one machine should open one connection and use the overload taking it. That overload's remarks cover the position contract and concurrent writes. A walk made of separate calls, such as one page per HTTP request, cannot share a snapshot, so it can repeat or miss rows written during the walk.</para>
        /// </summary>
        /// <param name="countyId">The partition key identifying the county part.</param>
        /// <param name="columnUniqueIds">The optional list of column unique identifiers to project. Null projects every column.</param>
        /// <param name="lastPosition">The position the previous page returned, or <see langword="null"/> to start at the beginning of the partition.</param>
        /// <param name="pageSize">The maximum number of rows to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task whose result pairs the page with the position to continue from: a position when the page came back full, <see cref="string.Empty"/> when the partition is exhausted, <c>(null, null)</c> when the read failed or declined.</returns>
        public async Task<(Core.IO.Table.Classes.Table? Table, string? Position)> PullByPhysicalOrderAsync(int countyId, IEnumerable<string>? columnUniqueIds, string? lastPosition, int pageSize = 250, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection_Db = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection_Db is null)
            {
                return (null, null);
            }

            await npgsqlConnection_Db.OpenAsync(cancellationToken);

            return await PullByPhysicalOrderAsync(npgsqlConnection_Db, countyId, columnUniqueIds, lastPosition, pageSize, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously computes single-value aggregate statistics on a specific building data column inside a county partition or across all partitions, applying optional dynamic filters.
        /// </summary>
        /// <param name="columnUniqueId">The unique identifier of the column to aggregate.</param>
        /// <param name="singlevalueAggregateFunction">The single-value aggregate calculation function.</param>
        /// <param name="countyId">The optional partition county identifier. If null, aggregation is performed across all partitions.</param>
        /// <param name="filterGroup">The optional dynamic hierarchical filters to apply prior to aggregation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the async operation, returning the aggregate result as a <see cref="System.Text.Json.Nodes.JsonNode"/>.</returns>
        public async Task<System.Text.Json.Nodes.JsonNode?> GetAggregateSummaryAsync(string columnUniqueId, SinglevalueAggregateFunction singlevalueAggregateFunction, int? countyId = null, FilterGroup? filterGroup = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection_Db = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection_Db is null)
            {
                return null;
            }
            await npgsqlConnection_Db.OpenAsync(cancellationToken);

            return await GetAggregateSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection_Db, columnUniqueId, singlevalueAggregateFunction, countyId, filterGroup, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously computes several single-value aggregate measures - different columns, different functions - over the same scope (one optional partition, one optional filter) in a single database command and a single connection.
        /// <para>The measures arrive as one <see cref="System.Text.Json.Nodes.JsonArray"/> in request order; an aggregate that yields SQL <c>NULL</c> (for example <c>Avg</c> over zero rows) is preserved as JSON <c>null</c>. A measure naming a column the table does not hold fails the whole call - never a partial result (ZiolkowskiJakub/DiGi.GIS.WebAPI#56).</para>
        /// </summary>
        /// <param name="measures">The single-value aggregate measures to compute, each pairing a column unique identifier with its function.</param>
        /// <param name="countyId">The optional partition county identifier. If null, aggregation is performed across all partitions.</param>
        /// <param name="filterGroup">The optional dynamic hierarchical filters applied to every measure.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the async operation, returning the measures as a <see cref="System.Text.Json.Nodes.JsonArray"/> in request order, or <see langword="null"/> when the column metadata read failed.</returns>
        public async Task<System.Text.Json.Nodes.JsonNode?> GetAggregateSummariesAsync(IReadOnlyList<AggregateMeasure> measures, int? countyId = null, FilterGroup? filterGroup = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection_Db = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection_Db is null)
            {
                return null;
            }
            await npgsqlConnection_Db.OpenAsync(cancellationToken);

            return await GetAggregateSummariesAsync(npgsqlConnection_Db, measures, countyId, filterGroup, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously computes multi-value aggregate statistics on a specific building data column inside a county partition or across all partitions, applying optional dynamic filters.
        /// </summary>
        /// <param name="columnUniqueId">The unique identifier of the column to aggregate.</param>
        /// <param name="multivalueAggregateFunction">The multi-value aggregate calculation function.</param>
        /// <param name="countyId">The optional partition county identifier. If null, aggregation is performed across all partitions.</param>
        /// <param name="separator">The optional custom string delimiter; if null, it is automatically detected.</param>
        /// <param name="filterGroup">The optional dynamic hierarchical filters to apply prior to aggregation.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the async operation, returning the aggregate result as a <see cref="System.Text.Json.Nodes.JsonNode"/>.</returns>
        public async Task<System.Text.Json.Nodes.JsonNode?> GetAggregateSummaryAsync(string columnUniqueId, MultivalueAggregateFunction multivalueAggregateFunction, int? countyId = null, string? separator = null, FilterGroup? filterGroup = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection_Db = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection_Db is null)
            {
                return null;
            }
            await npgsqlConnection_Db.OpenAsync(cancellationToken);

            return await GetAggregateSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection_Db, columnUniqueId, multivalueAggregateFunction, countyId, separator, filterGroup, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously generates a value distribution histogram for a specific building data column inside a county partition or across all partitions, applying optional dynamic filters.
        /// <para>The buckets are of equal value width by default; <see cref="HistogramBucketing.EqualCount"/> asks for buckets of equal row count instead, which keeps the resolution of a skewed column (floor areas, heights) where its rows are (ZiolkowskiJakub/DiGi.PostgreSQL#7).</para>
        /// </summary>
        /// <param name="columnUniqueId">The unique identifier of the column to aggregate.</param>
        /// <param name="bucketCount">The total number of buckets to segment the value range (or the value-ordered rows) into.</param>
        /// <param name="countyId">The optional partition county identifier. If null, histogram is generated across all partitions.</param>
        /// <param name="filterGroup">The optional dynamic hierarchical filters to apply prior to generating the histogram.</param>
        /// <param name="histogramBucketing">The bucketing rule: equal value width (the default) or equal row count.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the async operation, returning the histogram aggregate result as a <see cref="System.Text.Json.Nodes.JsonArray"/>.</returns>
        public async Task<System.Text.Json.Nodes.JsonArray?> GetHistogramSummaryAsync(string columnUniqueId, int bucketCount, int? countyId = null, FilterGroup? filterGroup = null, HistogramBucketing histogramBucketing = HistogramBucketing.EqualWidth, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection_Db = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection_Db is null)
            {
                return null;
            }
            await npgsqlConnection_Db.OpenAsync(cancellationToken);

            return await GetHistogramSummaryAsync<Core.IO.Table.Classes.Column>(npgsqlConnection_Db, columnUniqueId, bucketCount, countyId, filterGroup, histogramBucketing, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the building data rows of one county partition, or of the whole table.
        /// <para>A county that has no partition answers -1 rather than 0, and the two mean different things: never written against written and empty. Reporting both as zero would hide a county no run has reached.</para>
        /// </summary>
        /// <param name="countyId">The optional identifier of the county partition to count. If null, the whole table is counted.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the row count, or -1 when there is no such partition or no connection could be built.</returns>
        public async Task<long> GetCountAsync(int? countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return -1;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string tableName = Constants.TableName.BuildingData;
            if (countyId is not null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.CountAsync(npgsqlConnection, tableName, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the building data rows of a county partition, or of the whole table, matching an optional dynamic hierarchical filter group.
        /// <para>This is the filtered <c>COUNT(*)</c>: the answer is the row count of the scope and does not depend on any column being non-null, so a sparse column is not mistaken for the scope's row count. A <c>null</c> or empty <paramref name="filterGroup"/> counts the whole partition scope; a filter that matches nothing answers <c>0</c>.</para>
        /// </summary>
        /// <param name="filterGroup">The optional dynamic hierarchical filters to apply prior to counting; <c>null</c> (or an empty group) counts the whole partition scope.</param>
        /// <param name="countyId">The optional identifier of the county partition to count. If null, the count is across all partitions.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the row count, <c>0</c> when nothing matches, or null when no connection could be built or a filter column's stored type cannot be resolved.</returns>
        public async Task<long?> GetCountByFilterGroupAsync(FilterGroup? filterGroup, int? countyId = null, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetCountAsync(npgsqlConnection, filterGroup, countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the planner's row estimate for one county partition, or for the whole table.
        /// <para>Far cheaper than counting on a partition of millions and accurate to a few percent, but it reflects the last time the partition was analysed rather than this moment.</para>
        /// <para>The estimate comes from <c>pg_class.reltuples</c>, which a partitioned parent carries as -1 until something analyses it - so a null <paramref name="countyId"/>, and a county partition that has never been analysed, both answer -1 rather than a number. Pass <paramref name="analyze"/> to settle it, or count instead.</para>
        /// </summary>
        /// <param name="countyId">The optional identifier of the county partition to estimate. If null, the whole table is estimated.</param>
        /// <param name="analyze">Runs an analysis before reading the estimate, which costs a scan but makes the answer current.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the estimated row count, -1 when the partition exists but has not been analysed, or null when there is no such partition or no connection could be built.</returns>
        public async Task<long?> GetEstimatedCountAsync(int? countyId, bool analyze = false, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string tableName = Constants.TableName.BuildingData;
            if (countyId is not null && countyId.HasValue)
            {
                tableName = string.Format("{0}_{1}", tableName, countyId.Value);
            }

            return await DiGi.PostgreSQL.Query.EstimatedCountAsync(npgsqlConnection, tableName, analyze, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the references the building data holds for one county.
        /// <para>Only the reference column is read. It is the counterpart of a table pull for a caller that wants to know which buildings are covered rather than what is stored about them - pulling the rows to reach one column would move every derived value of every building across the connection.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the set of references, an empty set when the county holds none, or null when no connection could be built.</returns>
        public async Task<HashSet<string>?> GetReferencesByCountyIdAsync(int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetReferencesByCountyIdAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the references the building data holds for one county, over the given connection.
        /// </summary>
        /// <param name="npgsqlConnection">The Npgsql connection instance used to execute the query. This value can be null.</param>
        /// <param name="countyId">The identifier of the county to read.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the set of references, an empty set when the county holds none, or null when the connection is null.</returns>
        public async Task<HashSet<string>?> GetReferencesByCountyIdAsync(NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            string commandText = $@"
                SELECT reference
                FROM {Constants.TableName.BuildingData}
                WHERE county_id = @countyId
                  AND reference IS NOT NULL;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });

            HashSet<string> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (!npgsqlDataReader.IsDBNull(0))
                {
                    result.Add(npgsqlDataReader.GetString(0));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the references the building data holds under more than one county, ordered by collision count descending.
        /// <para>A reference addresses one building of one county, so a reference filed under several counties is a defect rather than a fact about the data. It is what a write that resolved a reference outside the county it was processing leaves behind, and nothing removes it afterwards.</para>
        /// </summary>
        /// <param name="limit">The maximum number of references to return. Defaults to 100.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the duplicated references, an empty list when there are none, or null when no connection could be built.</returns>
        public async Task<List<Building2DReferenceDuplicate>?> GetDuplicateReferencesAsync(int limit = 100, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetDuplicateReferencesAsync(npgsqlConnection, limit, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the references the building data holds under more than one county, over the given connection.
        /// </summary>
        /// <param name="npgsqlConnection">The Npgsql connection instance used to execute the query. This value can be null.</param>
        /// <param name="limit">The maximum number of references to return. Defaults to 100.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the duplicated references, an empty list when there are none, or null when the connection is null.</returns>
        public static async Task<List<Building2DReferenceDuplicate>?> GetDuplicateReferencesAsync(NpgsqlConnection? npgsqlConnection, int limit = 100, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            // Grouped on the county rather than on the row: the primary key already holds a reference to one row
            // per county, so a reference appearing twice is by definition appearing under two counties.
            string commandText = $@"
                SELECT reference, COUNT(DISTINCT county_id) AS count, ARRAY_AGG(DISTINCT county_id ORDER BY county_id) AS county_ids
                FROM {Constants.TableName.BuildingData}
                WHERE reference IS NOT NULL
                GROUP BY reference
                HAVING COUNT(DISTINCT county_id) > 1
                ORDER BY count DESC
                LIMIT @limit;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit });

            List<Building2DReferenceDuplicate> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                string? reference = npgsqlDataReader.IsDBNull(0) ? null : npgsqlDataReader.GetString(0);
                long count = npgsqlDataReader.IsDBNull(1) ? 0 : npgsqlDataReader.GetInt64(1);
                int[]? countyIds = npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetFieldValue<int[]>(2);

                result.Add(new Building2DReferenceDuplicate(reference, count, countyIds));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the counties whose building data holds a row for one reference.
        /// <para>The targeted form of the duplicate reference read, for checking a single building. More than one county means the reference was written outside the county it belongs to.</para>
        /// </summary>
        /// <param name="reference">The building reference to look up.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the county identifiers in ascending order, an empty list when the reference is not stored, or null when no reference was given or no connection could be built.</returns>
        public async Task<List<int>?> GetCountyIdsByReferenceAsync(string? reference, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            string commandText = $@"
                SELECT DISTINCT county_id
                FROM {Constants.TableName.BuildingData}
                WHERE reference = @reference
                ORDER BY county_id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("reference", NpgsqlDbType.Text) { Value = reference });

            List<int> result = [];

            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                if (!npgsqlDataReader.IsDBNull(0))
                {
                    result.Add(npgsqlDataReader.GetInt32(0));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously moves the rows held for the given building references onto <paramref name="countyId"/>, so that they stay under the same county polygon part as the building itself.
        /// <para>Called after a building has been re-filed under the part its footprint lies in. A row left behind under the part the building came from is unreachable: every read here filters on <c>county_id</c> first. Nothing is deleted - a row the destination will not take is left where it is and its reference is not reported.</para>
        /// <para>The move, its collision guards and what it costs are described on <see cref="Modify.RefreshCountyIdsAsync(NpgsqlConnection, string, IEnumerable{string}, int, IEnumerable{int}, int, int, CancellationToken)"/>.</para>
        /// </summary>
        /// <param name="references">The building references known to belong to <paramref name="countyId"/>.</param>
        /// <param name="countyId">The identifier of the county polygon part the rows should be held under.</param>
        /// <param name="countyIds_Source">The parts the rows may currently sit under, normally the other parts of the same county code. When null every part is searched.</param>
        /// <param name="batchSize">The number of references sent in one statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the references that had at least one row moved, or null when no references were given or the connection could not be created.</returns>
        public async Task<HashSet<string>?> RefreshCountyIdsAsync(IEnumerable<string>? references, int countyId, IEnumerable<int>? countyIds_Source = null, int batchSize = 1000, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (references is null)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await npgsqlConnection.RefreshCountyIdsAsync(TableName, references, countyId, countyIds_Source, batchSize, commandTimeout, cancellationToken);
        }
    }
}
