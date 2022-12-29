namespace BitMono.Obfuscation;

public class MembersResolver
{
    public IEnumerable<IMetadataMember> Resolve(IProtection protection, IEnumerable<IMetadataMember> members, IEnumerable<IMemberResolver> resolvers)
    {
        foreach (var definition in members) 
        {
            foreach (var resolver in resolvers)
            {
                if (resolver.Resolve(protection, definition))
                {
                    yield return definition;
                }
            }
        }
    }
}