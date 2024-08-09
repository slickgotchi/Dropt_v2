using System.Linq;

namespace Dropt.Utils
{
    public static class ArrayExtensions
    {
        public static TElement[] RemoveAt<TElement>(this TElement[] source, int indexToRemove)
        {
            return source.Where((_, index) => index != indexToRemove).ToArray();
        }
    }
}