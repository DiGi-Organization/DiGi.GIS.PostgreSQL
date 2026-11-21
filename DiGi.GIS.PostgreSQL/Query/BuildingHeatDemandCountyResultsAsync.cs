using DiGi.Analytical.Building.Regulation.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Asynchronously summarises the stored heat demand of one method and one input variant in the given county parts, one row per county part, method version, building type and year-built band (DiGi.GIS.PostgreSQL#146, for DiGi-Organization/DiGi.Analytical#10).
        /// <para>Results of another method or input variant never enter the summary (DiGi-Organization/DiGi.GIS.PostgreSQL#152): <paramref name="method"/> and <paramref name="inputVariant"/> are the integers of <c>HeatDemandMethod</c> and <c>HeatDemandInputVariant</c>, taken as integers because this assembly references no energy assembly; their defaults are <c>Monthly</c> (1) and <c>Generated</c> (1).</para>
        /// <para>Plain SQL over the columns and the <c>object</c> JSONB of <see cref="Constants.TableName.BuildingHeatDemand"/>, so a host that serves it needs no energy engine. The building type and the year built are the ones the calculation stored (<c>BuildingType</c>, <c>YearBuilt</c>); the quality counts come from its <c>HeatDemandResult</c> (<c>ExcludedComponents</c>, <c>GlazingFallbackCount</c>, <c>MissingIrradiationCount</c>), a missing or null value counting as 0.</para>
        /// <para>The bands: <paramref name="yearBuiltBounds"/> is sorted and de-duplicated; band 0 is below the first bound, band i from the bound i - 1 to the year before the bound i, the last band from the last bound up. Without bounds every year is band 0. A result without a year built is its own band, index null.</para>
        /// <para>Separate statement texts for a scoped and an unscoped read and for a method version or none, so a scoped read prunes to its partitions (<c>Coding - PostgreSQL.md</c> §3).</para>
        /// </summary>
        /// <param name="npgsqlConnection">The open <see cref="NpgsqlConnection"/> of the Storage database. This value can be null.</param>
        /// <param name="countyIds">The county parts to summarise; null for every part that holds results.</param>
        /// <param name="methodVersion">The method version to summarise, within <paramref name="method"/>; null for every version, one row each.</param>
        /// <param name="yearBuiltBounds">The bounds of the year-built bands, such as the first years of a typology's construction periods; null or empty for one band of every year.</param>
        /// <param name="method">The integer of the <c>HeatDemandMethod</c> to summarise; 1 = Monthly.</param>
        /// <param name="inputVariant">The integer of the <c>HeatDemandInputVariant</c> to summarise; 1 = Generated.</param>
        /// <param name="commandTimeout">The timeout in seconds for the command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task whose result holds the summaries ordered by county part, method version, building type and band (the band without a year built last); empty when the table is missing, or null when the connection is null.</returns>
        public static async Task<List<BuildingHeatDemandCountyResult>?> BuildingHeatDemandCountyResultsAsync(
            this NpgsqlConnection? npgsqlConnection,
            IEnumerable<int>? countyIds,
            int? methodVersion = null,
            IEnumerable<short>? yearBuiltBounds = null,
            int method = 1,
            int inputVariant = 1,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null)
            {
                return null;
            }

            SortedSet<short> yearBuiltBounds_Sorted = yearBuiltBounds is null ? [] : [.. yearBuiltBounds];
            List<short> yearBuiltBounds_List = [.. yearBuiltBounds_Sorted];

            List<string> conditions = ["method = @method", "input_variant = @input_variant"];
            if (countyIds is not null)
            {
                conditions.Add("county_id = ANY(@county_ids)");
            }

            if (methodVersion is not null)
            {
                conditions.Add("method_version = @method_version");
            }

            string whereClause = conditions.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", conditions);

            // width_bucket gives 0 below the first bound and the bound count from the last bound up; a null year stays null.
            string bandExpression = yearBuiltBounds_List.Count == 0 ? "CASE WHEN year_built IS NULL THEN NULL ELSE 0 END" : "width_bucket(year_built, @year_built_bounds)";

            string commandText = $@"
                SELECT county_id, method, method_version, input_variant, building_type, band,
                       COUNT(*),
                       COUNT(*) FILTER (WHERE solar_source = 1),
                       COUNT(*) FILTER (WHERE solar_source = 2),
                       SUM(heating_energy_need),
                       AVG(heating_energy_need_per_area),
                       percentile_cont(0.1) WITHIN GROUP (ORDER BY heating_energy_need_per_area),
                       percentile_cont(0.5) WITHIN GROUP (ORDER BY heating_energy_need_per_area),
                       percentile_cont(0.9) WITHIN GROUP (ORDER BY heating_energy_need_per_area),
                       SUM(design_heat_load),
                       COUNT(*) FILTER (WHERE excluded_component_count > 0),
                       COALESCE(SUM(excluded_component_count), 0),
                       COUNT(*) FILTER (WHERE glazing_fallback_count > 0),
                       COALESCE(SUM(glazing_fallback_count), 0),
                       COUNT(*) FILTER (WHERE missing_irradiation_count > 0),
                       MIN(created_at),
                       MAX(created_at)
                FROM (
                    SELECT county_id, method, method_version, input_variant, solar_source, heating_energy_need, heating_energy_need_per_area, design_heat_load, created_at, year_built,
                           building_type,
                           excluded_component_count,
                           glazing_fallback_count,
                           missing_irradiation_count,
                           {bandExpression} AS band
                    FROM (
                        SELECT county_id, method, method_version, input_variant, solar_source, heating_energy_need, heating_energy_need_per_area, design_heat_load, created_at,
                               (object->>'YearBuilt')::int AS year_built,
                               COALESCE((object->>'BuildingType')::int, 0) AS building_type,
                               CASE WHEN jsonb_typeof(object->'HeatDemandResult'->'ExcludedComponents') = 'array' THEN jsonb_array_length(object->'HeatDemandResult'->'ExcludedComponents') ELSE 0 END AS excluded_component_count,
                               COALESCE((object->'HeatDemandResult'->>'GlazingFallbackCount')::int, 0) AS glazing_fallback_count,
                               COALESCE((object->'HeatDemandResult'->>'MissingIrradiationCount')::int, 0) AS missing_irradiation_count
                        FROM {Constants.TableName.BuildingHeatDemand}
                        {whereClause}
                    ) AS building_heat_demand_row
                ) AS building_heat_demand_band
                GROUP BY county_id, method, method_version, input_variant, building_type, band
                ORDER BY county_id, method, method_version, input_variant, building_type, band NULLS LAST;";

            List<BuildingHeatDemandCountyResult> result = [];
            try
            {
                await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection) { CommandTimeout = commandTimeout };
                if (countyIds is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("county_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = new List<int>(countyIds).ToArray() });
                }

                npgsqlCommand.Parameters.Add(new NpgsqlParameter("method", NpgsqlDbType.Smallint) { Value = (short)method });
                npgsqlCommand.Parameters.Add(new NpgsqlParameter("input_variant", NpgsqlDbType.Smallint) { Value = (short)inputVariant });
                if (methodVersion is not null)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("method_version", NpgsqlDbType.Integer) { Value = methodVersion.Value });
                }

                if (yearBuiltBounds_List.Count != 0)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("year_built_bounds", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = yearBuiltBounds_List.ConvertAll(x => (int)x).ToArray() });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    int? yearBuiltBandIndex = npgsqlDataReader.IsDBNull(5) ? null : npgsqlDataReader.GetInt32(5);

                    short? yearBuiltMin = null;
                    short? yearBuiltMax = null;
                    if (yearBuiltBandIndex is int yearBuiltBandIndex_Value && yearBuiltBounds_List.Count != 0)
                    {
                        if (yearBuiltBandIndex_Value > 0)
                        {
                            yearBuiltMin = yearBuiltBounds_List[yearBuiltBandIndex_Value - 1];
                        }

                        if (yearBuiltBandIndex_Value < yearBuiltBounds_List.Count)
                        {
                            yearBuiltMax = (short)(yearBuiltBounds_List[yearBuiltBandIndex_Value] - 1);
                        }
                    }

                    result.Add(new BuildingHeatDemandCountyResult(
                        npgsqlDataReader.GetInt32(0),
                        npgsqlDataReader.GetInt16(1),
                        npgsqlDataReader.GetInt32(2),
                        npgsqlDataReader.GetInt16(3),
                        (BuildingType)npgsqlDataReader.GetInt32(4),
                        yearBuiltBandIndex,
                        yearBuiltMin,
                        yearBuiltMax,
                        npgsqlDataReader.GetInt64(6),
                        npgsqlDataReader.GetInt64(7),
                        npgsqlDataReader.GetInt64(8),
                        DoubleOrNaN(9),
                        DoubleOrNaN(10),
                        DoubleOrNaN(11),
                        DoubleOrNaN(12),
                        DoubleOrNaN(13),
                        DoubleOrNaN(14),
                        npgsqlDataReader.GetInt64(15),
                        npgsqlDataReader.GetInt64(16),
                        npgsqlDataReader.GetInt64(17),
                        npgsqlDataReader.GetInt64(18),
                        npgsqlDataReader.GetInt64(19),
                        npgsqlDataReader.IsDBNull(20) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(20),
                        npgsqlDataReader.IsDBNull(21) ? null : npgsqlDataReader.GetFieldValue<DateTimeOffset>(21)));

                    // A group with no finite value aggregates to null, read as NaN.
                    double DoubleOrNaN(int ordinal)
                    {
                        return npgsqlDataReader.IsDBNull(ordinal) ? double.NaN : npgsqlDataReader.GetDouble(ordinal);
                    }
                }
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return [];
            }

            return result;
        }
    }
}
