namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class NoNamespaces : Protection
{
    public NoNamespaces(ProtectionContext context) : base(context)
    {
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        foreach (var type in parameters.Members.OfType<TypeDefinition>())
        {
            if (type.HasNamespace())
            {
                type.Namespace = string.Empty;
            }
        }
        return Task.CompletedTask;
    }
}