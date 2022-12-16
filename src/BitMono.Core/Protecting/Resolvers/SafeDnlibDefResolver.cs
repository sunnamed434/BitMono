namespace BitMono.Core.Protecting.Resolvers;

public class SafeDnlibDefResolver : IDnlibDefResolver
{
    private readonly IDnlibDefObfuscationAttributeResolver m_DnlibDefObfuscationAttributeResolver;
    private readonly DnlibDefSpecificNamespaceCriticalAnalyzer m_DnlibDefSpecificNamespaceCriticalAnalyzer;

    public SafeDnlibDefResolver(
        IDnlibDefObfuscationAttributeResolver dnlibDefObfuscationAttributeResolver,
        DnlibDefSpecificNamespaceCriticalAnalyzer dnlibDefSpecificNamespaceCriticalAnalyzer)
    {
        m_DnlibDefObfuscationAttributeResolver = dnlibDefObfuscationAttributeResolver;
        m_DnlibDefSpecificNamespaceCriticalAnalyzer = dnlibDefSpecificNamespaceCriticalAnalyzer;
    }

    public bool Resolve(string feature, IDnlibDef dnlibDef)
    {
        if (m_DnlibDefObfuscationAttributeResolver.Resolve(feature, dnlibDef))
        {
            return false;
        }
        if (m_DnlibDefSpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(dnlibDef) == false)
        {
            return false;
        }
        return true;
    }
}
