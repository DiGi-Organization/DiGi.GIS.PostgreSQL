using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Asynchronously reads the building models of every building whose footprint bounding box lies within or intersects a circle, with the county part each one is filed under.
        /// <para>The same selection as the Web API's <c>gis/buildingmodel/itemsbycircle</c>, which the solar radiation solve of the 3D viewers takes its neighbours from: the buildings are found in <c>building_2d</c> (Main database) and their models read from <c>building_model</c> (Storage database), one statement per county part the circle reaches - no join across the two databases (<c>Coding - PostgreSQL.md</c> §6). Unlike that route, the models keep the bare <c>building_2d</c> reference in <c>BuildingModelParameter.Reference</c>, the key stored solar radiation and <c>building_data</c> are held under.</para>
        /// <para>A building without a stored model is simply absent; a county part whose read fails throws, because a partial set of neighbours would understate the shading.</para>
        /// </summary>
        /// <param name="building2DPostgreSQLConverter">The converter that finds the buildings in <c>building_2d</c>.</param>
        /// <param name="buildingModelPostgreSQLConverter">The converter that reads their models from <c>building_model</c>.</param>
        /// <param name="circle2D">The circle, in EPSG:2180.</param>
        /// <param name="tolerance">The distance by which the circle is widened, in m.</param>
        /// <param name="commandTimeout">The timeout in seconds for the execution of each command. A value of 0 disables the timeout.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains each model with the county part it was read from, empty when no building lies in the circle, or null when an input is missing or the buildings could not be found.</returns>
        public static async Task<List<(int CountyId, DiGi.Analytical.Building.Classes.BuildingModel BuildingModel)>?> BuildingModelsByCircle2DAsync(this Building2DPostgreSQLConverter? building2DPostgreSQLConverter, BuildingModelPostgreSQLConverter? buildingModelPostgreSQLConverter, Circle2D? circle2D, double tolerance = Core.Constants.Tolerance.Distance, int commandTimeout = 30, CancellationToken cancellationToken = default)
        {
            if (building2DPostgreSQLConverter is null || buildingModelPostgreSQLConverter is null || circle2D is null)
            {
                return null;
            }

            List<Building2DReference>? building2DReferences = await building2DPostgreSQLConverter.GetBuilding2DReferencesByCircle2DAsync(circle2D, tolerance, commandTimeout, cancellationToken);
            if (building2DReferences is null)
            {
                return null;
            }

            List<(int CountyId, DiGi.Analytical.Building.Classes.BuildingModel BuildingModel)> result = [];

            // building_model is partitioned by county part, and the part is a mandatory filter: one read per part.
            foreach (IGrouping<int, Building2DReference> grouping in building2DReferences.Where(x => x.CountyId is not null && !string.IsNullOrWhiteSpace(x.Reference)).GroupBy(x => x.CountyId!.Value).OrderBy(x => x.Key))
            {
                List<string> references = [.. grouping.Select(x => x.Reference!).Distinct()];

                List<BuildingModel>? buildingModels = await buildingModelPostgreSQLConverter.GetItemsByReferencesAsync(references, grouping.Key, null, true, commandTimeout: commandTimeout, cancellationToken: cancellationToken);
                if (buildingModels is null)
                {
                    continue;
                }

                foreach (BuildingModel? buildingModel in buildingModels)
                {
                    if (buildingModel?.ToDiGi() is DiGi.Analytical.Building.Classes.BuildingModel buildingModel_DiGi)
                    {
                        result.Add((grouping.Key, buildingModel_DiGi));
                    }
                }
            }

            return result;
        }
    }
}
