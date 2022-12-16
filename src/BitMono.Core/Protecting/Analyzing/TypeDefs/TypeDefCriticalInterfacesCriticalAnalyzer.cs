namespace BitMono.Core.Protecting.Analyzing.TypeDefs;

public class TypeDefCriticalInterfacesCriticalAnalyzer : ICriticalAnalyzer<TypeDef>
{
    private readonly IConfiguration m_Configuration;

    public TypeDefCriticalInterfacesCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(TypeDef typeDef)
    {
        var criticalInterfaces = m_Configuration.GetCriticalInterfaces();
        if (typeDef.Interfaces.Any(i => criticalInterfaces.FirstOrDefault(c => c.Equals(i.Interface.Name)) != null))
        {
            return false;
        }
        return true;
    }
}