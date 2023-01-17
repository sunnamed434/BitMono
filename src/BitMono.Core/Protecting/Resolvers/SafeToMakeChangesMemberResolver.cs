namespace BitMono.Core.Protecting.Resolvers;

public class SafeToMakeChangesMemberResolver : IMemberResolver
{
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly CriticalAttributeResolver m_CriticalAttributeResolver;
    private readonly SpecificNamespaceCriticalAnalyzer m_SpecificNamespaceCriticalAnalyzer;

    public SafeToMakeChangesMemberResolver(
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        CriticalAttributeResolver criticalAttributeResolver,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_CriticalAttributeResolver = criticalAttributeResolver;
        m_SpecificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        var feature = protection.GetName();
        if (member is IHasCustomAttribute customAttribute)
        {
            if (m_ObfuscationAttributeResolver.Resolve(feature, customAttribute))
            {
                return false;
            }
            if (m_CriticalAttributeResolver.Resolve(feature, customAttribute))
            {
                return false;
            }
        }
        if (m_SpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(member) == false)
        {
            return false;
        }
        return true;
    }
}