namespace BitMono.Core.Protecting.Resolvers;

public class SafeToMakeChangesMemberResolver : IMemberResolver
{
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly NoInliningMethodMemberResolver m_NoInliningMethodMemberResolver;
    private readonly SpecificNamespaceCriticalAnalyzer m_SpecificNamespaceCriticalAnalyzer;

    public SafeToMakeChangesMemberResolver(
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        NoInliningMethodMemberResolver noInliningMethodMemberResolver,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_NoInliningMethodMemberResolver = noInliningMethodMemberResolver;
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
            if (m_NoInliningMethodMemberResolver.Resolve(protection, customAttribute))
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