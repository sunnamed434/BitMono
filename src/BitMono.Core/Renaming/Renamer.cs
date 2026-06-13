namespace BitMono.Core.Renaming;

[SuppressMessage("ReSharper", "InvertIf")]
public class Renamer
{
    private readonly NameCriticalAnalyzer _nameCriticalAnalyzer;
    private readonly SpecificNamespaceCriticalAnalyzer _specificNamespaceCriticalAnalyzer;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly RandomNext _randomNext;

    public Renamer(
        NameCriticalAnalyzer nameCriticalAnalyzer,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer,
        ObfuscationSettings obfuscationSettings,
        RandomNext randomNext)
    {
        _nameCriticalAnalyzer = nameCriticalAnalyzer;
        _specificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
        _obfuscationSettings = obfuscationSettings;
        _randomNext = randomNext;
    }

    public string RenameUnsafely()
    {
        var strings = _obfuscationSettings.RandomStrings!;
        var randomStringOne = strings[_randomNext(0, strings.Length - 1)] + " " + strings[_randomNext(0, strings.Length - 1)];
        var randomStringTwo = strings[_randomNext(0, strings.Length - 1)];
        var randomStringThree = strings[_randomNext(0, strings.Length - 1)];
        return $"{randomStringTwo} {randomStringOne}.{randomStringThree}";
    }

    private bool ShouldRenameType(TypeDefinition? typeDefinition)
    {
        // Types in framework-reserved namespaces (e.g. PolySharp polyfills) are matched by full
        // name by the compiler/runtime and must never be renamed. See IsInReservedNamespace.
        return typeDefinition == null || (_nameCriticalAnalyzer.NotCriticalToMakeChanges(typeDefinition) && !typeDefinition.IsExplicitLayout && !typeDefinition.IsInReservedNamespace());
    }

    /// <summary>
    /// Whether <paramref name="method"/> may be safely renamed according to the same
    /// safety gate that <see cref="Rename(IMetadataMember)"/> applies (critical-name and
    /// declaring-type checks). Lets callers that need a single shared name across several
    /// related members decide atomically before renaming any of them.
    /// </summary>
    public bool CanRename(MethodDefinition method)
    {
        return _nameCriticalAnalyzer.NotCriticalToMakeChanges(method) && ShouldRenameType(method.DeclaringType);
    }

    public void Rename(IMetadataMember member)
    {
        if (member is TypeDefinition type)
        {
            if (ShouldRenameType(type) && ShouldRenameType(type.DeclaringType))
            {
                type.Name = RenameUnsafely();
            }
        }
        if (member is MethodDefinition method)
        {
            if (_nameCriticalAnalyzer.NotCriticalToMakeChanges(method) && ShouldRenameType(method.DeclaringType))
            {
                method.Name = RenameUnsafely();
            }
        }
        if (member is FieldDefinition field)
        {
            if (!field.IsCompilerGenerated() && ShouldRenameType(field.DeclaringType))
        {
            field.Name = RenameUnsafely();
            }
        }
        if (member is ParameterDefinition parameter)
        {
            if (!parameter.IsCompilerGenerated())
        {
            parameter.Name = RenameUnsafely();
            }
        }
    }

    public void Rename(params IMetadataMember[] members)
    {
        for (var i = 0; i < members.Length; i++)
        {
            Rename(members[i]);
        }
    }

    public void RemoveNamespace(IMetadataMember member)
    {
        if (member is TypeDefinition type)
        {
            if (_specificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(type) && !type.IsInReservedNamespace())
            {
                type.Namespace = string.Empty;
            }
        }
        if (member is MethodDefinition method)
        {
            if (_nameCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                if (method.DeclaringType != null && !method.DeclaringType.IsInReservedNamespace())
                {
                    method.DeclaringType.Namespace = string.Empty;
                }
            }
        }
        if (member is FieldDefinition field)
        {
            if (field.DeclaringType != null && !field.DeclaringType.IsInReservedNamespace())
            {
                field.DeclaringType.Namespace = string.Empty;
            }
        }
    }

    public void RemoveNamespace(params IMetadataMember[] members)
    {
        members.ForEach(RemoveNamespace);
    }
}
