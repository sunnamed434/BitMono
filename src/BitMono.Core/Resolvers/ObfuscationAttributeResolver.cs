namespace BitMono.Core.Resolvers;

[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
public class ObfuscationAttributeResolver : AttributeResolver<ObfuscationAttributeData>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly string m_AttributeNamespace;
    private readonly string m_AttributeName;

    public ObfuscationAttributeResolver(IOptions<ObfuscationSettings> configuration)
    {
        _obfuscationSettings = configuration.Value;
        m_AttributeNamespace = typeof(ObfuscationAttribute).Namespace;
        m_AttributeName = nameof(ObfuscationAttribute);
    }

    public override bool Resolve(string? featureName, IHasCustomAttribute from, out ObfuscationAttributeData? model)
    {
        model = null;
        if (_obfuscationSettings.ObfuscationAttributeObfuscationExclude == false)
        {
            return false;
        }
        if (AttemptAttributeResolver.TryResolve(from, m_AttributeNamespace, m_AttributeName, out var attributesResolve) == false)
        {
            return false;
        }

        for (var i = 0; i < attributesResolve.Count; i++)
        {
            var attribute = attributesResolve[i];
            if (attribute.NamedValues?.TryGetTypedValue(nameof(ObfuscationAttribute.Feature), out string? feature) == true)
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