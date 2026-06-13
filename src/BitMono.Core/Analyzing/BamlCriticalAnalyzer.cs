using BitMono.Core.Analyzing.Baml;

namespace BitMono.Core.Analyzing;

/// <summary>
/// Treats every member whose declaring type is referenced by the assembly's compiled WPF XAML
/// (BAML) as critical, so the renamer leaves it (and its members) alone. Renaming or
/// namespace-stripping a XAML-referenced type makes the app crash at XAML load with
/// <c>XamlParseException</c>. See https://github.com/sunnamed434/BitMono/issues/212.
/// </summary>
public class BamlCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    private readonly WpfBamlContextAccessor _bamlContextAccessor;

    public BamlCriticalAnalyzer(WpfBamlContextAccessor bamlContextAccessor)
    {
        _bamlContextAccessor = bamlContextAccessor;
    }

    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        var context = _bamlContextAccessor.GetContext();
        if (context == null || context.XamlTypes.Count == 0)
        {
            return true;
        }
        var declaringType = GetDeclaringType(member);
        return declaringType == null || !context.XamlTypes.Contains(declaringType);
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
