namespace BitMono.Core.Protecting.Analyzing.DnlibDefs;

public class DnlibDefSpecificNamespaceCriticalAnalyzer : ICriticalAnalyzer<IMemberDefinition>
{
    private readonly IConfiguration m_Configuration;

    public DnlibDefSpecificNamespaceCriticalAnalyzer(IBitMonoObfuscationConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(IMemberDefinition memberDefinition)
    {
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.SpecificNamespacesObfuscationOnly)) == false)
        {
            return true;
        }

        var specificNamespaces = m_Configuration.GetSpecificNamespaces();
        if (memberDefinition is TypeDefinition typeDefinition && typeDefinition.HasNamespace()) 
        {
            if (specificNamespaces.Any(s => s.Equals(typeDefinition.Namespace.Value)) == false)
            {
                return false;
            }
        }
        if (memberDefinition is MethodDefinition methodDefinition && methodDefinition.DeclaringType.HasNamespace())
        {
            if (specificNamespaces.Any(s => s.Equals(methodDefinition.DeclaringType.Namespace.Value)) == false)
            {
                return false;
            }
        }
        if (memberDefinition is FieldDefinition fieldDefinition && fieldDefinition.DeclaringType.HasNamespace())
        {
            if (specificNamespaces.Any(s => s.Equals(fieldDefinition.DeclaringType.Namespace.Value)) == false)
            {
                return false;
            }
        }
        return true;
    }
}