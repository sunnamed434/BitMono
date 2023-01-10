namespace BitMono.Obfuscation;

public class MembersResolver
{
    public IEnumerable<IMetadataMember> Resolve(IProtection protection, IEnumerable<IMetadataMember> members, IEnumerable<IMemberResolver> resolvers)
    {
        foreach (var member in members) 
        {
            if (canBeResolved(protection, member, resolvers))
            {
                yield return member;
            }
        }
    }
    private bool canBeResolved(IProtection protection, IMetadataMember member, IEnumerable<IMemberResolver> resolvers)
    {
        return resolvers.All(r => r.Resolve(protection, member) == true);
    }
}