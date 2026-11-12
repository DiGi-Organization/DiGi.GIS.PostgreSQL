using DiGi.GIS.Classes;
using DiGi.GIS.Enums;
using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Create
    {
        /// <summary>
        /// Draws a stratified building shape labelling sample from solved candidate footprints: each solver class is a stratum, so rare classes are present.
        /// <para>Allocation: every class receives <c>min(n_class, size / classCount)</c>, which takes a class smaller than the equal quota whole; what the small classes leave unused is spread over the classes with footprints to spare in proportion to their spare counts (largest remainder, ties to the lower enum value). The sample holds <c>min(size, candidate count)</c> items.</para>
        /// <para>The draw is deterministic for the same candidates and <paramref name="seed"/>, whatever their input order: candidates are sorted by (county id, ordinal reference) and deduplicated on that pair, each class is shuffled by one <see cref="Random"/> seeded with <paramref name="seed"/> in enum order, and the selection is shuffled once more before ordinals 0…n−1 are assigned, so the labelling order does not group by class. Candidates without a reference, and those whose solver shape is <see cref="BuildingShape.Undefined"/>, are dropped; the candidates' own sample key and ordinal are ignored.</para>
        /// </summary>
        /// <param name="buildingShapeSampleItems">The candidate footprints with their solver shape.</param>
        /// <param name="sampleKey">The identity given to every item of the sample.</param>
        /// <param name="size">The requested number of items.</param>
        /// <param name="seed">The seed of the draw.</param>
        /// <returns>The sample items in labelling order, or null when the candidates are null, the key is blank or the size is not positive.</returns>
        public static List<BuildingShapeSampleItem>? BuildingShapeSampleItems(this IEnumerable<BuildingShapeSampleItem?>? buildingShapeSampleItems, string? sampleKey, int size, int seed)
        {
            if (buildingShapeSampleItems is null || string.IsNullOrWhiteSpace(sampleKey) || size <= 0)
            {
                return null;
            }

            int Compare(BuildingShapeSampleItem buildingShapeSampleItem_1, BuildingShapeSampleItem buildingShapeSampleItem_2)
            {
                int result = buildingShapeSampleItem_1.CountyId.CompareTo(buildingShapeSampleItem_2.CountyId);
                return result != 0 ? result : string.CompareOrdinal(buildingShapeSampleItem_1.Reference, buildingShapeSampleItem_2.Reference);
            }

            void Shuffle(List<BuildingShapeSampleItem> buildingShapeSampleItems_Shuffle, Random random_Shuffle)
            {
                for (int i = buildingShapeSampleItems_Shuffle.Count - 1; i > 0; i--)
                {
                    int j = random_Shuffle.Next(i + 1);
                    (buildingShapeSampleItems_Shuffle[i], buildingShapeSampleItems_Shuffle[j]) = (buildingShapeSampleItems_Shuffle[j], buildingShapeSampleItems_Shuffle[i]);
                }
            }

            List<BuildingShapeSampleItem> buildingShapeSampleItems_Valid = [];
            foreach (BuildingShapeSampleItem? buildingShapeSampleItem in buildingShapeSampleItems)
            {
                if (buildingShapeSampleItem is null || string.IsNullOrWhiteSpace(buildingShapeSampleItem.Reference) || buildingShapeSampleItem.SolverShape == BuildingShape.Undefined)
                {
                    continue;
                }

                buildingShapeSampleItems_Valid.Add(buildingShapeSampleItem);
            }

            buildingShapeSampleItems_Valid.Sort(Compare);

            SortedDictionary<BuildingShape, List<BuildingShapeSampleItem>> dictionary = [];
            for (int i = 0; i < buildingShapeSampleItems_Valid.Count; i++)
            {
                BuildingShapeSampleItem buildingShapeSampleItem = buildingShapeSampleItems_Valid[i];
                if (i > 0 && Compare(buildingShapeSampleItems_Valid[i - 1], buildingShapeSampleItem) == 0)
                {
                    continue;
                }

                if (!dictionary.TryGetValue(buildingShapeSampleItem.SolverShape, out List<BuildingShapeSampleItem>? buildingShapeSampleItems_Class))
                {
                    buildingShapeSampleItems_Class = [];
                    dictionary[buildingShapeSampleItem.SolverShape] = buildingShapeSampleItems_Class;
                }

                buildingShapeSampleItems_Class.Add(buildingShapeSampleItem);
            }

            List<BuildingShapeSampleItem> result = [];
            if (dictionary.Count == 0)
            {
                return result;
            }

            Random random = new(seed);
            foreach (List<BuildingShapeSampleItem> buildingShapeSampleItems_Class in dictionary.Values)
            {
                Shuffle(buildingShapeSampleItems_Class, random);
            }

            int quota = size / dictionary.Count;

            Dictionary<BuildingShape, int> counts = [];
            int count_Taken = 0;
            int count_Spare = 0;
            foreach (KeyValuePair<BuildingShape, List<BuildingShapeSampleItem>> keyValuePair in dictionary)
            {
                int count = Math.Min(keyValuePair.Value.Count, quota);
                counts[keyValuePair.Key] = count;
                count_Taken += count;
                count_Spare += keyValuePair.Value.Count - count;
            }

            int count_Leftover = Math.Min(size - count_Taken, count_Spare);
            if (count_Leftover > 0)
            {
                List<(BuildingShape BuildingShape, double Remainder)> remainders = [];
                int count_Distributed = 0;
                foreach (KeyValuePair<BuildingShape, List<BuildingShapeSampleItem>> keyValuePair in dictionary)
                {
                    int spare = keyValuePair.Value.Count - counts[keyValuePair.Key];
                    if (spare == 0)
                    {
                        continue;
                    }

                    double share = (double)count_Leftover * spare / count_Spare;
                    int count = Math.Min(spare, (int)Math.Floor(share));
                    counts[keyValuePair.Key] += count;
                    count_Distributed += count;
                    remainders.Add((keyValuePair.Key, share - count));
                }

                remainders.Sort((x, y) => x.Remainder != y.Remainder ? y.Remainder.CompareTo(x.Remainder) : x.BuildingShape.CompareTo(y.BuildingShape));

                int index = 0;
                while (count_Distributed < count_Leftover && remainders.Count > 0)
                {
                    BuildingShape buildingShape = remainders[index % remainders.Count].BuildingShape;
                    if (counts[buildingShape] < dictionary[buildingShape].Count)
                    {
                        counts[buildingShape]++;
                        count_Distributed++;
                    }

                    index++;
                }
            }

            List<BuildingShapeSampleItem> buildingShapeSampleItems_Selected = [];
            foreach (KeyValuePair<BuildingShape, List<BuildingShapeSampleItem>> keyValuePair in dictionary)
            {
                buildingShapeSampleItems_Selected.AddRange(keyValuePair.Value.GetRange(0, counts[keyValuePair.Key]));
            }

            Shuffle(buildingShapeSampleItems_Selected, random);

            for (int i = 0; i < buildingShapeSampleItems_Selected.Count; i++)
            {
                BuildingShapeSampleItem buildingShapeSampleItem = buildingShapeSampleItems_Selected[i];
                result.Add(new BuildingShapeSampleItem(sampleKey, i, buildingShapeSampleItem.CountyId, buildingShapeSampleItem.Reference, buildingShapeSampleItem.SolverShape));
            }

            return result;
        }
    }
}
