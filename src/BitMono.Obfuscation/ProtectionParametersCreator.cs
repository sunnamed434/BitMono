namespace BitMono.Obfuscation;

public class ProtectionParametersCreator
{
    private readonly MembersResolver m_MembersResolver;
    private readonly IEnumerable<IMemberResolver> m_Resolvers;

    public ProtectionParametersCreator(MembersResolver membersResolver, IEnumerable<IMemberResolver> resolvers)
    {
        m_MembersResolver = membersResolver;
        m_Resolvers = resolvers;
    }

    public ProtectionParameters Create(IProtection protection, ModuleDefinition moduleDefinition)
    {
        var definitions = moduleDefinition.FindDefinitions();
        var targets = m_MembersResolver.Resolve(protection, definitions, m_Resolvers).ToList();
        return new ProtectionParameters(targets);
    }
}