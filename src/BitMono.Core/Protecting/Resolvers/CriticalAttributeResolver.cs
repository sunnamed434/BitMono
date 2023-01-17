namespace BitMono.Core.Protecting.Resolvers;

public class CriticalAttributeResolver : AttributeResolver
{
    private readonly Criticals m_Criticals;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;

    public CriticalAttributeResolver(IOptions<Criticals> criticals, AttemptAttributeResolver attemptAttributeResolver)
    {
        m_Criticals = criticals.Value;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public override bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out CustomAttributeResolve attributeResolve)
    {
        attributeResolve = null;
        if (m_Criticals.UseCriticalAttributes == false)
        {
            return false;
        }
        foreach (var criticalAttribute in m_Criticals.CriticalAttributes)
        {
            if (m_AttemptAttributeResolver.TryResolve(from, criticalAttribute.Namespace, criticalAttribute.Name, out attributeResolve))
            {
                return true;
            }
        }
        return false;
    }
}