using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Energy
{
    public static partial class Create
    {
        /// <summary>
        /// Asynchronously creates the <see cref="Constants.TableName.BuildingHeatDemand"/> table and its indexes, partitioned by county part (DiGi.GIS.PostgreSQL#143).
        /// <para>One row holds one building's <c>BuildingHeatDemandResult</c>: the <c>object</c> column carries the whole result, the nested monthly heat demand and the provenance included, and the summary columns (<c>heating_energy_need</c>, <c>heating_energy_need_per_area</c>, <c>design_heat_load</c>) and the provenance keys (weather and solar source, construction flavour and library version) repeat what queries filter and aggregate on.</para>
        /// <para>The key <c>(county_id, building_model_id, method, method_version, input_variant)</c> is unique (DiGi-Organization/DiGi.GIS.PostgreSQL#152, Heat Demand Method Identity DiGi-Organization/DiGi.GIS.Analytical#19): <c>method</c> and <c>input_variant</c> are the integers of the result's <c>HeatDemandMethod</c> and <c>HeatDemandInputVariant</c>, so results of different methods or input variants are stored side by side. <c>NULLS NOT DISTINCT</c> although every column of it is NOT NULL, so the index stays correct if a column ever becomes nullable.</para>
        /// <para>Every statement is idempotent, and the method also migrates a table created before the two columns: they are added with the default 1 (<c>Monthly</c>, <c>Generated</c> - the only method and input variant there were), which fills every existing row, and the default is dropped so a write must name both; the key moves from <c>(county_id, building_model_id, method_version)</c> to the new one. The migration runs only while a column is missing, so the writes that call this method take no table lock once it is done.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> instance used to execute the command.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the table exists afterwards; otherwise, false.</returns>
        public static async Task<bool> TableAsync_BuildingHeatDemand(this NpgsqlConnection? npgsqlConnection, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return false;
            }

            string commandText = $@"
                CREATE TABLE IF NOT EXISTS {Constants.TableName.BuildingHeatDemand} (
                    id BIGINT GENERATED ALWAYS AS IDENTITY,
                    county_id INT NOT NULL,
                    building_model_id BIGINT NOT NULL,
                    reference TEXT NOT NULL,
                    method SMALLINT NOT NULL,
                    method_version INT NOT NULL,
                    input_variant SMALLINT NOT NULL,
                    weather_source_type INT NOT NULL,
                    weather_source_id TEXT,
                    solar_source INT NOT NULL,
                    solar_solver_version INT,
                    solar_radius DOUBLE PRECISION,
                    construction_flavour INT NOT NULL,
                    construction_library_version TEXT,
                    heating_energy_need DOUBLE PRECISION,
                    heating_energy_need_per_area DOUBLE PRECISION,
                    design_heat_load DOUBLE PRECISION,
                    object JSONB NOT NULL,
                    created_at timestamptz DEFAULT now(),
                    PRIMARY KEY (id, county_id)
                ) PARTITION BY LIST (county_id);

                -- A table created before the method and the input variant: both added as 1 (Monthly, Generated) - the only ones
                -- there were - which fills every row, then the default dropped. Guarded, so a migrated table is not locked again.
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = '{Constants.TableName.BuildingHeatDemand}' AND column_name = 'method') THEN
                        ALTER TABLE {Constants.TableName.BuildingHeatDemand} ADD COLUMN method SMALLINT NOT NULL DEFAULT 1;
                        ALTER TABLE {Constants.TableName.BuildingHeatDemand} ALTER COLUMN method DROP DEFAULT;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = '{Constants.TableName.BuildingHeatDemand}' AND column_name = 'input_variant') THEN
                        ALTER TABLE {Constants.TableName.BuildingHeatDemand} ADD COLUMN input_variant SMALLINT NOT NULL DEFAULT 1;
                        ALTER TABLE {Constants.TableName.BuildingHeatDemand} ALTER COLUMN input_variant DROP DEFAULT;
                    END IF;
                END $$;

                -- The key of a stored result and the conflict target of every write: one row per building model, method,
                -- method version and input variant. It replaces the key of one row per building model and method version.
                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingHeatDemand}_identity
                ON {Constants.TableName.BuildingHeatDemand} (county_id, building_model_id, method, method_version, input_variant) NULLS NOT DISTINCT;

                DROP INDEX IF EXISTS idx_{Constants.TableName.BuildingHeatDemand}_key;

                -- The fallback read, which looks for the building model under every county part.
                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingHeatDemand}_building_model_id
                ON {Constants.TableName.BuildingHeatDemand} (building_model_id);

                CREATE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingHeatDemand}_county_id_reference
                ON {Constants.TableName.BuildingHeatDemand} (county_id, reference);
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
                Serilog.Modify.Log(ex, "{Type}: the {Table} table could not be created", nameof(TableAsync_BuildingHeatDemand), Constants.TableName.BuildingHeatDemand);
                return false;
            }
        }
    }
}
