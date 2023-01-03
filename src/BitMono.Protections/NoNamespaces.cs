namespace BitMono.Protections;

[DoNotResolve(Members.SpecialRuntime)]
public class NoNamespaces : IProtection
{
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        foreach (var type in parameters.Targets.OfType<TypeDefinition>())
        {
            if (type.HasNamespace())
            {
                type.Namespace = string.Empty;
            }
        }
        return Task.CompletedTask;
    }
}