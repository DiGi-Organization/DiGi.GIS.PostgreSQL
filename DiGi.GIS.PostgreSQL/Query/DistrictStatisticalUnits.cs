using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Returns the district units of a municipality's county: the BDL units of type <see cref="MunicipalityType.Warsaw_district"/> whose code shares the municipality's first nine digits (DiGi-Organization/DiGi.GIS.PostgreSQL#148).
        /// <para>BDL publishes Warsaw both as the city (<c>071412865011</c>, "M.st.Warszawa od 2002") and as its 18 districts (<c>0714128650x8</c>, "Bemowo - dzielnica", ...), all under the county <c>071412865000</c>. For any other municipality the list is empty.</para>
        /// </summary>
        /// <param name="rootStatisticalUnit">The root statistical unit hierarchy.</param>
        /// <param name="statisticalUnit_Municipality">The municipality's unit.</param>
        /// <returns>The district units, in hierarchy order; empty when there are none or either argument is null.</returns>
        public static List<StatisticalUnit> DistrictStatisticalUnits(this StatisticalUnit? rootStatisticalUnit, StatisticalUnit? statisticalUnit_Municipality)
        {
            List<StatisticalUnit> result = [];

            string? code_Municipality = statisticalUnit_Municipality?.UnitCode?.Code;
            if (rootStatisticalUnit is null || code_Municipality is null || code_Municipality.Length != 12)
            {
                return result;
            }

            IEnumerable<StatisticalUnit>? statisticalUnits = rootStatisticalUnit.GetStatisticalUnits(includeNested: true, x => x?.MunicipalityType() == MunicipalityType.Warsaw_district);
            if (statisticalUnits is null)
            {
                return result;
            }

            foreach (StatisticalUnit statisticalUnit in statisticalUnits)
            {
                string? code = statisticalUnit?.UnitCode?.Code;
                if (statisticalUnit is not null && code is not null && code.Length == 12 && string.Compare(code, 0, code_Municipality, 0, 9, StringComparison.Ordinal) == 0)
                {
                    result.Add(statisticalUnit);
                }
            }

            return result;
        }
    }
}
