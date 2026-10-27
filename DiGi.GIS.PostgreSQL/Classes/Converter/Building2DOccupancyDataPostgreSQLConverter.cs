using DiGi.GIS.Interfaces;
using DiGi.PostgreSQL.Classes;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Provides functionality to convert building 2D occupancy data for PostgreSQL database operations.
    /// </summary>
    public class Building2DOccupancyDataPostgreSQLConverter : Building2DReferencedObjectPostgreSQLConverter<Building2DOccupancyData, IOccupancyData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Building2DOccupancyDataPostgreSQLConverter"/> class.
        /// </summary>
        /// <param name="connectionData">The connection data used to connect to the PostgreSQL database.</param>
        public Building2DOccupancyDataPostgreSQLConverter(ConnectionData? connectionData)
            : base(connectionData)
        {
        }

        /// <summary>
        /// Gets the name of the table in the PostgreSQL database where building 2D occupancy data is stored.
        /// </summary>
        public override string TableName => Constants.TableName.OccupancyData_Building2D;

        protected override Building2DOccupancyData Create(long id, int? countyId, string? uniqueId, string? reference, JsonObject? @object, DateTime? createdAt)
        {
            return new Building2DOccupancyData()
            {
                Id = id,
                CountyId = countyId,
                UniqueId = uniqueId,
                Reference = reference,
                Object = @object,
                CreatedAt = createdAt
            };
        }

        /// <summary>
        /// Asynchronously takes a read-only snapshot of the occupancy rows, one entry per county part (<c>county_id</c>), to compare the stored occupancies before and after a re-run.
        /// <para>Each entry carries the row count, the sum of the stored <c>Occupancy</c> values, a SHA-256 checksum and the number of figured subdivisions. The checksum hashes <c>reference</c> and the raw JSON text of <c>Occupancy</c>, joined by line feeds and ordered by <c>reference</c> in byte order (<c>COLLATE "C"</c>), so it does not depend on storage order or on any culture.</para>
        /// <para>A figured subdivision is a <c>building_2d.subdivision_id</c> reached by at least two of the county part's occupancy rows through <c>(county_id, reference)</c>; rows whose building has no subdivision are not counted. Rows are grouped by <c>county_id</c>, never by county code, because a code names one row per polygon part.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the queries.</param>
        /// <param name="countyId">The optional identifier of the county part to restrict the snapshot to. When null every county part is measured.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to each command executed. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task whose result holds one entry per county part ordered by identifier - an empty list when no row is stored - or null when the connection is null or the timeout is negative.</returns>
        public async Task<List<Building2DOccupancySnapshotResult>?> GetOccupancySnapshotsAsync(NpgsqlConnection? npgsqlConnection, int? countyId = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || commandTimeout < 0)
            {
                return null;
            }

            string where = countyId.HasValue ? "WHERE county_id = @countyId AND reference IS NOT NULL" : "WHERE reference IS NOT NULL";

            string commandText_Rows = $@"
                SELECT county_id,
                       COUNT(*),
                       COALESCE(SUM((object ->> 'Occupancy')::double precision), 0),
                       encode(sha256(convert_to(COALESCE(string_agg(reference || E'\n' || COALESCE(object ->> 'Occupancy', ''), E'\n' ORDER BY reference COLLATE ""C""), ''), 'UTF8')), 'hex')
                FROM {TableName}
                {where}
                GROUP BY county_id
                ORDER BY county_id;";

            string commandText_Subdivisions = $@"
                SELECT sub.county_id, COUNT(*)
                FROM (
                    SELECT o.county_id, b.subdivision_id
                    FROM {TableName} o
                    JOIN {Constants.TableName.Building2D} b ON b.county_id = o.county_id AND b.reference = o.reference
                    WHERE {(countyId.HasValue ? "o.county_id = @countyId AND " : string.Empty)}b.subdivision_id IS NOT NULL
                    GROUP BY o.county_id, b.subdivision_id
                    HAVING COUNT(*) >= 2
                ) sub
                GROUP BY sub.county_id;";

            List<(int CountyId, long Count, double OccupancySum, string? Checksum)> rows = [];
            await using (NpgsqlCommand npgsqlCommand = new(commandText_Rows, npgsqlConnection))
            {
                npgsqlCommand.CommandTimeout = commandTimeout;
                if (countyId.HasValue)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    if (npgsqlDataReader.IsDBNull(0))
                    {
                        continue;
                    }

                    rows.Add((npgsqlDataReader.GetInt32(0), npgsqlDataReader.GetInt64(1), npgsqlDataReader.GetDouble(2), npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetString(3)));
                }
            }

            Dictionary<int, long> figuredSubdivisionCounts = [];
            await using (NpgsqlCommand npgsqlCommand = new(commandText_Subdivisions, npgsqlConnection))
            {
                npgsqlCommand.CommandTimeout = commandTimeout;
                if (countyId.HasValue)
                {
                    npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId.Value });
                }

                await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
                while (await npgsqlDataReader.ReadAsync(cancellationToken))
                {
                    figuredSubdivisionCounts[npgsqlDataReader.GetInt32(0)] = npgsqlDataReader.GetInt64(1);
                }
            }

            List<Building2DOccupancySnapshotResult> building2DOccupancySnapshotResults = [];
            foreach ((int CountyId, long Count, double OccupancySum, string? Checksum) row in rows)
            {
                figuredSubdivisionCounts.TryGetValue(row.CountyId, out long figuredSubdivisionCount);
                building2DOccupancySnapshotResults.Add(new Building2DOccupancySnapshotResult(row.CountyId, row.Count, row.OccupancySum, row.Checksum, figuredSubdivisionCount));
            }

            return building2DOccupancySnapshotResults;
        }

        /// <summary>
        /// Asynchronously reads, for one county part, every subdivision its buildings are filed under next to the occupancy figure the subdivision carries and the sum of the occupancy rows stored for those buildings.
        /// <para>A subdivision holding no other one must have its figure stored in full, whatever the draw did: the check that the distribution conserves a total. A subdivision that holds others hands part of its figure to the buildings of the nested ones, so its stored sum is below the figure by design - <see cref="Building2DOccupancySubdivisionResult.ContainedSubdivisionCount"/> tells the two apart, deciding nesting on the bounding boxes of the subdivisions of the same municipality and county part, which over-approximates it.</para>
        /// <para>Buildings are attributed by <c>building_2d.subdivision_id</c>, so buildings of an unfigured subdivision that borrow a container's figure are counted under their own subdivision, not the container.</para>
        /// </summary>
        /// <param name="npgsqlConnection">The <see cref="NpgsqlConnection"/> used to execute the query.</param>
        /// <param name="countyId">The identifier of the county part the buildings are filed under.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command executed. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task whose result holds one entry per subdivision ordered by identifier - an empty list when the county part holds none - or null when the connection is null or the timeout is negative.</returns>
        public async Task<List<Building2DOccupancySubdivisionResult>?> GetOccupancySubdivisionSnapshotsAsync(NpgsqlConnection? npgsqlConnection, int countyId, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (npgsqlConnection is null || commandTimeout < 0)
            {
                return null;
            }

            string commandText = $@"
                WITH s AS (
                    SELECT b.subdivision_id AS id,
                           COUNT(*) AS stored_count,
                           SUM((o.object ->> 'Occupancy')::double precision) AS stored_sum
                    FROM {Constants.TableName.Building2D} b
                    JOIN {TableName} o ON o.county_id = b.county_id AND o.reference = b.reference
                    WHERE b.county_id = @countyId AND b.subdivision_id IS NOT NULL
                    GROUP BY b.subdivision_id
                )
                SELECT s.id,
                       a.county_id,
                       (a.object ->> 'Occupancy')::double precision,
                       s.stored_sum,
                       s.stored_count,
                       (SELECT COUNT(*)
                        FROM {Constants.TableName.AdministrativeAreal2D} t
                        WHERE t.type_id = @typeId
                          AND t.id <> a.id
                          AND t.county_id IS NOT DISTINCT FROM a.county_id
                          AND t.municipality_id IS NOT DISTINCT FROM a.municipality_id
                          AND t.min_x >= a.min_x AND t.min_y >= a.min_y AND t.max_x <= a.max_x AND t.max_y <= a.max_y)
                FROM s
                LEFT JOIN {Constants.TableName.AdministrativeAreal2D} a ON a.id = s.id
                ORDER BY s.id;";

            await using NpgsqlCommand npgsqlCommand = new(commandText, npgsqlConnection);
            npgsqlCommand.CommandTimeout = commandTimeout;
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("countyId", NpgsqlDbType.Integer) { Value = countyId });
            npgsqlCommand.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlDbType.Smallint) { Value = (short)Enums.AdministrativeArealType.Subdivision });

            List<Building2DOccupancySubdivisionResult> building2DOccupancySubdivisionResults = [];
            await using NpgsqlDataReader npgsqlDataReader = await npgsqlCommand.ExecuteReaderAsync(cancellationToken);
            while (await npgsqlDataReader.ReadAsync(cancellationToken))
            {
                building2DOccupancySubdivisionResults.Add(new Building2DOccupancySubdivisionResult(
                    npgsqlDataReader.GetInt32(0),
                    npgsqlDataReader.IsDBNull(1) ? null : npgsqlDataReader.GetInt32(1),
                    npgsqlDataReader.IsDBNull(2) ? null : npgsqlDataReader.GetDouble(2),
                    npgsqlDataReader.IsDBNull(3) ? 0 : npgsqlDataReader.GetDouble(3),
                    npgsqlDataReader.GetInt64(4),
                    npgsqlDataReader.IsDBNull(5) ? 0 : npgsqlDataReader.GetInt64(5)));
            }

            return building2DOccupancySubdivisionResults;
        }

        /// <summary>
        /// Asynchronously reads the subdivision conservation figures of one county part. See <see cref="GetOccupancySubdivisionSnapshotsAsync(NpgsqlConnection?, int, int, CancellationToken)"/>.
        /// </summary>
        /// <param name="countyId">The identifier of the county part the buildings are filed under.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to the command executed. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task whose result holds one entry per subdivision, or null when the connection could not be built or the timeout is negative.</returns>
        public async Task<List<Building2DOccupancySubdivisionResult>?> GetOccupancySubdivisionSnapshotsAsync(int countyId, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (commandTimeout < 0)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetOccupancySubdivisionSnapshotsAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously takes a read-only snapshot of the occupancy rows, one entry per county part. See <see cref="GetOccupancySnapshotsAsync(NpgsqlConnection?, int?, int, CancellationToken)"/>.
        /// </summary>
        /// <param name="countyId">The optional identifier of the county part to restrict the snapshot to. When null every county part is measured.</param>
        /// <param name="commandTimeout">The timeout in seconds applied to each command executed. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task whose result holds one entry per county part, or null when the connection could not be built or the timeout is negative.</returns>
        public async Task<List<Building2DOccupancySnapshotResult>?> GetOccupancySnapshotsAsync(int? countyId = null, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (commandTimeout < 0)
            {
                return null;
            }

            await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(ConnectionData);
            if (npgsqlConnection is null)
            {
                return null;
            }

            await npgsqlConnection.OpenAsync(cancellationToken);

            return await GetOccupancySnapshotsAsync(npgsqlConnection, countyId, commandTimeout, cancellationToken);
        }
    }
}