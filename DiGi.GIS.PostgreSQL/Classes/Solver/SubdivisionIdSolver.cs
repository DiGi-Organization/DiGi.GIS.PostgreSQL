using DiGi.Core.Interfaces;
using DiGi.Geometry.Core.Enums;
using DiGi.Geometry.Planar.Classes;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// Picks the <c>subdivision_id</c> of a building from a subdivision layer held in memory - the same pick <c>Building2DPostgreSQLConverter.GetSubdivisionIdAsync</c> makes against the database, without the round trip.
    /// <para>Built once per county over the subdivision rows whose boxes meet the county's, and reused for every building of that county: set <see cref="Input"/> to the building, call <see cref="Solve"/>, read <see cref="Output"/>. The database path re-read and deserialised every overlapping subdivision polygon per building - in Warsaw the 412 KB city outline 155 307 times - and intersected the building with each; that ran at about 26 buildings a second, a week for the estate (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/79">DiGi.GIS.PostgreSQL#79</see>).</para>
    /// <para>The pick is the one of <c>Query.SubdivisionId</c>: among the candidates whose box meets the building's, the largest overlap wins, ties go to the smallest container, then the lowest identifier; a single candidate is taken as it stands. Containment is decided from the building's outline vertices through a <see cref="PolygonalFace2DPointRelationSolver"/> per subdivision - every vertex inside or on the polygon means the overlap is the building's own area, which is what every container of a nested layer scores - and only a building straddling a boundary is intersected. A concavity of a subdivision boundary narrower than a building could slip between two inside vertices unseen; the pick is a rule for attributing a building to one unit, not a survey, and that is accepted.</para>
    /// <para><see cref="Solve"/> answers false when the layer holds no subdivision whose box meets the building's - the building may sit outside the layer loaded, or in a place only an <see cref="GIS.Classes.AdministrativeDivision"/> covers - and the caller falls back to the database path for it. Not thread-safe.</para>
    /// </summary>
    public class SubdivisionIdSolver : IOneToOneSolver<GIS.Classes.Building2D, int?>
    {
        private readonly double tolerance;
        private readonly List<(int Id, BoundingBox2D BoundingBox2D, PolygonalFace2D PolygonalFace2D, double Area, PolygonalFace2DPointRelationSolver PolygonalFace2DPointRelationSolver)> subdivisions = [];

        private GIS.Classes.Building2D? building2D_Input = null;
        private int? output = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubdivisionIdSolver"/> class over the given administrative rows.
        /// </summary>
        /// <param name="administrativeAreal2Ds">The rows of the layer. Rows that are not an <see cref="GIS.Classes.AdministrativeSubdivision"/> with a polygon are skipped - the <see cref="GIS.Classes.AdministrativeDivision"/> rows stored under the subdivision type are the database path's last resort, not a candidate here.</param>
        /// <param name="tolerance">The distance tolerance of the box test and the containment test, and the area tolerance of the pick.</param>
        public SubdivisionIdSolver(IEnumerable<AdministrativeAreal2D>? administrativeAreal2Ds, double tolerance = Core.Constants.Tolerance.MacroDistance)
        {
            this.tolerance = tolerance;

            if (administrativeAreal2Ds is null)
            {
                return;
            }

            foreach (AdministrativeAreal2D administrativeAreal2D in administrativeAreal2Ds)
            {
                if (administrativeAreal2D?.ToDiGi() is not GIS.Classes.AdministrativeSubdivision administrativeSubdivision || administrativeSubdivision.PolygonalFace2D is not PolygonalFace2D polygonalFace2D || polygonalFace2D.GetBoundingBox() is not BoundingBox2D boundingBox2D)
                {
                    continue;
                }

                subdivisions.Add((administrativeAreal2D.Id, boundingBox2D, polygonalFace2D, polygonalFace2D.GetArea(), new PolygonalFace2DPointRelationSolver(polygonalFace2D, tolerance)));
            }
        }

        /// <summary>
        /// Gets the number of subdivisions the layer holds.
        /// </summary>
        public int Count
        {
            get
            {
                return subdivisions.Count;
            }
        }

        /// <summary>
        /// Sets the building to attribute on the next <see cref="Solve"/> call.
        /// </summary>
        public GIS.Classes.Building2D? Input
        {
            set
            {
                building2D_Input = value;
            }
        }

        /// <summary>
        /// Gets the subdivision identifier picked by the last successful <see cref="Solve"/>, or null when the candidates met the building's box but none overlapped it.
        /// </summary>
        public int? Output
        {
            get
            {
                return output;
            }
        }

        /// <summary>
        /// Attributes <see cref="Input"/> to one subdivision of the layer and stores the pick in <see cref="Output"/>.
        /// </summary>
        /// <returns>True if the layer could answer - including a null <see cref="Output"/> for a building overlapping none of the candidates whose box it meets; false if the building has no polygon or no subdivision's box meets it, in which case the database path decides.</returns>
        public bool Solve()
        {
            output = null;
            if (building2D_Input?.PolygonalFace2D is not PolygonalFace2D polygonalFace2D || polygonalFace2D.GetBoundingBox() is not BoundingBox2D boundingBox2D)
            {
                return false;
            }

            // The same first cut as the database path: every subdivision whose box meets the building's box,
            // widened by the tolerance.
            List<int> candidates = [];
            for (int i = 0; i < subdivisions.Count; i++)
            {
                if (subdivisions[i].BoundingBox2D.InRange(boundingBox2D, tolerance))
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            if (candidates.Count == 1)
            {
                output = subdivisions[candidates[0]].Id;
                return true;
            }

            List<Point2D>? point2Ds = polygonalFace2D.ExternalEdge?.GetPoints();
            if (point2Ds is null || point2Ds.Count == 0)
            {
                return false;
            }

            // A subdivision holding every outline vertex holds the building: its overlap is the building's own
            // area, the most any candidate can score, so the pick among such containers is the smallest one.
            int index_Container = -1;
            foreach (int index in candidates)
            {
                PolygonalFace2DPointRelationSolver polygonalFace2DPointRelationSolver = subdivisions[index].PolygonalFace2DPointRelationSolver;

                bool contains = true;
                for (int j = 0; j < point2Ds.Count; j++)
                {
                    polygonalFace2DPointRelationSolver.Input = point2Ds[j];
                    if (!polygonalFace2DPointRelationSolver.Solve() || polygonalFace2DPointRelationSolver.Output == PointRelation.Outside)
                    {
                        contains = false;
                        break;
                    }
                }

                if (!contains)
                {
                    continue;
                }

                if (index_Container == -1)
                {
                    index_Container = index;
                    continue;
                }

                double area_Difference = subdivisions[index].Area - subdivisions[index_Container].Area;
                if (area_Difference < -tolerance || (System.Math.Abs(area_Difference) <= tolerance && subdivisions[index].Id < subdivisions[index_Container].Id))
                {
                    index_Container = index;
                }
            }

            if (index_Container != -1)
            {
                output = subdivisions[index_Container].Id;
                return true;
            }

            // Straddling a boundary: the overlaps decide, exactly as on the database path.
            List<(int Id, double Area_Intersection, double Area_Container)> tuples = [];
            foreach (int index in candidates)
            {
                List<PolygonalFace2D>? polygonalFace2Ds_Intersection = Geometry.Planar.Query.Intersection(polygonalFace2D, subdivisions[index].PolygonalFace2D);

                double area_Intersection = 0;
                if (polygonalFace2Ds_Intersection is not null && polygonalFace2Ds_Intersection.Count != 0)
                {
                    area_Intersection = polygonalFace2Ds_Intersection.ConvertAll(x => x.GetArea()).Sum();
                }

                if (area_Intersection <= tolerance)
                {
                    continue;
                }

                tuples.Add((subdivisions[index].Id, area_Intersection, subdivisions[index].Area));
            }

            output = Query.SubdivisionId(tuples, tolerance);
            return true;
        }
    }
}
