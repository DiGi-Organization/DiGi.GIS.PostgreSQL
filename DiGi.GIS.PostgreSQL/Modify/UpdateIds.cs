using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Updates the identification properties of the destination administrative areal object using values from the source object.
        /// </summary>
        /// <param name="administrativeAreal2D_Destination">The destination AdministrativeAreal2D object to be updated.</param>
        /// <param name="administrativeAreal2D_Source">The source AdministrativeAreal2D object containing the new identification values.</param>
        /// <returns>True if the IDs were successfully updated; otherwise, false if either the destination or source object is null.</returns>
        public static bool UpdateIds(this AdministrativeAreal2D? administrativeAreal2D_Destination, AdministrativeAreal2D? administrativeAreal2D_Source)
        {
            if (administrativeAreal2D_Destination is null || administrativeAreal2D_Source is null)
            {
                return false;
            }

            administrativeAreal2D_Destination.CountryId = administrativeAreal2D_Source.CountryId;
            administrativeAreal2D_Destination.CountyId = administrativeAreal2D_Source.CountyId;
            administrativeAreal2D_Destination.MunicipalityId = administrativeAreal2D_Source.MunicipalityId;
            administrativeAreal2D_Destination.VoivodeshipId = administrativeAreal2D_Source.VoivodeshipId;

            SetId(administrativeAreal2D_Destination, administrativeAreal2D_Source);

            return true;
        }

        /// <summary>
        /// Finds the parent of the destination administrative areal object among the given sources by geometry, and updates its identification properties from that parent.
        /// <para>The sources are expected to be one administrative level, and all of it - the search is by geometry, not by an existing identifier. A source is chosen by containment of a sample point taken from the destination (its bounding box centroid, falling back to an internal point of its polygon); where several sources contain that point, the smallest of them wins as the most specific.</para>
        /// <para>When no source contains the point, the destination is assigned to the source covering the <b>majority of its own area</b> instead. The BDOT10k settlement layer (<c>OT_ADMS_A</c>) and the administrative-division layer (<c>OT_ADJA_A</c>) are digitised independently, so a handful of settlements sit just outside every municipality polygon and their sample point lands in a gap. Requiring a majority - rather than any overlap - leaves a destination whose level genuinely holds no parent unassigned, so the caller can search the next level up instead of filing it under a neighbour it merely shares a border with. Full account: https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/14.</para>
        /// </summary>
        /// <param name="administrativeAreal2D_Destination">The destination AdministrativeAreal2D object to be updated.</param>
        /// <param name="administrativeAreal2Ds_Source">The candidate source AdministrativeAreal2D objects, one administrative level, to be searched for a parent.</param>
        /// <param name="tolerance">The distance tolerance used by the containment checks.</param>
        /// <returns>True if a parent was found and the IDs were updated; otherwise, false.</returns>
        public static bool UpdateIds(this AdministrativeAreal2D? administrativeAreal2D_Destination, IEnumerable<AdministrativeAreal2D>? administrativeAreal2Ds_Source, double tolerance = Core.Constants.Tolerance.MacroDistance)
        {
            if (administrativeAreal2D_Destination is null || administrativeAreal2Ds_Source is null)
            {
                return false;
            }

            AdministrativeAreal2D administrativeAreal2D = administrativeAreal2D_Destination;

            List<AdministrativeAreal2D> administrativeAreal2Ds = administrativeAreal2Ds_Source as List<AdministrativeAreal2D> ?? [.. administrativeAreal2Ds_Source];
            if (administrativeAreal2Ds.Count == 0)
            {
                return false;
            }

            if (administrativeAreal2D.BoundingBox2D is not BoundingBox2D boundingBox2D || boundingBox2D.GetCentroid() is not Point2D point2D)
            {
                return false;
            }

            // ToDiGi deserializes the whole polygon out of JSON and PolygonalFace2D clones on every
            // access, so the destination face is resolved at most once per call and kept here.
            PolygonalFace2D? polygonalFace2D = null;
            bool polygonalFace2D_Resolved = false;

            PolygonalFace2D? PolygonalFace2D_Destination()
            {
                if (!polygonalFace2D_Resolved)
                {
                    polygonalFace2D_Resolved = true;
                    polygonalFace2D = administrativeAreal2D.ToDiGi()?.PolygonalFace2D;
                }

                return polygonalFace2D;
            }

            bool UpdateIds_ByOverlap()
            {
                if (PolygonalFace2D_Destination() is not PolygonalFace2D polygonalFace2D_Destination)
                {
                    return false;
                }

                double area_Destination = polygonalFace2D_Destination.GetArea();
                if (area_Destination <= 0)
                {
                    return false;
                }

                AdministrativeAreal2D? administrativeAreal2D_Match = null;
                double area_Match = 0;

                foreach (AdministrativeAreal2D administrativeAreal2D_Source in administrativeAreal2Ds)
                {
                    if (administrativeAreal2D_Source?.BoundingBox2D is not BoundingBox2D boundingBox2D_Source || !boundingBox2D_Source.InRange(boundingBox2D, tolerance))
                    {
                        continue;
                    }

                    if (administrativeAreal2D_Source.ToDiGi()?.PolygonalFace2D is not PolygonalFace2D polygonalFace2D_Source)
                    {
                        continue;
                    }

                    List<PolygonalFace2D>? polygonalFace2Ds_Intersection = polygonalFace2D_Destination.Intersection(polygonalFace2D_Source);
                    if (polygonalFace2Ds_Intersection is null || polygonalFace2Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    double area = 0;
                    foreach (PolygonalFace2D polygonalFace2D_Intersection in polygonalFace2Ds_Intersection)
                    {
                        area += polygonalFace2D_Intersection.GetArea();
                    }

                    if (area > area_Match)
                    {
                        area_Match = area;
                        administrativeAreal2D_Match = administrativeAreal2D_Source;
                    }
                }

                if (administrativeAreal2D_Match is null || area_Match <= area_Destination / 2)
                {
                    return false;
                }

                return UpdateIds(administrativeAreal2D, administrativeAreal2D_Match);
            }

            List<AdministrativeAreal2D> administrativeAreal2Ds_Filtered = administrativeAreal2Ds.FindAll(x => x?.BoundingBox2D is BoundingBox2D boundingBox2D_Source && boundingBox2D_Source.InRange(point2D, tolerance));
            if (administrativeAreal2Ds_Filtered.Count == 0)
            {
                // A bounding box centroid can sit outside a concave unit; an internal point of the
                // polygon is always inside it.
                if (PolygonalFace2D_Destination()?.GetInternalPoint() is Point2D point2D_Internal)
                {
                    point2D = point2D_Internal;
                }

                administrativeAreal2Ds_Filtered = administrativeAreal2Ds.FindAll(x => x?.BoundingBox2D is BoundingBox2D boundingBox2D_Source && boundingBox2D_Source.InRange(point2D, tolerance));
            }

            if (administrativeAreal2Ds_Filtered.Count == 0)
            {
                return UpdateIds_ByOverlap();
            }

            if (administrativeAreal2Ds_Filtered.Count == 1)
            {
                return UpdateIds(administrativeAreal2D, administrativeAreal2Ds_Filtered[0]);
            }

            // A single source whose bounding box holds the whole destination bounding box settles it
            // without deserializing any polygon.
            if (boundingBox2D.GetPoints() is List<Point2D> point2Ds && point2Ds.Count != 0)
            {
                List<AdministrativeAreal2D> administrativeAreal2Ds_Filtered_Temp = administrativeAreal2Ds_Filtered.FindAll(x => point2Ds.TrueForAll(y => x.BoundingBox2D!.InRange(y, tolerance)));
                if (administrativeAreal2Ds_Filtered_Temp.Count == 1)
                {
                    return UpdateIds(administrativeAreal2D, administrativeAreal2Ds_Filtered_Temp[0]);
                }
            }

            if (PolygonalFace2D_Destination()?.GetInternalPoint() is Point2D point2D_Temp)
            {
                point2D = point2D_Temp;
            }

            AdministrativeAreal2D? administrativeAreal2D_Source_Match = null;
            double area_Source_Match = 0;

            foreach (AdministrativeAreal2D administrativeAreal2D_Source in administrativeAreal2Ds_Filtered)
            {
                if (administrativeAreal2D_Source?.ToDiGi()?.PolygonalFace2D is not PolygonalFace2D polygonalFace2D_Source || !polygonalFace2D_Source.InRange(point2D, tolerance))
                {
                    continue;
                }

                // Where the polygons of several sources hold the point, the smallest of them is the
                // most specific parent.
                double area = polygonalFace2D_Source.GetArea();
                if (administrativeAreal2D_Source_Match is null || area < area_Source_Match)
                {
                    administrativeAreal2D_Source_Match = administrativeAreal2D_Source;
                    area_Source_Match = area;
                }
            }

            if (administrativeAreal2D_Source_Match is null)
            {
                return UpdateIds_ByOverlap();
            }

            return UpdateIds(administrativeAreal2D, administrativeAreal2D_Source_Match);
        }
    }
}
