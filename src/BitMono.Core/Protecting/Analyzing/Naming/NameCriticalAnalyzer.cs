namespace BitMono.Core.Protecting.Analyzing.Naming;
public class NameCriticalAnalyzer :
    ICriticalAnalyzer<TypeDefinition>,
    ICriticalAnalyzer<MethodDefinition>
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

    public bool NotCriticalToMakeChanges(TypeDefinition typeDefinition)
    {
        if (m_TypeDefCriticalInterfacesCriticalAnalyzer.NotCriticalToMakeChanges(typeDefinition) == false)
        {
            return false;
        }
        if (m_TypeDefCriticalBaseTypesCriticalAnalyzer.NotCriticalToMakeChanges(typeDefinition) == false)
        {
            return false;
        }
        return true;
    }
    public bool NotCriticalToMakeChanges(MethodDefinition methodDefinition)
    {
        var criticalMethodNames = m_Configuration.GetCriticalMethods();
        if (criticalMethodNames.Any(c => c.Equals(methodDefinition.Name)))
        {
            return false;
        }
        return true;
    }
}