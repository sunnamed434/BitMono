namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeResolver : AttributeResolver
{
    private readonly Obfuscation m_Obfuscation;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;
    private readonly string m_AttributeNamespace;
    private readonly string m_AttributeName;

    public ObfuscationAttributeResolver(IOptions<Obfuscation> configuration, AttemptAttributeResolver attemptAttributeResolver)
    {
        m_Obfuscation = configuration.Value;
        m_AttemptAttributeResolver = attemptAttributeResolver;
        m_AttributeNamespace = typeof(ObfuscationAttribute).Namespace;
        m_AttributeName = nameof(ObfuscationAttribute);
    }

    public override bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out CustomAttributeResolve attributeResolve)
    {
        attributeResolve = null;
        if (m_Obfuscation.ObfuscationAttributeObfuscationExclude == false)
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(from, m_AttributeNamespace, m_AttributeName, out attributeResolve) == false)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(feature))
        {
            return true;
        }
        if (attributeResolve.KeyValuePairs.TryGetValue(nameof(ObfuscationAttribute.Feature), out var value) == false)
        {
            return false;
        }
        if (value is string valueFeature)
        {
            if (valueFeature.Equals(feature, StringComparison.OrdinalIgnoreCase))
            {
                var exclude = attributeResolve.KeyValuePairs.TryGetValueOrDefault(nameof(ObfuscationAttribute.Exclude), defaultValue: true);
                if (exclude)
                {
                    return true;
                }
            }
        }
        return false;
    }
}