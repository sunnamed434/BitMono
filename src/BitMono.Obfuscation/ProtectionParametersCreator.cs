namespace BitMono.Obfuscation;

public class ProtectionParametersCreator
{
    private readonly DnlibDefsResolver m_DnlibDefsResolver;
    private readonly IEnumerable<IMemberDefinitionfResolver> m_Resolvers;

    public ProtectionParametersCreator(DnlibDefsResolver dnlibDefsResolver, IEnumerable<IMemberDefinitionfResolver> resolvers)
    {
        m_DnlibDefsResolver = dnlibDefsResolver;
        m_Resolvers = resolvers;
    }

    public ProtectionParameters Create(string feature, ModuleDefinition moduleDefinition)
    {
        var definitions = moduleDefinition.FindDefinitions();
        var targets = m_DnlibDefsResolver.Resolve(feature, definitions, m_Resolvers).ToList();
        return new ProtectionParameters(targets);
    }
}