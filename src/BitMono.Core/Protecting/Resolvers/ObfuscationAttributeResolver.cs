namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeResolver : AttributeResolver<ObfuscationAttributeData>
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

    public override bool Resolve(string? featureName, IHasCustomAttribute from, out ObfuscationAttributeData? model)
    {
        model = null;
        if (m_Obfuscation.ObfuscationAttributeObfuscationExclude == false)
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(from, m_AttributeNamespace, m_AttributeName, out var attributesResolve) == false)
        {
            return false;
        }
        foreach (var attribute in attributesResolve)
        {
            if (attribute.NamedValues.TryGetTypedValue(nameof(ObfuscationAttribute.Feature), out string? feature))
            {
                if (feature.Equals(featureName, StringComparison.OrdinalIgnoreCase))
                {
                    var exclude = attribute.NamedValues.GetValueOrDefault(nameof(ObfuscationAttribute.Exclude), defaultValue: true);
                    var applyToMembers = attribute.NamedValues.GetValueOrDefault(nameof(ObfuscationAttribute.Exclude), defaultValue: true);
                    var stripAfterObfuscation = attribute.NamedValues.GetValueOrDefault(nameof(ObfuscationAttribute.StripAfterObfuscation), defaultValue: true);
                    if (exclude)
                    {
                        model = new ObfuscationAttributeData
                        {
                            Exclude = exclude,
                            ApplyToMembers = applyToMembers,
                            StripAfterObfuscation = stripAfterObfuscation,
                            Feature = feature,
                            CustomAttribute = attribute.Attribute
                        };
                        return true;
                    }
                }
            }
        }
        return false;
    }
}