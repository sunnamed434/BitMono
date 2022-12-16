namespace BitMono.Utilities.Extensions.dnlib;

public static class GenericExtensions
{
    public static TTokenProvider UpdateRowId<TTokenProvider>(this TTokenProvider source, ModuleDef moduleDef)
        where TTokenProvider : IMDTokenProvider
    {
        return moduleDef.UpdateRowId(source);
    }
    public static IEnumerable<TTokenProvider> UpdateRowIds<TTokenProvider>(this IEnumerable<TTokenProvider> source, ModuleDef moduleDef)
        where TTokenProvider : IMDTokenProvider
    {
        source.ForEach((d) => UpdateRowId(d, moduleDef));
        return source;
    }
}