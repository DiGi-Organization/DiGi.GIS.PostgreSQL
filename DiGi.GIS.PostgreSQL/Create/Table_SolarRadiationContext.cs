using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.IO;
using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Builds the urban-context table of a set of buildings: per building, the annual irradiation of its walls by facing sector, of its roof and of the whole building for each neighbour radius, the shortfall of each radius against the isolated building, and the building's descriptors from <c>building_data</c> (DiGi.GIS.PostgreSQL#107, #136).
        /// <para>Irradiation is the area-weighted average over the usable surfaces of a group (a finite area greater than zero), in kWh/m² per year, as in <see cref="Modify.Update_SolarRadiation"/>. A surface whose outward normal is steeper than 60° from the vertical (|n<sub>z</sub>| ≥ 0.5) counts as roof, any other as wall; a wall is filed under the cardinal sector of its normal's azimuth, the eight sectors of the solar radiation summary. A group with no usable surface is left empty.</para>
        /// <para>The reduction of a group at radius r is <c>1 - E(r) / E(0)</c>: the share of the isolated building's irradiation (radius 0, <c>SolarRadiusIsolated</c>) its neighbours take away. It is written only when <paramref name="radii"/> holds 0 and both values exist.</para>
        /// <para>The descriptors copied are the year built (calculated), storeys, building shape (calculated), settlement type, general function and the residential flag, and every <c>Radial …</c> ratio column the building data table carries. A building is written when the building data table holds its row and it has at least one result; with <paramref name="residentialOnly"/> only a building whose row says it is residential.</para>
        /// </summary>
        /// <param name="buildingSolarRadiationResults">The stored results, of any of <paramref name="radii"/>; the first result of a county part, reference and radius is used.</param>
        /// <param name="radii">The radii to write, in m, exactly as stored, in column order.</param>
        /// <param name="table_BuildingData">The building data rows of the buildings, keyed by county identifier and reference.</param>
        /// <param name="residentialOnly">True to write only the buildings whose building data row is residential.</param>
        /// <returns>The table, or null when an input is missing or the building data table has no county or reference column.</returns>
        public static Table? Table_SolarRadiationContext(IEnumerable<GIS.Classes.BuildingSolarRadiationResult?>? buildingSolarRadiationResults, IList<double>? radii, Table? table_BuildingData, bool residentialOnly)
        {
            if (buildingSolarRadiationResults is null || radii is null || table_BuildingData is null)
            {
                return null;
            }

            // The building data columns: the key, the residential flag, and the descriptors to copy.
            string? uniqueId_CountyId = Core.IO.Query.UniqueId(IO.Constants.Column.CountyId);
            string? uniqueId_Reference = Core.IO.Query.UniqueId(IO.Constants.Column.Reference);
            string? uniqueId_IsResidential = Core.IO.Query.UniqueId(IO.Constants.Column.IsResidential);
            List<string?> uniqueIds_Descriptor =
            [
                Core.IO.Query.UniqueId(IO.Constants.Column.CalculatedYearBuilt),
                Core.IO.Query.UniqueId(IO.Constants.Column.Storeys),
                Core.IO.Query.UniqueId(IO.Constants.Column.CalculatedBuildingShape),
                Core.IO.Query.UniqueId(IO.Constants.Column.SettlementType),
                Core.IO.Query.UniqueId(IO.Constants.Column.BuildingGeneralFunction),
                uniqueId_IsResidential,
            ];

            Column? column_CountyId_Source = null;
            Column? column_Reference_Source = null;
            Column? column_IsResidential_Source = null;
            List<Column> columns_Descriptor_Source = [];
            List<Column> columns_Radial_Source = [];
            foreach (Column column in table_BuildingData.Columns)
            {
                string? uniqueId = Core.IO.Query.UniqueId(column);
                if (uniqueId is not null && uniqueId == uniqueId_CountyId)
                {
                    column_CountyId_Source = column;
                }
                else if (uniqueId is not null && uniqueId == uniqueId_Reference)
                {
                    column_Reference_Source = column;
                }

                if (uniqueId is not null && uniqueId == uniqueId_IsResidential)
                {
                    column_IsResidential_Source = column;
                }

                if (uniqueId is not null && uniqueIds_Descriptor.Contains(uniqueId))
                {
                    columns_Descriptor_Source.Add(column);
                }
                else if (column.Name is string name && name.StartsWith("Radial ", StringComparison.Ordinal))
                {
                    columns_Radial_Source.Add(column);
                }
            }

            if (column_CountyId_Source is null || column_Reference_Source is null)
            {
                return null;
            }

            // The first result of each county part, reference and radius.
            Dictionary<(int, string, double), GIS.Classes.BuildingSolarRadiationResult> results = [];
            foreach (GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult in buildingSolarRadiationResults)
            {
                if (buildingSolarRadiationResult?.Reference is string reference && !string.IsNullOrWhiteSpace(reference) && !results.ContainsKey((buildingSolarRadiationResult.CountyId, reference, buildingSolarRadiationResult.Radius)))
                {
                    results[(buildingSolarRadiationResult.CountyId, reference, buildingSolarRadiationResult.Radius)] = buildingSolarRadiationResult;
                }
            }

            // The groups of surfaces, in column order: the eight wall sectors, all walls, the roof, the whole building.
            string[] groupNames = ["Wall North", "Wall Northeast", "Wall East", "Wall Southeast", "Wall South", "Wall Southwest", "Wall West", "Wall Northwest", "Wall", "Roof", "Building"];
            int groupCount = groupNames.Length;

            Table result = new();
            Column column_CountyId = result.UpdateColumn<Column>(new Column(IO.Constants.Column.CountyId.Name, typeof(int)))!;
            Column column_Reference = result.UpdateColumn<Column>(new Column(IO.Constants.Column.Reference.Name, typeof(string)))!;

            List<(Column Source, Column Target)> columns_Copy = [];
            foreach (Column column in columns_Descriptor_Source)
            {
                columns_Copy.Add((column, result.UpdateColumn<Column>(new Column(column.Name, column.Type))!));
            }

            foreach (Column column in columns_Radial_Source)
            {
                columns_Copy.Add((column, result.UpdateColumn<Column>(new Column(column.Name, column.Type))!));
            }

            int index_Isolated = radii.IndexOf(0);
            Column[,] columns_Irradiation = new Column[radii.Count, groupCount];
            Column?[,] columns_Reduction = new Column?[radii.Count, groupCount];
            for (int r = 0; r < radii.Count; r++)
            {
                string radiusText = radii[r].ToString(CultureInfo.InvariantCulture);
                for (int g = 0; g < groupCount; g++)
                {
                    columns_Irradiation[r, g] = result.UpdateColumn<Column>(new Column(string.Format(CultureInfo.InvariantCulture, "{0} Irradiation [r={1} m]", groupNames[g], radiusText), typeof(float)))!;
                }
            }

            for (int r = 0; r < radii.Count; r++)
            {
                if (index_Isolated < 0 || r == index_Isolated)
                {
                    continue;
                }

                string radiusText = radii[r].ToString(CultureInfo.InvariantCulture);
                for (int g = 0; g < groupCount; g++)
                {
                    columns_Reduction[r, g] = result.UpdateColumn<Column>(new Column(string.Format(CultureInfo.InvariantCulture, "{0} Reduction [r={1} m]", groupNames[g], radiusText), typeof(float)));
                }
            }

            int rowCount = table_BuildingData.RowCount;
            for (int i = 0; i < rowCount; i++)
            {
                Row? row_Source = table_BuildingData.GetRow(i);
                if (row_Source is null || !row_Source.TryGetValue(column_CountyId_Source.Index, out object? value_CountyId) || !Core.Query.TryConvert(value_CountyId, out int countyId))
                {
                    continue;
                }

                if (!row_Source.TryGetValue(column_Reference_Source.Index, out string? reference) || string.IsNullOrWhiteSpace(reference))
                {
                    continue;
                }

                if (residentialOnly && (column_IsResidential_Source is null || !row_Source.TryGetValue(column_IsResidential_Source.Index, out object? value_IsResidential) || !Core.Query.TryConvert(value_IsResidential, out bool isResidential) || !isResidential))
                {
                    continue;
                }

                double[,] irradiations = new double[radii.Count, groupCount];
                bool found = false;
                for (int r = 0; r < radii.Count; r++)
                {
                    for (int g = 0; g < groupCount; g++)
                    {
                        irradiations[r, g] = double.NaN;
                    }

                    if (results.TryGetValue((countyId, reference!, radii[r]), out GIS.Classes.BuildingSolarRadiationResult? buildingSolarRadiationResult))
                    {
                        found = true;
                        double[] numerators = new double[groupCount];
                        double[] areas = new double[groupCount];
                        foreach (GIS.Classes.SurfaceSolarRadiation? surfaceSolarRadiation in buildingSolarRadiationResult.SurfaceSolarRadiations ?? [])
                        {
                            double area = surfaceSolarRadiation?.Area ?? double.NaN;
                            double irradiation = surfaceSolarRadiation?.Irradiation ?? double.NaN;
                            if (!double.IsFinite(area) || area <= 0 || !double.IsFinite(irradiation))
                            {
                                continue;
                            }

                            foreach (int g in Groups(surfaceSolarRadiation!.Normal))
                            {
                                numerators[g] += irradiation * area;
                                areas[g] += area;
                            }
                        }

                        for (int g = 0; g < groupCount; g++)
                        {
                            if (areas[g] > 0)
                            {
                                irradiations[r, g] = numerators[g] / areas[g];
                            }
                        }
                    }
                }

                if (!found)
                {
                    continue;
                }

                Row row = result.AddRow();
                IO.Modify.SetValue(row, column_CountyId, countyId);
                IO.Modify.SetValue(row, column_Reference, reference);
                foreach ((Column source, Column target) in columns_Copy)
                {
                    if (row_Source.TryGetValue(source.Index, out object? value) && value is not null && value is not DBNull)
                    {
                        IO.Modify.SetValue(row, target, value);
                    }
                }

                for (int r = 0; r < radii.Count; r++)
                {
                    for (int g = 0; g < groupCount; g++)
                    {
                        double irradiation = irradiations[r, g];
                        if (double.IsFinite(irradiation))
                        {
                            IO.Modify.SetValue(row, columns_Irradiation[r, g], (float)irradiation);
                        }

                        double irradiation_Isolated = index_Isolated < 0 ? double.NaN : irradiations[index_Isolated, g];
                        if (columns_Reduction[r, g] is Column column_Reduction && double.IsFinite(irradiation) && double.IsFinite(irradiation_Isolated) && irradiation_Isolated > 0)
                        {
                            IO.Modify.SetValue(row, column_Reduction, (float)(1 - (irradiation / irradiation_Isolated)));
                        }
                    }
                }

                result.AddRow(row, false);
            }

            return result;

            // The groups a surface counts toward: a wall its sector (when it has one), all walls and the building; a roof
            // the roof and the building; a surface without a normal the building only.
            IEnumerable<int> Groups(Vector3D? normal)
            {
                Vector3D? normal_Unit = normal?.Unit;
                if (normal_Unit is null || !double.IsFinite(normal_Unit.Z))
                {
                    yield return 10;
                    yield break;
                }

                if (Math.Abs(normal_Unit.Z) >= 0.5)
                {
                    yield return 9;
                    yield return 10;
                    yield break;
                }

                double horizontal = Math.Sqrt((normal_Unit.X * normal_Unit.X) + (normal_Unit.Y * normal_Unit.Y));
                if (horizontal > 0)
                {
                    double azimuth = ((Math.Atan2(normal_Unit.X, normal_Unit.Y) * 180.0 / Math.PI) + 360.0) % 360.0;
                    int sector = GIS.Query.CardinalDirection(azimuth) switch
                    {
                        GIS.Enums.CardinalDirection.North => 0,
                        GIS.Enums.CardinalDirection.NorthEast => 1,
                        GIS.Enums.CardinalDirection.East => 2,
                        GIS.Enums.CardinalDirection.SouthEast => 3,
                        GIS.Enums.CardinalDirection.South => 4,
                        GIS.Enums.CardinalDirection.SouthWest => 5,
                        GIS.Enums.CardinalDirection.West => 6,
                        GIS.Enums.CardinalDirection.NorthWest => 7,
                        _ => -1,
                    };

                    if (sector >= 0)
                    {
                        yield return sector;
                    }
                }

                yield return 8;
                yield return 10;
            }
        }
    }
}
