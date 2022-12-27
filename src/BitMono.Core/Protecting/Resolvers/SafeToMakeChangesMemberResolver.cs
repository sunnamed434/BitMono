using BitMono.Core.Protecting.Analyzing;

namespace BitMono.Core.Protecting.Resolvers;

public class SafeToMakeChangesMemberResolver : IMemberResolver
{
    private readonly IObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly SpecificNamespaceCriticalAnalyzer m_SpecificNamespaceCriticalAnalyzer;

    public SafeToMakeChangesMemberResolver(
        IObfuscationAttributeResolver obfuscationAttributeResolver,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_SpecificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
    }

    public bool Resolve(string feature, IMetadataMember member)
    {
        if (member is IHasCustomAttribute from)
        {
            if (m_ObfuscationAttributeResolver.Resolve(feature, from))
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