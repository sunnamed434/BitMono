namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Baml)]
public class NoNamespaces : Protection
{
    public NoNamespaces(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        foreach (var type in Context.Parameters.Members.OfType<TypeDefinition>())
        {
            if (!type.HasNamespace())
            {
                continue;
            }
            if (type.IsInReservedNamespace())
            {
                continue;
            }

            type.Namespace = string.Empty;
        }
        return Task.CompletedTask;
    }
}