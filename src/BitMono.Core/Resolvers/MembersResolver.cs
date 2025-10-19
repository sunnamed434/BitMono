namespace BitMono.Core.Resolvers;

public static class MembersResolver
{
    public static IEnumerable<IMetadataMember> Resolve(IProtection protection, IEnumerable<IMetadataMember> members,
        IEnumerable<IMemberResolver> resolvers)
    {
        return members.Where(x => CanBeResolved(protection, x, resolvers));
    }
    private static bool CanBeResolved(IProtection protection, IMetadataMember member,
        IEnumerable<IMemberResolver> resolvers)
    {
        return resolvers.All(x => x.Resolve(protection, member));
    }
}