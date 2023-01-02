using BitMono.Core.Extensions;

namespace BitMono.Core.Protecting.Analyzing;

public class SpecificNamespaceCriticalAnalyzer : ICriticalAnalyzer<IMetadataMember>
{
    private readonly IConfiguration m_Configuration;

    public SpecificNamespaceCriticalAnalyzer(IBitMonoObfuscationConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(IMetadataMember member)
    {
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.SpecificNamespacesObfuscationOnly)) == false)
        {
            return true;
        }

        var specificNamespaces = m_Configuration.GetSpecificNamespaces();
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