using BitMono.Shared.Models;

namespace BitMono.Protections;

[UsedImplicitly]
[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class NoNamespaces : Protection
{
    private readonly ObfuscationSettings _obfuscationSettings;

    public NoNamespaces(ObfuscationSettings obfuscationSettings, ProtectionContext context) : base(context)
    {
        _obfuscationSettings = obfuscationSettings;
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