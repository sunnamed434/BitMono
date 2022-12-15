namespace BitMono.Core.Protecting.Analyzing.Naming;
public class NameCriticalAnalyzer :
    ICriticalAnalyzer<TypeDef>,
    ICriticalAnalyzer<MethodDef>
{
    private readonly TypeDefCriticalInterfacesCriticalAnalyzer m_TypeDefCriticalInterfacesCriticalAnalyzer;
    private readonly TypeDefCriticalBaseTypesCriticalAnalyzer m_TypeDefCriticalBaseTypesCriticalAnalyzer;
    private readonly IConfiguration m_Configuration;

    public NameCriticalAnalyzer(
        TypeDefCriticalInterfacesCriticalAnalyzer typeDefCriticalInterfacesCriticalAnalyzer,
        TypeDefCriticalBaseTypesCriticalAnalyzer typeDefCriticalBaseTypesCriticalAnalyzer,
        IBitMonoCriticalsConfiguration configuration)
    {
        m_TypeDefCriticalInterfacesCriticalAnalyzer = typeDefCriticalInterfacesCriticalAnalyzer;
        m_TypeDefCriticalBaseTypesCriticalAnalyzer = typeDefCriticalBaseTypesCriticalAnalyzer;
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(TypeDef typeDef)
    {
        if (m_TypeDefCriticalInterfacesCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
        {
            return false;
        }
        if (m_TypeDefCriticalBaseTypesCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
        {
            return false;
        }
        return true;
    }
    public bool NotCriticalToMakeChanges(MethodDef methodDef)
    {
        var criticalMethodNames = m_Configuration.GetCriticalMethods();
        if (criticalMethodNames.Any(c => c.Equals(methodDef.Name)))
        {
            return false;
        }
        return true;
    }
}