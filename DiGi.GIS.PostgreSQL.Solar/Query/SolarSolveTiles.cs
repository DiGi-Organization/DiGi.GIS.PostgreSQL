using DiGi.GIS.PostgreSQL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiGi.GIS.PostgreSQL.Solar
{
    public static partial class Query
    {
        /// <summary>
        /// Puts the buildings of a county part in the order the batch solar radiation solve walks them, and cuts the walk at <paramref name="buildingCountMax"/> buildings.
        /// <para>Buildings are grouped into square tiles of <paramref name="tileSize"/> by their centroid; tiles go in row order (by Y, then by X) and the references of a tile in ordinal order, so a re-run walks the same batches. The cut is made after ordering: the buildings kept are the first ones of the walk, a compact cluster, and the last tile kept may be partial (DiGi.GIS.PostgreSQL#138).</para>
        /// <para>A centroid that is null, has no reference or has a non-finite coordinate is stepped over; a repeated reference is kept once, at its first occurrence.</para>
        /// </summary>
        /// <param name="building2DCentroids">The centroids of the buildings to solve.</param>
        /// <param name="tileSize">The edge length of a tile, in m. Values below 1 are treated as 1.</param>
        /// <param name="buildingCountMax">The largest number of buildings kept in all. Null keeps every building; values below 1 are treated as 1.</param>
        /// <returns>The references of each tile in walk order, or null when <paramref name="building2DCentroids"/> is null.</returns>
        public static List<List<string>>? SolarSolveTiles(this IEnumerable<Building2DCentroid?>? building2DCentroids, double tileSize, int? buildingCountMax = null)
        {
            if (building2DCentroids is null)
            {
                return null;
            }

            double tileSize_Valid = double.IsFinite(tileSize) ? Math.Max(1, tileSize) : 1;
            int countMax = buildingCountMax is int buildingCountMax_Value ? Math.Max(1, buildingCountMax_Value) : int.MaxValue;

            HashSet<string> references = [];
            List<(long X, long Y, string Reference)> keys = [];
            foreach (Building2DCentroid? building2DCentroid in building2DCentroids)
            {
                string? reference = building2DCentroid?.Reference;
                if (building2DCentroid is null || string.IsNullOrWhiteSpace(reference) || !double.IsFinite(building2DCentroid.X) || !double.IsFinite(building2DCentroid.Y) || !references.Add(reference!))
                {
                    continue;
                }

                keys.Add(((long)Math.Floor(building2DCentroid.X / tileSize_Valid), (long)Math.Floor(building2DCentroid.Y / tileSize_Valid), reference!));
            }

            List<List<string>> result = [];
            int count = 0;
            foreach (IGrouping<(long, long), (long X, long Y, string Reference)> tile in keys.GroupBy(x => (x.X, x.Y)).OrderBy(x => x.Key.Item2).ThenBy(x => x.Key.Item1))
            {
                if (count >= countMax)
                {
                    break;
                }

                List<string> references_Tile = [.. tile.Select(x => x.Reference).OrderBy(x => x, StringComparer.Ordinal).Take(countMax - count)];
                count += references_Tile.Count;
                result.Add(references_Tile);
            }

            return result;
        }
    }
}
