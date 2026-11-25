using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.OverlayNG;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Matches the footprints of one open building dataset to the BDOT10k footprints in memory and returns the link rows of the components a county group owns (DiGi.GIS.PostgreSQL#127). No I/O: the caller reads both sides - the dataset from Storage, BDOT10k from Main - and hands them over.
        /// <para><b>Edges.</b> Candidate pairs come from an STR tree over the BDOT10k envelopes. A pair is joined when its overlap is positive and covers at least <see cref="OpenBuildingMatchParameters.MinOverlapShare"/> of the smaller footprint, or - only when <see cref="OpenBuildingMatchParameters.MaxCentroidDistance"/> is positive - when it does not overlap and the centroids are that close. The overlap is measured with NetTopologySuite's robust overlay on geometries converted once per footprint.</para>
        /// <para><b>Classes.</b> The connected components of the edges: one and one is <see cref="OpenBuildingMatchClass.OneToOne"/> at an intersection over union of at least <see cref="OpenBuildingMatchParameters.IoUThreshold"/> and <see cref="OpenBuildingMatchClass.OneToOneWeak"/> below it; then one-to-many, many-to-one, many-to-many; a lone BDOT10k footprint is an <see cref="OpenBuildingMatchClass.Omission"/> and a lone dataset footprint a <see cref="OpenBuildingMatchClass.Commission"/>. A footprint below <see cref="OpenBuildingMatchParameters.MinArea"/> takes no part and is returned as <see cref="OpenBuildingMatchClass.Excluded"/>; one without a usable geometry (<see cref="OpenBuildingMatchItem.Geometry"/> null) is not returned at all - the caller counts it as rejected.</para>
        /// <para><b>Ownership.</b> A component belongs to the county part of its smallest BDOT10k key (county id, then reference, ordinal), or of its smallest dataset key when it has no BDOT10k member, and only the components owned by <paramref name="countyIds_Owned"/> are returned. A group that reads a margin across its edge therefore never returns a component the neighbouring group returns too, and nothing is counted twice. The owning footprint's centroid is the component's representative point.</para>
        /// <para><b>Areas.</b> Every row carries the area of each of its footprints and the part of it the other side covers - every overlapping footprint counts, linked or not, unioned when there are several - so the area-based completeness and correctness of the ISPRS benchmark (Rottensteiner et al., 2014) are read from the rows alone, as are size classes.</para>
        /// <para><b>Determinism.</b> Both sides are de-duplicated on (county id, key) and ordered ordinally before anything else, components are numbered in that order from one, and a component's rows are ordered by their BDOT10k then dataset member - the output is the same for the same input, under any culture.</para>
        /// </summary>
        /// <param name="openBuildingMatchItems_Dataset">The dataset footprints of the group and its margin.</param>
        /// <param name="openBuildingMatchItems_BDOT">The BDOT10k footprints of the group and its margin.</param>
        /// <param name="openBuildingMatchParameters">The rule parameters.</param>
        /// <param name="countyIds_Owned">The county parts whose components are returned: the group, without the margin's counties.</param>
        /// <returns>The link rows of the owned components, or null when any argument is null.</returns>
        public static List<OpenBuildingMatch>? OpenBuildingMatches(IEnumerable<OpenBuildingMatchItem>? openBuildingMatchItems_Dataset, IEnumerable<OpenBuildingMatchItem>? openBuildingMatchItems_BDOT, OpenBuildingMatchParameters? openBuildingMatchParameters, ISet<int>? countyIds_Owned)
        {
            if (openBuildingMatchItems_Dataset is null || openBuildingMatchItems_BDOT is null || openBuildingMatchParameters is null || countyIds_Owned is null)
            {
                return null;
            }

            static int Compare(OpenBuildingMatchItem x, OpenBuildingMatchItem y)
            {
                int result = x.CountyId.CompareTo(y.CountyId);
                return result != 0 ? result : string.CompareOrdinal(x.Key, y.Key);
            }

            static List<OpenBuildingMatchItem> Sorted(IEnumerable<OpenBuildingMatchItem> openBuildingMatchItems)
            {
                List<OpenBuildingMatchItem> result = [];
                HashSet<(int, string)> keys = [];
                foreach (OpenBuildingMatchItem openBuildingMatchItem in openBuildingMatchItems)
                {
                    // A margin read and a county read can both return a footprint; the first one stays.
                    if (openBuildingMatchItem is not null && keys.Add((openBuildingMatchItem.CountyId, openBuildingMatchItem.Key)))
                    {
                        result.Add(openBuildingMatchItem);
                    }
                }

                result.Sort(Compare);
                return result;
            }

            List<OpenBuildingMatchItem> openBuildingMatchItems_D = Sorted(openBuildingMatchItems_Dataset);
            List<OpenBuildingMatchItem> openBuildingMatchItems_B = Sorted(openBuildingMatchItems_BDOT);

            int count_D = openBuildingMatchItems_D.Count;
            int count_B = openBuildingMatchItems_B.Count;

            double minArea = openBuildingMatchParameters.MinArea;
            double minOverlapShare = openBuildingMatchParameters.MinOverlapShare;
            double maxCentroidDistance = openBuildingMatchParameters.MaxCentroidDistance;
            double ioUThreshold = openBuildingMatchParameters.IoUThreshold;

            // A footprint takes part in the matching when it has a usable geometry and reaches the minimum area.
            bool IsMatched(OpenBuildingMatchItem openBuildingMatchItem)
            {
                return openBuildingMatchItem.Geometry is not null && !(openBuildingMatchItem.Area < minArea);
            }

            // Nodes 0 .. count_B - 1 are the BDOT10k footprints, count_B .. count_B + count_D - 1 the dataset ones.
            // The BDOT10k side comes first so that the first node met of a component is its owner (rule 6).
            int[] parents = new int[count_B + count_D];
            for (int i = 0; i < parents.Length; i++)
            {
                parents[i] = i;
            }

            int Find(int i)
            {
                while (parents[i] != i)
                {
                    parents[i] = parents[parents[i]];
                    i = parents[i];
                }

                return i;
            }

            void Union(int i, int j)
            {
                int root_I = Find(i);
                int root_J = Find(j);
                if (root_I == root_J)
                {
                    return;
                }

                // The smaller node becomes the root, so a root is always the smallest node of its component.
                if (root_I < root_J)
                {
                    parents[root_J] = root_I;
                }
                else
                {
                    parents[root_I] = root_J;
                }
            }

            STRtree<int> sTRtree = new();
            int count_Tree = 0;
            for (int i = 0; i < count_B; i++)
            {
                OpenBuildingMatchItem openBuildingMatchItem = openBuildingMatchItems_B[i];
                if (IsMatched(openBuildingMatchItem))
                {
                    sTRtree.Insert(openBuildingMatchItem.Geometry!.EnvelopeInternal, i);
                    count_Tree++;
                }
            }

            // (index of the BDOT10k footprint, index of the dataset footprint, overlap area)
            List<(int B, int D, double Intersection)> edges = [];

            // Every pair with a positive overlap, linked or not: what each footprint has covering it, for the
            // area-based completeness and correctness (Rottensteiner et al., 2014).
            List<(int B, int D, double Intersection)> overlaps = [];

            if (count_Tree > 0)
            {
                sTRtree.Build();

                for (int d = 0; d < count_D; d++)
                {
                    OpenBuildingMatchItem openBuildingMatchItem_D = openBuildingMatchItems_D[d];
                    if (!IsMatched(openBuildingMatchItem_D))
                    {
                        continue;
                    }

                    NetTopologySuite.Geometries.Geometry geometry_D = openBuildingMatchItem_D.Geometry!;
                    Envelope envelope_D = geometry_D.EnvelopeInternal;

                    Envelope envelope_Search = new(envelope_D);
                    if (maxCentroidDistance > 0)
                    {
                        envelope_Search.ExpandBy(maxCentroidDistance);
                    }

                    List<int> indexes_B = [.. sTRtree.Query(envelope_Search)];
                    indexes_B.Sort();

                    foreach (int b in indexes_B)
                    {
                        OpenBuildingMatchItem openBuildingMatchItem_B = openBuildingMatchItems_B[b];
                        NetTopologySuite.Geometries.Geometry geometry_B = openBuildingMatchItem_B.Geometry!;

                        double intersection = 0;
                        if (envelope_D.Intersects(geometry_B.EnvelopeInternal))
                        {
                            try
                            {
                                intersection = OverlayNGRobust.Overlay(geometry_D, geometry_B, NetTopologySuite.Operation.Overlay.SpatialFunction.Intersection)?.Area ?? 0;
                            }
                            catch (TopologyException)
                            {
                                // Both geometries are valid (repaired on the way in), so the robust overlay failing
                                // is a numerical corner case: the pair is treated as not overlapping, never guessed.
                                intersection = 0;
                            }
                        }

                        if (intersection > 0)
                        {
                            overlaps.Add((b, d, intersection));
                        }

                        bool edge = intersection > 0 && intersection / Math.Min(openBuildingMatchItem_D.Area, openBuildingMatchItem_B.Area) >= minOverlapShare;

                        if (!edge && maxCentroidDistance > 0 && !(intersection > 0))
                        {
                            double dx = openBuildingMatchItem_D.CentroidX - openBuildingMatchItem_B.CentroidX;
                            double dy = openBuildingMatchItem_D.CentroidY - openBuildingMatchItem_B.CentroidY;
                            edge = Math.Sqrt((dx * dx) + (dy * dy)) <= maxCentroidDistance;
                        }

                        if (edge)
                        {
                            edges.Add((b, d, intersection));
                            Union(b, count_B + d);
                        }
                    }
                }
            }

            // The covered area of each footprint: the part the other side covers. One partner is the overlap already
            // measured; several are unioned first, so two dataset footprints overlapping each other on one register
            // building are not counted twice. A footprint without a partner is covered by nothing.
            double[] coveredAreas_B = new double[count_B];
            double[] coveredAreas_D = new double[count_D];

            double CoveredArea(NetTopologySuite.Geometries.Geometry geometry, List<NetTopologySuite.Geometries.Geometry> geometries_Partner, double intersection_Sum)
            {
                try
                {
                    NetTopologySuite.Geometries.Geometry geometry_Union = OverlayNGRobust.Union(geometry.Factory.BuildGeometry(geometries_Partner));
                    return OverlayNGRobust.Overlay(geometry, geometry_Union, NetTopologySuite.Operation.Overlay.SpatialFunction.Intersection)?.Area ?? 0;
                }
                catch (TopologyException)
                {
                    // The sum of the pairwise overlaps bounds the union from above; the footprint's area bounds it too.
                    return Math.Min(intersection_Sum, geometry.Area);
                }
            }

            foreach (IGrouping<int, (int B, int D, double Intersection)> grouping in overlaps.GroupBy(x => x.B))
            {
                List<(int B, int D, double Intersection)> overlaps_B = [.. grouping];
                coveredAreas_B[grouping.Key] = overlaps_B.Count == 1
                    ? overlaps_B[0].Intersection
                    : CoveredArea(openBuildingMatchItems_B[grouping.Key].Geometry!, overlaps_B.ConvertAll(x => openBuildingMatchItems_D[x.D].Geometry!), overlaps_B.Sum(x => x.Intersection));
            }

            foreach (IGrouping<int, (int B, int D, double Intersection)> grouping in overlaps.GroupBy(x => x.D))
            {
                List<(int B, int D, double Intersection)> overlaps_D = [.. grouping];
                coveredAreas_D[grouping.Key] = overlaps_D.Count == 1
                    ? overlaps_D[0].Intersection
                    : CoveredArea(openBuildingMatchItems_D[grouping.Key].Geometry!, overlaps_D.ConvertAll(x => openBuildingMatchItems_B[x.B].Geometry!), overlaps_D.Sum(x => x.Intersection));
            }

            // The members and edges of each component, keyed by its root (its smallest node).
            Dictionary<int, List<int>> nodes_ByRoot = [];
            for (int i = 0; i < parents.Length; i++)
            {
                int root = Find(i);
                if (!nodes_ByRoot.TryGetValue(root, out List<int>? nodes))
                {
                    nodes = [];
                    nodes_ByRoot[root] = nodes;
                }

                nodes.Add(i);
            }

            Dictionary<int, List<(int B, int D, double Intersection)>> edges_ByRoot = [];
            foreach ((int B, int D, double Intersection) edge in edges)
            {
                int root = Find(edge.B);
                if (!edges_ByRoot.TryGetValue(root, out List<(int B, int D, double Intersection)>? edges_Root))
                {
                    edges_Root = [];
                    edges_ByRoot[root] = edges_Root;
                }

                edges_Root.Add(edge);
            }

            OpenBuildingMatchItem Item(int node)
            {
                return node < count_B ? openBuildingMatchItems_B[node] : openBuildingMatchItems_D[node - count_B];
            }

            OpenBuildingMatch Pair(int countyId, long componentId, OpenBuildingMatchClass openBuildingMatchClass, (int B, int D, double Intersection) edge, double pointX, double pointY)
            {
                OpenBuildingMatchItem openBuildingMatchItem_B = openBuildingMatchItems_B[edge.B];
                OpenBuildingMatchItem openBuildingMatchItem_D = openBuildingMatchItems_D[edge.D];

                double union = openBuildingMatchItem_D.Area + openBuildingMatchItem_B.Area - edge.Intersection;
                double dx = openBuildingMatchItem_D.CentroidX - openBuildingMatchItem_B.CentroidX;
                double dy = openBuildingMatchItem_D.CentroidY - openBuildingMatchItem_B.CentroidY;

                return new OpenBuildingMatch(
                    countyId,
                    componentId,
                    openBuildingMatchClass,
                    openBuildingMatchItem_B.CountyId,
                    openBuildingMatchItem_B.Key,
                    openBuildingMatchItem_D.CountyId,
                    openBuildingMatchItem_D.Key,
                    openBuildingMatchItem_D.PrimarySource,
                    union > 0 ? edge.Intersection / union : 0,
                    openBuildingMatchItem_D.Area / openBuildingMatchItem_B.Area,
                    Math.Sqrt((dx * dx) + (dy * dy)),
                    dx,
                    dy,
                    openBuildingMatchItem_B.Area,
                    openBuildingMatchItem_D.Area,
                    coveredAreas_B[edge.B],
                    coveredAreas_D[edge.D],
                    pointX,
                    pointY);
            }

            OpenBuildingMatch Single(int countyId, long componentId, OpenBuildingMatchClass openBuildingMatchClass, int node)
            {
                OpenBuildingMatchItem openBuildingMatchItem = Item(node);
                bool isBDOT = node < count_B;

                // An excluded footprint takes no part in the matching, so nothing is said about what covers it.
                double? coveredArea = openBuildingMatchClass == OpenBuildingMatchClass.Excluded ? null : (isBDOT ? coveredAreas_B[node] : coveredAreas_D[node - count_B]);

                return new OpenBuildingMatch(
                    countyId,
                    componentId,
                    openBuildingMatchClass,
                    isBDOT ? openBuildingMatchItem.CountyId : null,
                    isBDOT ? openBuildingMatchItem.Key : null,
                    isBDOT ? null : openBuildingMatchItem.CountyId,
                    isBDOT ? null : openBuildingMatchItem.Key,
                    isBDOT ? null : openBuildingMatchItem.PrimarySource,
                    null,
                    null,
                    null,
                    null,
                    null,
                    isBDOT ? openBuildingMatchItem.Area : null,
                    isBDOT ? null : openBuildingMatchItem.Area,
                    isBDOT ? coveredArea : null,
                    isBDOT ? null : coveredArea,
                    openBuildingMatchItem.CentroidX,
                    openBuildingMatchItem.CentroidY);
            }

            List<OpenBuildingMatch> result = [];
            long componentId = 0;

            // Walking the nodes in order meets every component first at its root - its smallest node, which is
            // its smallest BDOT10k key when it has one - so the components are numbered in key order.
            for (int node = 0; node < parents.Length; node++)
            {
                if (Find(node) != node)
                {
                    continue;
                }

                OpenBuildingMatchItem openBuildingMatchItem_Owner = Item(node);
                if (openBuildingMatchItem_Owner.Geometry is null || !countyIds_Owned.Contains(openBuildingMatchItem_Owner.CountyId))
                {
                    // A footprint without a usable geometry has no edge and is the caller's rejection; a component
                    // owned by another county part is that part's to return.
                    continue;
                }

                int countyId = openBuildingMatchItem_Owner.CountyId;
                componentId++;

                if (!IsMatched(openBuildingMatchItem_Owner))
                {
                    result.Add(Single(countyId, componentId, OpenBuildingMatchClass.Excluded, node));
                    continue;
                }

                List<int> nodes = nodes_ByRoot[node];
                if (nodes.Count == 1)
                {
                    result.Add(Single(countyId, componentId, node < count_B ? OpenBuildingMatchClass.Omission : OpenBuildingMatchClass.Commission, node));
                    continue;
                }

                int count_NodesB = 0;
                foreach (int node_Member in nodes)
                {
                    if (node_Member < count_B)
                    {
                        count_NodesB++;
                    }
                }

                int count_NodesD = nodes.Count - count_NodesB;

                List<(int B, int D, double Intersection)> edges_Component = edges_ByRoot[node];
                edges_Component.Sort((x, y) => x.B != y.B ? x.B.CompareTo(y.B) : x.D.CompareTo(y.D));

                OpenBuildingMatchClass openBuildingMatchClass;
                if (count_NodesB == 1 && count_NodesD == 1)
                {
                    (int B, int D, double Intersection) edge = edges_Component[0];
                    double union = openBuildingMatchItems_D[edge.D].Area + openBuildingMatchItems_B[edge.B].Area - edge.Intersection;
                    double ioU = union > 0 ? edge.Intersection / union : 0;

                    openBuildingMatchClass = ioU >= ioUThreshold ? OpenBuildingMatchClass.OneToOne : OpenBuildingMatchClass.OneToOneWeak;
                }
                else if (count_NodesD == 1)
                {
                    openBuildingMatchClass = OpenBuildingMatchClass.OneToMany;
                }
                else if (count_NodesB == 1)
                {
                    openBuildingMatchClass = OpenBuildingMatchClass.ManyToOne;
                }
                else
                {
                    openBuildingMatchClass = OpenBuildingMatchClass.ManyToMany;
                }

                foreach ((int B, int D, double Intersection) edge in edges_Component)
                {
                    result.Add(Pair(countyId, componentId, openBuildingMatchClass, edge, openBuildingMatchItem_Owner.CentroidX, openBuildingMatchItem_Owner.CentroidY));
                }
            }

            return result;
        }
    }
}
