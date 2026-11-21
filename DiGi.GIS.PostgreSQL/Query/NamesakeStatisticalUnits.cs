using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Returns the other municipality units of the same name in the same voivodeship as the given one, in code order - the units a gmina that changed its type or county is also listed under (DiGi-Organization/DiGi.GIS.PostgreSQL#149).
        /// <para>A gmina that changed its type (rural to urban-rural) or its county since a census keeps its name on two BDL units, and which of them holds the data depends on whether BDL has moved the history yet: Czerwińsk nad Wisłą holds its 2021 census on the current unit 071422520043, Nowe Miasto still on the historical 071422520082 while its current 071422520083 holds nothing. <see cref="Match(StatisticalUnit?, string?, string?, Enums.AdministrativeArealType)"/> takes the unit the code names; a reader whose unit holds no data falls back to these.</para>
        /// <para>Only municipality-level units are compared (<see cref="StatisticalUnitType.municipalities"/>), so the town and rural parts of an urban-rural gmina (" - miasto", " - obszar wiejski") are not namesakes; names are compared trimmed and case-insensitively, the voivodeship by the first four digits of the code.</para>
        /// </summary>
        /// <param name="rootStatisticalUnit">The root statistical unit hierarchy. This value can be null.</param>
        /// <param name="statisticalUnit">The municipality unit. This value can be null.</param>
        /// <returns>The namesakes, the given unit excluded; empty when either argument is null or the unit has no name or a code shorter than 4 digits.</returns>
        public static List<StatisticalUnit> NamesakeStatisticalUnits(this StatisticalUnit? rootStatisticalUnit, StatisticalUnit? statisticalUnit)
        {
            List<StatisticalUnit> result = [];

            string? code = statisticalUnit?.UnitCode?.Code;
            string name = statisticalUnit?.Name?.Trim().ToUpperInvariant() ?? string.Empty;
            if (rootStatisticalUnit is null || code is null || code.Length < 4 || string.IsNullOrEmpty(name))
            {
                return result;
            }

            IEnumerable<StatisticalUnit>? statisticalUnits = rootStatisticalUnit.GetStatisticalUnits(includeNested: true, x => x?.GetStatisticalUnitType() == StatisticalUnitType.municipalities);
            if (statisticalUnits is null)
            {
                return result;
            }

            foreach (StatisticalUnit statisticalUnit_Namesake in statisticalUnits)
            {
                string? code_Namesake = statisticalUnit_Namesake?.UnitCode?.Code;
                if (statisticalUnit_Namesake is null || code_Namesake is null || code_Namesake == code || code_Namesake.Length < 4 || string.Compare(code_Namesake, 0, code, 0, 4, StringComparison.Ordinal) != 0)
                {
                    continue;
                }

                if (statisticalUnit_Namesake.Name?.Trim().ToUpperInvariant() == name)
                {
                    result.Add(statisticalUnit_Namesake);
                }
            }

            result.Sort((x, y) => string.CompareOrdinal(x.UnitCode?.Code, y.UnitCode?.Code));
            return result;
        }
    }
}
