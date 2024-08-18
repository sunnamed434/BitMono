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
        if (_obfuscationSettings.ObfuscationAttributeObfuscationExclude == false)
        {
            return false;
        }
        if (AttemptAttributeResolver.TryResolve(from, _attributeNamespace, _attributeName,
                out var attributesResolve) == false)
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
            if (namedValues.TryGetTypedValue(nameof(ObfuscationAttribute.Feature), out string? feature) == false)
            {
                continue;
            }
            if (feature.Equals(featureName, StringComparison.OrdinalIgnoreCase) == false)
            {
                continue;
            }

            var exclude =
                namedValues.GetValueOrDefault(nameof(ObfuscationAttribute.Exclude),
                    defaultValue: true);
            var applyToMembers =
                namedValues.GetValueOrDefault(nameof(ObfuscationAttribute.Exclude),
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
        model = attributes;
        return true;
    }
}