namespace BitMono.Core.Resolvers;

public class ObfuscationAttributeResolver : AttributeResolver<IReadOnlyList<ObfuscationAttributeData>>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly string _attributeNamespace;
    private readonly string _attributeName;

    public ObfuscationAttributeResolver(IOptions<ObfuscationSettings> configuration)
    {
        _obfuscationSettings = configuration.Value;
        _attributeNamespace = typeof(ObfuscationAttribute).Namespace!;
        _attributeName = nameof(ObfuscationAttribute);
    }

    public override bool Resolve(string? featureName, IHasCustomAttribute from, [NotNullWhen(true)] out IReadOnlyList<ObfuscationAttributeData>? model)
    {
        model = null;
        if (!_obfuscationSettings.ObfuscationAttributeObfuscationExclude)
        {
            return false;
        }
        if (!AttemptAttributeResolver.TryResolve(from, _attributeNamespace, _attributeName,
                out var attributesResolve))
        {
            return false;
        }

        var attributes = new List<ObfuscationAttributeData>();
        foreach (var attribute in attributesResolve)
        {
            var namedValues = attribute.NamedValues;
            if (namedValues == null)
            {
                continue;
            }
            
            var hasFeature = namedValues.TryGetTypedValue(nameof(ObfuscationAttribute.Feature), out string? featureValue);
            var feature = hasFeature ? featureValue : "all";
            
            if (hasFeature)
            {
                if (!string.Equals(feature, featureName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(featureName))
                {
                    continue;
                }
            }

            var exclude =
                namedValues.GetValueOrDefault(nameof(ObfuscationAttribute.Exclude),
                    defaultValue: true);
            
            if (!exclude)
            {
                continue;
            }
            
            var applyToMembers =
                namedValues.GetValueOrDefault(nameof(ObfuscationAttribute.ApplyToMembers),
                    defaultValue: true);
            var stripAfterObfuscation =
                namedValues.GetValueOrDefault(nameof(ObfuscationAttribute.StripAfterObfuscation),
                    defaultValue: true);

            attributes.Add(new ObfuscationAttributeData
            {
                Exclude = exclude,
                ApplyToMembers = applyToMembers,
                StripAfterObfuscation = stripAfterObfuscation,
                Feature = feature,
                CustomAttribute = attribute.Attribute
            });
        }
        
        if (attributes.Count == 0)
        {
            return false;
        }
        
        model = attributes;
        return true;
    }
}