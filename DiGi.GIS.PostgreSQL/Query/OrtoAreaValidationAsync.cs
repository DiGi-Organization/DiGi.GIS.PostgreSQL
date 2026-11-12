using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.IO.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Asynchronously compares the interpreted reference sample of one test area with BDOT10k and, optionally, with open building datasets (paper 01, sections 4.4 and 5.2; DiGi.GIS.WebAPI.UI#83).
        /// <para>Labels of one epoch are resolved per object (footprint identity <c>(county_id, reference)</c> or drawn-object identity): the state every non-indeterminate interpreter agrees on stands, and disagreement resolves to indeterminate. A reference object is one resolved as present. A dataset footprint and a labelled object match when either contains the other's internal point; their envelopes are compared first. Footprints are counted for the area when the centre of their envelope lies inside the area's bounding box.</para>
        /// <para>Every table read lives on the Main database (<c>orto_area</c>, <c>orto_area_label</c>, <c>orto_area_object</c>, <c>building_2d</c> and the open building tables), so no join across databases is needed; the matching runs in memory and is sized for 1 km test areas, not for counties.</para>
        /// </summary>
        /// <param name="ortoAreaPostgreSQLConverter">The converter reading the test area.</param>
        /// <param name="ortoAreaLabelPostgreSQLConverter">The converter reading the area's labels.</param>
        /// <param name="ortoAreaObjectPostgreSQLConverter">The converter reading the area's drawn objects.</param>
        /// <param name="building2DPostgreSQLConverter">The converter reading the BDOT10k footprints in the area.</param>
        /// <param name="openBuildingPostgreSQLConverter">The converter reading the open dataset footprints; may be null when no dataset is requested.</param>
        /// <param name="areaKey">The invariant location key of the test area.</param>
        /// <param name="epochYear">The imagery epoch to evaluate, or null for the latest epoch labelled in the area.</param>
        /// <param name="datasetKeys">The open building datasets to compare, or null to compare BDOT10k only.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task whose result is the comparison, or null when a required converter or the area key is null, the area does not exist, or a requested dataset key is unknown or its table does not exist.</returns>
        public static async Task<OrtoAreaValidationResult?> OrtoAreaValidationAsync(
            OrtoAreaPostgreSQLConverter? ortoAreaPostgreSQLConverter,
            OrtoAreaLabelPostgreSQLConverter? ortoAreaLabelPostgreSQLConverter,
            OrtoAreaObjectPostgreSQLConverter? ortoAreaObjectPostgreSQLConverter,
            Building2DPostgreSQLConverter? building2DPostgreSQLConverter,
            OpenBuildingPostgreSQLConverter? openBuildingPostgreSQLConverter,
            string? areaKey,
            short? epochYear,
            IEnumerable<string>? datasetKeys,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default)
        {
            if (ortoAreaPostgreSQLConverter is null || ortoAreaLabelPostgreSQLConverter is null || ortoAreaObjectPostgreSQLConverter is null || building2DPostgreSQLConverter is null || string.IsNullOrWhiteSpace(areaKey))
            {
                return null;
            }

            List<string> datasetKeys_List = datasetKeys is null ? [] : [.. datasetKeys.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal)];
            if (datasetKeys_List.Count != 0 && openBuildingPostgreSQLConverter is null)
            {
                return null;
            }

            OrtoArea? ortoArea = await ortoAreaPostgreSQLConverter.GetOrtoAreaAsync(areaKey, commandTimeout, cancellationToken);
            if (ortoArea?.BoundingBox2D is not BoundingBox2D boundingBox2D_Area)
            {
                return null;
            }

            List<OrtoAreaLabel> ortoAreaLabels = await ortoAreaLabelPostgreSQLConverter.GetOrtoAreaLabelsAsync(areaKey, commandTimeout, cancellationToken) ?? [];

            short? epochYear_Resolved = epochYear ?? (ortoAreaLabels.Count == 0 ? null : ortoAreaLabels.Max(x => x.EpochYear));
            List<OrtoAreaLabel> ortoAreaLabels_Epoch = epochYear_Resolved is short epochYear_Value ? [.. ortoAreaLabels.Where(x => x.EpochYear == epochYear_Value)] : [];

            // State codes of orto_area_label: 0 present, 1 absent, 2 indeterminate.
            const short state_Present = 0;
            const short state_Absent = 1;
            const short state_Indeterminate = 2;

            // Resolve each object's state across interpreters. The key keeps the two identities apart.
            Dictionary<string, short> states_ByKey = [];
            Dictionary<string, OrtoAreaLabel> ortoAreaLabels_ByKey = [];
            long disagreementCount = 0;

            foreach (IGrouping<string, OrtoAreaLabel> grouping in ortoAreaLabels_Epoch.GroupBy(x => x.ObjectId is not null ? $"o:{x.ObjectId}" : $"f:{x.CountyId}:{x.Reference}", StringComparer.Ordinal))
            {
                List<short> states_Decided = [.. grouping.Select(x => x.State).Where(x => x != state_Indeterminate).Distinct()];

                short state = states_Decided.Count == 1 ? states_Decided[0] : state_Indeterminate;
                if (states_Decided.Count > 1)
                {
                    disagreementCount++;
                }

                states_ByKey[grouping.Key] = state;
                ortoAreaLabels_ByKey[grouping.Key] = grouping.First();
            }

            int interpreterCount = ortoAreaLabels_Epoch.Select(x => x.Interpreter).Distinct(StringComparer.OrdinalIgnoreCase).Count();

            long footprintPresentCount = 0, footprintAbsentCount = 0, footprintIndeterminateCount = 0;
            long drawnPresentCount = 0, drawnAbsentCount = 0, drawnIndeterminateCount = 0;

            foreach (KeyValuePair<string, short> keyValuePair in states_ByKey)
            {
                bool drawn = ortoAreaLabels_ByKey[keyValuePair.Key].ObjectId is not null;
                switch (keyValuePair.Value)
                {
                    case state_Present:
                        if (drawn) { drawnPresentCount++; } else { footprintPresentCount++; }
                        break;

                    case state_Absent:
                        if (drawn) { drawnAbsentCount++; } else { footprintAbsentCount++; }
                        break;

                    default:
                        if (drawn) { drawnIndeterminateCount++; } else { footprintIndeterminateCount++; }
                        break;
                }
            }

            List<OrtoAreaDatasetValidationResult>? ortoAreaDatasetValidationResults = null;

            if (datasetKeys_List.Count != 0 && openBuildingPostgreSQLConverter is not null)
            {
                // The labelled shapes: BDOT10k footprints by their identity, drawn objects by their id.
                List<Classes.Building2D> building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByBoundingBox2DAsync(boundingBox2D_Area, commandTimeout: commandTimeout, cancellationToken: cancellationToken) ?? [];
                Dictionary<string, PolygonalFace2D> polygonalFace2Ds_ByKey = [];
                foreach (Classes.Building2D building2D in building2Ds)
                {
                    if (building2D.ToDiGi()?.PolygonalFace2D is PolygonalFace2D polygonalFace2D)
                    {
                        polygonalFace2Ds_ByKey[$"f:{building2D.CountyId}:{building2D.Reference}"] = polygonalFace2D;
                    }
                }

                List<OrtoAreaObject> ortoAreaObjects = await ortoAreaObjectPostgreSQLConverter.GetOrtoAreaObjectsAsync(areaKey, commandTimeout, cancellationToken) ?? [];
                Dictionary<string, Polygon2D> polygon2Ds_ByKey = [];
                foreach (OrtoAreaObject ortoAreaObject in ortoAreaObjects)
                {
                    if (ortoAreaObject.Polygon2D is Polygon2D polygon2D)
                    {
                        polygon2Ds_ByKey[$"o:{ortoAreaObject.ObjectId}"] = polygon2D;
                    }
                }

                // One labelled shape: its state, envelope, internal point and containment test.
                List<(short State, BoundingBox2D BoundingBox2D, Point2D? InternalPoint, Func<Point2D, bool> Inside)> shapes_Labelled = [];
                foreach (KeyValuePair<string, short> keyValuePair in states_ByKey)
                {
                    if (polygonalFace2Ds_ByKey.TryGetValue(keyValuePair.Key, out PolygonalFace2D? polygonalFace2D) && polygonalFace2D.GetBoundingBox() is BoundingBox2D boundingBox2D_Face)
                    {
                        shapes_Labelled.Add((keyValuePair.Value, boundingBox2D_Face, polygonalFace2D.GetInternalPoint(), x => polygonalFace2D.Inside(x)));
                    }
                    else if (polygon2Ds_ByKey.TryGetValue(keyValuePair.Key, out Polygon2D? polygon2D) && polygon2D.GetBoundingBox() is BoundingBox2D boundingBox2D_Polygon)
                    {
                        shapes_Labelled.Add((keyValuePair.Value, boundingBox2D_Polygon, polygon2D.GetInternalPoint(), x => polygon2D.Inside(x)));
                    }
                }

                long referenceCount = footprintPresentCount + drawnPresentCount;

                ortoAreaDatasetValidationResults = [];
                foreach (string datasetKey in datasetKeys_List)
                {
                    List<Footprint>? footprints = await openBuildingPostgreSQLConverter.GetFootprintsByBoundingBox2DAsync(datasetKey, boundingBox2D_Area, commandTimeout, cancellationToken);
                    if (footprints is null)
                    {
                        return null;
                    }

                    // Each footprint as its faces; a multi-outer relation contributes every face.
                    List<List<PolygonalFace2D>> polygonalFace2Ds_ByFootprint = [];
                    foreach (Footprint footprint in footprints)
                    {
                        List<PolygonalFace2D> polygonalFace2Ds_Footprint = footprint.Geometry switch
                        {
                            PolygonalFace2D polygonalFace2D_Footprint => [polygonalFace2D_Footprint],
                            GeometryCollection2D geometryCollection2D => [.. geometryCollection2D.OfType<PolygonalFace2D>()],
                            _ => [],
                        };

                        if (polygonalFace2Ds_Footprint.Count != 0)
                        {
                            polygonalFace2Ds_ByFootprint.Add(polygonalFace2Ds_Footprint);
                        }
                    }

                    HashSet<int> indexes_Detected = [];
                    long footprintCount = 0, matchedPresentFootprintCount = 0, matchedAbsentFootprintCount = 0, matchedIndeterminateFootprintCount = 0, unmatchedFootprintCount = 0;

                    foreach (List<PolygonalFace2D> polygonalFace2Ds_Footprint in polygonalFace2Ds_ByFootprint)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        List<BoundingBox2D> boundingBox2Ds_Footprint = [.. polygonalFace2Ds_Footprint.Select(x => x.GetBoundingBox()).OfType<BoundingBox2D>()];
                        if (boundingBox2Ds_Footprint.Count == 0)
                        {
                            continue;
                        }

                        BoundingBox2D boundingBox2D_Footprint = new(boundingBox2Ds_Footprint);
                        bool inArea = boundingBox2D_Area.InRange(boundingBox2D_Footprint.GetCentroid());

                        bool matchedPresent = false, matchedAbsent = false, matchedIndeterminate = false;

                        for (int i = 0; i < shapes_Labelled.Count; i++)
                        {
                            (short state, BoundingBox2D boundingBox2D_Labelled, Point2D? internalPoint_Labelled, Func<Point2D, bool> inside_Labelled) = shapes_Labelled[i];
                            if (!boundingBox2D_Labelled.InRange(boundingBox2D_Footprint))
                            {
                                continue;
                            }

                            bool matched = polygonalFace2Ds_Footprint.Exists(x => (internalPoint_Labelled is not null && x.Inside(internalPoint_Labelled)) || (x.GetInternalPoint() is Point2D internalPoint_Footprint && inside_Labelled(internalPoint_Footprint)));
                            if (!matched)
                            {
                                continue;
                            }

                            switch (state)
                            {
                                case state_Present:
                                    matchedPresent = true;
                                    indexes_Detected.Add(i);
                                    break;

                                case state_Absent:
                                    matchedAbsent = true;
                                    break;

                                default:
                                    matchedIndeterminate = true;
                                    break;
                            }
                        }

                        if (!inArea)
                        {
                            continue;
                        }

                        footprintCount++;
                        if (matchedPresent)
                        {
                            matchedPresentFootprintCount++;
                        }
                        else if (matchedAbsent)
                        {
                            matchedAbsentFootprintCount++;
                        }
                        else if (matchedIndeterminate)
                        {
                            matchedIndeterminateFootprintCount++;
                        }
                        else
                        {
                            unmatchedFootprintCount++;
                        }
                    }

                    ortoAreaDatasetValidationResults.Add(new OrtoAreaDatasetValidationResult(
                        datasetKey,
                        referenceCount,
                        indexes_Detected.Count,
                        footprintCount,
                        matchedPresentFootprintCount,
                        matchedAbsentFootprintCount,
                        matchedIndeterminateFootprintCount,
                        unmatchedFootprintCount));
                }
            }

            return new OrtoAreaValidationResult(
                areaKey,
                ortoArea.Stratum,
                epochYear_Resolved,
                interpreterCount,
                footprintPresentCount,
                footprintAbsentCount,
                footprintIndeterminateCount,
                drawnPresentCount,
                drawnAbsentCount,
                drawnIndeterminateCount,
                disagreementCount,
                ortoAreaDatasetValidationResults);
        }
    }
}
