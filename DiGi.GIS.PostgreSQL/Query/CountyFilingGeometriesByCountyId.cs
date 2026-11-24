using DiGi.Geometry.Planar.Interfaces;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Indexes the exterior ring of every county part once for filing, keyed by the same part identifiers as the rings.
        /// <para>Built beside <see cref="Polygonal2DsByCountyId(IEnumerable{AdministrativeAreal2D})"/> once per run and passed to <see cref="OpenBuildingCountyId"/>, which then files each footprint against the indexes instead of converting every candidate county outline again (<see cref="Create.CountyFilingGeometry(IPolygonal2D)"/>). A part the factory leaves to the legacy call - a small ring, a ring NetTopologySuite refuses - is simply absent, and the filing falls back to its ring.</para>
        /// <para>Run-local: the result is read concurrently by the conversion workers of DiGi.GIS.PostgreSQL#170 and never written after this returns.</para>
        /// </summary>
        /// <param name="polygonal2Ds_ByCountyId">The exterior ring of every county part, keyed by part identifier.</param>
        /// <returns>The indexed ring of each part that has one, keyed by part identifier. Empty when nothing could be indexed.</returns>
        public static Dictionary<int, CountyFilingGeometry> CountyFilingGeometriesByCountyId(this IDictionary<int, IPolygonal2D>? polygonal2Ds_ByCountyId)
        {
            Dictionary<int, CountyFilingGeometry> result = [];

            if (polygonal2Ds_ByCountyId is null)
            {
                return result;
            }

            foreach (KeyValuePair<int, IPolygonal2D> pair in polygonal2Ds_ByCountyId)
            {
                if (pair.Value.CountyFilingGeometry() is CountyFilingGeometry countyFilingGeometry)
                {
                    result[pair.Key] = countyFilingGeometry;
                }
            }

            return result;
        }
    }
}
