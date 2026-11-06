using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Planar.Interfaces;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Decides which county part one reprojected open building footprint is filed under, and whether a footprint that files nowhere is out of scope or a rejection (DiGi.GIS.PostgreSQL#125).
        /// <para>The decision runs entirely in memory against the county rows loaded once per run - a query per footprint would put a database round trip on every row of a multi-million-row import (Coding - PostgreSQL.md section 3). The bounding-box pass reproduces the candidate prefilter of <c>GetAdministrativeAreal2DsByBoundingBox2DAsync</c> - the same stored envelope columns grown by the same tolerance - and the containment and overlap decision then reuses <see cref="CountyId(IDictionary{int, IPolygonal2D}, IPolygonal2D, double)"/>.</para>
        /// <para>The three outcomes are told apart by the second return channel, because null alone cannot separate them:</para>
        /// <para>- <b>Filed</b>: the result is a county part identifier, already inside the scope.</para>
        /// <para>- <b>Filtered</b> (<c>rejected</c> false): the footprint falls outside the scope bounding box, or resolves to a county part outside the scope list. Ordinary for a run scoped to pilot areas over a nationwide file; not counted as a rejection, because nothing was lost - the row was never in scope.</para>
        /// <para>- <b>Rejected</b> (<c>rejected</c> true): no county envelope reaches the footprint, or no county polygon intersects it within tolerance - a footprint outside the digitised territory, such as one carried by the Geofabrik border buffer. Counted as a rejection and never written.</para>
        /// <para>A polygonal footprint files through containment and, where several parts contain it, through <see cref="CountyId(IDictionary{int, IPolygonal2D}, IPolygonal2D, double)"/> (maximal overlap, then the smallest covering part, then the lowest identifier). A centroid files through the lowest-numbered face containing it, holes included, as <see cref="IdsByPoint2Ds(IDictionary{int, PolygonalFace2D}, IReadOnlyList{Point2D}, double)"/> does; a centroid that no face contains within tolerance files nowhere rather than being widened to a nearest part, because a centroid carries no extent to compare distances with.</para>
        /// </summary>
        /// <param name="boundingBox2D">The envelope of the reprojected footprint, in EPSG:2180 (easting in X).</param>
        /// <param name="polygonal2D">The exterior ring a polygonal footprint files by, in EPSG:2180; null for a centroid.</param>
        /// <param name="point2D">The centroid a point footprint files by, in EPSG:2180; null for a polygon.</param>
        /// <param name="polygonal2Ds_ByCountyId">The exterior ring of every county part loaded for the run, keyed by part identifier.</param>
        /// <param name="boundingBox2Ds_ByCountyId">The stored envelope of every county part loaded for the run, keyed by part identifier - the same identifiers as the rings.</param>
        /// <param name="polygonalFace2Ds_ByCountyId">The face of every county part loaded for the run, holes included; required for centroid filing and ignored for polygon filing. Null for a footprint dataset.</param>
        /// <param name="countyIds_Scope">The identifiers of the county parts this run may write to; null when every loaded county part is in scope.</param>
        /// <param name="boundingBox2D_Scope">The scope bounding box, in EPSG:2180; null for a run scoped by county list.</param>
        /// <param name="tolerance">The distance a footprint may lie outside a part and still count as intersecting it, and the gap the envelope prefilter grows by.</param>
        /// <param name="rejected">Set true when the footprint files nowhere and the reason is a rejection rather than a scope filter.</param>
        /// <returns>The identifier of the county part to file under, or null when the footprint is filtered or rejected.</returns>
        public static int? OpenBuildingCountyId(
            BoundingBox2D? boundingBox2D,
            IPolygonal2D? polygonal2D,
            Point2D? point2D,
            IDictionary<int, IPolygonal2D>? polygonal2Ds_ByCountyId,
            IDictionary<int, BoundingBox2D>? boundingBox2Ds_ByCountyId,
            IDictionary<int, PolygonalFace2D>? polygonalFace2Ds_ByCountyId,
            ISet<int>? countyIds_Scope,
            BoundingBox2D? boundingBox2D_Scope,
            double tolerance,
            out bool rejected)
        {
            rejected = false;

            if (boundingBox2D is null)
            {
                // Nothing can be placed: the caller already counts a null reprojection separately, so this
                // stays a rejection rather than a silent drop.
                rejected = true;
                return null;
            }

            // Scope first: a footprint outside the pilot box was never in scope, and saying so before the
            // border test keeps a nationwide file scoped to one box from counting its whole residue as rejections.
            if (boundingBox2D_Scope is not null && !boundingBox2D_Scope.InRange(boundingBox2D, tolerance))
            {
                return null;
            }

            if (boundingBox2Ds_ByCountyId is null || boundingBox2Ds_ByCountyId.Count == 0)
            {
                rejected = true;
                return null;
            }

            // The envelope prefilter: the counties whose stored box reaches the footprint's box. No envelope
            // reaches it means the footprint lies outside every loaded county - the Geofabrik buffer case.
            List<int> candidates = [];
            foreach (KeyValuePair<int, BoundingBox2D> pair in boundingBox2Ds_ByCountyId)
            {
                if (pair.Value is not null && pair.Value.InRange(boundingBox2D, tolerance))
                {
                    candidates.Add(pair.Key);
                }
            }

            if (candidates.Count == 0)
            {
                rejected = true;
                return null;
            }

            candidates.Sort();

            int? countyId;

            if (point2D is not null)
            {
                // Centroid filing: the lowest-numbered face containing the point within tolerance, holes
                // included. Candidates are already few, so the linear walk over them costs nothing a grid would.
                if (polygonalFace2Ds_ByCountyId is null)
                {
                    rejected = true;
                    return null;
                }

                countyId = null;
                foreach (int candidate in candidates)
                {
                    if (polygonalFace2Ds_ByCountyId.TryGetValue(candidate, out PolygonalFace2D? polygonalFace2D) && polygonalFace2D is not null && polygonalFace2D.InRange(point2D, tolerance))
                    {
                        countyId = candidate;
                        break;
                    }
                }

                if (countyId is null)
                {
                    // Nothing contains the point, not even within tolerance: outside the digitised territory.
                    rejected = true;
                    return null;
                }
            }
            else
            {
                if (polygonal2D is null || polygonal2Ds_ByCountyId is null)
                {
                    rejected = true;
                    return null;
                }

                // Polygon filing: the parts the footprint actually intersects. An empty intersection set is
                // the rejection case - Query.CountyId would answer it with the nearest part, and a nearest
                // part tens of kilometres away is precisely the border-buffer row that must never be written.
                List<int> intersecting = [];
                Dictionary<int, IPolygonal2D>? polygonal2Ds_Intersecting = null;

                foreach (int candidate in candidates)
                {
                    if (!polygonal2Ds_ByCountyId.TryGetValue(candidate, out IPolygonal2D? polygonal2D_County) || polygonal2D_County is null)
                    {
                        continue;
                    }

                    if (!polygonal2D_County.InRange(polygonal2D, tolerance))
                    {
                        continue;
                    }

                    intersecting.Add(candidate);

                    if (intersecting.Count > 1)
                    {
                        // Built only for the rare multi-part case; a single intersection is already the answer.
                        // The first intersection seeds it on the step that discovers a second one.
                        polygonal2Ds_Intersecting ??= [];

                        if (!polygonal2Ds_Intersecting.ContainsKey(intersecting[0]) && polygonal2Ds_ByCountyId.TryGetValue(intersecting[0], out IPolygonal2D? polygonal2D_First) && polygonal2D_First is not null)
                        {
                            polygonal2Ds_Intersecting[intersecting[0]] = polygonal2D_First;
                        }

                        polygonal2Ds_Intersecting[candidate] = polygonal2D_County;
                    }
                }

                if (intersecting.Count == 0)
                {
                    rejected = true;
                    return null;
                }

                if (intersecting.Count == 1)
                {
                    countyId = intersecting[0];
                }
                else
                {
                    // Several parts reach the footprint: containment cannot separate them, so the overlap and
                    // smallest-covering rules of CountyId decide, breaking every remaining tie by identifier.
                    countyId = polygonal2Ds_Intersecting.CountyId(polygonal2D, tolerance) ?? intersecting[0];
                }
            }

            if (countyIds_Scope is not null && !countyIds_Scope.Contains(countyId.Value))
            {
                // In the country, outside this run's scope: the row belongs to a later run, not to a rejection list.
                return null;
            }

            return countyId;
        }
    }
}
