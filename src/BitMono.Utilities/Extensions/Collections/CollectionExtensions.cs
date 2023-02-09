namespace BitMono.Utilities.Extensions.Collections;

public static class CollectionExtensions
{
    public static List<T> Swap<T>(this List<T> source, int index1, int index2)
    {
        (source[index1], source[index2]) = (source[index2], source[index1]);
        return source;
    }
}