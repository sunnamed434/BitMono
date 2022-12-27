namespace BitMono.Core.Protecting.Resolvers;

public class MethodImplAttributeExcludeResolver : IMethodImplAttributeExcludeResolver
{
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;
    private readonly IConfiguration m_Configuration;

    public MethodImplAttributeExcludeResolver(IAttemptAttributeResolver attemptAttributeResolver, IBitMonoObfuscationConfiguration configuration)
    {
        m_AttemptAttributeResolver = attemptAttributeResolver;
        m_Configuration = configuration.Configuration;
    }

    public bool TryResolve(IHasCustomAttribute from, [AllowNull] out Dictionary<string, CustomAttributesResolve> keyValuePairs)
    {
        keyValuePairs = null;
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.NoInliningMethodObfuscationExcluding)) == false)
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(from, typeof(MethodImplAttribute), out keyValuePairs) == false)
        {
            return false;
        }
        if (keyValuePairs.TryGetValue(nameof(MethodImplAttribute.Value), out CustomAttributesResolve resolve) 
            && resolve.Value is MethodImplOptions options)
        {
            if (options.HasFlag(MethodImplOptions.NoInlining))
            {
                return true;
            }
        }
        return false;
    }
}