using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Names the subdivision whose occupancy figure a building filed under the given subdivision takes its share from: the subdivision itself when it carries a figure, otherwise the smallest of its containers that does.
        /// <para>The subdivision layer nests and the source figures are patchy at the deeper levels - in Warsaw 13 neighbourhoods carry none while their districts do - so a building in Jelonki, which has no figure, takes Bemowo's density rather than nothing (<see href="https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/80">DiGi.GIS.PostgreSQL#80</see>). An explicit zero is a figure. A subdivision with no figure anywhere up its chain of containers answers <see langword="null"/>, and its buildings stay unwritten.</para>
        /// </summary>
        /// <param name="subdivisionId">The identifier of the subdivision the building is filed under.</param>
        /// <param name="containerIds_ById">The containers of each subdivision, smallest first, as <see cref="ContainerIds(IReadOnlyDictionary{int, Geometry.Planar.Classes.PolygonalFace2D}, double)"/> gives them.</param>
        /// <param name="occupancies_BySubdivisionId">The stored figure of each subdivision, <see langword="null"/> where the source carries none.</param>
        /// <returns>The identifier of the subdivision whose figure applies, or <see langword="null"/> when neither the subdivision nor any of its containers carries one.</returns>
        public static int? OccupancySubdivisionId(int subdivisionId, IReadOnlyDictionary<int, List<int>>? containerIds_ById, IReadOnlyDictionary<int, uint?>? occupancies_BySubdivisionId)
        {
            if (occupancies_BySubdivisionId is null)
            {
                return null;
            }

            if (occupancies_BySubdivisionId.TryGetValue(subdivisionId, out uint? occupancy) && occupancy is not null)
            {
                return subdivisionId;
            }

            if (containerIds_ById is null || !containerIds_ById.TryGetValue(subdivisionId, out List<int>? containerIds) || containerIds is null)
            {
                return null;
            }

            for (int i = 0; i < containerIds.Count; i++)
            {
                if (occupancies_BySubdivisionId.TryGetValue(containerIds[i], out uint? occupancy_Container) && occupancy_Container is not null)
                {
                    return containerIds[i];
                }
            }

            return null;
        }
    }
}
