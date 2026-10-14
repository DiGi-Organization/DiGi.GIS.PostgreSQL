using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Picks the one subdivision a building is filed under, out of every subdivision its outline overlaps.
        /// <para>The subdivision layer nests - a city, its districts and their neighbourhoods are all subdivisions of the one municipality - so a building is usually inside several at once, and each of those containers yields the same overlap: the building's own area. The overlap decides only where the building straddles a boundary. Among the candidates whose overlap is the largest, within <paramref name="tolerance"/> of one another, the <b>smallest container</b> wins: the most specific unit the building sits in, and the one unit the per-building consumers - occupancy, building data - need exactly one of. The identifier is the last resort, for two candidates that are the same size to the tolerance.</para>
        /// <para>Before <see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/77">DiGi.GIS.PostgreSQL#77</see> the tie went to the lowest identifier, which filed 155 307 Warsaw buildings under the 49 lowest ids and none under any district.</para>
        /// <para>The largest-overlap band is a filter rather than a comparer on purpose: a comparer treating values within a tolerance as equal is not transitive and a sort over it is undefined.</para>
        /// </summary>
        /// <param name="tuples">One tuple per candidate: its identifier, the area of its overlap with the building, and the area of the candidate itself. Candidates with a non-positive overlap are ignored.</param>
        /// <param name="tolerance">The area within which two overlaps, or two container areas, count as equal.</param>
        /// <returns>The identifier of the chosen subdivision, or <see langword="null"/> when there is no candidate.</returns>
        public static int? SubdivisionId(IEnumerable<(int Id, double Area_Intersection, double Area_Container)>? tuples, double tolerance = Core.Constants.Tolerance.MacroDistance)
        {
            if (tuples is null)
            {
                return null;
            }

            // Walked twice, so materialised once.
            List<(int Id, double Area_Intersection, double Area_Container)> tuples_List = [.. tuples];

            double area_Intersection_Max = double.NegativeInfinity;
            foreach ((int Id, double Area_Intersection, double Area_Container) tuple in tuples_List)
            {
                if (tuple.Area_Intersection > 0 && tuple.Area_Intersection > area_Intersection_Max)
                {
                    area_Intersection_Max = tuple.Area_Intersection;
                }
            }

            if (double.IsNegativeInfinity(area_Intersection_Max))
            {
                return null;
            }

            int? result = null;
            double area_Container_Result = double.PositiveInfinity;

            foreach ((int Id, double Area_Intersection, double Area_Container) tuple in tuples_List)
            {
                if (tuple.Area_Intersection <= 0 || tuple.Area_Intersection < area_Intersection_Max - tolerance)
                {
                    continue;
                }

                if (result is null)
                {
                    result = tuple.Id;
                    area_Container_Result = tuple.Area_Container;
                    continue;
                }

                if (tuple.Area_Container < area_Container_Result - tolerance || (Math.Abs(tuple.Area_Container - area_Container_Result) <= tolerance && tuple.Id < result.Value))
                {
                    result = tuple.Id;
                    area_Container_Result = tuple.Area_Container;
                }
            }

            return result;
        }
    }
}
