namespace BitMono.Core.Resolvers;

public class ObfuscateAssemblyAttributeResolver : AttributeResolver<ObfuscateAssemblyAttributeData>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly string _attributeNamespace;
    private readonly string _attributeName;

    public ObfuscateAssemblyAttributeResolver(IOptions<ObfuscationSettings> configuration)
    {
        _obfuscationSettings = configuration.Value;
        _attributeNamespace = typeof(ObfuscateAssemblyAttribute).Namespace;
        _attributeName = nameof(ObfuscateAssemblyAttribute);
    }

    public override bool Resolve(string? feature, IHasCustomAttribute from, out ObfuscateAssemblyAttributeData? model)
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

        var attribute = attributesResolve!.First();
        var assemblyIsPrivate = attribute.FixedValues![0] is bool;
        var stripAfterObfuscation =
            attribute.NamedValues!.GetValueOrDefault(nameof(ObfuscateAssemblyAttribute.StripAfterObfuscation),
                defaultValue: true);
        model = new ObfuscateAssemblyAttributeData(assemblyIsPrivate, stripAfterObfuscation, attribute.Attribute);
        return true;
    }
}