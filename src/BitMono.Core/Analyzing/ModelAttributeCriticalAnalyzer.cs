using BitMono.Core.Resolvers;

namespace BitMono.Core.Analyzing;

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
        var criticalAttributes = m_Criticals.CriticalModelAttributes;
        for (var i = 0; i < criticalAttributes.Count; i++)
        {
            var attribute = criticalAttributes[i];
            if (m_AttemptAttributeResolver.TryResolve(customAttribute, attribute.Namespace, attribute.Name))
            {
                return false;
            }
        }
        return true;
    }
}