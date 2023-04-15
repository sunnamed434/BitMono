namespace BitMono.Obfuscation.Abstractions;

public class MembersResolver
{
    public static IEnumerable<IMetadataMember> Resolve(IProtection protection, IEnumerable<IMetadataMember> members, IEnumerable<IMemberResolver> resolvers)
    {
        return members.Where(member => CanBeResolved(protection, member, resolvers));
    }
    private static bool CanBeResolved(IProtection protection, IMetadataMember member, IEnumerable<IMemberResolver> resolvers)
    {
        return resolvers.All(r => r.Resolve(protection, member));
    }
}