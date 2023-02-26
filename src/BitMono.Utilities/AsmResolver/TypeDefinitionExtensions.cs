namespace BitMono.Utilities.AsmResolver;

public static class TypeDefinitionExtensions
{
    public static bool HasBaseType(this TypeDefinition source)
    {
        return source.BaseType != null;
    }
    public static bool HasNamespace(this TypeDefinition source)
    {
        return Utf8String.IsNullOrEmpty(source.Namespace) == false;
    }
}