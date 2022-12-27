namespace BitMono.Core.Protecting.Analyzing;

public class ModelAttributeCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;

    public ModelAttributeCriticalAnalyzer(IAttemptAttributeResolver attemptAttributeResolver)
    {
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute customAttribute)
    {
        if (m_AttemptAttributeResolver.TryResolve(customAttribute, typeof(SerializableAttribute), out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(customAttribute, typeof(XmlAttributeAttribute), out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(customAttribute, typeof(XmlArrayItemAttribute), out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(customAttribute, typeof(JsonPropertyAttribute), out _))
        {
            return false;
        }
        return false;
    }
}