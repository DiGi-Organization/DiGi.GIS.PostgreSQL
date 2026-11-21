using DiGi.BDL.Enums;
using DiGi.GIS.Classes;
using DiGi.GIS.Interfaces;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Creates the statistical data of a municipality listed under several BDL units of one name - a gmina that changed its type or county - by merging its namesakes' collections into its own (DiGi-Organization/DiGi.GIS.PostgreSQL#149).
        /// <para>BDL splits such a gmina's data between the units by when it was published: Czerwińsk nad Wisłą holds its population of 2008-2019 on the historical unit 071422520042 and of 2020-2025 and the 2021 census on the current 071422520043; Nowe Miasto the census and the population up to 2021 on the historical 071422520082 and only 2022 on on the current 071422520083. No one unit holds it all, in either direction.</para>
        /// <para>A namesake is a distinct gmina, not the same one under another unit, when it holds the 2021 census while the result already does: a town and the rural gmina around it share a name (Płońsk 071422520011 and 071422520092, Bochnia, Gorlice - 143 pairs in one county, 22 more across counties), and each holds its own census. Such a namesake is skipped, so one gmina's numbers never fill another's gaps. A successor or predecessor never does: of two units of one gmina at most one holds the census.</para>
        /// <para>The result starts from <paramref name="statisticalDataCollection"/> - the unit the code names - and takes from <paramref name="statisticalDataCollections_Namesake"/>, in order, every series it lacks, and every year of a yearly series it holds no value for. A value of 0 or NaN counts as missing, since BDL answers 0 for a year it has not published; a year all of them leave at 0 stays 0. Series are matched by name, as a collection keys them. The inputs are not changed.</para>
        /// </summary>
        /// <param name="statisticalDataCollection">The collection of the unit the code names. This value can be null.</param>
        /// <param name="statisticalDataCollections_Namesake">The collections of its namesakes (<c>Query.NamesakeStatisticalUnits</c>), most preferred first. This value can be null.</param>
        /// <returns>The merged collection, keeping the code and identity of <paramref name="statisticalDataCollection"/> (of the first namesake when that is null); null when there is no collection at all.</returns>
        public static StatisticalDataCollection? MergedStatisticalDataCollection(this StatisticalDataCollection? statisticalDataCollection, IEnumerable<StatisticalDataCollection?>? statisticalDataCollections_Namesake)
        {
            List<StatisticalDataCollection> statisticalDataCollections = [];
            if (statisticalDataCollection is not null)
            {
                statisticalDataCollections.Add(statisticalDataCollection);
            }

            foreach (StatisticalDataCollection? statisticalDataCollection_Namesake in statisticalDataCollections_Namesake ?? [])
            {
                if (statisticalDataCollection_Namesake is not null)
                {
                    statisticalDataCollections.Add(statisticalDataCollection_Namesake);
                }
            }

            if (statisticalDataCollections.Count == 0)
            {
                return null;
            }

            StatisticalDataCollection result = new(statisticalDataCollections[0]);
            for (int i = 1; i < statisticalDataCollections.Count; i++)
            {
                if (HoldsCensus(result) && HoldsCensus(statisticalDataCollections[i]))
                {
                    continue;
                }

                foreach (IStatisticalData statisticalData in statisticalDataCollections[i].StatisticalDatas ?? [])
                {
                    if (statisticalData?.Name is not string name)
                    {
                        continue;
                    }

                    IStatisticalData? statisticalData_Result = result.GetStatisticalData(name);
                    if (statisticalData_Result is null)
                    {
                        if (Core.Query.Clone(statisticalData) is IStatisticalData statisticalData_Clone)
                        {
                            result.Add(statisticalData_Clone);
                        }

                        continue;
                    }

                    if (statisticalData_Result is not StatisticalYearlyDoubleData statisticalYearlyDoubleData_Result || statisticalData is not StatisticalYearlyDoubleData statisticalYearlyDoubleData)
                    {
                        continue;
                    }

                    // Every year the result holds a value for, then the years only the namesake holds one for.
                    SortedDictionary<short, double> values = [];
                    foreach (short year in statisticalYearlyDoubleData_Result.Years)
                    {
                        if (statisticalYearlyDoubleData_Result.TryGetValue(year, out double value))
                        {
                            values[year] = value;
                        }
                    }

                    bool changed = false;
                    foreach (short year in statisticalYearlyDoubleData.Years)
                    {
                        if (!statisticalYearlyDoubleData.TryGetValue(year, out double value) || IsMissing(value))
                        {
                            continue;
                        }

                        if (!values.TryGetValue(year, out double value_Result) || IsMissing(value_Result))
                        {
                            values[year] = value;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        result.Remove(name);
                        result.Add(new StatisticalYearlyDoubleData(name, statisticalYearlyDoubleData_Result.Reference, values));
                    }
                }
            }

            return result;

            static bool IsMissing(double value)
            {
                return double.IsNaN(value) || value == 0;
            }

            static bool HoldsCensus(StatisticalDataCollection statisticalDataCollection_Census)
            {
                StatisticalYearlyDoubleData? statisticalYearlyDoubleData = GIS.Query.StatisticalData<StatisticalYearlyDoubleData>(statisticalDataCollection_Census, Variable.inhabited_dwellings_total_census_2021);
                return statisticalYearlyDoubleData is not null && statisticalYearlyDoubleData.TryGetValue(2021, out double count) && !IsMissing(count);
            }
        }
    }
}
