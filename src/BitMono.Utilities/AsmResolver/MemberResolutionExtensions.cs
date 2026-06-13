namespace BitMono.Utilities.AsmResolver;

/// <summary>
/// Resolution helpers that preserve the pre-6.0 AsmResolver behavior of returning
/// <c>null</c> on failure.
/// <para>
/// AsmResolver 6.0 removed the parameterless <c>Resolve()</c> in favor of
/// <c>Resolve(RuntimeContext)</c> (which throws) and routes all metadata resolution
/// through a <see cref="RuntimeContext"/>. These helpers resolve a member against the
/// runtime context of the module it lives in (<see cref="IModuleProvider.ContextModule"/>),
/// mirroring the old null-on-failure semantics the codebase relies on.
/// </para>
/// </summary>
public static class MemberResolutionExtensions
{
    public static TypeDefinition? ResolveOrNull(this ITypeDefOrRef? type)
    {
        if (type == null)
        {
            return null;
        }
        return type.Resolve(type.ContextModule?.RuntimeContext, out var definition) == ResolutionStatus.Success
            ? definition as TypeDefinition
            : null;
    }

    public static MethodDefinition? ResolveOrNull(this IMethodDescriptor? method)
    {
        if (method == null)
        {
            return null;
        }
        return method.Resolve(method.ContextModule?.RuntimeContext, out var definition) == ResolutionStatus.Success
            ? definition as MethodDefinition
            : null;
    }

    public static FieldDefinition? ResolveOrNull(this IFieldDescriptor? field)
    {
        if (field == null)
        {
            return null;
        }
        return field.Resolve(field.ContextModule?.RuntimeContext, out var definition) == ResolutionStatus.Success
            ? definition as FieldDefinition
            : null;
    }
}
