namespace BitMono.Core.Protecting.Resolvers;

public class AssemblyResolve
{
    public HashSet<AssemblyReference> ResolvedReferences { get; set; }
    public HashSet<AssemblyReference> FailedToResolveReferences { get; set; }
    public bool Succeed { get; set; }
}