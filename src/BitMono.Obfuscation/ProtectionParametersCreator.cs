namespace BitMono.Obfuscation;

public class ProtectionParametersCreator
{
    private readonly MembersResolver m_DnlibDefsResolver;
    private readonly IEnumerable<IMemberResolver> m_Resolvers;

    public ProtectionParametersCreator(MembersResolver dnlibDefsResolver, IEnumerable<IMemberResolver> resolvers)
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