namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeResolver : IObfuscationAttributeResolver
{
    private readonly IConfiguration m_Configuration;
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;

    public ObfuscationAttributeResolver(IBitMonoObfuscationConfiguration configuration, IAttemptAttributeResolver attemptAttributeResolver)
    {
        m_Configuration = configuration.Configuration;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool Resolve([AllowNull] string feature, IHasCustomAttribute from)
    {
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.ObfuscationAttributeObfuscationExcluding)) == false)
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(from, typeof(ObfuscationAttribute), out Dictionary<string, CustomAttributesResolve> keyValuePairs))
        {
            if (string.IsNullOrWhiteSpace(feature))
            {
                return true;
            }
            if (keyValuePairs.TryGetValue(nameof(ObfuscationAttribute.Feature), out CustomAttributesResolve resolve)
                && resolve.Value is string valueFeature)
            {
                if (valueFeature.Equals(feature, StringComparison.OrdinalIgnoreCase))
                {
                    if (keyValuePairs.TryGetValue(nameof(ObfuscationAttribute.Exclude), out resolve) && resolve.Value is bool exclude)
                    {
                        if (exclude)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public bool Resolve(IHasCustomAttribute from)
    {
        return Resolve(feature: null, from);
    }
    public bool Resolve(Type featureType, IHasCustomAttribute from)
    {
        return Resolve(feature: featureType.GetName(), from);
    }
    public bool Resolve<TFeature>(IHasCustomAttribute from) where TFeature : IProtection
    {
        return Resolve(typeof(TFeature), from);
    }
}