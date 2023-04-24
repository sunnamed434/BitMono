namespace BitMono.Core.Resolvers;

public class ProtectionsResolve
{
#pragma warning disable CS8618
    public List<IProtection> FoundProtections { get; set; }
    public List<string> DisabledProtections { get; set; }
    public List<string> UnknownProtections { get; set; }
#pragma warning restore CS8618
}