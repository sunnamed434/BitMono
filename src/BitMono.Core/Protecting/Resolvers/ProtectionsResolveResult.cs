namespace BitMono.Core.Protecting.Resolvers;

public class ProtectionsResolveResult
{
    public List<IProtection> FoundProtections { get; set; }
    public List<string> DisabledProtections { get; set; }
}