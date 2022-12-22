namespace BitMono.Core.Protecting.Resolvers;

public class SafeToChangesMemberResolver : IMemberDefinitionfResolver
{
    private readonly IObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly SpecificNamespaceCriticalAnalyzer m_SpecificNamespaceCriticalAnalyzer;

    public SafeToChangesMemberResolver(
        IObfuscationAttributeResolver obfuscationAttributeResolver,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_SpecificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
    }

    public bool Resolve(string feature, IMemberDefinition memberDefinition)
    {
        if (memberDefinition is IHasCustomAttribute from)
        {
            if (m_ObfuscationAttributeResolver.Resolve(feature, from))
            {
                return false;
            }
        }
        if (m_SpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(memberDefinition) == false)
        {
            return false;
        }
        return true;
    }
}
