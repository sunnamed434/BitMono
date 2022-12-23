namespace BitMono.Obfuscation;

public class MembersResolver
{
    public IEnumerable<IMemberDefinition> Resolve(string feature, IEnumerable<IMemberDefinition> definitions, IEnumerable<IMemberResolver> resolvers)
    {
        foreach (var definition in definitions) 
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