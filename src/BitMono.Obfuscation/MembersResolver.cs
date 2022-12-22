namespace BitMono.Obfuscation;

public class MembersResolver
{
    public IEnumerable<IMemberDefinition> Resolve(string feature, IEnumerable<IMemberDefinition> definitions, IEnumerable<IMemberDefinitionfResolver> resolvers)
    {
        foreach (var definition in definitions) 
        {
            foreach (var resolver in resolvers)
            {
                if (resolver.Resolve(feature, definition))
                {
                    yield return definition;
                }
            }
        }
    }
}