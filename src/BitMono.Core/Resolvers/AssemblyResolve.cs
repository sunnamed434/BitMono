namespace BitMono.Core.Resolvers;

public class AssemblyResolve
{
#pragma warning disable CS8618
    public List<AssemblyReference> ResolvedReferences { get; set; }
    public List<AssemblyReference> FailedToResolveReferences { get; set; }
#pragma warning restore CS8618
    public bool Succeed { get; set; }
}