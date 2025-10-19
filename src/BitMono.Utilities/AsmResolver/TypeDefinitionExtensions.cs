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

    /// <summary>
    /// Gets the type and all its base types in the inheritance hierarchy.
    /// </summary>
    /// <param name="source">The type to get the hierarchy for.</param>
    /// <returns>An enumerable of the type and all its base types.</returns>
    public static IEnumerable<TypeDefinition> GetTypeAndBaseTypes(this TypeDefinition source)
    {
        var current = source;
        while (current != null)
        {
            yield return current;
            current = current.BaseType?.Resolve();
        }
    }
}