using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Operation.Distance;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Converts the exterior ring of one county part to NetTopologySuite once and indexes it for filing, or returns null where the legacy <see cref="IPolygonal2D.InRange(ISegmentable2D, double)"/> call would not have converted it either.
        /// <para>The result answers exactly what <see cref="Polygon2D.InRange(ISegmentable2D, double)"/> answers on its NetTopologySuite branch (see <see cref="Query.InRange(Classes.CountyFilingGeometry, LinearRing, double)"/>), so it is built only where that branch is the one taken for a county of this size: a <see cref="Polygon2D"/> of more than 16 points whose conversion succeeds. Any other ring - a small part, another <see cref="IPolygonal2D"/> type, a ring NetTopologySuite refuses - answers null and stays on the legacy call, which then decides exactly as before.</para>
        /// <para>Thread safety: <see cref="PreparedPolygon"/> and <see cref="IndexedFacetDistance"/> build parts of their indexes lazily on first use (the point-in-area interval tree, the segment intersection finder). NetTopologySuite 2.6 guards those builds with locks, and this factory additionally forces every one of them here, on the run's thread, before any conversion worker starts - so the workers of DiGi.GIS.PostgreSQL#170 only read finished indexes. Every query allocates its own visitors, detectors and queues.</para>
        /// </summary>
        /// <param name="polygonal2D">The exterior ring of the county part.</param>
        /// <returns>The indexed county ring, or null when the legacy call keeps deciding for this ring.</returns>
        public static Classes.CountyFilingGeometry? CountyFilingGeometry(this IPolygonal2D? polygonal2D)
        {
            if (polygonal2D is not Polygon2D polygon2D)
            {
                return null;
            }

            List<Point2D>? point2Ds = polygon2D.GetPoints();
            if (point2Ds is null || point2Ds.Count <= 16)
            {
                return null;
            }

            if (polygon2D.ToNTS_Polygon() is not Polygon polygon)
            {
                return null;
            }

            PreparedPolygon preparedPolygon = new(polygon);
            IndexedFacetDistance indexedFacetDistance = new(polygon);

            // Force the lazily built parts now, on this thread: the envelope, the segment intersection finder
            // (its monotone-chain tree is built in its constructor) and the point-in-area locator, whose
            // interval tree is created and packed on the first Locate. The facet distance tree is built in
            // the constructor above.
            _ = polygon.EnvelopeInternal;
            _ = preparedPolygon.IntersectionFinder;
            preparedPolygon.PointLocator.Locate(polygon.Coordinate);

            return new Classes.CountyFilingGeometry(preparedPolygon, indexedFacetDistance);
        }
    }
}
