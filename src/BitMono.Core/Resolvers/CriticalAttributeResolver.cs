namespace BitMono.Core.Resolvers;

[UsedImplicitly]
[SuppressMessage("ReSharper", "InvertIf")]
public class CriticalAttributeResolver : AttributeResolver<CustomAttributeResolve>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalAttributeResolver(IOptions<CriticalsSettings> criticals)
    {
        _criticalsSettings = criticals.Value;
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
            if (AttemptAttributeResolver.TryResolve(from, criticalAttribute.Namespace, criticalAttribute.Name,
                    out var attributesResolve))
            {
                attributeResolve = attributesResolve!.First();
                return true;
            }
        }
        return false;
    }
}