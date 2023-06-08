namespace BitMono.Core.Resolvers;

[UsedImplicitly]
[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
public class CriticalAttributeResolver : AttributeResolver<CustomAttributeResolve>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalAttributeResolver(IOptions<CriticalsSettings> criticals)
    {
        _criticalsSettings = criticals.Value;
    }

    public override bool Resolve(string? feature, IHasCustomAttribute from,
        out CustomAttributeResolve? attributeResolve)
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