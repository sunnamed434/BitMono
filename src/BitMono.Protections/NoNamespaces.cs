namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class NoNamespaces : Protection
{
    public NoNamespaces(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        foreach (var type in Context.Parameters.Members.OfType<TypeDefinition>())
        {
            if (type.HasNamespace())
            {
                type.Namespace = string.Empty;
            }
        }
        return Task.CompletedTask;
    }
}