namespace BitMono.Core.Analyzing;

public class SpecificNamespaceCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    private readonly ObfuscationSettings _obfuscationSettings;

    public SpecificNamespaceCriticalAnalyzer(IOptions<ObfuscationSettings> obfuscation)
    {
        _obfuscationSettings = obfuscation.Value;
    }

    private static string? GetNamespace(IMetadataMember member)
    {
        if (member is TypeDefinition type)
        {
            return type.Namespace?.Value;
        }
        if (member is MethodDefinition method)
        {
            return method.DeclaringType?.Namespace?.Value;
        }
        if (member is FieldDefinition field)
        {
            return field.DeclaringType?.Namespace?.Value;
        }
        return null;
    }

    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        if (!_obfuscationSettings.SpecificNamespacesObfuscationOnly)
        {
            return true;
        }

        string[] specificNamespaces = _obfuscationSettings.SpecificNamespaces!;
        string ns = GetNamespace(member) ?? string.Empty;
        return Array.IndexOf(specificNamespaces, ns) != -1;
    }
}