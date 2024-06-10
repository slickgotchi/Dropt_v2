#region

using System.Collections.Generic;

#endregion

namespace CarlosLab.Common.Extensions
{
    public static class ListExtension
    {
        public static IEnumerable<T> GetNotNullItems<T>(this List<T> list)
        {
            foreach (T item in list)
            {
                if (item != null)
                    yield return item;
            }
        }

        public static void Move<T>(this List<T> list, int sourceIndex, int destIndex)
        {
            if (sourceIndex == destIndex)
                return;

            T item = list[sourceIndex];
            list.RemoveAt(sourceIndex);
            list.Insert(destIndex, item);
        }
    }
}