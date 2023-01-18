namespace BitMono.Core.Protecting.Resolvers;

public class SafeToMakeChangesMemberResolver : IMemberResolver
{
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver m_ObfuscateAssemblyAttributeResolver;
    private readonly CriticalAttributeResolver m_CriticalAttributeResolver;
    private readonly SerializableBitCriticalAnalyzer m_SerializableBitCriticalAnalyzer;
    private readonly SpecificNamespaceCriticalAnalyzer m_SpecificNamespaceCriticalAnalyzer;

    public SafeToMakeChangesMemberResolver(
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        ObfuscateAssemblyAttributeResolver obfuscateAssemblyAttributeResolver,
        CriticalAttributeResolver criticalAttributeResolver,
        SerializableBitCriticalAnalyzer serializableBitCriticalAnalyzer,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_ObfuscateAssemblyAttributeResolver = obfuscateAssemblyAttributeResolver;
        m_CriticalAttributeResolver = criticalAttributeResolver;
        m_SerializableBitCriticalAnalyzer = serializableBitCriticalAnalyzer;
        m_SpecificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        if (member is IHasCustomAttribute customAttribute)
        {
            var feature = protection.GetName();
            if (m_ObfuscationAttributeResolver.Resolve(feature, customAttribute))
            {
                return false;
            }
            if (m_ObfuscateAssemblyAttributeResolver.Resolve(customAttribute))
            {
                return false;
            }
            if (m_CriticalAttributeResolver.Resolve(feature, customAttribute))
            {
                return false;
            }
        }
        if (member is TypeDefinition type)
        {
            if (m_SerializableBitCriticalAnalyzer.NotCriticalToMakeChanges(type) == false)
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