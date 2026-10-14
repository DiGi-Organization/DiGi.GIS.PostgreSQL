using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Asynchronously measures, for one county, how much of the buildings inside each given polygon the orthophoto store holds.
        /// <para>What the estimated partition counts cannot answer. Both tables are partitioned by <c>county_id</c>, so <c>reltuples</c> describes a whole county and there is no figure for any area inside it to be had from it - reporting the county's own factor for a subdivision is <see href="https://github.com/DiGi-Organization/DiGi.GIS.WebAPI/issues/8">DiGi.GIS.WebAPI issue #8</see>. This counts instead of estimating, and costs one read per side however many polygons are asked about.</para>
        /// <para><b>Membership is decided by geometry, not by the stored <c>subdivision_id</c>.</b> A building belongs to a polygon when its bounding-box centre lies strictly inside it - what <see cref="PolygonalFace2D.Inside(Point2D, double)"/> answers, asked through a <see cref="PolygonalFace2DPointRelationSolver"/> built once per polygon, because a city outline of 4 000 vertices tested against 155 000 centres through the face itself took 52 seconds on the deployed host (<see href="https://github.com/DiGi-Organization/DiGi.Geometry/issues/5">DiGi.Geometry#5</see>). The column files a building under one subdivision only, so where the subdivision layer nests it cannot say which buildings a district holds - Warsaw's districts counted zero by column while holding 155 307 buildings between them (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>). By polygon, a building inside a neighbourhood counts for the neighbourhood, its district and its city alike; <b>the results of nested polygons therefore overlap and must not be summed</b> - a caller wanting a municipality asks for the municipality's own polygon.</para>
        /// <para>The orthophoto side's own <c>subdivision_id</c> is deliberately not used. That column has never been written: not one of the 8 384 055 rows stored across 225 counties carries a value, measured 2026-08-26 through <c>gis/ortodatas/summariesbycountyids</c>. The orthophoto side is asked only whether it holds a reference.</para>
        /// <para>The two tables live in different databases - <c>building_2d</c> in the main store, <c>orto_datas</c> in the storage one - so this cannot be a join and is not one. The building side is read as bounding-box centres (<see cref="Building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(int, IEnumerable{int}?, int, CancellationToken)"/>, the JSONB column untouched), the orthophoto side as references, and the two are matched in memory. Each centre is tested against every polygon.</para>
        /// </summary>
        /// <param name="ortoDatasPostgreSQLConverter">The converter reading the orthophoto store.</param>
        /// <param name="building2DPostgreSQLConverter">The converter reading the building store.</param>
        /// <param name="countyId">The identifier of the county to measure. One polygon part, not a code - a multi-part county is measured a part at a time.</param>
        /// <param name="polygonalFace2Ds_ById">The polygons to measure, keyed by the identifier the results are reported under.</param>
        /// <param name="tolerance">The distance tolerance of the containment test.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains one <see cref="OrtoDatasCoverageResult"/> per polygon that holds at least one of the county's buildings, in identifier order, plus one carrying a null <see cref="OrtoDatasCoverageResult.AdministrativeAreal2DId"/> for the buildings inside none of them when there are any; or null when either converter or the polygon map is missing, either side could not be read, or the county holds no orthophoto row at all.</returns>
        public static async Task<List<OrtoDatasCoverageResult>?> CoveragesAsync(this OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter, Building2DPostgreSQLConverter? building2DPostgreSQLConverter, int countyId, IReadOnlyDictionary<int, PolygonalFace2D>? polygonalFace2Ds_ById, double tolerance = Core.Constants.Tolerance.MacroDistance, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (ortoDatasPostgreSQLConverter is null || building2DPostgreSQLConverter is null || polygonalFace2Ds_ById is null)
            {
                return null;
            }

            // Only the keys are wanted here - the reference is the whole of what the orthophoto side can say.
            // The read projects two columns and never object, which is what makes a whole-county comparison
            // affordable at all: that column holds the imagery for every year the row carries.
            Dictionary<string, int?>? subdivisionIds_OrtoDatas = await ortoDatasPostgreSQLConverter.GetSubdivisionIdsByCountyIdAsync(countyId, commandTimeout, cancellationToken);
            // An empty result is not an empty answer. A county holding no orthophoto row at all is a county
            // nothing has ever been downloaded for, which is a different fact from one that was downloaded and
            // covers none of its buildings - and the caller has to be able to tell them apart, because the
            // second is a measurement of nought per cent and the first is no measurement. Answered as nothing
            // here so that it reaches a caller the same way an absent partition does at county level, rather
            // than as a set of areas each reporting an authoritative-looking zero.
            if (subdivisionIds_OrtoDatas is null || subdivisionIds_OrtoDatas.Count == 0)
            {
                return null;
            }

            List<Building2DCentroid>? building2DCentroids = await building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(countyId, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
            if (building2DCentroids is null)
            {
                return null;
            }

            // One index per polygon, built once: it reads the rings into flat arrays and answers each centre from
            // the few edges whose vertical span covers it, with the box check inside. Every centre is tested
            // against every polygon, so this is the loop's whole cost.
            List<(int Id, PolygonalFace2DPointRelationSolver PolygonalFace2DPointRelationSolver)> tuples = [];
            foreach (KeyValuePair<int, PolygonalFace2D> keyValuePair in polygonalFace2Ds_ById)
            {
                if (keyValuePair.Value is PolygonalFace2D polygonalFace2D)
                {
                    PolygonalFace2DPointRelationSolver polygonalFace2DPointRelationSolver = new(polygonalFace2D, tolerance);
                    if (polygonalFace2DPointRelationSolver.EdgeCount != 0)
                    {
                        tuples.Add((keyValuePair.Key, polygonalFace2DPointRelationSolver));
                    }
                }
            }

            Dictionary<int, long> building2DCounts = [];
            Dictionary<int, long> ortoDatasCounts = [];

            long building2DCount_Unassigned = 0;
            long ortoDatasCount_Unassigned = 0;

            // A reference is unique only per county_id and no constraint enforces it, so a repeat inside one
            // county would otherwise be counted twice on the building side while the orthophoto side, keyed by
            // a unique index, counts it once - and the factor would climb above what the county actually holds.
            HashSet<string> references = [];

            foreach (Building2DCentroid building2DCentroid in building2DCentroids)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (building2DCentroid?.Reference is not string reference || string.IsNullOrWhiteSpace(reference))
                {
                    continue;
                }

                if (!references.Add(reference))
                {
                    continue;
                }

                bool hasOrtoDatas = subdivisionIds_OrtoDatas.ContainsKey(reference);
                Point2D point2D = new(building2DCentroid.X, building2DCentroid.Y);

                bool assigned = false;
                foreach ((int Id, PolygonalFace2DPointRelationSolver PolygonalFace2DPointRelationSolver) tuple in tuples)
                {
                    tuple.PolygonalFace2DPointRelationSolver.Input = point2D;
                    if (!tuple.PolygonalFace2DPointRelationSolver.Solve() || tuple.PolygonalFace2DPointRelationSolver.Output != PointRelation.Inside)
                    {
                        continue;
                    }

                    assigned = true;

                    building2DCounts.TryGetValue(tuple.Id, out long building2DCount);
                    building2DCounts[tuple.Id] = building2DCount + 1;

                    if (hasOrtoDatas)
                    {
                        ortoDatasCounts.TryGetValue(tuple.Id, out long ortoDatasCount);
                        ortoDatasCounts[tuple.Id] = ortoDatasCount + 1;
                    }
                }

                if (!assigned)
                {
                    building2DCount_Unassigned++;

                    if (hasOrtoDatas)
                    {
                        ortoDatasCount_Unassigned++;
                    }
                }
            }

            List<OrtoDatasCoverageResult> result = [];

            foreach (int id in building2DCounts.Keys.OrderBy(x => x))
            {
                ortoDatasCounts.TryGetValue(id, out long ortoDatasCount);
                result.Add(new OrtoDatasCoverageResult(countyId, id, building2DCounts[id], ortoDatasCount));
            }

            // Kept apart rather than folded into a neighbour, and absent altogether when the county has none.
            // Leaving these out of the result entirely would hide a county whose polygons do not cover its
            // buildings behind a set of figures that look complete.
            if (building2DCount_Unassigned > 0)
            {
                result.Add(new OrtoDatasCoverageResult(countyId, null, building2DCount_Unassigned, ortoDatasCount_Unassigned));
            }

            return result;
        }
    }
}
