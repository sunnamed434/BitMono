namespace BitMono.Utilities.Extensions.AsmResolver;

public static class GenericExtensions
{
    public static TMetadataMember AssignNextAvailableToken<TMetadataMember>(this TMetadataMember source, ModuleDefinition moduleDefinition)
        where TMetadataMember : MetadataMember
    {
        moduleDefinition.TokenAllocator.AssignNextAvailableToken(source);
        return source;
    }
}