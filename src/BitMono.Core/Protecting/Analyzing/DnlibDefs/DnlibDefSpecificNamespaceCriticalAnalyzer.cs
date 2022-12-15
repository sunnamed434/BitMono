namespace BitMono.Core.Protecting.Analyzing.DnlibDefs;

public class DnlibDefSpecificNamespaceCriticalAnalyzer : ICriticalAnalyzer<IDnlibDef>
{
    private readonly IConfiguration m_Configuration;

    public DnlibDefSpecificNamespaceCriticalAnalyzer(IBitMonoObfuscationConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(IDnlibDef dnlibDef)
    {
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.SpecificNamespacesObfuscationOnly)) == false)
        {
            return true;
        }

        var specificNamespaces = m_Configuration.GetSpecificNamespaces();
        if (dnlibDef is TypeDef typeDef && typeDef.HasNamespace()) 
        {
            if (specificNamespaces.Any(s => s.Equals(typeDef.Namespace.String)) == false)
            {
                return false;
            }
        }
        if (dnlibDef is MethodDef methodDef && methodDef.DeclaringType.HasNamespace())
        {
            if (specificNamespaces.Any(s => s.Equals(methodDef.DeclaringType.Namespace.String)) == false)
            {
                return false;
            }
        }
        if (dnlibDef is FieldDef fieldDef && fieldDef.DeclaringType.HasNamespace())
        {
            if (specificNamespaces.Any(s => s.Equals(fieldDef.DeclaringType.Namespace.String)) == false)
            {
                return false;
            }
        }
        return true;
    }
}