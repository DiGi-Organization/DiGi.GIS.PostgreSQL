using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Counts how many of the given buildings a spatial read of their own surroundings brought back.
        /// <para>The surroundings of a set of buildings are read over the area those buildings cover, so every one of them is inside it and a correct read returns all of them. What comes back short says which of two different things went wrong, and they are not answered by the same fix: none of them back means the read is not reaching the partition they are filed under - a county whose territory is disconnected is one row per polygon part, and pruning to the wrong part answers an empty set rather than an error. Some of them back is per building instead: <c>min_x</c> to <c>max_y</c> are nullable and the overlap test is made on them, so a building whose stored box is missing drops out of its own neighbourhood.</para>
        /// <para>Matched on county and reference together, because a reference is unique only within a county.</para>
        /// <para>See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/64.</para>
        /// </summary>
        /// <param name="building2Ds">The buildings the surroundings were read for. May be <see langword="null"/>.</param>
        /// <param name="building2Ds_Neighbour">The surroundings that came back. May be <see langword="null"/>.</param>
        /// <param name="countyId">The county part the buildings are filed under.</param>
        /// <returns>The number of <paramref name="building2Ds"/> present in <paramref name="building2Ds_Neighbour"/>.</returns>
        public static int SubjectCount(IEnumerable<Building2D>? building2Ds, IEnumerable<Building2D>? building2Ds_Neighbour, int countyId)
        {
            if (building2Ds is null || building2Ds_Neighbour is null)
            {
                return 0;
            }

            HashSet<string> references = [];
            foreach (Building2D building2D in building2Ds)
            {
                if (building2D?.Reference is string reference && !string.IsNullOrWhiteSpace(reference))
                {
                    references.Add(reference);
                }
            }

            if (references.Count == 0)
            {
                return 0;
            }

            HashSet<string> references_Found = [];
            foreach (Building2D building2D_Neighbour in building2Ds_Neighbour)
            {
                if (building2D_Neighbour?.CountyId != countyId)
                {
                    continue;
                }

                if (building2D_Neighbour.Reference is string reference_Neighbour && references.Contains(reference_Neighbour))
                {
                    references_Found.Add(reference_Neighbour);
                }
            }

            return references_Found.Count;
        }
    }
}
