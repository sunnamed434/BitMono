namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Baml)]
public class NoNamespaces : Protection
{
    public NoNamespaces(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        // WPF BAML references types by full name (namespace + name); XAML-referenced types are kept
        // out of Context.Parameters.Members by the MemberInclusionFlags.Baml gate (see
        // BamlCriticalAnalyzer), so stripping their namespace can't break XAML load. See issue #212.
        foreach (var type in Context.Parameters.Members.OfType<TypeDefinition>())
        {
            if (type.HasNamespace() == false)
            {
                continue;
            }
            // Keep framework-reserved namespaces (e.g. PolySharp polyfills) intact - the
            // compiler/runtime match those types by full name. See IsInReservedNamespace.
            if (type.IsInReservedNamespace())
            {
                continue;
            }

            type.Namespace = string.Empty;
        }
        return Task.CompletedTask;
    }
}