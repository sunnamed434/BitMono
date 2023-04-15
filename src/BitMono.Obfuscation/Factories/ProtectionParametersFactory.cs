namespace BitMono.Obfuscation.Factories;

public class ProtectionParametersFactory
{
    private readonly IEnumerable<IMemberResolver> m_MemberResolvers;

    public ProtectionParametersFactory(IEnumerable<IMemberResolver> memberResolvers)
    {
        m_MemberResolvers = memberResolvers;
    }

    public ProtectionParameters Create(IProtection protection, ModuleDefinition module)
    {
        var definitions = module.FindMembers();
        var targets = MembersResolver.Resolve(protection, definitions, m_MemberResolvers).ToList();
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