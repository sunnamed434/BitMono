namespace BitMono.Obfuscation.Stripping;

[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
public class ObfuscationAttributesStripper
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver m_ObfuscateAssemblyAttributeResolver;

    public ObfuscationAttributesStripper(
        ObfuscationSettings obfuscationSettings,
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        ObfuscateAssemblyAttributeResolver obfuscateAssemblyAttributeResolver)
    {
        _obfuscationSettings = obfuscationSettings;
        _obfuscationAttributeResolver = obfuscationAttributeResolver;
        m_ObfuscateAssemblyAttributeResolver = obfuscateAssemblyAttributeResolver;
    }

    public ObfuscationAttributesStrip Strip(StarterContext context, ProtectionsSort protectionsSort)
    {
        var obfuscationAttributesSuccessStrip = new List<CustomAttribute>();
        var obfuscationAttributesFailStrip = new List<CustomAttribute>();
        var obfuscateAssemblyAttributesSuccessStrip = new List<CustomAttribute>();
        var obfuscateAssemblyAttributesFailStrip = new List<CustomAttribute>();
        foreach (var customAttribute in context.Module.FindMembers().OfType<IHasCustomAttribute>())
        {
            foreach (var protection in protectionsSort.ProtectionsResolve.FoundProtections)
            {
                var protectionName = protection.GetName();
                if (_obfuscationSettings.StripObfuscationAttributes)
                {
                    if (_obfuscationAttributeResolver.Resolve(protectionName, customAttribute, out var obfuscationAttributeData))
                    {
                        if (obfuscationAttributeData!.StripAfterObfuscation)
                        {
                            var attribute = obfuscationAttributeData.CustomAttribute;
                            if (customAttribute.CustomAttributes.Remove(attribute))
                            {
                                obfuscationAttributesSuccessStrip.Add(attribute);
                            }
                            else
                            {
                                obfuscationAttributesFailStrip.Add(attribute);
                            }
                        }
                    }
                    if (m_ObfuscateAssemblyAttributeResolver.Resolve(null, customAttribute, out var obfuscateAssemblyAttributeData))
                    {
                        var attribute = obfuscateAssemblyAttributeData!.CustomAttribute;
                        if (customAttribute.CustomAttributes.Remove(attribute))
                        {
                            obfuscateAssemblyAttributesSuccessStrip.Add(attribute);
                        }
                        else
                        {
                            obfuscateAssemblyAttributesFailStrip.Add(attribute);
                        }
                    }
                }
            }
        }
        return new ObfuscationAttributesStrip
        {
            ObfuscationAttributesSuccessStrip = obfuscationAttributesSuccessStrip,
            ObfuscationAttributesFailStrip = obfuscationAttributesFailStrip,
            ObfuscateAssemblyAttributesSuccessStrip = obfuscateAssemblyAttributesSuccessStrip,
            ObfuscateAssemblyAttributesFailStrip = obfuscateAssemblyAttributesFailStrip
        };
    }
}