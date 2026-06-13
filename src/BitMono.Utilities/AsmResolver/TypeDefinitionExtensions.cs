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

    // Framework-owned namespaces that hold compiler/runtime "magic" types matched by full name
    // (e.g. PolySharp polyfills). User code never lives here, so any type defined under them is left
    // unrenamed/unstripped - renaming would break the feature it backs. See #97.
    private static readonly string[] ReservedNamespaces =
    {
        "System.Runtime.CompilerServices",
        "System.Diagnostics.CodeAnalysis",
        "System.Runtime.Versioning",
        "System.Runtime.InteropServices",
        "System.Diagnostics",
    };

    /// <summary>
    /// Whether the type lives in a framework-owned namespace that holds compiler/runtime
    /// "magic" types matched by full name (e.g. PolySharp polyfills). Such types must never be
    /// renamed nor have their namespace stripped.
    /// </summary>
    public static bool IsInReservedNamespace(this TypeDefinition? source)
    {
        var @namespace = source?.Namespace?.Value;
        if (string.IsNullOrEmpty(@namespace))
        {
            return false;
        }
        return Array.IndexOf(ReservedNamespaces, @namespace) != -1;
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
            current = current.BaseType?.ResolveOrNull();
        }
    }
}