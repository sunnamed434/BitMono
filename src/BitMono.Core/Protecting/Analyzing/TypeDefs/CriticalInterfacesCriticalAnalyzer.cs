namespace BitMono.Core.Protecting.Analyzing.TypeDefs;

public class CriticalInterfacesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly IConfiguration m_Configuration;

    public CriticalInterfacesCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition typeDefinition)
    {
        var criticalInterfaces = m_Configuration.GetCriticalInterfaces();
        if (typeDefinition.Interfaces.Any(i => criticalInterfaces.FirstOrDefault(c => c.Equals(i.Interface.Name)) != null))
        {
            return false;
        }
        return true;
    }
}