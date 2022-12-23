namespace BitMono.Core.Protecting.Analyzing.TypeDefs;

public class AttributeCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;

    public AttributeCriticalAnalyzer(IAttemptAttributeResolver attemptAttributeResolver)
    {
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute customAttribute)
    {
        if (m_AttemptAttributeResolver.TryResolve<SerializableAttribute>(customAttribute, null, out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve<XmlAttributeAttribute>(customAttribute, null, out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve<XmlArrayItemAttribute>(customAttribute, null, out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve<JsonPropertyAttribute>(customAttribute, null, out _))
        {
            return false;
        }
        return false;
    }
}