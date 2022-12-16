namespace BitMono.Core.Protecting.Analyzing.TypeDefs;

public class TypeDefModelCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;

    public TypeDefModelCriticalAnalyzer(IAttemptAttributeResolver attemptAttributeResolver)
    {
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute from)
    {
        if (m_AttemptAttributeResolver.TryResolve<SerializableAttribute>(from, null, null, out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve<XmlAttributeAttribute>(from, null, null, out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve<XmlArrayItemAttribute>(from, null, null, out _))
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve<JsonPropertyAttribute>(from, null, null, out _))
        {
            return false;
        }
        return false;
    }
}