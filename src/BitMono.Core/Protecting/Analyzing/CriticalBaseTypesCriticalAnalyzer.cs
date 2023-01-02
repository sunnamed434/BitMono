namespace BitMono.Core.Protecting.Analyzing;

public class CriticalBaseTypesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly IConfiguration m_Configuration;

    public CriticalBaseTypesCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition typeDefinition)
    {
        if (m_Configuration.GetValue<bool>("UseCriticalBaseTypes") == false)
        {
            return true;
        }
        if (typeDefinition.HasBaseType())
        {
            var criticalBaseTypes = m_Configuration.GetCriticalBaseTypes();
            if (criticalBaseTypes.FirstOrDefault(c => c.StartsWith(typeDefinition.BaseType.Name.Value.Split('`')[0])) != null)
            {
                return false;
            }
        }
        return true;
    }
}