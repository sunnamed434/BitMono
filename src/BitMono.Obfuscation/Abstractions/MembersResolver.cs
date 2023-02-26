namespace BitMono.Obfuscation.Abstractions;

public class MembersResolver
{
    public IEnumerable<IMetadataMember> Resolve(IProtection protection, IEnumerable<IMetadataMember> members, IEnumerable<IMemberResolver> resolvers)
    {
        return members.Where(member => canBeResolved(protection, member, resolvers));
    }
    private bool canBeResolved(IProtection protection, IMetadataMember member, IEnumerable<IMemberResolver> resolvers)
    {
        return resolvers.All(r => r.Resolve(protection, member));
    }
}