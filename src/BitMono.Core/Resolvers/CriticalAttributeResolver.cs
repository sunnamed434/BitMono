namespace BitMono.Core.Resolvers;

[UsedImplicitly]
[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
[SuppressMessage("ReSharper", "InvertIf")]
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
        var criticalAttributes = _criticalsSettings.CriticalAttributes;
        for (var i = 0; i < criticalAttributes.Count; i++)
        {
            var criticalAttribute = criticalAttributes[i];
            if (m_AttemptAttributeResolver.TryResolve(from, criticalAttribute.Namespace, criticalAttribute.Name, out var attributesResolve))
            {
                attributeResolve = attributesResolve.First();
                return true;
            }
        }
        return false;
    }
}