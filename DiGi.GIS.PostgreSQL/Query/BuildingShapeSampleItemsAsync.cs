using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Asynchronously draws a stratified building shape labelling sample from the footprints of the given county parts.
        /// <para>Every <c>building_2d</c> footprint of each county id (a part id, not a county code) is read and classified with a default <see cref="BuildingShapeSolver"/>; the solved footprints then go through <see cref="Create.BuildingShapeSampleItems(IEnumerable{BuildingShapeSampleItem}, string, int, int)"/>, which stratifies by solver class and fixes the labelling order. Footprints the solver cannot classify are dropped. Each item keeps the county id its row is filed under, since a reference is unique only within its county part. Nothing is written: the caller stores the result with <see cref="BuildingShapePostgreSQLConverter.InsertSampleAsync(IEnumerable{BuildingShapeSampleItem}, int, int, CancellationToken)"/>.</para>
        /// <para>The solver runs once per footprint, so the time grows with the county populations (a whole county part per read; tens of thousands of footprints each).</para>
        /// </summary>
        /// <param name="building2DPostgreSQLConverter">The converter reading <c>building_2d</c> on the Main database.</param>
        /// <param name="sampleKey">The identity given to every item of the sample.</param>
        /// <param name="countyIds">The county part ids to draw from.</param>
        /// <param name="size">The requested number of items.</param>
        /// <param name="seed">The seed of the draw.</param>
        /// <param name="commandTimeout">The timeout in seconds for each county read. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the sample items in labelling order, or null when the converter or the county ids are null, the key is blank or the size is not positive.</returns>
        public static async Task<List<BuildingShapeSampleItem>?> BuildingShapeSampleItemsAsync(this Building2DPostgreSQLConverter? building2DPostgreSQLConverter, string? sampleKey, IEnumerable<int>? countyIds, int size = 500, int seed = 12, int commandTimeout = 600, CancellationToken cancellationToken = default)
        {
            if (building2DPostgreSQLConverter is null || countyIds is null || string.IsNullOrWhiteSpace(sampleKey) || size <= 0)
            {
                return null;
            }

            List<BuildingShapeSampleItem> buildingShapeSampleItems = [];
            foreach (int countyId in new SortedSet<int>(countyIds))
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<Classes.Building2D>? building2Ds = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(countyId, subdivisionIds: null, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (building2Ds is null)
                {
                    continue;
                }

                foreach (Classes.Building2D building2D in building2Ds)
                {
                    if (string.IsNullOrWhiteSpace(building2D.Reference) || building2D.ToDiGi() is not GIS.Classes.Building2D building2D_GIS)
                    {
                        continue;
                    }

                    BuildingShapeSolver buildingShapeSolver = new()
                    {
                        Input = building2D_GIS
                    };

                    bool solved;
                    try
                    {
                        solved = buildingShapeSolver.Solve();
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        // A footprint whose geometry breaks the solver cannot be sampled; it is dropped like an unclassified one.
                        Serilog.Modify.Log(Serilog.Enums.LogEventLevel.Warning, "{Method}: footprint {Reference} of county part {CountyId} dropped, the solver failed: {Message}", nameof(BuildingShapeSampleItemsAsync), building2D.Reference, countyId, exception.Message);
                        continue;
                    }

                    if (!solved || buildingShapeSolver.Output == BuildingShape.Undefined)
                    {
                        continue;
                    }

                    buildingShapeSampleItems.Add(new BuildingShapeSampleItem(sampleKey, 0, building2D.CountyId ?? countyId, building2D.Reference, buildingShapeSolver.Output));
                }
            }

            return Create.BuildingShapeSampleItems(buildingShapeSampleItems, sampleKey, size, seed);
        }
    }
}
