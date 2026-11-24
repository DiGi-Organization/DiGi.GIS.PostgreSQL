using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Operation.Distance;

namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The exterior ring of one county part, converted to NetTopologySuite and indexed once per run, so that filing a footprint against it costs an index lookup instead of a conversion of the whole outline (DiGi.GIS.PostgreSQL#170 measured it as the dominant cost of the import).
    /// <para><see cref="DiGi.Geometry.Planar.Interfaces.IPolygonal2D.InRange(DiGi.Geometry.Planar.Interfaces.ISegmentable2D, double)"/> on a <see cref="DiGi.Geometry.Planar.Classes.Polygon2D"/> converts both shapes to NetTopologySuite on every call once either has more than 16 points, so filing one footprint rebuilt the outlines of thousands of vertices of every candidate county - about 99 % of the open building import. This holds the result of that conversion, prepared for the two questions the call asks: whether the outline intersects the footprint (<see cref="PreparedPolygon"/>) and, when it does not, how far apart they are (<see cref="IndexedFacetDistance"/>).</para>
    /// <para>Run-local derived data, like the dictionaries of county rings it is built beside: built by <see cref="Create.CountyFilingGeometry(DiGi.Geometry.Planar.Interfaces.IPolygonal2D)"/> on the run's thread, never cached across runs. The factory forces every lazily built index of both structures before it returns, so the parallel conversion workers of DiGi.GIS.PostgreSQL#170 only ever read them; the constructor only assigns.</para>
    /// </summary>
    public class CountyFilingGeometry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CountyFilingGeometry"/> class from structures already built and warmed; use <see cref="Create.CountyFilingGeometry(DiGi.Geometry.Planar.Interfaces.IPolygonal2D)"/>, which builds them.
        /// </summary>
        /// <param name="preparedPolygon">The prepared exterior ring of the county part, as a polygon.</param>
        /// <param name="indexedFacetDistance">The facet distance index of the same polygon.</param>
        public CountyFilingGeometry(PreparedPolygon preparedPolygon, IndexedFacetDistance indexedFacetDistance)
        {
            PreparedPolygon = preparedPolygon;
            IndexedFacetDistance = indexedFacetDistance;
        }

        /// <summary>
        /// Gets the prepared exterior ring of the county part, as a polygon; its <c>Geometry</c> is the polygon the legacy call would have converted.
        /// </summary>
        public PreparedPolygon PreparedPolygon { get; }

        /// <summary>
        /// Gets the facet distance index of the same polygon - the distance between outlines, without the interior.
        /// </summary>
        public IndexedFacetDistance IndexedFacetDistance { get; }
    }
}
