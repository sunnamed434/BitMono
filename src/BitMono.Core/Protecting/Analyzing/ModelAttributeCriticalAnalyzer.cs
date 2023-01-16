namespace BitMono.Core.Protecting.Analyzing;

public class ModelAttributeCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly IConfiguration m_Configuration;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;

    public ModelAttributeCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration, AttemptAttributeResolver attemptAttributeResolver)
    {
        m_Configuration = configuration.Configuration;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute customAttribute)
    {
        if (m_Configuration.GetValue<bool>(nameof(Criticals.UseCriticalModelAttributes)) == false)
        {
            return true;
        }
        foreach (var attribute in m_Configuration.GetCriticalModelAttributes())
        {
            if (m_AttemptAttributeResolver.TryResolve(customAttribute, attribute.Namespace, attribute.Name))
            {
                return false;
            }
        }
        return true;
    }
}