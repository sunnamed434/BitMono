namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeExcludingResolver : IObfuscationAttributeExcludingResolver
{
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;
    private readonly IConfiguration m_Configuration;

    public ObfuscationAttributeExcludingResolver(IAttemptAttributeResolver attemptAttributeResolver, IBitMonoObfuscationConfiguration configuration)
    {
        m_AttemptAttributeResolver = attemptAttributeResolver;
        m_Configuration = configuration.Configuration;
    }

    public bool TryResolve(string feature, IHasCustomAttribute from, [AllowNull] out ObfuscationAttribute obfuscationAttribute)
    {
        obfuscationAttribute = null;
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.ObfuscationAttributeObfuscationExcluding)) == false)
        {
            return false;
        }
        return m_AttemptAttributeResolver.TryResolve(from, o => o.Feature.Equals(feature, StringComparison.OrdinalIgnoreCase),
            strip => strip.StripAfterObfuscation, out obfuscationAttribute);
    }
}