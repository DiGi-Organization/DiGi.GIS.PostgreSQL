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
        /// <para>The key <c>(county_id, building_model_id, method_version)</c> is unique, <c>NULLS NOT DISTINCT</c> although every column of it is NOT NULL, so the index stays correct if a column ever becomes nullable. Every statement is idempotent.</para>
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
                    method_version INT NOT NULL,
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

                -- The key of a stored result and the conflict target of every write: one row per building model and method version.
                CREATE UNIQUE INDEX IF NOT EXISTS idx_{Constants.TableName.BuildingHeatDemand}_key
                ON {Constants.TableName.BuildingHeatDemand} (county_id, building_model_id, method_version) NULLS NOT DISTINCT;

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
