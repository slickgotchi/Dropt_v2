using System;
using System.Collections.Generic;
using System.Linq;
using Chest;

namespace Chests.WeightRandom
{
    public static class WeightRandomExtensions
    {
        public static TData GetRandom<TData>(this IEnumerable<WeightVariable<TData>> source)
        {
            var random = new Random();
            var cursor = 0;
            var total = source.Sum(temp => temp.Chance);
            var actualValue = random.Next(cursor, total);

            foreach (var weightItem in source)
            {
                cursor += weightItem.Chance;

                if (cursor >= actualValue)
                {
                    return weightItem.Data;
                }
            }

            return default;
        }

        public static WeightVariable<int>[] ToWeightArray(this ActivePlayersData[] source, int playersCount)
        {
            var result = new List<WeightVariable<int>>();

            for (int i = 0; i < source.Length; i++)
            {
                if (i >= playersCount)
                {
                    break;
                }

                var subSource = source[i];

                for (int j = 0; j < subSource.Percents.Length; j++)
                {
                    var percent = subSource.Percents[j];

                    result.Add(new WeightVariable<int>(j + 1, percent));
                }
            }

            return result.ToArray();
        }
    }
}