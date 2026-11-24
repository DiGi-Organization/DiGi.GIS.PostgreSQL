using DiGi.GIS.PostgreSQL.Classes;
using NetTopologySuite.Geometries;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Decides whether a footprint ring intersects an indexed county ring or lies closer to it than the tolerance - the answer <see cref="DiGi.Geometry.Planar.Classes.Polygon2D.InRange(DiGi.Geometry.Planar.Interfaces.ISegmentable2D, double)"/> gives on its NetTopologySuite branch, without converting the county outline again.
        /// <para>The legacy branch is <see cref="DiGi.Geometry.Planar.Query.InRange(NetTopologySuite.Geometries.Geometry, NetTopologySuite.Geometries.Geometry, double)"/> on the county polygon and the footprint ring: intersection first, then the distance between the county polygon and the footprint ring taken as a filled polygon. Both steps are answered from the indexes:</para>
        /// <para>- Intersection: the prepared polygon answers the same predicate as the plain one, in logarithmic rather than linear time per footprint vertex.</para>
        /// <para>- Distance, once they do not intersect: the shapes are either apart, where the distance between the areas is the distance between the outlines - which the facet index computes from the same segment pairs, in the same argument order, as the plain distance - or the county lies inside the footprint's area, where the area distance is zero but the outline distance is not. That second case is possible only when the footprint's envelope covers the county's, and there the legacy step runs unchanged.</para>
        /// </summary>
        /// <param name="countyFilingGeometry">The indexed county ring.</param>
        /// <param name="linearRing">The footprint ring, converted once per footprint.</param>
        /// <param name="tolerance">The gap below which the shapes are in range.</param>
        /// <returns>True when the footprint intersects the county ring or lies closer to it than the tolerance; otherwise false, also when either argument is null.</returns>
        public static bool InRange(this CountyFilingGeometry? countyFilingGeometry, LinearRing? linearRing, double tolerance)
        {
            if (countyFilingGeometry is null || linearRing is null)
            {
                return false;
            }

            if (countyFilingGeometry.PreparedPolygon.Intersects(linearRing))
            {
                return true;
            }

            NetTopologySuite.Geometries.Geometry geometry = countyFilingGeometry.PreparedPolygon.Geometry;

            if (linearRing.EnvelopeInternal.Covers(geometry.EnvelopeInternal))
            {
                // The county may lie inside the footprint's area - the one configuration where the distance
                // between outlines differs from the distance between areas. Never met on real data; decided
                // exactly as before rather than reasoned about.
                return DiGi.Geometry.Planar.Query.InRange(geometry, linearRing, tolerance);
            }

            return countyFilingGeometry.IndexedFacetDistance.Distance(linearRing) < tolerance;
        }
    }
}
