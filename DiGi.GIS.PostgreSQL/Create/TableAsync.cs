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
                CREATE EXTENSION IF NOT EXISTS pg_trgm;

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
        /// Asynchronously creates the <see cref="Constants.TableName.AddressPoint"/> table and its indexes, together with the <c>pg_trgm</c> extension its search index needs.
        /// <para>One row is one PRG address point. <c>reference_id</c> (the PRG local identifier) is the only unique key, so a re-import of a newer version replaces the row rather than appending one, and a point that now falls in a sibling county part moves with it. The table is therefore not partitioned: a partitioned table would have to put <c>county_id</c> into that key. <c>county_id</c> is the county <b>part</b> id, never a county code (<c>Coding - GIS Administrative Data.md</c>).</para>
        /// <para><c>search_text</c> is normalised in C# by <see cref="Query.SearchText(string?)"/>, at import and at query time alike, and indexed with a GIN trigram index. <c>unaccent()</c> is not <c>IMMUTABLE</c>, so an index cannot be built on it, and a leading-wildcard <c>ILIKE</c> over it scans the whole table. Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_AddressPoint(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE EXTENSION IF NOT EXISTS pg_trgm;

                CREATE TABLE IF NOT EXISTS {Constants.TableName.AddressPoint} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    reference_id TEXT NOT NULL UNIQUE,
                    version timestamptz,
                    county_id INT NOT NULL,
                    building_reference TEXT,
                    match_type SMALLINT NOT NULL,
                    distance DOUBLE PRECISION,
                    street TEXT,
                    ulic_id TEXT,
                    house_number TEXT,
                    locality TEXT,
                    simc_id TEXT,
                    postal_code TEXT,
                    x DOUBLE PRECISION NOT NULL,
                    y DOUBLE PRECISION NOT NULL,
                    search_text TEXT NOT NULL,
                    object JSONB NOT NULL,
                    created_at timestamptz DEFAULT now()
                );

                -- Structured lookup: a street of a county part by its TERYT ULIC id and house number.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AddressPoint}_county_id_ulic_id_house_number
                ON {Constants.TableName.AddressPoint} (county_id, ulic_id, house_number);

                -- Reverse lookup: the address points of a building.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AddressPoint}_building_reference
                ON {Constants.TableName.AddressPoint} (building_reference);

                -- Serves both the similarity operator (%) and the partial match (LIKE '%text%') of the address search.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.AddressPoint}_search_text
                ON {Constants.TableName.AddressPoint} USING gin (search_text gin_trgm_ops);
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_AddressPoint)}): {ex.Message}");
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
        /// Asynchronously creates the <see cref="Constants.TableName.BuildingDataset"/> release register in the PostgreSQL database.
        /// <para>One row is one release of one open building dataset, and <c>(dataset_key, version, download_date)</c> is unique under <c>NULLS NOT DISTINCT</c> so that a re-import of the same release revises its row rather than adding one (DiGi.GIS.PostgreSQL#125). The unique constraint already leads with <c>dataset_key</c>, so a second index on that column alone would only cost storage and write time. The table lives on the Main database, beside <c>building_2d</c>. Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_BuildingDataset(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.BuildingDataset} (
                    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    dataset_key TEXT NOT NULL,
                    version TEXT NOT NULL,
                    download_date DATE NOT NULL,
                    source_url TEXT,
                    licence TEXT,
                    source_observation_date TEXT,
                    attribute_names JSONB NOT NULL,
                    file_name TEXT,
                    sha256 TEXT,
                    row_count BIGINT,
                    created_at timestamptz DEFAULT now(),
                    UNIQUE NULLS NOT DISTINCT (dataset_key, version, download_date)
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_BuildingDataset)}): {ex.Message}");
                return false;
            }
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
        /// Asynchronously creates the <see cref="Constants.TableName.GHS_OBAT_Building"/> centroid table in the PostgreSQL database.
        /// <para>One row is one GHS-OBAT centroid of one release, keyed by <c>overture_id</c> - the identifier the centroid links through to the Overture footprint - and filed under <c>county_id</c> (DiGi.GIS.PostgreSQL#125). Unlike the five footprint tables it is not partitioned: a centroid row is one small object, and the link key would otherwise have to carry the partition key to stay unique. <c>primary_source</c> is held here as in every other open building table because the acceptance criterion covers every stored row; the envelope columns are absent because a centroid's position lives inside <c>object</c>. The table lives on the Main database, beside <c>building_2d</c>. Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_GHS_OBAT_Building(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.GHS_OBAT_Building} (
                    overture_id TEXT PRIMARY KEY,
                    county_id INT NOT NULL,
                    dataset_id INT NOT NULL,
                    provenance JSONB,
                    primary_source TEXT NOT NULL,
                    object JSONB NOT NULL,
                    created_at timestamptz DEFAULT now()
                );

                -- The read by county, and the scoped delete a replacement release runs before it writes.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.GHS_OBAT_Building}_county_id
                ON {Constants.TableName.GHS_OBAT_Building} (county_id);";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_GHS_OBAT_Building)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates one of the five partitioned open building footprint tables (EUBUCCO, Overture, OSM, DBSM, Microsoft) along with its supporting indexes.
        /// <para>The table name is an identifier, and an identifier cannot be a parameter - so it is resolved against the stored list in <see cref="Query.IsOpenBuildingTableName(string?)"/> and rejected before any SQL is built, and only the names that survive are interpolated (DiGi.GIS.PostgreSQL#125; Coding - PostgreSQL.md section 5). The GHS-OBAT centroid table has its own shape and its own method.</para>
        /// <para>One row is one footprint of one release, filed under the county part it lies in and keyed by <c>(dataset_id, county_id, source_id)</c> under a <c>NULLS NOT DISTINCT</c> index, so a source identifier that is null still collides like any other and a re-import cannot append a duplicate. The GiST box index on the parent is inherited by every partition and serves the bounding-box prefilter. Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="tableName">The table to create; must be one of the five footprint table names.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OpenBuilding2D(this NpgsqlConnection? npgsqlConnection, string? tableName, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || !Query.IsOpenBuildingTableName(tableName) || string.Equals(tableName, Constants.TableName.GHS_OBAT_Building, StringComparison.Ordinal))
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    county_id INT NOT NULL,
                    dataset_id INT NOT NULL,
                    source_id TEXT,
                    provenance JSONB,
                    primary_source TEXT NOT NULL,
                    min_x DOUBLE PRECISION NOT NULL,
                    min_y DOUBLE PRECISION NOT NULL,
                    max_x DOUBLE PRECISION NOT NULL,
                    max_y DOUBLE PRECISION NOT NULL,
                    object JSONB NOT NULL,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id),
                    UNIQUE NULLS NOT DISTINCT (dataset_id, county_id, source_id)
                ) PARTITION BY LIST (county_id);

                -- The bounding-box prefilter of the spatial lookup: an R-Tree instead of a sequential scan
                -- across every partition, exactly as on building_2d. Inherited by every partition.
                CREATE INDEX IF NOT EXISTS idx_{tableName}_bbox
                ON {tableName} USING gist (box(point(min_x, min_y), point(max_x, max_y)));";

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OpenBuilding2D)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates a partition of one of the five partitioned open building footprint tables based on the specified county identifier.
        /// <para>The statement also analyses a partition it has just created, so a fresh partition reports its row count from the first query on (<see cref="Query.PartitionCommandText(string, int)"/>). The table name is resolved against <see cref="Query.IsOpenBuildingTableName(string?)"/> before any SQL is built.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="tableName">The partitioned parent table; must be one of the five footprint table names.</param>
        /// <param name="countyId">The identifier of the county the partition holds; it names the partition and is its single list value.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OpenBuilding2D_Partition(this NpgsqlConnection? npgsqlConnection, string? tableName, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || !Query.IsOpenBuildingTableName(tableName) || string.Equals(tableName, Constants.TableName.GHS_OBAT_Building, StringComparison.Ordinal))
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(tableName, countyId);

            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
                npgsqlCommand.CommandTimeout = commandTimeout;

                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OpenBuilding2D_Partition)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.OrtoSheet"/> table, the orthophoto sheet index, with its supporting indexes.
        /// <para>One row per sheet image, unique on <c>download_url</c>. The footprint box (<c>min_x</c> … <c>max_y</c>, EPSG:2180, easting in X) carries the spatial lookup: the schema has no PostGIS, so <see cref="OrtoSheetPostgreSQLConverter.GetOrtoSheetsByBoundingBoxAsync(NpgsqlConnection?, Geometry.Planar.Classes.BoundingBox2D?, double, int, CancellationToken)"/> prefilters on the box as one bounded range per axis, served by the <c>min_x</c> and <c>min_y</c> indexes, and the exact footprint test is made in C#. The table lives on the Storage database beside <c>orto_datas</c> (DiGi.GIS.PostgreSQL#122).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists or was created; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoSheet(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.OrtoSheet} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    sheet_id TEXT NOT NULL,
                    type_name TEXT,
                    year INT NOT NULL,
                    date timestamptz,
                    date_precision SMALLINT NOT NULL,
                    pixel_size DOUBLE PRECISION,
                    color_mode SMALLINT NOT NULL,
                    orto_type SMALLINT NOT NULL,
                    sheet_filled BOOLEAN,
                    register_date timestamptz,
                    min_x DOUBLE PRECISION NOT NULL,
                    min_y DOUBLE PRECISION NOT NULL,
                    max_x DOUBLE PRECISION NOT NULL,
                    max_y DOUBLE PRECISION NOT NULL,
                    download_url TEXT NOT NULL UNIQUE,
                    object JSONB NOT NULL,
                    created_at timestamptz DEFAULT now()
                );

                -- The box prefilter of the spatial lookup: one bounded range per axis (Query.OrtoSheetBoundingBoxCommandText).
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoSheet}_min_x
                ON {Constants.TableName.OrtoSheet} (min_x);

                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoSheet}_min_y
                ON {Constants.TableName.OrtoSheet} (min_y);

                -- The per-year counts of an import.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoSheet}_year
                ON {Constants.TableName.OrtoSheet} (year);
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OrtoSheet)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.OrtoArea"/> table, the full-interpretation test area registry, with its supporting indexes.
        /// <para>One row per test area, unique on <c>area_key</c>, the invariant location key of the area's bounding box. The envelope columns (<c>min_x</c> ... <c>max_y</c>, EPSG:2180, easting in X) carry the box for keyed and ranged lookup, served by the single-column indexes; <c>object</c> carries the whole <see cref="OrtoArea"/>, sampled cells included, so one read returns the area with its grid (<see cref="OrtoAreaPostgreSQLConverter.GetOrtoAreaAsync(NpgsqlConnection?, string, int, CancellationToken)"/>). The table lives on the Main database, beside <c>building_2d</c> (DiGi.GIS.PostgreSQL#129). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists or was created; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoArea(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.OrtoArea} (
                    area_key TEXT NOT NULL PRIMARY KEY,
                    min_x DOUBLE PRECISION NOT NULL,
                    min_y DOUBLE PRECISION NOT NULL,
                    max_x DOUBLE PRECISION NOT NULL,
                    max_y DOUBLE PRECISION NOT NULL,
                    name TEXT,
                    stratum TEXT,
                    cell_size DOUBLE PRECISION NOT NULL,
                    cell_count INTEGER NOT NULL,
                    seed BIGINT NOT NULL,
                    object JSONB NOT NULL
                );

                -- The box lookup of the area: one bounded range per axis, served by single-column indexes.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoArea}_min_x
                ON {Constants.TableName.OrtoArea} (min_x);

                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoArea}_min_y
                ON {Constants.TableName.OrtoArea} (min_y);
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OrtoArea)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.OrtoAreaObject"/> table, the polygons drawn in test areas, with its supporting indexes.
        /// <para>One row per drawn polygon, unique on <c>object_id</c>, the server-assigned identifier that stays stable across revisions. <c>area_key</c> carries the owning area and carries an index of its own, which is what the read by area runs on (<see cref="OrtoAreaObjectPostgreSQLConverter.GetOrtoAreaObjectsAsync(NpgsqlConnection?, string, int, CancellationToken)"/>); <c>cell_index</c> is null for a centroid outside every sampled cell; <c>created_at</c> stores the drawing moment from the object, not the moment the row was inserted; <c>object</c> carries the whole <see cref="OrtoAreaObject"/>, polygon included. The table lives on the Main database, beside <c>building_2d</c> (DiGi.GIS.PostgreSQL#129). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists or was created; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoAreaObject(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.OrtoAreaObject} (
                    object_id TEXT NOT NULL PRIMARY KEY,
                    area_key TEXT NOT NULL,
                    cell_index INTEGER,
                    created_by TEXT NOT NULL,
                    created_at timestamptz NOT NULL,
                    object JSONB NOT NULL
                );

                -- The read by area (Query.OrtoAreaObjectsByAreaKeyCommandText) filters on area_key alone.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoAreaObject}_area_key
                ON {Constants.TableName.OrtoAreaObject} (area_key);
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OrtoAreaObject)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.OrtoAreaLabel"/> table, the interpretation labels of test areas, with its unique identity index.
        /// <para>One row per (area, object identity, epoch, interpreter). The object identity is nullable - <c>county_id</c> with <c>reference</c> is the footprint identity, <c>object_id</c> the drawn-object identity, and exactly one of the two is set - so the unique index behind the label upsert is written <c>NULLS NOT DISTINCT</c>: rows that agree only in those NULLs conflict like any others, one interpreter's re-upsert revises her or his own row and a second interpreter keeps a parallel one (<see cref="OrtoAreaLabelPostgreSQLConverter.UpdateAsync(NpgsqlConnection?, System.Collections.Generic.IEnumerable{OrtoAreaLabel}, int, int, CancellationToken)"/>). <c>area_key</c> leads the index, so it also serves the read by area (<see cref="OrtoAreaLabelPostgreSQLConverter.GetOrtoAreaLabelsAsync(NpgsqlConnection?, string, int, CancellationToken)"/>). The table lives on the Main database, beside <c>building_2d</c> (DiGi.GIS.PostgreSQL#129). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists or was created; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoAreaLabel(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.OrtoAreaLabel} (
                    area_key TEXT NOT NULL,
                    county_id INTEGER,
                    reference TEXT,
                    object_id TEXT,
                    epoch_year SMALLINT NOT NULL,
                    state SMALLINT NOT NULL,
                    observation_date_min DATE,
                    observation_date_max DATE,
                    interpreter TEXT NOT NULL,
                    note TEXT,
                    updated_at timestamptz NOT NULL
                );

                -- The conflict target of the label upsert: one row per (area, object identity, epoch, interpreter).
                -- The identity pair is nullable - county_id and reference are the footprint identity, object_id
                -- the drawn-object identity, exactly one of the two is set - so NULLS NOT DISTINCT makes rows that
                -- agree only in those NULLs conflict like any others: one interpreter's re-upsert revises her or
                -- his row in place, while a second interpreter's label inserts a row of its own (PostgreSQL 15+).
                -- area_key leads the index, which is also what serves the read by area (Query.OrtoAreaLabelsByAreaKeyCommandText).
                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.OrtoAreaLabel}_identity
                ON {Constants.TableName.OrtoAreaLabel} (area_key, county_id, reference, object_id, epoch_year, interpreter) NULLS NOT DISTINCT;
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OrtoAreaLabel)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.OrtoAreaImage"/> table, the materialised area tiles of test areas, with its unique area-and-epoch key.
        /// <para>One row per area and imagery epoch, unique on <c>(area_key, epoch_year)</c>, which is the conflict target of the tile upsert: a re-fetch replaces the row, never appends, and the negative rows of status 1 (no imagery) and 2 (fetch error) are stored with a null <c>bytes</c> to cache their answer. The envelope columns (<c>min_x</c> ... <c>max_y</c>, EPSG:2180, easting in X) carry the area box read back with the tile (<see cref="OrtoAreaImagePostgreSQLConverter.GetOrtoAreaImageAsync(NpgsqlConnection?, string, short, int, CancellationToken)"/>). The table lives on the Storage database beside <c>orto_datas</c> (DiGi.GIS.PostgreSQL#129). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists or was created; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoAreaImage(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.OrtoAreaImage} (
                    area_key TEXT NOT NULL,
                    min_x DOUBLE PRECISION NOT NULL,
                    min_y DOUBLE PRECISION NOT NULL,
                    max_x DOUBLE PRECISION NOT NULL,
                    max_y DOUBLE PRECISION NOT NULL,
                    epoch_year SMALLINT NOT NULL,
                    width INTEGER NOT NULL,
                    height INTEGER NOT NULL,
                    status SMALLINT NOT NULL,
                    source_url TEXT,
                    fetched_at timestamptz NOT NULL,
                    bytes BYTEA,
                    UNIQUE (area_key, epoch_year)
                );
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OrtoAreaImage)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.OrtoDataAcquisition"/> table, partitioned by county part like <see cref="Constants.TableName.OrtoDatas"/>.
        /// <para>One row per stored image: the key <c>(county_id, reference, year)</c> names the image by its building and its requested year (<see cref="GIS.Classes.OrtoData.DateTime"/>). <c>acquisition_date</c> is null and <c>date_precision</c> is <see cref="GIS.Enums.OrtoDatePrecision.Undefined"/> when no index sheet dates the image; a missing row means the image was not backfilled yet. The table, not the members inside <c>orto_datas.object</c>, is the source of truth for the acquisition date (DiGi.GIS.PostgreSQL#123). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_OrtoDataAcquisition(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.OrtoDataAcquisition} (
                    county_id INT NOT NULL,
                    reference TEXT NOT NULL,
                    year INT NOT NULL,
                    acquisition_date timestamptz,
                    date_precision SMALLINT NOT NULL,
                    sheet_id TEXT,
                    content_hash TEXT,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (county_id, reference, year)
                ) PARTITION BY LIST (county_id);
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_OrtoDataAcquisition)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the partition of the <see cref="Constants.TableName.OrtoDataAcquisition"/> table for one county part.
        /// </summary>
        /// <param name="npgsqlConnection">The PostgreSQL connection instance used to execute the command.</param>
        /// <param name="countyId">The county part the partition holds.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the partition exists afterwards; false when the connection is null.</returns>
        public static async Task<bool> TableAsync_OrtoDataAcquisition_Partition(this NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = Query.PartitionCommandText(Constants.TableName.OrtoDataAcquisition, countyId);

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;

            await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken);

            return true;
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

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.BuildingShapeSample"/> table, the fixed, ordered building shape labelling samples.
        /// <para>One row per footprint of a sample, unique on <c>(sample_key, county_id, reference)</c> (a reference is unique only within its county part) and on <c>(sample_key, ordinal)</c>, the labelling order every annotator follows; the second index also serves the next-item read. <c>solver_shape</c> is the <see cref="GIS.Enums.BuildingShape"/> value the solver gave at sampling time. The table lives on the Main database (DiGi.GIS.PostgreSQL#135). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists or was created; otherwise, false.</returns>
        public static async Task<bool> TableAsync_BuildingShapeSample(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.BuildingShapeSample} (
                    sample_key TEXT NOT NULL,
                    ordinal INTEGER NOT NULL,
                    county_id INTEGER NOT NULL,
                    reference TEXT NOT NULL,
                    solver_shape SMALLINT NOT NULL
                );

                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingShapeSample}_identity
                ON {Constants.TableName.BuildingShapeSample} (sample_key, county_id, reference);

                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingShapeSample}_ordinal
                ON {Constants.TableName.BuildingShapeSample} (sample_key, ordinal);
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_BuildingShapeSample)}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.BuildingShapeLabel"/> table, the manual building shape labels of the labelling samples.
        /// <para>One row per sample item and interpreter, unique on <c>(sample_key, county_id, reference, interpreter)</c>, which is the conflict target of the label upsert: an interpreter's re-label replaces her or his own row, while a second interpreter keeps a parallel one. Every identity column is <c>NOT NULL</c>, so no <c>NULLS NOT DISTINCT</c> is needed. The table lives on the Main database beside <see cref="Constants.TableName.BuildingShapeSample"/> (DiGi.GIS.PostgreSQL#135). Every statement is idempotent.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists or was created; otherwise, false.</returns>
        public static async Task<bool> TableAsync_BuildingShapeLabel(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.BuildingShapeLabel} (
                    sample_key TEXT NOT NULL,
                    county_id INTEGER NOT NULL,
                    reference TEXT NOT NULL,
                    shape SMALLINT NOT NULL,
                    interpreter TEXT NOT NULL,
                    note TEXT,
                    updated_at timestamptz NOT NULL
                );

                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingShapeLabel}_identity
                ON {Constants.TableName.BuildingShapeLabel} (sample_key, county_id, reference, interpreter);
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
                Console.WriteLine($"Postgres Error ({nameof(TableAsync_BuildingShapeLabel)}): {ex.Message}");
                return false;
            }
        }
    }
}