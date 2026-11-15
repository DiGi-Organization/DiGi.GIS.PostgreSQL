using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Picks the EPW station nearest to a point from a list of station locations, as <c>EPWFilePostgreSQLConverter.GetEPWFileWithIdAsync</c> picks it in SQL: the smallest planar distance between EPSG:2180 points.
        /// <para>For a batch that positions many buildings against the stations of <c>EPWFilePostgreSQLConverter.GetEPWFileLocationsAsync</c>, instead of one database round trip - and one weather file - per building. Two stations at exactly the same distance resolve to the lower id; the SQL leaves such a tie unspecified.</para>
        /// </summary>
        /// <param name="ePWFileLocations">The id and EPSG:2180 location of each station.</param>
        /// <param name="x">The X coordinate (EPSG:2180 easting, in metres).</param>
        /// <param name="y">The Y coordinate (EPSG:2180 northing, in metres).</param>
        /// <returns>The id of the nearest station, or null when the list is null or empty or the point is not finite.</returns>
        public static int? NearestEPWFileId(this IEnumerable<(int Id, double X, double Y)>? ePWFileLocations, double x, double y)
        {
            if (ePWFileLocations is null || !double.IsFinite(x) || !double.IsFinite(y))
            {
                return null;
            }

            int? result = null;
            double distanceSquared_Min = double.MaxValue;
            foreach ((int id, double x_Station, double y_Station) in ePWFileLocations)
            {
                double dx = x_Station - x;
                double dy = y_Station - y;
                double distanceSquared = (dx * dx) + (dy * dy);
                if (distanceSquared < distanceSquared_Min || (distanceSquared == distanceSquared_Min && result is not null && id < result.Value))
                {
                    distanceSquared_Min = distanceSquared;
                    result = id;
                }
            }

            return result;
        }
    }
}
