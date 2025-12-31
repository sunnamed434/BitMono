namespace BitMono.Shared.Extensions;

public static class CollectionExtensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            ICollection<T> collection => collection.Count == 0,
            IReadOnlyCollection<T> readOnly => readOnly.Count == 0,
            _ => !source.Any()
        };
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source is null || source.IsEmpty();
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}
