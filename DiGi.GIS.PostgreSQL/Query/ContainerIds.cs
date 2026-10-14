using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Finds, for every face, the other faces that contain it - the nesting of a layer whose areas lie inside one another, as a city holds its districts and their neighbourhoods.
        /// <para>A face is contained by another when the other is larger, its bounding box holds this face's box within <paramref name="tolerance"/>, and a point known to be inside this face (<see cref="PolygonalFace2D.GetInternalPoint(double)"/>) is inside the other by the <see cref="Building2DPostgreSQLConverter.IsInside(PolygonalFace2D?, BoundingBox2D?, Point2D?, double)"/> rule. One interior point rather than a polygon intersection: the areas of one layer do not cross, they nest or they touch, and a point settles which in constant time where an intersection would cost the product of the two rings.</para>
        /// <para>The containers of a face are listed <b>smallest first</b>, so the first is its immediate parent and the last is the outermost. A face with no container is top level; a layer that does not nest at all - every flat county - answers an empty list for every face. That is what the municipality occupancy roll-up sums over, and what makes a nested city count once rather than once per level (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see>).</para>
        /// <para>Boxes, areas and interior points are derived once up front. A face whose box or interior point cannot be derived neither contains nor is contained.</para>
        /// </summary>
        /// <param name="polygonalFace2Ds_ById">The faces of one layer, keyed by identifier. May be <see langword="null"/>.</param>
        /// <param name="tolerance">The distance within which a box edge still counts as inside the container's box, and the tolerance of the interior point test.</param>
        /// <returns>Every identifier given mapped to the identifiers of the faces containing it, smallest first; an empty list for a top-level face. Empty when nothing was given.</returns>
        public static Dictionary<int, List<int>> ContainerIds(this IReadOnlyDictionary<int, PolygonalFace2D>? polygonalFace2Ds_ById, double tolerance = Core.Constants.Tolerance.MacroDistance)
        {
            Dictionary<int, List<int>> result = [];

            if (polygonalFace2Ds_ById is null || polygonalFace2Ds_ById.Count == 0)
            {
                return result;
            }

            List<(int Id, PolygonalFace2D PolygonalFace2D, BoundingBox2D BoundingBox2D, double Area, Point2D Point2D_Internal)> tuples = [];

            foreach (KeyValuePair<int, PolygonalFace2D> keyValuePair in polygonalFace2Ds_ById)
            {
                result[keyValuePair.Key] = [];

                if (keyValuePair.Value is not PolygonalFace2D polygonalFace2D)
                {
                    continue;
                }

                if (polygonalFace2D.GetBoundingBox() is not BoundingBox2D boundingBox2D || polygonalFace2D.GetInternalPoint(tolerance) is not Point2D point2D_Internal)
                {
                    continue;
                }

                tuples.Add((keyValuePair.Key, polygonalFace2D, boundingBox2D, polygonalFace2D.GetArea(), point2D_Internal));
            }

            // Largest first, so walking back from a face meets its containers smallest first.
            tuples.Sort((x, y) =>
            {
                int comparison = y.Area.CompareTo(x.Area);
                return comparison != 0 ? comparison : x.Id.CompareTo(y.Id);
            });

            for (int i = 0; i < tuples.Count; i++)
            {
                (int Id, PolygonalFace2D PolygonalFace2D, BoundingBox2D BoundingBox2D, double Area, Point2D Point2D_Internal) tuple_Inner = tuples[i];

                List<int> containerIds = result[tuple_Inner.Id];

                // Only the larger faces, which all precede this one, can contain it; walked backwards, the
                // nearest in size comes first.
                for (int j = i - 1; j >= 0; j--)
                {
                    (int Id, PolygonalFace2D PolygonalFace2D, BoundingBox2D BoundingBox2D, double Area, Point2D Point2D_Internal) tuple_Outer = tuples[j];

                    if (tuple_Outer.Area <= tuple_Inner.Area)
                    {
                        continue;
                    }

                    if (!Contains(tuple_Outer.BoundingBox2D, tuple_Inner.BoundingBox2D, tolerance))
                    {
                        continue;
                    }

                    if (!Building2DPostgreSQLConverter.IsInside(tuple_Outer.PolygonalFace2D, tuple_Outer.BoundingBox2D, tuple_Inner.Point2D_Internal, tolerance))
                    {
                        continue;
                    }

                    containerIds.Add(tuple_Outer.Id);
                }
            }

            return result;

            // BoundingBox2D.Inside is strict, and a district shares its outer edge with the city it sits in - the
            // box test has to admit a shared edge, so it is written on the coordinates with the tolerance on the
            // outside of the container.
            static bool Contains(BoundingBox2D boundingBox2D_Outer, BoundingBox2D boundingBox2D_Inner, double tolerance)
            {
                return boundingBox2D_Inner.Min.X >= boundingBox2D_Outer.Min.X - tolerance
                    && boundingBox2D_Inner.Min.Y >= boundingBox2D_Outer.Min.Y - tolerance
                    && boundingBox2D_Inner.Max.X <= boundingBox2D_Outer.Max.X + tolerance
                    && boundingBox2D_Inner.Max.Y <= boundingBox2D_Outer.Max.Y + tolerance;
            }
        }
    }
}
