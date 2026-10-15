using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Asynchronously creates the AdministrativeArea2D table in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_AdministrativeArea2D(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            // Using timestamptz to ensure consistent time tracking across different time zones
            string commandText = $@"
                CREATE EXTENSION IF NOT EXISTS unaccent;

                CREATE TABLE IF NOT EXISTS {Constants.TableName.AdministrativeAreal2D} (
                    id SERIAL PRIMARY KEY,
                    reference TEXT NOT NULL UNIQUE,
                    code TEXT,
                    name TEXT,
                    type_id SMALLINT NOT NULL,
                    min_x DOUBLE PRECISION,
                    min_y DOUBLE PRECISION,
                    max_x DOUBLE PRECISION,
                    max_y DOUBLE PRECISION,
                    country_id INT,
                    voivodeship_id INT,
                    county_id INT,
                    municipality_id INT,
                    object JSONB,
                    created_at timestamptz DEFAULT now()
                );

                -- 1. Spatial index using GiST and box type for fast Bounding Box searches
                -- This index directly supports the '&&' operator used in GetAdministrativeAreal2DsByBoundingBox2DAsync
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AdministrativeAreal2D}_bbox
                ON {Constants.TableName.AdministrativeAreal2D} USING gist (box(point(min_x, min_y), point(max_x, max_y)));

                -- 2. Index for type filtering (often used together with spatial queries)
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AdministrativeAreal2D}_type_id
                ON {Constants.TableName.AdministrativeAreal2D} (type_id);

                -- 3. Composite indices for hierarchical administrative lookups
                -- These speed up filtering by voivodeship, county, etc.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AdministrativeAreal2D}_hierarchy
                ON {Constants.TableName.AdministrativeAreal2D} (voivodeship_id, county_id, municipality_id);

                -- 4. Composite index for type and code lookups
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AdministrativeAreal2D}_type_code
                ON {Constants.TableName.AdministrativeAreal2D} (type_id, code);

                -- 5. Index for child queries filtering by county_id
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AdministrativeAreal2D}_county_id
                ON {Constants.TableName.AdministrativeAreal2D} (county_id);";

            try
            {
                // Explicitly using NpgsqlCommand type instead of implicit typing
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the AdministrativeArea2DReferencedObject table for the specified table name.
        /// <para><c>reference</c> is what every read of this table filters on, so it carries an index of its own. <c>unique_id</c> needs none: the <c>UNIQUE</c> constraint on it is already an index, and a second one on the same column would only cost storage and write time.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="tableName">The name of the table associated with the administrative area 2D referenced object.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_AdministrativeArea2DReferencedObject(this NpgsqlConnection? npgsqlConnection, string tableName, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            // Combined command: create the table and the index supporting reads by reference.
            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    id SERIAL PRIMARY KEY,
                    unique_id TEXT NOT NULL UNIQUE,
                    reference TEXT NOT NULL,
                    object JSONB,
                    created_at timestamptz DEFAULT now()
                );

                -- An area holds one row per stored object, so reference is not unique here. It is
                -- however the only column reads filter on, and without this index every one of them
                -- is a sequential scan of the whole table.
                CREATE INDEX IF NOT EXISTS idx_{tableName}_reference
                ON {tableName} (reference);";

            try
            {
                // Explicitly using NpgsqlCommand type instead of implicit typing
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                // Logging the error to console - in ASP.NET Core we will later replace this with ILogger
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_AdministrativeArea2DReferencedObject)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the partitioned <see cref="Building"/> table along with its supporting composite index, if it does not already exist.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Building(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            // Combined command: Create partitioned table and the supporting index
            // The index on the parent table will be inherited by all child partitions.
            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.Building} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    lod SMALLINT,
                    year SMALLINT,
                    min_x DOUBLE PRECISION,
                    min_y DOUBLE PRECISION,
                    min_z DOUBLE PRECISION,
                    max_x DOUBLE PRECISION,
                    max_y DOUBLE PRECISION,
                    max_z DOUBLE PRECISION,
                    object JSONB,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id)
                ) PARTITION BY LIST (county_id);

                -- Optimization: Composite index for County + Reference + LOD + Year
                -- NULLS NOT DISTINCT ensures ON CONFLICT works even when lod or year are NULL (PostgreSQL 15+)
                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.Building}_ref_lod_year
                ON {Constants.TableName.Building} (county_id, reference, lod, year) NULLS NOT DISTINCT;

                -- CRITICAL: Spatial index using GiST and box type.
                -- Enables an R-Tree lookup for the bounding box fallback in GetBuildingByReferenceAsync
                -- instead of a sequential scan across the county partition.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.Building}_bbox
                ON {Constants.TableName.Building} USING gist (box(point(min_x, min_y), point(max_x, max_y)));

                -- Supports GetBuildingByLatestCreatedAtAsync, which is ORDER BY created_at DESC LIMIT 1.
                -- Without it that query is a sequential scan plus sort across every partition, and it is
                -- what the import's resume prompt blocks on before any work can start.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.Building}_created_at
                ON {Constants.TableName.Building} (created_at DESC);
                ";

            try
            {
                // Explicitly using NpgsqlCommand type instead of implicit typing
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                // Logging the error to console - in ASP.NET Core we will later replace this with ILogger
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_Building)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates a partition for the <see cref="Building"/> table based on the specified county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The unique identifier of the county for which the partition is being created.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Building_Partition(this NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(Constants.TableName.Building, countyId);

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }

        /// <summary>
        /// Asynchronously creates the Building2D table in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The Npgsql connection instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Building2D(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            // Combined command: Create partitioned table and the supporting index
            // The index on the parent table will be inherited by all child partitions.
            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.Building2D} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    code TEXT,
                    min_x DOUBLE PRECISION,
                    min_y DOUBLE PRECISION,
                    max_x DOUBLE PRECISION,
                    max_y DOUBLE PRECISION,
                    subdivision_id INT,
                    object JSONB,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id),
                    UNIQUE (reference, county_id)
                ) PARTITION BY LIST (county_id);

                -- 1. CRITICAL: Spatial index using GiST and box type.
                -- This is essential for Bounding Box searches at the scale of millions of buildings.
                -- It allows PostgreSQL to perform an R-Tree search instead of a Sequential Scan.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.Building2D}_bbox
                ON {Constants.TableName.Building2D} USING gist (box(point(min_x, min_y), point(max_x, max_y)));

                -- 2. Hierarchy index: Subdivision filtering.
                -- Useful for grouping buildings within estates or specific technical zones.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.Building2D}_subdivision_id
                ON {Constants.TableName.Building2D} (subdivision_id);
                ";

            try
            {
                // Explicitly using NpgsqlCommand type instead of implicit typing
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates a partition for the Building2D table associated with the specified county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The integer identifier of the county for which the partition is being created.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the Building2D partition was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Building2D_Partition(this NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(Constants.TableName.Building2D, countyId);

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }

        /// <summary>
        /// Asynchronously creates the Building 2D reference table in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="tableName">The name of the table to be created for Building 2D references.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other methods as a token for cancelling the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Building2DReference(this NpgsqlConnection? npgsqlConnection, string? tableName, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || string.IsNullOrWhiteSpace(tableName))
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    subdivision_id INT,
                    created_at timestamptz DEFAULT now(),
                    claimed_at timestamptz,
                    attempts INT NOT NULL DEFAULT 0,
                    PRIMARY KEY (id, county_id)
                );

                ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS claimed_at timestamptz;
                ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS attempts INT NOT NULL DEFAULT 0;

                CREATE UNIQUE INDEX IF NOT EXISTS idx_{tableName}_county_id_reference
                    ON {tableName} (county_id, reference);

                CREATE INDEX IF NOT EXISTS idx_{tableName}_created_at
                    ON {tableName} (created_at ASC);

                CREATE INDEX IF NOT EXISTS idx_{tableName}_claimed_at
                    ON {tableName} (claimed_at ASC NULLS FIRST, created_at ASC);
                ";

            try
            {
                // Explicitly specifying NpgsqlCommand type
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException npgsqlException)
            {
                Serilog.Modify.Log(npgsqlException, "{Method} failed for table {TableName}", nameof(TableAsync_Building2DReference), tableName);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the Building 2D Referenced Object table for the specified table name.
        /// <para>The two constraints carry the addressing convention described on <see cref="Building2DReferencedObject{TUniqueObject}"/>. <c>UNIQUE (county_id, unique_id)</c> makes one <b>stored object</b> the unit of a row, and the absence of any constraint on <c>(county_id, reference)</c> is deliberate: a building may hold several rows here, so writes append rather than replace.</para>
        /// <para>Do not add a unique constraint on <c>(county_id, reference)</c> to stop the table growing on re-runs. It would reduce the table to one row per building and discard every record after the first. The plain index created on that pair is not a constraint and places no such restriction on what may be stored.</para>
        /// <para>Indexes: <c>(county_id, reference)</c> is the primary access path and every read filters on it, so it carries an index. <c>(county_id, unique_id)</c> carries none of its own, because the <c>UNIQUE</c> constraint is already an index on exactly those columns in that order.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="tableName">The <see cref="System.String"/> representing the name of the table to be created.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The task result is a <see cref="System.Boolean"/> value indicating whether the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Building2DReferencedObject(this NpgsqlConnection? npgsqlConnection, string tableName, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            // Combined command: create the partitioned table and the index supporting reads by
            // reference. An index on the parent is inherited by every partition, existing and future.
            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    unique_id TEXT NOT NULL,
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    object JSONB,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id),
                    UNIQUE (county_id, unique_id)
                ) PARTITION BY LIST (county_id);

                -- The primary access path of the table. Deliberately not unique: a building holds one
                -- row per stored object, so several rows share a (county_id, reference). county_id
                -- leads because it matches the partition key and is the sole filter of
                -- GetReferencesAsync.
                CREATE INDEX IF NOT EXISTS idx_{tableName}_county_id_reference
                ON {tableName} (county_id, reference);";

            try
            {
                // Explicitly using NpgsqlCommand type instead of implicit typing
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                // Logging the error to console - in ASP.NET Core we will later replace this with ILogger
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_Building2DReferencedObject)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates a partition for the Building2DReferencedObject table based on the specified table name and county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="tableName">The name of the parent table that is being partitioned.</param>
        /// <param name="countyId">The integer identifier of the county for which the partition is created.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Building2DReferencedObject_Partition(this NpgsqlConnection? npgsqlConnection, string tableName, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(tableName, countyId);

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.BuildingSolarRadiation"/> table and its indexes, partitioned by county part.
        /// <para>One row holds one building's <see cref="GIS.Classes.BuildingSolarRadiationResult"/>: the <c>object</c> column carries it with every surface's shading factors removed, and <c>shading_factors</c> carries those factors packed into one <c>bytea</c> (<see cref="Convert.ToSystem_Bytes(System.Collections.Generic.IEnumerable{byte[]})"/>), compressed with lz4. Keeping the factors out of the JSONB avoids a third more for base64 and lets lz4 collapse the night-time zeros.</para>
        /// <para>The key <c>(county_id, building_model_id, radius, solver_version)</c> is unique and every column of it is NOT NULL, so <c>NULLS NOT DISTINCT</c> is not needed. The weather columns describe the stored summary and are not part of the key. Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_BuildingSolarRadiation(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.BuildingSolarRadiation} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    building_model_id BIGINT NOT NULL,
                    radius DOUBLE PRECISION NOT NULL,
                    solver_version INT NOT NULL,
                    weather_source_type INT NOT NULL,
                    weather_source_id TEXT,
                    weather_period_start timestamptz,
                    weather_period_end timestamptz,
                    user_email TEXT,
                    object JSONB NOT NULL,
                    shading_factors BYTEA COMPRESSION lz4 NOT NULL,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id)
                ) PARTITION BY LIST (county_id);

                -- The key of a stored result and the conflict target of every write: one row per building model, radius and solver version.
                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingSolarRadiation}_key
                ON {Constants.TableName.BuildingSolarRadiation} (county_id, building_model_id, radius, solver_version);

                -- The fallback read, which looks for the building model under every county part.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingSolarRadiation}_building_model_id
                ON {Constants.TableName.BuildingSolarRadiation} (building_model_id);

                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingSolarRadiation}_county_id_reference
                ON {Constants.TableName.BuildingSolarRadiation} (county_id, reference);
                ";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_BuildingSolarRadiation)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the partition of the <see cref="Constants.TableName.BuildingSolarRadiation"/> table for one county part.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The id of the county part the partition holds.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_BuildingSolarRadiation_Partition(this NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(Constants.TableName.BuildingSolarRadiation, countyId);

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_BuildingSolarRadiation_Partition)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the epw_file table in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_EPWFile(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.EPWFile} (
                    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    name TEXT NOT NULL UNIQUE,
                    x DOUBLE PRECISION NOT NULL,
                    y DOUBLE PRECISION NOT NULL,
                    object JSONB NOT NULL,
                    created_at timestamptz DEFAULT now()
                );
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.EPWFile}_location
                ON {Constants.TableName.EPWFile} USING gist ((point(x, y)));
            ";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_EPWFile)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the OrtoDatas table in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the OrtoDatas table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoDatas(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            // Combined command: Create partitioned table and the supporting index
            // The index on the parent table will be inherited by all child partitions.
            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.OrtoDatas} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    min_x DOUBLE PRECISION,
                    min_y DOUBLE PRECISION,
                    max_x DOUBLE PRECISION,
                    max_y DOUBLE PRECISION,
                    subdivision_id INT,
                    object JSONB,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id)
                ) PARTITION BY LIST (county_id);

                -- Index for subdivision (already in your code)
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoDatas}_subdivision
                ON {Constants.TableName.OrtoDatas} (subdivision_id);

                -- Optimization: Composite index for County + Reference
                -- This is highly effective because of your partitioning strategy.
                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoDatas}_ref
                ON {Constants.TableName.OrtoDatas} (county_id, reference);
                ";

            try
            {
                // Explicitly using NpgsqlCommand type instead of implicit typing
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                // Logging the error to console - in ASP.NET Core we will later replace this with ILogger
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OrtoDatas)}): {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Asynchronously creates a partition for the OrtoDatas table based on the specified county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The unique identifier of the county for which the partition is being created.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoDatas_Partition(this NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(Constants.TableName.OrtoDatas, countyId);

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }
        
        /// <summary>
        /// Asynchronously creates the partitioned <see cref="Constants.TableName.TerrainPoint"/> table along with its supporting indexes in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_TerrainPoint(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            // The primary key is what makes a re-import idempotent: it is the conflict target of the
            // ON CONFLICT DO NOTHING that every write goes through, so a county can be imported twice,
            // and overlapping source tiles can repeat a point, without either aborting the import.
            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.TerrainPoint} (
                    county_id INT NOT NULL,
                    subdivision_id INT,
                    x DOUBLE PRECISION NOT NULL,
                    y DOUBLE PRECISION NOT NULL,
                    z DOUBLE PRECISION NOT NULL,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (county_id, x, y)
                ) PARTITION BY LIST (county_id);

                -- 2D Geometric GiST Index: Fast R-Tree spatial indexing for bounding box queries
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.TerrainPoint}_point
                ON {Constants.TableName.TerrainPoint} USING gist ((point(x, y)));

                -- Hierarchy index: Subdivision filtering within a county
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.TerrainPoint}_subdivision_id
                ON {Constants.TableName.TerrainPoint} (subdivision_id);
                ";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_TerrainPoint)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates a partition for the <see cref="Constants.TableName.TerrainPoint"/> table based on the specified county identifier.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="countyId">The integer identifier of the county for which the partition is created.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_TerrainPoint_Partition(this NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(Constants.TableName.TerrainPoint, countyId);

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_TerrainPoint_Partition)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.Unit"/> table along with its supporting indexes in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_Unit(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.Unit} (
                    id TEXT PRIMARY KEY,
                    name TEXT,
                    level SMALLINT NOT NULL,
                    has_description BOOLEAN NOT NULL DEFAULT FALSE,
                    created_at timestamptz DEFAULT now()
                );

                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.Unit}_level
                ON {Constants.TableName.Unit} (level);

                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.Unit}_name
                ON {Constants.TableName.Unit} (name);
                ";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_Unit)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.StatisticalDataCollection"/> table in the PostgreSQL database.
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table was created successfully; otherwise, false.</returns>
        public static async Task<bool> TableAsync_StatisticalDataCollection(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.StatisticalDataCollection} (
                    id TEXT PRIMARY KEY,
                    object JSONB NOT NULL,
                    created_at timestamptz DEFAULT now()
                );";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_StatisticalDataCollection)}): {ex.Message}");
                return false;
            }
        }
    }
}