namespace BitMono.Utilities.Extensions.AsmResolver;

public static class GenericExtensions
{
    public static TMetadataMember AssignNextAvaliableToken<TMetadataMember>(this TMetadataMember source, ModuleDefinition moduleDefinition)
        where TMetadataMember : MetadataMember
    {
        moduleDefinition.TokenAllocator.AssignNextAvailableToken(source);
        return source;
    }
    public static IEnumerable<TMetadataMember> AssingNextAvaliableTokens<TMetadataMember>(this IEnumerable<TMetadataMember> source, ModuleDefinition moduleDefinition)
        where TMetadataMember : MetadataMember
    {
        source.ForEach((d) => AssignNextAvaliableToken(d, moduleDefinition));
        return source;
    }
}