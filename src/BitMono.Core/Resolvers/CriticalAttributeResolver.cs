namespace BitMono.Core.Resolvers;

public class CriticalAttributeResolver : AttributeResolver<CustomAttributeResolve>
{
    private readonly CriticalsSettings _criticalsSettings;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;

    public CriticalAttributeResolver(IOptions<CriticalsSettings> criticals, AttemptAttributeResolver attemptAttributeResolver)
    {
        _criticalsSettings = criticals.Value;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public override bool Resolve(string? feature, IHasCustomAttribute from, out CustomAttributeResolve? attributeResolve)
    {
        attributeResolve = null;
        if (_criticalsSettings.UseCriticalAttributes == false)
        {
            return false;
        }
        foreach (var criticalAttribute in _criticalsSettings.CriticalAttributes)
        {
            if (m_AttemptAttributeResolver.TryResolve(from, criticalAttribute.Namespace, criticalAttribute.Name, out var attributesResolve))
            {
                attributeResolve = attributesResolve.First();
                return true;
            }
        }
        return false;
    }
}