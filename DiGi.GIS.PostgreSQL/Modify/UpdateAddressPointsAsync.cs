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
        /// <para><paramref name="countyIds"/> are county part ids, normally every polygon part of one county, never a county code: 18 counties have disconnected territory stored as one row per part (<c>Coding - GIS Administrative Data.md</c>). A point is assigned to the part whose face contains it (<see cref="Query.IdsByPoint2Ds(IDictionary{int, PolygonalFace2D}?, IReadOnlyList{Point2D}?, double)"/>, lowest id on overlap); a point in none of the parts is, when <paramref name="retryOutside"/> is true, retried against the parts of other counties containing it and filed under the part it lies in (<see cref="AddressPointUpdateResult.Rescued"/>), because the commune code of a point can name a county whose boundary differs from the BDOT10k polygon its building is filed under (DiGi.GIS.PostgreSQL#109); only a point in no county part is counted as outside and not written, and so is a rescued point that matches no building of the part it lies in (<see cref="AddressPointMatchType.Unmatched"/>), because a point of that kind is a PRG coordinate error far from its commune rather than a boundary mismatch (DiGi.GIS.PostgreSQL#118).</para>
        /// <para>Known limit: a point is matched only against the buildings of its own part, so a point within <paramref name="maxDistance"/> of a building filed under a sibling part, across the part boundary, is matched to its own part's nearest building or left unmatched.</para>
        /// <para>Known limit: PRG places many points on a plot, road or verge rather than on a building, so a share of every county stays <see cref="AddressPointMatchType.Unmatched"/> (8.5 % nationally, 5 % to 11.5 % per voivodeship, up to 36 % in a few counties; DiGi.GIS.PostgreSQL#111, #113). In the sampled worst counties the far points lie on empty plots, fields or roads, or 25 m to 60 m from a mapped building; BDOT10k coverage did not explain them, and it is not reported upstream. The 10 m <paramref name="maxDistance"/> is kept (a larger value risks matching a neighbour's building in towns, DiGi.GIS.PostgreSQL#114).</para>
        /// <para>Re-running an import replaces each point's row unless the stored PRG version is newer (<see cref="AddressPointPostgreSQLConverter.UpdateAsync(Npgsql.NpgsqlConnection?, int, IEnumerable{AddressPointMatch?}?, int, int, CancellationToken)"/>). All three converters read and write the Main database.</para>
        /// </summary>
        /// <param name="addressPointPostgreSQLConverter">The converter of the address point table.</param>
        /// <param name="building2DPostgreSQLConverter">The converter the building footprints of each part are read through.</param>
        /// <param name="administrativeAreal2DPostgreSQLConverter">The converter the county part polygons are read through.</param>
        /// <param name="addressPoints">The address points of the county.</param>
        /// <param name="countyIds">The county part ids of the county.</param>
        /// <param name="maxDistance">The distance, in m, within which a point outside every footprint is assigned to the nearest one.</param>
        /// <param name="retryOutside">True to retry a point that lies in none of <paramref name="countyIds"/> against the county parts of other counties that contain it; false to count it as outside.</param>
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
            bool retryOutside = true,
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
            foreach (AddressPoint? addressPoint in addressPoints)
            {
                if (addressPoint?.Location is null || string.IsNullOrWhiteSpace(addressPoint.ReferenceId) || addressPoint.SearchText() is null)
                {
                    skipped++;
                    continue;
                }

                addressPoints_Valid.Add(addressPoint);
            }

            (Dictionary<int, List<AddressPoint>> AddressPoints_ByCountyId, List<AddressPoint> AddressPoints_Outside)? addressPointsByCountyId = polygonalFace2Ds_ById.AddressPointsByCountyId(addressPoints_Valid);
            if (addressPointsByCountyId is null)
            {
                return null;
            }

            Dictionary<int, List<AddressPoint>> addressPoints_ByCountyId = addressPointsByCountyId.Value.AddressPoints_ByCountyId;
            int outside = addressPointsByCountyId.Value.AddressPoints_Outside.Count;
            int rescued = 0;
            HashSet<int> countyIds_Rescued = [];
            if (retryOutside && outside != 0)
            {
                // The commune code of a point can name a county whose polygon does not contain it, while the building under it is filed
                // under the county that does: the PRG and BDOT10k boundaries differ (DiGi.GIS.PostgreSQL#109). Such a point is filed under the part it lies in.
                BoundingBox2D boundingBox2D = new(addressPointsByCountyId.Value.AddressPoints_Outside.Select(x => x.Location).OfType<Point2D>());
                HashSet<int>? countyIds_Rescue = await administrativeAreal2DPostgreSQLConverter.GetCountyIdsByBoundingBox2DAsync(boundingBox2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (countyIds_Rescue is null)
                {
                    return null;
                }

                countyIds_Rescue.ExceptWith(polygonalFace2Ds_ById.Keys);
                if (countyIds_Rescue.Count != 0)
                {
                    List<Classes.AdministrativeAreal2D>? administrativeAreal2Ds_Rescue = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DsByIdsAsync(countyIds_Rescue, commandTimeout, cancellationToken);
                    if (administrativeAreal2Ds_Rescue is null)
                    {
                        return null;
                    }

                    (Dictionary<int, List<AddressPoint>> AddressPoints_ByCountyId, List<AddressPoint> AddressPoints_Outside)? addressPointsByCountyId_Rescue = administrativeAreal2Ds_Rescue.PolygonalFace2DsById().AddressPointsByCountyId(addressPointsByCountyId.Value.AddressPoints_Outside);
                    if (addressPointsByCountyId_Rescue is null)
                    {
                        return null;
                    }

                    foreach (KeyValuePair<int, List<AddressPoint>> keyValuePair in addressPointsByCountyId_Rescue.Value.AddressPoints_ByCountyId)
                    {
                        addressPoints_ByCountyId[keyValuePair.Key] = keyValuePair.Value;
                        countyIds_Rescued.Add(keyValuePair.Key);
                        rescued += keyValuePair.Value.Count;
                    }

                    outside = addressPointsByCountyId_Rescue.Value.AddressPoints_Outside.Count;
                }
            }

            int inside = 0;
            int nearest = 0;
            int unmatched = 0;
            long written = 0;
            List<int> countyIds_Written = [];
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

                // A rescue is evidence of a boundary mismatch only when a building of the part it lies in is there to match: PRG coordinate errors
                // (a point with its two coordinates reversed) land in distant counties and match nothing (DiGi.GIS.PostgreSQL#118).
                if (countyIds_Rescued.Contains(keyValuePair.Key))
                {
                    int unmatched_Rescued = addressPointMatches.RemoveAll(x => x.AddressPointMatchType == AddressPointMatchType.Unmatched);
                    outside += unmatched_Rescued;
                    rescued -= unmatched_Rescued;
                    if (addressPointMatches.Count == 0)
                    {
                        continue;
                    }
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

                countyIds_Written.Add(keyValuePair.Key);
                written += written_CountyId.Value;
            }

            return new AddressPointUpdateResult(countyIds_Written.OrderBy(x => x), inside, nearest, unmatched, outside, rescued, skipped, written);
        }
    }
}
