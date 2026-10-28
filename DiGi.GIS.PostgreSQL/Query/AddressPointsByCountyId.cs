using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Files each address point under the county part whose face contains it, without touching the database.
        /// <para>A point that lies in none of the faces is returned separately rather than filed anywhere, so a caller can retry it against the faces of other counties (<c>Modify.UpdateAddressPointsAsync</c>). The decision is the one <see cref="IdsByPoint2Ds(IDictionary{int, PolygonalFace2D}?, IReadOnlyList{Point2D}?, double)"/> makes: lowest identifier wins where faces overlap.</para>
        /// </summary>
        /// <param name="polygonalFace2Ds_ById">The county part faces to decide against, keyed by county part id.</param>
        /// <param name="addressPoints">The address points to file. Points without a location are returned as outside.</param>
        /// <param name="tolerance">The distance a point may lie outside a face and still be counted as within it.</param>
        /// <returns>The points filed under the county part id containing them, and the points lying in none of the faces; or null when an argument is null.</returns>
        public static (Dictionary<int, List<AddressPoint>> AddressPoints_ByCountyId, List<AddressPoint> AddressPoints_Outside)? AddressPointsByCountyId(this IDictionary<int, PolygonalFace2D>? polygonalFace2Ds_ById, IEnumerable<AddressPoint?>? addressPoints, double tolerance = Core.Constants.Tolerance.MacroDistance)
        {
            if (polygonalFace2Ds_ById is null || addressPoints is null)
            {
                return null;
            }

            List<AddressPoint> addressPoints_Located = [];
            List<Point2D> point2Ds = [];
            List<AddressPoint> addressPoints_Outside = [];
            foreach (AddressPoint? addressPoint in addressPoints)
            {
                if (addressPoint is null)
                {
                    continue;
                }

                if (addressPoint.Location is not Point2D point2D)
                {
                    addressPoints_Outside.Add(addressPoint);
                    continue;
                }

                addressPoints_Located.Add(addressPoint);
                point2Ds.Add(point2D);
            }

            int?[]? ids = polygonalFace2Ds_ById.IdsByPoint2Ds(point2Ds, tolerance);
            if (ids is null)
            {
                return null;
            }

            Dictionary<int, List<AddressPoint>> addressPoints_ByCountyId = [];
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] is not int countyId)
                {
                    addressPoints_Outside.Add(addressPoints_Located[i]);
                    continue;
                }

                if (!addressPoints_ByCountyId.TryGetValue(countyId, out List<AddressPoint>? addressPoints_CountyId))
                {
                    addressPoints_CountyId = [];
                    addressPoints_ByCountyId[countyId] = addressPoints_CountyId;
                }

                addressPoints_CountyId.Add(addressPoints_Located[i]);
            }

            return (addressPoints_ByCountyId, addressPoints_Outside);
        }
    }
}
