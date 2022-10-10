using System.Collections.Generic;

namespace BitMono.Utilities.Extensions.Collections
{
    public static class EnumerableExtensions
	{
        public static IList<T> Swap<T>(this IList<T> source, int index1, int index2)
        {
            T temp = source[index1];
            source[index1] = source[index2];
            source[index2] = temp;
            return source;
        }
    }
}