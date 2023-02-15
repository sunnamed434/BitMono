namespace BitMono.Core.Protecting.Resolvers;

public class AssemblyResolve
{
    public List<AssemblyReference>? ResolvedReferences { get; set; }
    public List<AssemblyReference>? FailedToResolveReferences { get; set; }
    public List<AssemblyReference>? BadImageReferences { get; set; }
    public bool Succeed { get; set; }
}