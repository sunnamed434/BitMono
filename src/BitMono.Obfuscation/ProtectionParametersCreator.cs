namespace BitMono.Obfuscation;

public class ProtectionParametersCreator
{
    private readonly MembersResolver m_MembersResolver;
    private readonly IEnumerable<IMemberResolver> m_MemberResolvers;

    public ProtectionParametersCreator(MembersResolver membersResolver, IEnumerable<IMemberResolver> memberResolvers)
    {
        m_MembersResolver = membersResolver;
        m_MemberResolvers = memberResolvers;
    }

    public ProtectionParameters Create(IProtection protection, ModuleDefinition module)
    {
        var definitions = module.FindDefinitions();
        var targets = m_MembersResolver.Resolve(protection, definitions, m_MemberResolvers).ToList();
        return new ProtectionParameters(targets);
    }
}