namespace BitMono.Core.Protecting.Analyzing;

public class SpecificNamespaceCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    private readonly Obfuscation m_Obfuscation;

    public SpecificNamespaceCriticalAnalyzer(IOptions<Obfuscation> obfuscation)
    {
        m_Obfuscation = obfuscation.Value;
    }

    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        if (m_Obfuscation.SpecificNamespacesObfuscationOnly == false)
        {
            return true;
        }
        var specificNamespaces = m_Obfuscation.SpecificNamespaces;
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