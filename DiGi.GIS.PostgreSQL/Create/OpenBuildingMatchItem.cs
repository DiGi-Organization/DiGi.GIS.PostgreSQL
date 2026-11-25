using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Builds the matching's view of one footprint (DiGi.GIS.PostgreSQL#127): converts its EPSG:2180 geometry to NetTopologySuite once, repairs it when it is invalid, and derives its area and centroid.
        /// <para>A <see cref="PolygonalFace2D"/> becomes a polygon with its holes; a <see cref="GeometryCollection2D"/> of faces (an OSM relation with several outer rings, a DBSM or Microsoft multipolygon) becomes one multipolygon, so the footprint is matched as the whole building the source describes. Converting here, once per footprint and run, is what keeps the overlay off the cost the open building import measured: a DiGi.Geometry overlay converts both shapes to NetTopologySuite on every call (DiGi.GIS.PostgreSQL#170).</para>
        /// <para>An invalid geometry (a self-intersecting or self-touching ring, common in OSM traces) is repaired with <see cref="GeometryFixer"/> and flagged <see cref="Classes.OpenBuildingMatchItem.Repaired"/>, because an overlay of an invalid polygon can throw or measure nothing. A geometry that is missing, of another kind (a centroid), empty, of zero area, or whose repair is not polygonal leaves the item without a geometry: the run counts it as rejected and never as an omission or a commission.</para>
        /// </summary>
        /// <param name="countyId">The identifier of the county part the footprint is filed under.</param>
        /// <param name="key">The key of the footprint within its county part: the BDOT10k <c>reference</c> or the dataset's <c>source_id</c>.</param>
        /// <param name="datasetId">The <c>building_dataset</c> register row of a dataset footprint, or null for a BDOT10k footprint.</param>
        /// <param name="primarySource">The normalised primary source of a dataset footprint, or null for a BDOT10k footprint.</param>
        /// <param name="geometry2D">The stored footprint geometry in EPSG:2180.</param>
        /// <returns>The item, with a null geometry when the footprint cannot be matched, or null when the key is missing - a footprint without a key cannot be written to the link table.</returns>
        public static Classes.OpenBuildingMatchItem? OpenBuildingMatchItem(int countyId, string? key, int? datasetId, string? primarySource, IGeometry2D? geometry2D)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            NetTopologySuite.Geometries.Geometry? geometry = null;

            switch (geometry2D)
            {
                case PolygonalFace2D polygonalFace2D:
                    geometry = polygonalFace2D.ToNTS();
                    break;

                case GeometryCollection2D geometryCollection2D:
                    List<Polygon> polygons = [];
                    foreach (ICollectable2D collectable2D in geometryCollection2D)
                    {
                        if (collectable2D is not PolygonalFace2D polygonalFace2D_Child || polygonalFace2D_Child.ToNTS() is not Polygon polygon)
                        {
                            polygons.Clear();
                            break;
                        }

                        polygons.Add(polygon);
                    }

                    if (polygons.Count == 1)
                    {
                        geometry = polygons[0];
                    }
                    else if (polygons.Count > 1)
                    {
                        geometry = polygons[0].Factory.CreateMultiPolygon([.. polygons]);
                    }

                    break;
            }

            bool repaired = false;

            if (geometry is not null && !geometry.IsEmpty && !geometry.IsValid)
            {
                geometry = GeometryFixer.Fix(geometry);
                repaired = true;
            }

            if (geometry is null || geometry.IsEmpty || geometry is not (Polygon or MultiPolygon) || !(geometry.Area > 0))
            {
                return new Classes.OpenBuildingMatchItem(countyId, key!, datasetId, primarySource, null, 0, double.NaN, double.NaN, repaired);
            }

            Point centroid = geometry.Centroid;

            return new Classes.OpenBuildingMatchItem(countyId, key!, datasetId, primarySource, geometry, geometry.Area, centroid.X, centroid.Y, repaired);
        }
    }
}
