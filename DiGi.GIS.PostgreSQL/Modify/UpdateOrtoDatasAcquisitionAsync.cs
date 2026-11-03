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
        /// <para>The partition is walked in physical order (<see cref="OrtoDatasPostgreSQLConverter.PullByPhysicalOrderAsync(NpgsqlConnection?, int, string?, int, int, CancellationToken)"/>), and a row is written back (<see cref="OrtoDatasPostgreSQLConverter.UpdateObjectsAsync(NpgsqlConnection?, int, IDictionary{long, GIS.Classes.OrtoDatas}?, int, int, CancellationToken)"/>) only when its series changed. A rewritten row moves ahead of the walk and is read again; it is skipped by its id, so every row counts once. Re-dating is idempotent, so a re-run writes nothing. The walk and the writes use two connections to the Storage database, which holds both tables.</para>
        /// <para><paramref name="countyIds"/> are county part ids: rows are re-dated where they are filed and never move (<c>Coding - GIS Administrative Data.md</c>). Import the sheet index first (<see cref="UpdateOrtoSheetsAsync"/>): without it every image is written back undated.</para>
        /// </summary>
        /// <param name="ortoDatasPostgreSQLConverter">The converter of the <c>orto_datas</c> table.</param>
        /// <param name="ortoSheetPostgreSQLConverter">The converter of the <c>orto_sheet</c> table.</param>
        /// <param name="countyIds">The county part ids to walk.</param>
        /// <param name="lastYear">The last year requested when the series were fetched (<c>OrtoDatasOptions.Years</c>).</param>
        /// <param name="pageSize">The number of rows read per page; a stored series is several hundred kilobytes.</param>
        /// <param name="commandTimeout">The timeout in seconds for each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>A task that represents the asynchronous operation. The task result counts the rows and images, or is null when a converter or the county parts are missing, a connection cannot be opened, or a read or write cannot run.</returns>
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

                // A row this walk rewrites gets a new position at the end of the heap, ahead of the walk, and is read again.
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

                    Dictionary<long, GIS.Classes.OrtoDatas> ortoDatas_Changed = [];
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

                        if (keyValuePair.Value.ToJsonObject()?.ToJsonString() != ortoDatas_Redated.ToJsonObject()?.ToJsonString())
                        {
                            ortoDatas_Changed[keyValuePair.Key] = ortoDatas_Redated;
                        }
                    }

                    if (ortoDatas_Changed.Count != 0)
                    {
                        long? written = await OrtoDatasPostgreSQLConverter.UpdateObjectsAsync(npgsqlConnection_Write, countyId, ortoDatas_Changed, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                        if (written is null)
                        {
                            return null;
                        }

                        changed += written.Value;
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
