namespace BitMono.Obfuscation;

public class DnlibDefsResolver
{
    public IEnumerable<IDnlibDef> Resolve(string feature, IEnumerable<IDnlibDef> definitions, IEnumerable<IDnlibDefResolver> resolvers)
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