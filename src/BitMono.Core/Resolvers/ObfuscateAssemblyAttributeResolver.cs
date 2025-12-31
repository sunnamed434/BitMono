namespace BitMono.Core.Resolvers;

public class ObfuscateAssemblyAttributeResolver : AttributeResolver<ObfuscateAssemblyAttributeData>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly string _attributeNamespace;
    private readonly string _attributeName;

    public ObfuscateAssemblyAttributeResolver(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
        _attributeNamespace = typeof(ObfuscateAssemblyAttribute).Namespace!;
        _attributeName = nameof(ObfuscateAssemblyAttribute);
    }

    public override bool Resolve(string? feature, IHasCustomAttribute from, [NotNullWhen(true)] out ObfuscateAssemblyAttributeData? model)
    {
        model = null;
        if (_obfuscationSettings.ObfuscateAssemblyAttributeObfuscationExclude == false)
        {
            return false;
        }
        if (AttemptAttributeResolver.TryResolve(from, _attributeNamespace,
                _attributeName, out var attributesResolve) == false)
        {
            return false;
        }
        var attribute = attributesResolve.First();
        var namedValues = attribute.NamedValues;
        if (namedValues == null)
        {
            return false;
        }
        var fixedValues = attribute.FixedValues;
        if (fixedValues == null || fixedValues.Count == 0)
        {
            return false;
        }
        if (fixedValues.ElementAtOrDefault(0) is not bool assemblyIsPrivate)
        {
            return false;
        }

        var stripAfterObfuscation =
            namedValues.GetValueOrDefault(nameof(ObfuscateAssemblyAttribute.StripAfterObfuscation),
                defaultValue: true);
        model = new ObfuscateAssemblyAttributeData(assemblyIsPrivate, stripAfterObfuscation, attribute.Attribute);
        return true;
    }
}