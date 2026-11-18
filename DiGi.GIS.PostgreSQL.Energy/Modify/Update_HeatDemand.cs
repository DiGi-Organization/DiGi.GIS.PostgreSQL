using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.Analytical.Energy.Classes;
using DiGi.GIS.IO;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.Energy
{
    public static partial class Modify
    {
        /// <summary>
        /// Writes the heat-demand summary of the given results into the four "Heat demand" columns of a <c>building_data</c> table, keyed by county identifier and reference (DiGi.GIS.PostgreSQL#143): the annual energy need for space heating [kWh], the energy need per reference floor area [kWh/m2], the design heat load [W] and the method version (<c>GIS.IO.Create.Columns_HeatDemand</c>).
        /// <para>The first result of a county and reference is the one written and the rest are stepped over, so the results reach this method in the caller's order of preference. A value that is not a finite number is left as a null cell, never a sentinel. A result without a reference or without a heat demand is stepped over.</para>
        /// </summary>
        /// <param name="table">The table to fill. This value can be null.</param>
        /// <param name="buildingHeatDemandResults">The results, most preferred first. This value can be null.</param>
        /// <returns>The number of buildings written; 0 when the table or the results are null.</returns>
        public static int Update_HeatDemand(this Table? table, IEnumerable<BuildingHeatDemandResult?>? buildingHeatDemandResults)
        {
            if (table is null || buildingHeatDemandResults is null)
            {
                return 0;
            }

            Column? column_CountyId = table.UpdateColumn<Column>(IO.Constants.Column.CountyId);
            Column? column_Reference = table.UpdateColumn<Column>(IO.Constants.Column.Reference);
            if (column_CountyId is null || column_Reference is null)
            {
                return 0;
            }

            List<Column> columns = [];
            foreach (Column column in IO.Create.Columns_HeatDemand())
            {
                if (table.UpdateColumn<Column>(column) is Column column_Added)
                {
                    columns.Add(column_Added);
                }
            }

            // HeatingEnergyNeed, HeatingEnergyNeedPerArea, DesignHeatLoad, HeatDemandMethodVersion, in that order.
            if (columns.Count != 4)
            {
                return 0;
            }

            Dictionary<(int, string), Row> rows = PostgreSQL.Query.RowsByCountyIdAndReference(table, column_CountyId, column_Reference);
            HashSet<(int, string)> written = [];

            int result = 0;
            foreach (BuildingHeatDemandResult? buildingHeatDemandResult in buildingHeatDemandResults)
            {
                string? reference = buildingHeatDemandResult?.Reference;
                DiGi.Analytical.Building.Energy.Classes.HeatDemandResult? heatDemandResult = buildingHeatDemandResult?.HeatDemandResult;
                if (buildingHeatDemandResult is null || heatDemandResult is null || string.IsNullOrWhiteSpace(reference) || !written.Add((buildingHeatDemandResult.CountyId, reference!)))
                {
                    continue;
                }

                if (!rows.TryGetValue((buildingHeatDemandResult.CountyId, reference!), out Row? row) || row is null)
                {
                    row = table.AddRow();
                    IO.Modify.SetValue(row, column_CountyId, buildingHeatDemandResult.CountyId);
                    IO.Modify.SetValue(row, column_Reference, reference);
                    rows[(buildingHeatDemandResult.CountyId, reference!)] = row;
                }

                SetIfFinite(row, columns[0], heatDemandResult.HeatingEnergyNeed);
                SetIfFinite(row, columns[1], heatDemandResult.HeatingEnergyNeedPerArea);
                SetIfFinite(row, columns[2], heatDemandResult.DesignHeatLoad);
                IO.Modify.SetValue(row, columns[3], heatDemandResult.MethodVersion);

                table.AddRow(row, false);
                result++;
            }

            return result;

            static void SetIfFinite(Row? row_Cell, Column? column_Cell, double value)
            {
                if (!double.IsNaN(value) && !double.IsInfinity(value))
                {
                    IO.Modify.SetValue(row_Cell, column_Cell, (float)value);
                }
            }
        }
    }
}
