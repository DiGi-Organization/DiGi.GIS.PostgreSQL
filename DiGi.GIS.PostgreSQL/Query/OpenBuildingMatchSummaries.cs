using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Aggregates link rows into the completeness and geometry measures of one dataset (DiGi.GIS.PostgreSQL#127): one total row, then one row per normalised primary source in ordinal order.
        /// <para>The in-memory twin of <see cref="OpenBuildingPostgreSQLConverter.GetMatchSummariesAsync(Npgsql.NpgsqlConnection?, string?, DiGi.Geometry.Planar.Classes.BoundingBox2D?, int, System.Threading.CancellationToken)"/>: the same counts, the same linear-interpolated percentiles (<c>percentile_cont</c>), the same completeness, correctness and quality of the ISPRS benchmark at object and area level, the same root mean square offsets and the same size classes, so a run that does not store its links (a sensitivity run) still reports its measures, and a fact can check the SQL against it. The definitions are those of <see cref="OpenBuildingMatchSummaryResult"/>: footprints are counted distinctly by (county id, key), classes by component, geometry measures over the one-to-one rows only, and a per-source row carries no BDOT10k figure.</para>
        /// <para>Areas are read per footprint, once, from the rows it appears in; excluded footprints take no part. When any footprint lacks a stored area - a link row written before the areas were stored - the area-based measures and the size classes are null rather than computed over part of the scope.</para>
        /// </summary>
        /// <param name="openBuildingMatches">The link rows of the dataset over the scope.</param>
        /// <param name="datasetKey">The dataset key the rows belong to.</param>
        /// <param name="datasetId">The <c>building_dataset</c> register row the rows belong to.</param>
        /// <param name="openBuildingMatchParameters">The rule parameters the rows were computed with.</param>
        /// <param name="computedAt">When the rows were computed, or null when not known.</param>
        /// <returns>The total row followed by one row per primary source, or null when the rows are null.</returns>
        public static List<OpenBuildingMatchSummaryResult>? OpenBuildingMatchSummaries(IEnumerable<OpenBuildingMatch>? openBuildingMatches, string? datasetKey, int datasetId, OpenBuildingMatchParameters? openBuildingMatchParameters, DateTimeOffset? computedAt)
        {
            if (openBuildingMatches is null)
            {
                return null;
            }

            List<OpenBuildingMatch> openBuildingMatches_All = [.. openBuildingMatches.Where(x => x is not null)];
            IReadOnlyList<double> bounds = Constants.OpenBuildingMatchSizeClass.Bounds;

            static double? Percentile(List<double> values, double fraction)
            {
                if (values.Count == 0)
                {
                    return null;
                }

                double position = fraction * (values.Count - 1);
                int index_Lower = (int)Math.Floor(position);
                int index_Upper = (int)Math.Ceiling(position);

                return values[index_Lower] + ((position - index_Lower) * (values[index_Upper] - values[index_Lower]));
            }

            static List<double> Values(List<OpenBuildingMatch> openBuildingMatches_OneToOne, Func<OpenBuildingMatch, double?> func)
            {
                List<double> result = [];
                foreach (OpenBuildingMatch openBuildingMatch in openBuildingMatches_OneToOne)
                {
                    if (func(openBuildingMatch) is double value)
                    {
                        result.Add(value);
                    }
                }

                result.Sort();
                return result;
            }

            static double? Rms(List<double> values)
            {
                return values.Count == 0 ? null : Math.Sqrt(values.Average(x => x * x));
            }

            static long Components(List<OpenBuildingMatch> openBuildingMatches_Group, OpenBuildingMatchClass openBuildingMatchClass)
            {
                HashSet<(int, long)> components = [];
                foreach (OpenBuildingMatch openBuildingMatch in openBuildingMatches_Group)
                {
                    if (openBuildingMatch.OpenBuildingMatchClass == openBuildingMatchClass)
                    {
                        components.Add((openBuildingMatch.CountyId, openBuildingMatch.ComponentId));
                    }
                }

                return components.Count;
            }

            int SizeClass(double area)
            {
                for (int i = 0; i < bounds.Count; i++)
                {
                    if (area < bounds[i])
                    {
                        return i;
                    }
                }

                return bounds.Count;
            }

            OpenBuildingMatchSummaryResult Summary(List<OpenBuildingMatch> openBuildingMatches_Group, string? primarySource)
            {
                bool isTotal = primarySource is null;

                // One entry per footprint (excluded ones apart): its area, the part the other side covers, and whether
                // it is an omission or a commission. A footprint repeats on every row of its component.
                Dictionary<(int, string), (double? Area, double? CoveredArea, bool Unmatched)> footprints_BDOT = [];
                Dictionary<(int, string), (double? Area, double? CoveredArea, bool Unmatched)> footprints_Dataset = [];
                long count_BDOTExcluded = 0;
                long count_DatasetExcluded = 0;
                long count_Omission = 0;
                long count_Commission = 0;
                List<OpenBuildingMatch> openBuildingMatches_OneToOne = [];

                foreach (OpenBuildingMatch openBuildingMatch in openBuildingMatches_Group)
                {
                    bool isExcluded = openBuildingMatch.OpenBuildingMatchClass == OpenBuildingMatchClass.Excluded;

                    if (openBuildingMatch.BDOTCountyId is int countyId_BDOT && openBuildingMatch.BDOTReference is string reference)
                    {
                        if (isExcluded)
                        {
                            count_BDOTExcluded++;
                        }
                        else
                        {
                            footprints_BDOT.TryAdd((countyId_BDOT, reference), (openBuildingMatch.BDOTArea, openBuildingMatch.BDOTCoveredArea, openBuildingMatch.OpenBuildingMatchClass == OpenBuildingMatchClass.Omission));
                        }
                    }

                    if (openBuildingMatch.SourceCountyId is int countyId_Source && openBuildingMatch.SourceId is string sourceId)
                    {
                        if (isExcluded)
                        {
                            count_DatasetExcluded++;
                        }
                        else
                        {
                            footprints_Dataset.TryAdd((countyId_Source, sourceId), (openBuildingMatch.SourceArea, openBuildingMatch.SourceCoveredArea, openBuildingMatch.OpenBuildingMatchClass == OpenBuildingMatchClass.Commission));
                        }
                    }

                    switch (openBuildingMatch.OpenBuildingMatchClass)
                    {
                        case OpenBuildingMatchClass.Omission:
                            count_Omission++;
                            break;

                        case OpenBuildingMatchClass.Commission:
                            count_Commission++;
                            break;

                        case OpenBuildingMatchClass.OneToOne:
                            openBuildingMatches_OneToOne.Add(openBuildingMatch);
                            break;
                    }
                }

                List<double> ioUs = Values(openBuildingMatches_OneToOne, x => x.IoU);
                List<double> areaRatios = Values(openBuildingMatches_OneToOne, x => x.AreaRatio);
                List<double> centroidDistances = Values(openBuildingMatches_OneToOne, x => x.CentroidDistance);
                List<double> dxs = Values(openBuildingMatches_OneToOne, x => x.Dx);
                List<double> dys = Values(openBuildingMatches_OneToOne, x => x.Dy);

                long count_BDOT = footprints_BDOT.Count;
                long count_Dataset = footprints_Dataset.Count;

                double? completeness = isTotal && count_BDOT > 0 ? (double)(count_BDOT - count_Omission) / count_BDOT : null;
                double? correctness = count_Dataset > 0 ? (double)(count_Dataset - count_Commission) / count_Dataset : null;

                double? quality = null;
                if (isTotal && completeness is double c && correctness is double r && (c + r - (c * r)) > 0)
                {
                    quality = c * r / (c + r - (c * r));
                }

                // Area-based completeness and correctness: unknown, not partial, when any footprint lacks its areas.
                static double? AreaShare(IEnumerable<(double? Area, double? CoveredArea, bool Unmatched)> footprints)
                {
                    double area = 0;
                    double area_Covered = 0;
                    foreach ((double? Area, double? CoveredArea, bool Unmatched) footprint in footprints)
                    {
                        if (footprint.Area is not double area_Footprint || footprint.CoveredArea is not double area_CoveredFootprint)
                        {
                            return null;
                        }

                        area += area_Footprint;
                        area_Covered += area_CoveredFootprint;
                    }

                    return area > 0 ? area_Covered / area : null;
                }

                double? areaCompleteness = isTotal ? AreaShare(footprints_BDOT.Values) : null;
                double? areaCorrectness = AreaShare(footprints_Dataset.Values);

                List<OpenBuildingMatchSizeClassResult>? sizeClasses = null;
                // A per-source row reports no BDOT10k figure, so only the sides it reports decide whether the classes are known.
                if ((!isTotal || footprints_BDOT.Values.All(x => x.Area is not null)) && footprints_Dataset.Values.All(x => x.Area is not null))
                {
                    long[] counts_BDOT = new long[bounds.Count + 1];
                    long[] counts_Omission = new long[bounds.Count + 1];
                    long[] counts_Dataset = new long[bounds.Count + 1];
                    long[] counts_Commission = new long[bounds.Count + 1];

                    foreach ((double? Area, double? CoveredArea, bool Unmatched) footprint in footprints_BDOT.Values)
                    {
                        int i = SizeClass(footprint.Area!.Value);
                        counts_BDOT[i]++;
                        if (footprint.Unmatched)
                        {
                            counts_Omission[i]++;
                        }
                    }

                    foreach ((double? Area, double? CoveredArea, bool Unmatched) footprint in footprints_Dataset.Values)
                    {
                        int i = SizeClass(footprint.Area!.Value);
                        counts_Dataset[i]++;
                        if (footprint.Unmatched)
                        {
                            counts_Commission[i]++;
                        }
                    }

                    sizeClasses = [];
                    for (int i = 0; i <= bounds.Count; i++)
                    {
                        sizeClasses.Add(new OpenBuildingMatchSizeClassResult(
                            i == 0 ? 0 : bounds[i - 1],
                            i < bounds.Count ? bounds[i] : null,
                            isTotal ? counts_BDOT[i] : null,
                            isTotal ? counts_Omission[i] : null,
                            counts_Dataset[i],
                            counts_Commission[i]));
                    }
                }

                List<double> positions = [];
                foreach (OpenBuildingMatch openBuildingMatch in openBuildingMatches_OneToOne)
                {
                    if (openBuildingMatch.Dx is double dx && openBuildingMatch.Dy is double dy)
                    {
                        positions.Add(Math.Sqrt((dx * dx) + (dy * dy)));
                    }
                }

                return new OpenBuildingMatchSummaryResult(
                    datasetKey,
                    datasetId,
                    primarySource,
                    isTotal ? count_BDOT : null,
                    count_Dataset,
                    isTotal ? count_BDOTExcluded : null,
                    count_DatasetExcluded,
                    Components(openBuildingMatches_Group, OpenBuildingMatchClass.OneToOne),
                    Components(openBuildingMatches_Group, OpenBuildingMatchClass.OneToOneWeak),
                    Components(openBuildingMatches_Group, OpenBuildingMatchClass.OneToMany),
                    Components(openBuildingMatches_Group, OpenBuildingMatchClass.ManyToOne),
                    Components(openBuildingMatches_Group, OpenBuildingMatchClass.ManyToMany),
                    isTotal ? count_Omission : null,
                    count_Commission,
                    isTotal && count_BDOT > 0 ? (double)count_Omission / count_BDOT : null,
                    count_Dataset > 0 ? (double)count_Commission / count_Dataset : null,
                    ioUs.Count == 0 ? null : ioUs.Average(),
                    Percentile(ioUs, 0.5),
                    Percentile(ioUs, 0.1),
                    Percentile(areaRatios, 0.5),
                    Percentile(centroidDistances, 0.5),
                    dxs.Count == 0 ? null : dxs.Average(),
                    Percentile(dxs, 0.5),
                    dys.Count == 0 ? null : dys.Average(),
                    Percentile(dys, 0.5),
                    Rms(dxs),
                    Rms(dys),
                    Rms(positions),
                    completeness,
                    correctness,
                    quality,
                    areaCompleteness,
                    areaCorrectness,
                    sizeClasses,
                    openBuildingMatchParameters,
                    computedAt);
            }

            List<OpenBuildingMatchSummaryResult> result = [Summary(openBuildingMatches_All, null)];

            SortedDictionary<string, List<OpenBuildingMatch>> openBuildingMatches_ByPrimarySource = new(StringComparer.Ordinal);
            foreach (OpenBuildingMatch openBuildingMatch in openBuildingMatches_All)
            {
                if (openBuildingMatch.PrimarySource is not string primarySource)
                {
                    continue;
                }

                if (!openBuildingMatches_ByPrimarySource.TryGetValue(primarySource, out List<OpenBuildingMatch>? openBuildingMatches_PrimarySource))
                {
                    openBuildingMatches_PrimarySource = [];
                    openBuildingMatches_ByPrimarySource[primarySource] = openBuildingMatches_PrimarySource;
                }

                openBuildingMatches_PrimarySource.Add(openBuildingMatch);
            }

            foreach (KeyValuePair<string, List<OpenBuildingMatch>> pair in openBuildingMatches_ByPrimarySource)
            {
                result.Add(Summary(pair.Value, pair.Key));
            }

            return result;
        }
    }
}
