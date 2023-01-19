namespace BitMono.Core.Protecting.Analyzing;

public class ModelAttributeCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly Criticals m_Criticals;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;

    public ModelAttributeCriticalAnalyzer(IOptions<Criticals> criticals, AttemptAttributeResolver attemptAttributeResolver)
    {
        m_Criticals = criticals.Value;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute customAttribute)
    {
        if (m_Criticals.UseCriticalModelAttributes == false)
        {
            return true;
        }
        foreach (var attribute in m_Criticals.CriticalModelAttributes)
        {
            if (m_AttemptAttributeResolver.TryResolve(customAttribute, attribute.Namespace, attribute.Name))
            {
                return false;
            }
        }
        return true;
    }
}