namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscateAssemblyAttributeResolver : AttributeResolver<ObfuscateAssemblyAttributeData>
{
    private readonly Obfuscation m_Obfuscation;
    private readonly AttemptAttributeResolver m_AttemptAttributeResolver;
    private readonly string m_AttributeNamespace;
    private readonly string m_AttributeName;

    public ObfuscateAssemblyAttributeResolver(IOptions<Obfuscation> configuration, AttemptAttributeResolver attemptAttributeResolver)
    {
        m_Obfuscation = configuration.Value;
        m_AttemptAttributeResolver = attemptAttributeResolver;
        m_AttributeNamespace = typeof(ObfuscateAssemblyAttribute).Namespace;
        m_AttributeName = nameof(ObfuscateAssemblyAttribute);
    }
    
    public override bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out ObfuscateAssemblyAttributeData model)
    {
        model = null;
        if (m_Obfuscation.ObfuscateAssemblyAttributeObfuscationExclude == false)
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