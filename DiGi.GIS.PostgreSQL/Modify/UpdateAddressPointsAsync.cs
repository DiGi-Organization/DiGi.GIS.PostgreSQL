using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Imports the PRG address points of one county into <c>address_point</c>: assigns every point to the county part it lies in, matches the points of each part to that part's <c>building_2d</c> footprints with <see cref="GIS.Create.AddressPointMatchResult(IEnumerable{AddressPoint?}?, IEnumerable{GIS.Classes.Building2D?}?, double, double)"/>, and stores the matches.
        /// <para><paramref name="countyIds"/> are county part ids, normally every polygon part of one county, never a county code: 18 counties have disconnected territory stored as one row per part (<c>Coding - GIS Administrative Data.md</c>). A point is assigned to the part whose face contains it (<see cref="Query.IdsByPoint2Ds(IDictionary{int, PolygonalFace2D}?, IReadOnlyList{Point2D}?, double)"/>, lowest id on overlap); a point in none of the parts is counted as outside and not written.</para>
        /// <para>Known limit: a point is matched only against the buildings of its own part, so a point within <paramref name="maxDistance"/> of a building filed under a sibling part, across the part boundary, is matched to its own part's nearest building or left unmatched.</para>
        /// <para>Re-running an import replaces each point's row unless the stored PRG version is newer (<see cref="AddressPointPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection?, int, IEnumerable{AddressPointMatch?}?, int, int, CancellationToken)"/>). All three converters read and write the Main database.</para>
        /// </summary>
        /// <param name="addressPointPostgreSQLConverter">The converter of the address point table.</param>
        /// <param name="building2DPostgreSQLConverter">The converter the building footprints of each part are read through.</param>
        /// <param name="administrativeAreal2DPostgreSQLConverter">The converter the county part polygons are read through.</param>
        /// <param name="addressPoints">The address points of the county.</param>
        /// <param name="countyIds">The county part ids of the county.</param>
        /// <param name="maxDistance">The distance, in m, within which a point outside every footprint is assigned to the nearest one.</param>
        /// <param name="batchSize">The number of rows written per statement.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>A task that represents the asynchronous operation. The task result counts the points by outcome and the rows written, or is null when a converter, the points or the parts are missing, the parts have no readable polygon, or a read or the write could not run.</returns>
        public static async Task<AddressPointUpdateResult?> UpdateAddressPointsAsync(
            this AddressPointPostgreSQLConverter? addressPointPostgreSQLConverter,
            Building2DPostgreSQLConverter? building2DPostgreSQLConverter,
            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter,
            IEnumerable<AddressPoint?>? addressPoints,
            IEnumerable<int>? countyIds,
            double maxDistance = 10.0,
            int batchSize = 1000,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            int[] countyIds_Array = countyIds is null ? [] : [.. countyIds.Distinct()];
            if (addressPointPostgreSQLConverter is null || building2DPostgreSQLConverter is null || administrativeAreal2DPostgreSQLConverter is null || addressPoints is null || countyIds_Array.Length == 0)
            {
                return null;
            }

            List<Classes.AdministrativeAreal2D>? administrativeAreal2Ds = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(countyIds_Array, commandTimeout, cancellationToken);
            Dictionary<int, PolygonalFace2D> polygonalFace2Ds_ById = administrativeAreal2Ds.PolygonalFace2DsById();
            if (polygonalFace2Ds_ById.Count == 0)
            {
                return null;
            }

            int skipped = 0;
            List<AddressPoint> addressPoints_Valid = [];
            List<Point2D> point2Ds = [];
            foreach (AddressPoint? addressPoint in addressPoints)
            {
                if (addressPoint?.Location is not Point2D point2D || string.IsNullOrWhiteSpace(addressPoint.ReferenceId) || addressPoint.SearchText() is null)
                {
                    skipped++;
                    continue;
                }

                addressPoints_Valid.Add(addressPoint);
                point2Ds.Add(point2D);
            }

            int?[]? ids = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds);
            if (ids is null)
            {
                return null;
            }

            int outside = 0;
            Dictionary<int, List<AddressPoint>> addressPoints_ByCountyId = [];
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] is not int countyId)
                {
                    outside++;
                    continue;
                }

                if (!addressPoints_ByCountyId.TryGetValue(countyId, out List<AddressPoint>? addressPoints_CountyId))
                {
                    addressPoints_CountyId = [];
                    addressPoints_ByCountyId[countyId] = addressPoints_CountyId;
                }

                addressPoints_CountyId.Add(addressPoints_Valid[i]);
            }

            int inside = 0;
            int nearest = 0;
            int unmatched = 0;
            long written = 0;
            foreach (KeyValuePair<int, List<AddressPoint>> keyValuePair in addressPoints_ByCountyId)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<Classes.Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(keyValuePair.Key, subdivisionIds: null, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (building2Ds is null)
                {
                    return null;
                }

                List<GIS.Classes.Building2D> building2Ds_GIS = Convert.ToDiGi(building2Ds) ?? [];

                AddressPointMatchResult? addressPointMatchResult = GIS.Create.AddressPointMatchResult(keyValuePair.Value, building2Ds_GIS, maxDistance);
                if (addressPointMatchResult?.AddressPointMatches is not List<AddressPointMatch> addressPointMatches)
                {
                    return null;
                }

                foreach (AddressPointMatch addressPointMatch in addressPointMatches)
                {
                    switch (addressPointMatch.AddressPointMatchType)
                    {
                        case AddressPointMatchType.Inside:
                            inside++;
                            break;

                        case AddressPointMatchType.Nearest:
                            nearest++;
                            break;

                        default:
                            unmatched++;
                            break;
                    }
                }

                long? written_CountyId = await addressPointPostgreSQLConverter.UpdateAsync(keyValuePair.Key, addressPointMatches, batchSize, commandTimeout, cancellationToken);
                if (written_CountyId is null)
                {
                    return null;
                }

                written += written_CountyId.Value;
            }

            return new AddressPointUpdateResult(addressPoints_ByCountyId.Keys.OrderBy(x => x), inside, nearest, unmatched, outside, skipped, written);
        }
    }
}
