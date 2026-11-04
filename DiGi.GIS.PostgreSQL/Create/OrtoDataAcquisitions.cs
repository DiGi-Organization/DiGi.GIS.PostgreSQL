using DiGi.GIS.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Builds the <see cref="Constants.TableName.OrtoDataAcquisition"/> rows of one stored series: one per image, keyed by its requested year.
        /// <para>Images without bytes are skipped. Two images of one year cannot both be a row, so the later one in the series wins, as it would on a re-fetch.</para>
        /// </summary>
        /// <param name="ortoDatas">The series, already re-dated (<see cref="GIS.Create.OrtoDatas(GIS.Classes.OrtoDatas?, IEnumerable{OrtoSheet?}?, int)"/>).</param>
        /// <param name="countyId">The county part the series is filed under.</param>
        /// <returns>The rows ordered by year, or null when the series or its reference is missing.</returns>
        public static List<OrtoDataAcquisition>? OrtoDataAcquisitions(this GIS.Classes.OrtoDatas? ortoDatas, int countyId)
        {
            if (ortoDatas is null || string.IsNullOrWhiteSpace(ortoDatas.Reference))
            {
                return null;
            }

            SortedDictionary<int, OrtoDataAcquisition> sortedDictionary = [];
            foreach (OrtoData ortoData in ortoDatas)
            {
                if (ortoData is null || ortoData.Bytes is null)
                {
                    continue;
                }

                int year = ortoData.DateTime.Year;
                sortedDictionary[year] = new OrtoDataAcquisition(countyId, ortoDatas.Reference, year, ortoData.AcquisitionDate, ortoData.AcquisitionDatePrecision, ortoData.SheetId, ortoData.ContentHash);
            }

            return [.. sortedDictionary.Values];
        }
    }
}
