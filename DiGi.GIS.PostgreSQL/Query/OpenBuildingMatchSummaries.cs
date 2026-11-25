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
        /// <para>The in-memory twin of <see cref="OpenBuildingPostgreSQLConverter.GetMatchSummariesAsync(Npgsql.NpgsqlConnection?, string?, DiGi.Geometry.Planar.Classes.BoundingBox2D?, int, System.Threading.CancellationToken)"/>: the same counts and the same linear-interpolated percentiles (<c>percentile_cont</c>), so a run that does not store its links (a sensitivity run) still reports its measures, and a fact can check the SQL against it. The definitions are those of <see cref="OpenBuildingMatchSummaryResult"/>: footprints are counted distinctly by (county id, key), classes by component, geometry measures over the one-to-one rows only, and a per-source row carries no BDOT10k figure.</para>
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

            OpenBuildingMatchSummaryResult Summary(List<OpenBuildingMatch> openBuildingMatches_Group, string? primarySource)
            {
                bool isTotal = primarySource is null;

                HashSet<(int, string)> keys_BDOT = [];
                HashSet<(int, string)> keys_Dataset = [];
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
                            keys_BDOT.Add((countyId_BDOT, reference));
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
                            keys_Dataset.Add((countyId_Source, sourceId));
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

                long count_BDOT = keys_BDOT.Count;
                long count_Dataset = keys_Dataset.Count;

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
