using BitMono.Core.Resolvers;

namespace BitMono.Core.Analyzing;

public class ModelAttributeCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly CriticalsSettings _criticalsSettings;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;

    public ModelAttributeCriticalAnalyzer(IOptions<CriticalsSettings> criticals, AttemptAttributeResolver attemptAttributeResolver)
    {
        _criticalsSettings = criticals.Value;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute customAttribute)
    {
        if (_criticalsSettings.UseCriticalModelAttributes == false)
        {
            return true;
        }
        var criticalAttributes = _criticalsSettings.CriticalModelAttributes;
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