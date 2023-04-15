namespace BitMono.Core.Analyzing;

public class SpecificNamespaceCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    private readonly ObfuscationSettings _obfuscationSettings;

    public SpecificNamespaceCriticalAnalyzer(IOptions<ObfuscationSettings> obfuscation)
    {
        _obfuscationSettings = obfuscation.Value;
    }

    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        if (_obfuscationSettings.SpecificNamespacesObfuscationOnly == false)
        {
            return true;
        }

        var specificNamespaces = _obfuscationSettings.SpecificNamespaces;
        if (member is TypeDefinition type && type.HasNamespace())
        {
            if (specificNamespaces.Any(s => s.Equals(type.Namespace.Value)) == false)
            {
                return false;
            }
        }
        if (member is MethodDefinition method && method.DeclaringType.HasNamespace())
        {
            if (specificNamespaces.Any(s => s.Equals(method.DeclaringType.Namespace.Value)) == false)
            {
                return false;
            }
        }
        if (member is FieldDefinition field && field.DeclaringType.HasNamespace())
        {
            if (specificNamespaces.Any(s => s.Equals(field.DeclaringType.Namespace.Value)) == false)
            {
                return false;
            }
        }
        return true;
    }
}