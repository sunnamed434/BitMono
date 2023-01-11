namespace BitMono.Core.Protecting.Analyzing;

public class CriticalMethodsCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly IConfiguration m_Configuration;

    public CriticalMethodsCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (m_Configuration.GetValue<bool>("UseCriticalMethods") == false)
        {
            return true;
        }
        var criticalMethodNames = m_Configuration.GetCriticalMethods();
        if (criticalMethodNames.Any(c => c.Equals(method.Name)) == false)
        {
            return true;
        }
        return false;
    }
}