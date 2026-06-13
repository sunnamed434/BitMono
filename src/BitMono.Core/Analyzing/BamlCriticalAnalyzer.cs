using BitMono.Core.Analyzing.Baml;
using BitMono.Core.Services;

namespace BitMono.Core.Analyzing;

/// <summary>
/// Treats every member whose declaring type is referenced by the assembly's compiled WPF XAML
/// (BAML) as critical, so the renamer leaves it (and its members) alone. Renaming or
/// namespace-stripping a XAML-referenced type makes the app crash at XAML load with
/// <c>XamlParseException</c>. See https://github.com/sunnamed434/BitMono/issues/212.
/// </summary>
public class BamlCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    private readonly IEngineContextAccessor _engineContextAccessor;
    private ModuleDefinition? _module;
    private HashSet<TypeDefinition>? _referencedTypes;

    public BamlCriticalAnalyzer(IEngineContextAccessor engineContextAccessor)
    {
        _engineContextAccessor = engineContextAccessor;
    }

    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        var module = _engineContextAccessor.Instance?.Module;
        if (module == null)
        {
            return true;
        }
        var referencedTypes = GetReferencedTypes(module);
        if (referencedTypes.Count == 0)
        {
            return true;
        }
        var declaringType = GetDeclaringType(member);
        return declaringType == null || referencedTypes.Contains(declaringType) == false;
    }

    private HashSet<TypeDefinition> GetReferencedTypes(ModuleDefinition module)
    {
        // Computed once per module (BAML parsing is shared across protections and members).
        if (ReferenceEquals(_module, module) == false || _referencedTypes == null)
        {
            _module = module;
            _referencedTypes = WpfBamlReferenceResolver.ResolveReferencedTypes(module);
        }
        return _referencedTypes;
    }

    private static TypeDefinition? GetDeclaringType(IMetadataMember member)
    {
        return member switch
        {
            TypeDefinition type => type,
            MethodDefinition method => method.DeclaringType,
            FieldDefinition field => field.DeclaringType,
            PropertyDefinition property => property.DeclaringType,
            EventDefinition @event => @event.DeclaringType,
            _ => null
        };
    }
}
