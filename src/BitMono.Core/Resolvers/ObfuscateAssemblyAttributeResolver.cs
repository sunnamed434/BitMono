namespace BitMono.Core.Resolvers;

public class ObfuscateAssemblyAttributeResolver : AttributeResolver<ObfuscateAssemblyAttributeData>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;
    private readonly string m_AttributeNamespace;
    private readonly string m_AttributeName;

    public ObfuscateAssemblyAttributeResolver(IOptions<ObfuscationSettings> configuration, AttemptAttributeResolver attemptAttributeResolver)
    {
        _obfuscationSettings = configuration.Value;
        m_AttemptAttributeResolver = attemptAttributeResolver;
        m_AttributeNamespace = typeof(ObfuscateAssemblyAttribute).Namespace;
        m_AttributeName = nameof(ObfuscateAssemblyAttribute);
    }

    public override bool Resolve(string? feature, IHasCustomAttribute from, out ObfuscateAssemblyAttributeData? model)
    {
        model = null;
        if (_obfuscationSettings.ObfuscateAssemblyAttributeObfuscationExclude == false)
        {
            return false;
        }
        if (m_AttemptAttributeResolver.TryResolve(from, m_AttributeNamespace, m_AttributeName, out var attributesResolve) == false)
        {
            return false;
        }
        var attribute = attributesResolve.First();
        var assemblyIsPrivate = attribute.FixedValues[0] is bool;
        var stripAfterObfuscation = attribute.NamedValues.GetValueOrDefault(nameof(ObfuscateAssemblyAttribute.StripAfterObfuscation), defaultValue: true);
        model = new ObfuscateAssemblyAttributeData
        {
            AssemblyIsPrivate = assemblyIsPrivate,
            StripAfterObfuscation = stripAfterObfuscation,
            CustomAttribute = attribute.Attribute
        };
        return true;
    }
}