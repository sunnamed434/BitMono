namespace BitMono.Utilities.AsmResolver;

public static class TypeDescriptorExtensions
{
    private static readonly string SystemTypeNamespace = typeof(Type).Namespace;
    private static readonly string SystemTypeName = nameof(Type);
    
    public static bool IsSystemType(this ITypeDefOrRef source)
    {
        return source.IsTypeOf(SystemTypeNamespace, SystemTypeName);
    }
}