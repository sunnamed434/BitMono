namespace BitMono.Core.Protecting.Resolvers;

public class SafeDnlibDefResolver : IMemberDefinitionfResolver
{
    private readonly IObfuscationAttributeResolver m_DnlibDefObfuscationAttributeResolver;
    private readonly DnlibDefSpecificNamespaceCriticalAnalyzer m_DnlibDefSpecificNamespaceCriticalAnalyzer;

    public SafeDnlibDefResolver(
        IObfuscationAttributeResolver dnlibDefObfuscationAttributeResolver,
        DnlibDefSpecificNamespaceCriticalAnalyzer dnlibDefSpecificNamespaceCriticalAnalyzer)
    {
        m_DnlibDefObfuscationAttributeResolver = dnlibDefObfuscationAttributeResolver;
        m_DnlibDefSpecificNamespaceCriticalAnalyzer = dnlibDefSpecificNamespaceCriticalAnalyzer;
    }

    public bool Resolve(string feature, IMemberDefinition memberDefenition)
    {
        if (memberDefenition is IHasCustomAttribute from)
        {
            if (m_DnlibDefObfuscationAttributeResolver.Resolve(feature, from))
            {
                return false;
            }
        }
        if (m_DnlibDefSpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(memberDefenition) == false)
        {
            return false;
        }
        return true;
    }
}
