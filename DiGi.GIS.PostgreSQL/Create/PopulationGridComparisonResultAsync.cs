using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Compares the modelled building occupancy of one county partition against one census year's 1 km population grid, cell by cell (DiGi.GIS.PostgreSQL#106).
        /// <para>The compared cells are the union of two rules: the cells whose centre lies inside the county outline, and the cells holding at least one of the county's building centres. The first keeps the cells the census fills but the model does not reach - a border cell whose populated half belongs to the neighbour, or a cell of unoccupied buildings - so their modelled sum of zero stays a real error rather than a silent omission; the second keeps every cell the county's buildings actually land in, including a border cell whose centre falls outside the outline. County buildings whose centre lies in no stored cell (a masked or absent cell, or outside the grid's coverage) are counted in <see cref="PopulationGridComparisonResult.BuildingOutsideCellCount"/> instead of being dropped.</para>
        /// <para>Reads run sequentially on their own connections without fanning out per building: occupancy arrives in batches of one thousand references through <c>Building2DReferencedObjectPostgreSQLConverter.GetItemsByReferencesAsync</c>, and the cell join runs in memory over the bounding-box prefiltered cells - the schema has no PostGIS, so the point-in-cell tests are C# geometry tests of the same kind <see cref="Query.IdsByPoint2Ds(IDictionary{int, PolygonalFace2D}?, IReadOnlyList{Point2D}?, double)"/> makes.</para>
        /// <para>Cells hidden by statistical secrecy carry no census value: they are compared as rows with a null error and a <see cref="Enums.PopulationDensityClass.Masked"/> class, counted in <see cref="PopulationGridComparisonResult.MaskedCellCount"/> and left out of every metric. The metrics themselves are <see cref="Query.Mae(IReadOnlyList{PopulationGridComparisonItem}?)"/>, <see cref="Query.Bias(IReadOnlyList{PopulationGridComparisonItem}?)"/> and <see cref="Query.R2(IReadOnlyList{PopulationGridComparisonItem}?)"/>.</para>
        /// </summary>
        /// <param name="building2DPostgreSQLConverter">The converter reading the building centres.</param>
        /// <param name="building2DOccupancyDataPostgreSQLConverter">The converter reading the stored per-building occupancy.</param>
        /// <param name="populationGridPostgreSQLConverter">The converter reading the census grid.</param>
        /// <param name="administrativeAreal2DPostgreSQLConverter">The converter reading the county outline.</param>
        /// <param name="countyId">The identifier of the county partition to compare - an identifier, never a TERYT code, which names every part of a multi-part county at once.</param>
        /// <param name="censusYear">The census year of the grid to compare against (for example 2021).</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout. Defaults to 600 seconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the comparison, or null when a converter, the county outline or a table is missing, or when the county cannot be read.</returns>
        public static async Task<PopulationGridComparisonResult?> PopulationGridComparisonResultAsync(
            this Building2DPostgreSQLConverter? building2DPostgreSQLConverter,
            Building2DOccupancyDataPostgreSQLConverter? building2DOccupancyDataPostgreSQLConverter,
            PopulationGridPostgreSQLConverter? populationGridPostgreSQLConverter,
            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter,
            int countyId,
            int censusYear,
            int commandTimeout = 600,
            CancellationToken cancellationToken = default)
        {
            if (building2DPostgreSQLConverter is null || building2DOccupancyDataPostgreSQLConverter is null || populationGridPostgreSQLConverter is null || administrativeAreal2DPostgreSQLConverter is null)
            {
                return null;
            }

            try
            {
                // The county outline and its box: the outline decides which cells belong to the comparison,
                // and the box is what the grid read prefilters on.
                AdministrativeAreal2D? county = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DByIdAsync(countyId, commandTimeout, cancellationToken);
                if (county?.ToDiGi()?.PolygonalFace2D is not PolygonalFace2D countyPolygonalFace2D || countyPolygonalFace2D.GetBoundingBox() is not BoundingBox2D countyBoundingBox2D)
                {
                    return null;
                }

                List<Building2DCentroid>? building2DCentroids = await building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(countyId, subdivisionIds: null, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (building2DCentroids is null)
                {
                    return null;
                }

                List<PopulationGridCell>? populationGridCells = await populationGridPostgreSQLConverter.GetCellsByBoundingBox2DAsync(countyBoundingBox2D, censusYear, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (populationGridCells is null)
                {
                    return null;
                }

                List<PopulationGridCell> populationGridCells_Valid = [];
                Dictionary<int, PolygonalFace2D> polygonalFace2Ds_Cell = [];
                foreach (PopulationGridCell? populationGridCell in populationGridCells)
                {
                    if (populationGridCell?.PolygonalFace2D is not PolygonalFace2D polygonalFace2D)
                    {
                        continue;
                    }

                    polygonalFace2Ds_Cell[populationGridCells_Valid.Count] = polygonalFace2D;
                    populationGridCells_Valid.Add(populationGridCell);
                }

                // The occupancy of every county building, read in batches over one connection: a reference is
                // unique per county part, and the first record of a reference wins - the reads are ordered newest
                // first, so the first record is the most recently written one.
                HashSet<string> references_Seen = [];
                List<string> references = [];
                foreach (Building2DCentroid building2DCentroid in building2DCentroids)
                {
                    if (building2DCentroid?.Reference is string reference && !string.IsNullOrWhiteSpace(reference) && references_Seen.Add(reference))
                    {
                        references.Add(reference);
                    }
                }

                Dictionary<string, uint> occupancies = [];
                const int batchSize = 1000;

                await using NpgsqlConnection? npgsqlConnection = DiGi.PostgreSQL.Create.NpgsqlConnection(building2DOccupancyDataPostgreSQLConverter.ConnectionData);
                if (npgsqlConnection is null)
                {
                    return null;
                }

                await npgsqlConnection.OpenAsync(cancellationToken);

                for (int offset = 0; offset < references.Count; offset += batchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    List<string> references_Batch = references.GetRange(offset, System.Math.Min(batchSize, references.Count - offset));
                    List<Building2DOccupancyData>? building2DOccupancyDatas = await building2DOccupancyDataPostgreSQLConverter.GetItemsByReferencesAsync(npgsqlConnection, references_Batch, countyId, fallbackByReference: true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                    if (building2DOccupancyDatas is null)
                    {
                        return null;
                    }

                    foreach (Building2DOccupancyData? building2DOccupancyData in building2DOccupancyDatas)
                    {
                        if (building2DOccupancyData?.Reference is string reference && !occupancies.ContainsKey(reference) && building2DOccupancyData.ToDiGi() is DiGi.GIS.Classes.OccupancyData occupancyData)
                        {
                            occupancies[reference] = occupancyData.Occupancy ?? 0;
                        }
                    }
                }

                // The building centres, in centroid order, decided against the cells once.
                List<Point2D> point2Ds = new(building2DCentroids.Count);
                foreach (Building2DCentroid building2DCentroid in building2DCentroids)
                {
                    point2Ds.Add(new Point2D(building2DCentroid.X, building2DCentroid.Y));
                }

                int?[] indexes_ByBuilding = polygonalFace2Ds_Cell.IdsByPoint2Ds(point2Ds) ?? new int?[point2Ds.Count];

                // Rule (a): the cells whose centre lies inside the county outline.
                HashSet<int> indexes_Selected = [];
                foreach (KeyValuePair<int, PolygonalFace2D> keyValuePair in polygonalFace2Ds_Cell)
                {
                    if (keyValuePair.Value.GetBoundingBox() is BoundingBox2D boundingBox2D && boundingBox2D.GetCentroid() is Point2D point2D && Building2DPostgreSQLConverter.IsInside(countyPolygonalFace2D, countyBoundingBox2D, point2D, Core.Constants.Tolerance.MacroDistance))
                    {
                        indexes_Selected.Add(keyValuePair.Key);
                    }
                }

                // Rule (b): the cells holding at least one county building, whatever the outline says - and the
                // buildings that landed in no stored cell, which are reported rather than dropped.
                int buildingOutsideCellCount = 0;
                foreach (int? index in indexes_ByBuilding)
                {
                    if (index is not int index_Matched)
                    {
                        buildingOutsideCellCount++;
                        continue;
                    }

                    indexes_Selected.Add(index_Matched);
                }

                long[] modelledPopulations = new long[populationGridCells_Valid.Count];
                int[] buildingCounts = new int[populationGridCells_Valid.Count];
                for (int i = 0; i < indexes_ByBuilding.Length; i++)
                {
                    if (indexes_ByBuilding[i] is not int index || !indexes_Selected.Contains(index) || i >= building2DCentroids.Count)
                    {
                        continue;
                    }

                    buildingCounts[index]++;
                    if (building2DCentroids[i]?.Reference is string reference && occupancies.TryGetValue(reference, out uint occupancy))
                    {
                        modelledPopulations[index] += occupancy;
                    }
                }

                List<PopulationGridComparisonItem> populationGridComparisonItems = [];
                int comparedCellCount = 0;
                int maskedCellCount = 0;
                foreach (int index in indexes_Selected)
                {
                    PopulationGridCell populationGridCell = populationGridCells_Valid[index];
                    int? censusPopulation = populationGridCell.Population;
                    long modelledPopulation = modelledPopulations[index];

                    if (censusPopulation is null)
                    {
                        maskedCellCount++;
                    }
                    else
                    {
                        comparedCellCount++;
                    }

                    long? error = censusPopulation is int censusPopulation_Value ? modelledPopulation - censusPopulation_Value : null;
                    populationGridComparisonItems.Add(new PopulationGridComparisonItem(populationGridCell.CellId, censusPopulation, modelledPopulation, error, buildingCounts[index], Create.PopulationDensityClass(censusPopulation)));
                }

                // Ordered by cell identifier, so the same inputs serialise to the same document.
                populationGridComparisonItems.Sort((x, y) => string.CompareOrdinal(x?.CellId, y?.CellId));

                return new PopulationGridComparisonResult(
                    countyId,
                    censusYear,
                    Query.Mae(populationGridComparisonItems),
                    Query.Bias(populationGridComparisonItems),
                    Query.R2(populationGridComparisonItems),
                    comparedCellCount,
                    maskedCellCount,
                    buildingOutsideCellCount,
                    populationGridComparisonItems);
            }
            catch (PostgresException postgresException) when (postgresException.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                // A fresh database is a supported state: a comparison against tables that do not exist yet
                // answers null rather than 42P01 (Coding - PostgreSQL.md §4).
                return null;
            }
        }
    }
}
