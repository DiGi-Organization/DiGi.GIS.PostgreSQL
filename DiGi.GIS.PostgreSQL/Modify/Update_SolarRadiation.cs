using DiGi.Core.IO.Table.Classes;
using DiGi.GIS.IO;
using DiGi.GIS.PostgreSQL.Classes;
using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Aggregates the yearly solar radiation of the stored <see cref="GIS.Classes.BuildingSolarRadiationResult"/>s into the 25 “Solar radiation” columns of the given table, keyed by county identifier and reference.
        /// <para>Irradiation is the area-weighted average over the building’s usable surfaces (a finite area greater than zero); energy is their sum. A surface is filed under the cardinal sector of its outward normal’s azimuth, in the same eight sectors as the external components area columns. The first record of a given county and reference is the one written and the rest are stepped over, so the collection has to reach this method in the caller’s order of preference - the converter returns the newest first.</para>
        /// <para>A surface with no usable area is skipped and counted. A usable surface with no classifiable facing direction - a missing or vertical outward normal, typically a flat roof - counts toward the building totals but toward no direction sector. A direction with no usable surface is left null, and a building with no usable surface is left null rather than zeroed.</para>
        /// </summary>
        /// <param name="table">The table to fill with the yearly solar radiation summary.</param>
        /// <param name="results">The stored solar radiation results, most preferred record of a county and reference first.</param>
        /// <returns>The outcome of the aggregation: the buildings written and the surfaces aggregated, plus the ones skipped or without a classifiable sector.</returns>
        public static SolarRadiationSummaryResult Update_SolarRadiation(this Table? table, IEnumerable<GIS.Classes.BuildingSolarRadiationResult>? results)
        {
            long processedReferenceCount = 0;
            long surfaceCount = 0;
            long surfaceWithoutSectorCount = 0;
            long skippedSurfaceCount = 0;

            if (table is null || results is null)
            {
                return new SolarRadiationSummaryResult();
            }

            Column? column_CountyId = table.UpdateColumn<Column>(IO.Constants.Column.CountyId);
            Column? column_Reference = table.UpdateColumn<Column>(IO.Constants.Column.Reference);
            if (column_CountyId is null || column_Reference is null)
            {
                return new SolarRadiationSummaryResult();
            }

            List<Column> columns_Solar = [];
            foreach (Column column in IO.Create.Columns_SolarRadiation())
            {
                Column? column_Added = table.UpdateColumn<Column>(column);
                if (column_Added is not null)
                {
                    columns_Solar.Add(column_Added);
                }
            }

            // 25 columns: the six building totals (index 0-5), then the sixteen per-direction values
            // (index 6-21; sector d carries its irradiation at 6 + 2d and its energy at 7 + 2d),
            // then the three weather source and range metadata columns (index 22-24).
            const int columnCount = 25;
            if (columns_Solar.Count != columnCount)
            {
                return new SolarRadiationSummaryResult();
            }

            Dictionary<(int, string), Row> dictionary_Row = Query.RowsByCountyIdAndReference(table, column_CountyId, column_Reference);
            HashSet<(int, string)> written = [];

            foreach (GIS.Classes.BuildingSolarRadiationResult? result in results)
            {
                if (result is null)
                {
                    continue;
                }

                string? reference = result.Reference;
                if (string.IsNullOrWhiteSpace(reference))
                {
                    continue;
                }

                int countyId = result.CountyId;

                if (!written.Add((countyId, reference)))
                {
                    continue;
                }

                // Per-building accumulators, reset for every building.
                double[] totalNumerator = new double[5]; // Irradiation, Beam, Diffuse, Ground, IrradiationUnshaded
                double totalArea = 0;
                double totalEnergy = 0;
                int buildingSurfaceCount = 0;
                int buildingWithoutSectorCount = 0;
                int buildingSkippedCount = 0;

                double[] sectorIrradiationNumerator = new double[8];
                double[] sectorArea = new double[8];
                double[] sectorEnergy = new double[8];
                int[] sectorSurfaceCount = new int[8];

                List<GIS.Classes.SurfaceSolarRadiation>? surfaceSolarRadiations = result.SurfaceSolarRadiations;
                if (surfaceSolarRadiations is not null)
                {
                    foreach (GIS.Classes.SurfaceSolarRadiation? surfaceSolarRadiation in surfaceSolarRadiations)
                    {
                        if (surfaceSolarRadiation is null)
                        {
                            continue;
                        }

                        double area = surfaceSolarRadiation.Area;
                        if (double.IsNaN(area) || area <= 0)
                        {
                            buildingSkippedCount++;
                            continue;
                        }

                        double irradiation = surfaceSolarRadiation.Irradiation;
                        double beam = surfaceSolarRadiation.Beam;
                        double diffuse = surfaceSolarRadiation.Diffuse;
                        double ground = surfaceSolarRadiation.Ground;
                        double irradiationUnshaded = surfaceSolarRadiation.IrradiationUnshaded;
                        double energy = surfaceSolarRadiation.Energy;

                        // Building totals - every usable surface, whatever its facing direction.
                        totalNumerator[0] += irradiation * area;
                        totalNumerator[1] += beam * area;
                        totalNumerator[2] += diffuse * area;
                        totalNumerator[3] += ground * area;
                        totalNumerator[4] += irradiationUnshaded * area;
                        totalArea += area;
                        totalEnergy += energy;
                        buildingSurfaceCount++;

                        int sector = SectorOf(surfaceSolarRadiation.Normal);
                        if (sector < 0)
                        {
                            buildingWithoutSectorCount++;
                            continue;
                        }

                        sectorIrradiationNumerator[sector] += irradiation * area;
                        sectorArea[sector] += area;
                        sectorEnergy[sector] += energy;
                        sectorSurfaceCount[sector]++;
                    }
                }

                Row? row = null;
                if (!dictionary_Row.TryGetValue((countyId, reference), out row) || row is null)
                {
                    row = table.AddRow();
                    IO.Modify.SetValue(row, column_CountyId, countyId);
                    IO.Modify.SetValue(row, column_Reference, reference);
                    dictionary_Row[(countyId, reference)] = row;
                }

                // The six building totals (index 0-5): the area-weighted averages and the energy sum.
                if (buildingSurfaceCount > 0 && totalArea > 0)
                {
                    SetIfFinite(row, columns_Solar[0], totalNumerator[0] / totalArea);
                    SetIfFinite(row, columns_Solar[1], totalEnergy);
                    SetIfFinite(row, columns_Solar[2], totalNumerator[1] / totalArea);
                    SetIfFinite(row, columns_Solar[3], totalNumerator[2] / totalArea);
                    SetIfFinite(row, columns_Solar[4], totalNumerator[3] / totalArea);
                    SetIfFinite(row, columns_Solar[5], totalNumerator[4] / totalArea);
                }

                // The sixteen per-direction values (index 6-21): a direction with no usable surface stays null.
                for (int d = 0; d < 8; d++)
                {
                    if (sectorSurfaceCount[d] > 0 && sectorArea[d] > 0)
                    {
                        SetIfFinite(row, columns_Solar[6 + 2 * d], sectorIrradiationNumerator[d] / sectorArea[d]);
                        SetIfFinite(row, columns_Solar[7 + 2 * d], sectorEnergy[d]);
                    }
                }

                // The three metadata columns (index 22-24): the weather source identity and the surrounding building range.
                // Set for every written row, independent of whether the building carries usable surfaces.
                if (!string.IsNullOrWhiteSpace(result.WeatherSourceId))
                {
                    IO.Modify.SetValue(row, columns_Solar[22], result.WeatherSourceId);
                }
                IO.Modify.SetValue(row, columns_Solar[23], (int)result.WeatherSourceType);
                if (double.IsFinite(result.Radius) && result.Radius > 0)
                {
                    IO.Modify.SetValue(row, columns_Solar[24], (float)result.Radius);
                }

                table.AddRow(row, false);

                processedReferenceCount++;
                surfaceCount += buildingSurfaceCount;
                surfaceWithoutSectorCount += buildingWithoutSectorCount;
                skippedSurfaceCount += buildingSkippedCount;
            }

            return new SolarRadiationSummaryResult(processedReferenceCount, surfaceCount, surfaceWithoutSectorCount, skippedSurfaceCount);

            // Writes the value only when it is a finite number - a non-finite irradiation or energy is left as a null cell
            // rather than a sentinel that would read as a real measurement.
            void SetIfFinite(Row? row_Cell, Column? column_Cell, double value)
            {
                if (double.IsFinite(value))
                {
                    IO.Modify.SetValue(row_Cell, column_Cell, (float)value);
                }
            }

            // The cardinal sector of a surface’s outward normal, or -1 when it has no classifiable facing direction -
            // a missing normal, or a vertical one (a flat roof facing straight up), which has no azimuth.
            int SectorOf(Vector3D? normal)
            {
                if (normal is null || normal.Length <= 0 || double.IsNaN(normal.Length))
                {
                    return -1;
                }

                Vector3D? normal_Unit = normal.Unit;
                if (normal_Unit is null)
                {
                    return -1;
                }

                double horizontal = Math.Sqrt(normal_Unit.X * normal_Unit.X + normal_Unit.Y * normal_Unit.Y);
                if (horizontal <= 0)
                {
                    return -1;
                }

                double azimuth = (Math.Atan2(normal_Unit.X, normal_Unit.Y) * 180.0 / Math.PI + 360.0) % 360.0;

                return SectorIndex(DiGi.GIS.Query.CardinalDirection(azimuth));
            }

            // Wall sectors, in the order of the columns: north, northeast, east, southeast, south, southwest, west, northwest.
            int SectorIndex(DiGi.GIS.Enums.CardinalDirection direction)
            {
                return direction switch
                {
                    DiGi.GIS.Enums.CardinalDirection.North => 0,
                    DiGi.GIS.Enums.CardinalDirection.NorthEast => 1,
                    DiGi.GIS.Enums.CardinalDirection.East => 2,
                    DiGi.GIS.Enums.CardinalDirection.SouthEast => 3,
                    DiGi.GIS.Enums.CardinalDirection.South => 4,
                    DiGi.GIS.Enums.CardinalDirection.SouthWest => 5,
                    DiGi.GIS.Enums.CardinalDirection.West => 6,
                    DiGi.GIS.Enums.CardinalDirection.NorthWest => 7,
                    _ => -1
                };
            }
        }
    }
}
