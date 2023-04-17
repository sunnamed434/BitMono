namespace BitMono.Core.Factories;

public class ProtectionParametersFactory
{
    private readonly ICollection<IMemberResolver> _memberResolvers;

    public ProtectionParametersFactory(ICollection<IMemberResolver> memberResolvers)
    {
        _memberResolvers = memberResolvers;
    }

    public ProtectionParameters Create(IProtection protection, ModuleDefinition module)
    {
        var definitions = module.FindMembers();
        var targets = MembersResolver.Resolve(protection, definitions, _memberResolvers).ToList();
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