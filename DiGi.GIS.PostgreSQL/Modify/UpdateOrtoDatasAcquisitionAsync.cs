using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Modify
    {
        /// <summary>
        /// Backfills the acquisition date, sheet and content hash of every stored image of the given county parts from the orthophoto sheet index (<c>orto_sheet</c>), DiGi.GIS#18.
        /// <para>Per county part: the sheets covering the box of its stored series are read once (<see cref="OrtoSheetPostgreSQLConverter.GetOrtoSheetsByBoundingBoxAsync(NpgsqlConnection?, BoundingBox2D?, double, int, CancellationToken)"/>) and bucketed into a grid of 1 km cells, a one-pass index built for this walk only. Every series is then re-dated against the sheets of the cell of its first image's centre (<see cref="GIS.Create.OrtoDatas(GIS.Classes.OrtoDatas?, IEnumerable{OrtoSheet?}?, int)"/>), so one image is tested against a few dozen sheets, not every sheet of the part.</para>
        /// <para>The partition is walked in physical order (<see cref="OrtoDatasPostgreSQLConverter.PullByPhysicalOrderAsync(NpgsqlConnection?, int, string?, int, int, CancellationToken)"/>), and the dates are written to <see cref="Constants.TableName.OrtoDataAcquisition"/>, one row per image (<see cref="OrtoDatasPostgreSQLConverter.UpdateOrtoDataAcquisitionsAsync"/>); <c>orto_datas</c> is only read. Nothing therefore moves ahead of the walk and every row is read once - the id check below is a guard, not a skip (DiGi.GIS.PostgreSQL#123: rewriting <c>object</c> cost about 0.5 MB per row, bloated the partition and read every rewritten row twice). A row whose acquisition rows already say the same is not written, so a re-run writes nothing. The walk and the writes use two connections to the Storage database, which holds all three tables.</para>
        /// <para>Series rewritten by the first backfill (DiGi.GIS.PostgreSQL.UI#37, county part 55417) keep the acquisition members inside <c>object</c>; they are harmless and ignored, and the side table is the source of truth.</para>
        /// <para><paramref name="countyIds"/> are county part ids: rows are re-dated where they are filed and never move (<c>Coding - GIS Administrative Data.md</c>). Import the sheet index first (<see cref="UpdateOrtoSheetsAsync"/>): without it every image is recorded as undated.</para>
        /// </summary>
        /// <param name="ortoDatasPostgreSQLConverter">The converter of the <c>orto_datas</c> table.</param>
        /// <param name="ortoSheetPostgreSQLConverter">The converter of the <c>orto_sheet</c> table.</param>
        /// <param name="countyIds">The county part ids to walk.</param>
        /// <param name="lastYear">The last year requested when the series were fetched (<c>OrtoDatasOptions.Years</c>).</param>
        /// <param name="pageSize">The number of rows read per page; a stored series is several hundred kilobytes.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>A task that represents the asynchronous operation. The task result counts the rows and images, with <see cref="OrtoDatasAcquisitionUpdateResult.Changed"/> the rows whose acquisition rows were inserted or changed, or is null when a converter or the county parts are missing, a connection cannot be opened, a table cannot be created, or a read or write cannot run.</returns>
        public static async Task<OrtoDatasAcquisitionUpdateResult?> UpdateOrtoDatasAcquisitionAsync(this OrtoDatasPostgreSQLConverter? ortoDatasPostgreSQLConverter, OrtoSheetPostgreSQLConverter? ortoSheetPostgreSQLConverter, IEnumerable<int>? countyIds, int lastYear, int pageSize = 100, int commandTimeout = 300, CancellationToken cancellationToken = default)
        {
            int[] countyIds_Array = countyIds is null ? [] : [.. countyIds.Distinct()];
            if (ortoDatasPostgreSQLConverter is null || ortoSheetPostgreSQLConverter is null || countyIds_Array.Length == 0)
            {
                return null;
            }

            const double cellSize = 1000;

            await using NpgsqlConnection? npgsqlConnection_Read = DiGi.PostgreSQL.Create.NpgsqlConnection(ortoDatasPostgreSQLConverter.ConnectionData);
            await using NpgsqlConnection? npgsqlConnection_Write = DiGi.PostgreSQL.Create.NpgsqlConnection(ortoDatasPostgreSQLConverter.ConnectionData);
            await using NpgsqlConnection? npgsqlConnection_Sheet = DiGi.PostgreSQL.Create.NpgsqlConnection(ortoSheetPostgreSQLConverter.ConnectionData);
            if (npgsqlConnection_Read is null || npgsqlConnection_Write is null || npgsqlConnection_Sheet is null)
            {
                return null;
            }

            await npgsqlConnection_Read.OpenAsync(cancellationToken);
            await npgsqlConnection_Write.OpenAsync(cancellationToken);
            await npgsqlConnection_Sheet.OpenAsync(cancellationToken);

            if (!await Create.TableAsync_OrtoDataAcquisition(npgsqlConnection_Write, commandTimeout, cancellationToken))
            {
                return null;
            }

            long sheets = 0;
            long rows = 0;
            long changed = 0;
            long images = 0;
            long dated = 0;

            foreach (int countyId in countyIds_Array)
            {
                BoundingBox2D? boundingBox2D = await OrtoDatasPostgreSQLConverter.GetBoundingBox2DAsync(npgsqlConnection_Read, countyId, commandTimeout, cancellationToken);
                if (boundingBox2D is null)
                {
                    continue;
                }

                List<OrtoSheet>? ortoSheets = await OrtoSheetPostgreSQLConverter.GetOrtoSheetsByBoundingBoxAsync(npgsqlConnection_Sheet, boundingBox2D, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (ortoSheets is null)
                {
                    return null;
                }

                sheets += ortoSheets.Count;

                await Create.TableAsync_OrtoDataAcquisition_Partition(npgsqlConnection_Write, countyId, commandTimeout, cancellationToken);

                // Every sheet is filed under each 1 km cell its footprint box touches.
                Dictionary<(long, long), List<OrtoSheet>> dictionary_Cells = [];
                foreach (OrtoSheet ortoSheet in ortoSheets)
                {
                    if (ortoSheet.Footprint?.GetBoundingBox() is not BoundingBox2D boundingBox2D_Sheet)
                    {
                        continue;
                    }

                    for (long x = Cell(boundingBox2D_Sheet.Min.X); x <= Cell(boundingBox2D_Sheet.Max.X); x++)
                    {
                        for (long y = Cell(boundingBox2D_Sheet.Min.Y); y <= Cell(boundingBox2D_Sheet.Max.Y); y++)
                        {
                            if (!dictionary_Cells.TryGetValue((x, y), out List<OrtoSheet>? ortoSheets_Cell))
                            {
                                ortoSheets_Cell = [];
                                dictionary_Cells[(x, y)] = ortoSheets_Cell;
                            }

                            ortoSheets_Cell.Add(ortoSheet);
                        }
                    }
                }

                // orto_datas is never written here, so no row moves; a row read twice would only come from a concurrent writer.
                HashSet<long> ids = [];

                string? position = null;
                while (position != string.Empty)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    (Dictionary<long, GIS.Classes.OrtoDatas>? ortoDatas_Page, string? position_Next) = await OrtoDatasPostgreSQLConverter.PullByPhysicalOrderAsync(npgsqlConnection_Read, countyId, position, pageSize, commandTimeout, cancellationToken);
                    if (ortoDatas_Page is null || position_Next is null)
                    {
                        return null;
                    }

                    List<OrtoDataAcquisition> ortoDataAcquisitions_Page = [];
                    foreach (KeyValuePair<long, GIS.Classes.OrtoDatas> keyValuePair in ortoDatas_Page)
                    {
                        if (!ids.Add(keyValuePair.Key))
                        {
                            continue;
                        }

                        rows++;

                        Point2D? point2D = keyValuePair.Value.FirstOrDefault()?.GetBoundingBox(GeometryContext.Global)?.GetCentroid();
                        List<OrtoSheet>? ortoSheets_Cell = null;
                        if (point2D is not null)
                        {
                            dictionary_Cells.TryGetValue((Cell(point2D.X), Cell(point2D.Y)), out ortoSheets_Cell);
                        }

                        GIS.Classes.OrtoDatas? ortoDatas_Redated = GIS.Create.OrtoDatas(keyValuePair.Value, ortoSheets_Cell, lastYear);
                        if (ortoDatas_Redated is null)
                        {
                            continue;
                        }

                        foreach (OrtoData ortoData in ortoDatas_Redated)
                        {
                            images++;
                            if (ortoData.AcquisitionDatePrecision != OrtoDatePrecision.Undefined)
                            {
                                dated++;
                            }
                        }

                        List<OrtoDataAcquisition>? ortoDataAcquisitions = ortoDatas_Redated.OrtoDataAcquisitions(countyId);
                        if (ortoDataAcquisitions is not null)
                        {
                            ortoDataAcquisitions_Page.AddRange(ortoDataAcquisitions);
                        }
                    }

                    if (ortoDataAcquisitions_Page.Count != 0)
                    {
                        HashSet<string>? references_Changed = await OrtoDatasPostgreSQLConverter.UpdateOrtoDataAcquisitionsAsync(npgsqlConnection_Write, countyId, ortoDataAcquisitions_Page, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        if (references_Changed is null)
                        {
                            return null;
                        }

                        changed += references_Changed.Count;
                    }

                    position = position_Next;
                }
            }

            return new OrtoDatasAcquisitionUpdateResult(countyIds_Array, sheets, rows, changed, images, dated);

            static long Cell(double value)
            {
                return (long)Math.Floor(value / cellSize);
            }
        }
    }
}
