namespace BitMono.Obfuscation;

public class MembersResolver
{
    public IEnumerable<IMetadataMember> Resolve(string feature, IEnumerable<IMetadataMember> members, IEnumerable<IMemberResolver> resolvers)
    {
        foreach (var definition in members) 
        {
            foreach (var resolver in resolvers)
            {
                if (resolver.Resolve(feature, definition))
                {
                    yield return definition;
                }
                else
                {
                    Console.WriteLine("Failed to resolve MemberDef!");
                }
            }
        }
    }
}