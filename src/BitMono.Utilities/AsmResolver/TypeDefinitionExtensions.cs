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

    // Compiler/runtime-recognised "magic" types are matched by their exact full name, so when a
    // project ships them as in-assembly polyfills/shims (e.g. PolySharp) they must keep their
    // original name AND namespace - renaming or namespace-stripping silently breaks the feature
    // they back. Real application code never lives in these framework-owned namespaces, so any
    // type defined under them is left untouched. See
    // https://github.com/sunnamed434/BitMono/issues/97.
    //
    //   System.Runtime.CompilerServices - IsExternalInit, RequiredMemberAttribute,
    //       CompilerFeatureRequiredAttribute, ModuleInitializerAttribute, SkipLocalsInitAttribute,
    //       CallerArgumentExpressionAttribute, InterpolatedStringHandler*, CallConv* markers, ...
    //   System.Diagnostics.CodeAnalysis - nullable analysis attributes (AllowNull, NotNull,
    //       MemberNotNull, ...), DynamicallyAccessedMembers, RequiresUnreferencedCode, ...
    //   System.Runtime.Versioning      - SupportedOSPlatform/UnsupportedOSPlatform,
    //       RequiresPreviewFeaturesAttribute, ...
    //   System.Runtime.InteropServices - UnmanagedCallersOnlyAttribute is resolved by the runtime
    //       by name to set up reverse P/Invoke; renaming it breaks the call.
    //   System.Diagnostics             - StackTraceHiddenAttribute is read by the runtime by name
    //       when formatting stack traces.
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