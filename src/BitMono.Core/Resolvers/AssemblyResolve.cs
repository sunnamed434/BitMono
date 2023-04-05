namespace BitMono.Core.Resolvers;

public class AssemblyResolve
{
    public List<AssemblyReference> ResolvedReferences { get; set; }
    public List<AssemblyReference> FailedToResolveReferences { get; set; }
    public bool Succeed { get; set; }
}