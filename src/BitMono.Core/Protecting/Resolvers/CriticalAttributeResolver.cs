namespace BitMono.Core.Protecting.Resolvers;

public class CriticalAttributeResolver : AttributeResolver
{
    private readonly IConfiguration m_Configuration;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;

    public CriticalAttributeResolver(IBitMonoCriticalsConfiguration configuration, AttemptAttributeResolver attemptAttributeResolver)
    {
        m_Configuration = configuration.Configuration;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public override bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out CustomAttributeResolve attributeResolve)
    {
        attributeResolve = null;
        if (m_Configuration.GetValue<bool>("UseCriticalAttributes") == false)
        {
            return false;
        }
        foreach (var criticalAttribute in m_Configuration.GetCriticalAttributes())
        {
            if (m_AttemptAttributeResolver.TryResolve(from, criticalAttribute.Namespace, criticalAttribute.Name, out attributeResolve))
            {
                return true;
            }
        }
        return false;
    }
}