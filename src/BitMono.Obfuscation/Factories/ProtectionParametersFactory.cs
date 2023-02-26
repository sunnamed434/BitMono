namespace BitMono.Obfuscation.Factories;

public class ProtectionParametersFactory
{
    private readonly MembersResolver m_MembersResolver;
    private readonly IEnumerable<IMemberResolver> m_MemberResolvers;

    public ProtectionParametersFactory(MembersResolver membersResolver, IEnumerable<IMemberResolver> memberResolvers)
    {
        m_MembersResolver = membersResolver;
        m_MemberResolvers = memberResolvers;
    }

    public ProtectionParameters Create(IProtection protection, ModuleDefinition module)
    {
        var definitions = module.FindMembers();
        var targets = m_MembersResolver.Resolve(protection, definitions, m_MemberResolvers).ToList();
        foreach (var method in targets.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                body.Instructions.CalculateOffsets();
            }
        }
        return new ProtectionParameters(targets);
    }
}