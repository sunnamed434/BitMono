namespace BitMono.Core.Resolvers;

public class CriticalAttributeResolver : AttributeResolver<CustomAttributeResolve>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalAttributeResolver(CriticalsSettings criticalsSettings)
    {
        _criticalsSettings = criticalsSettings;
    }

    public override bool Resolve(string? feature, IHasCustomAttribute from,
        [NotNullWhen(true)] out CustomAttributeResolve? attributeResolve)
    {
        attributeResolve = null;
        if (!_criticalsSettings.UseCriticalAttributes)
        {
            return false;
        }

        var criticalAttributes = _criticalsSettings.CriticalAttributes;
        foreach (var criticalAttribute in criticalAttributes)
        {
            if (!AttemptAttributeResolver.TryResolve(from, criticalAttribute.Namespace, criticalAttribute.Name,
                    out var attributesResolve))
            {
                continue;
            }

            attributeResolve = attributesResolve.First();
            return true;
        }
        return false;
    }
}