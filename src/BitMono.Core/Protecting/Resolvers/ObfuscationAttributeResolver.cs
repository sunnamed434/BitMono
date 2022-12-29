namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeResolver : AttributeResolver
{
    private readonly IConfiguration m_Configuration;
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;

    public ObfuscationAttributeResolver(IBitMonoObfuscationConfiguration configuration, IAttemptAttributeResolver attemptAttributeResolver)
    {
        m_Configuration = configuration.Configuration;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public override bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out CustomAttributeResolve attributeResolve)
    {
        attributeResolve = null;
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.ObfuscationAttributeObfuscationExclude)) == false)
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(from, typeof(ObfuscationAttribute), out Dictionary<string, CustomAttributeResolve> keyValuePairs) == false)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(feature))
        {
            return true; // just return the attribute (and say this is found) without checking the any values
        }
        if (keyValuePairs.TryGetValue(nameof(ObfuscationAttribute.Feature), out attributeResolve) == false)
        {
            return false;
        }
        if (attributeResolve.Value is string valueFeature)
        {
            if (valueFeature.Equals(feature, StringComparison.OrdinalIgnoreCase))
            {
                var exclude = keyValuePairs.TryGetValueOrDefault(nameof(ObfuscationAttribute.Exclude), defaultValue: true);
                if (exclude)
                {
                    return true;
                }
            }
        }
        return false;
    }
}