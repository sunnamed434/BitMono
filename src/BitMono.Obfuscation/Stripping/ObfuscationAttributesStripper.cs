namespace BitMono.Obfuscation.Stripping;

public class ObfuscationAttributesStripper
{
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver _obfuscateAssemblyAttributeResolver;

    public ObfuscationAttributesStripper(
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        ObfuscateAssemblyAttributeResolver obfuscateAssemblyAttributeResolver)
    {
        _obfuscationAttributeResolver = obfuscationAttributeResolver;
        _obfuscateAssemblyAttributeResolver = obfuscateAssemblyAttributeResolver;
    }

    public ObfuscationAttributesStrip Strip(StarterContext context, ProtectionsSort protectionsSort)
    {
        var obfuscationAttributesSuccessStrip = new List<CustomAttribute>();
        var obfuscationAttributesFailStrip = new List<CustomAttribute>();
        var obfuscateAssemblyAttributesSuccessStrip = new List<CustomAttribute>();
        var obfuscateAssemblyAttributesFailStrip = new List<CustomAttribute>();
        var protectionNames = protectionsSort.ProtectionsResolve.FoundProtections
            .Select(x => x.GetName())
            .ToList()
            .AsReadOnly();
        foreach (var customAttribute in context.Module.FindMembers().OfType<IHasCustomAttribute>())
        {
            context.ThrowIfCancellationRequested();

            StripAssemblyObfuscationAttribute(customAttribute, obfuscateAssemblyAttributesSuccessStrip, obfuscateAssemblyAttributesFailStrip);

            foreach (var protectionName in protectionNames)
            {
                context.ThrowIfCancellationRequested();

                StripObfuscationAttribute(protectionName, customAttribute, obfuscationAttributesSuccessStrip, obfuscationAttributesFailStrip,
                    context.CancellationToken);
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

    private void StripAssemblyObfuscationAttribute(IHasCustomAttribute customAttribute,
        List<CustomAttribute> obfuscateAssemblyAttributesSuccessStrip, List<CustomAttribute> obfuscateAssemblyAttributesFailStrip)
    {
        if (_obfuscateAssemblyAttributeResolver.Resolve(null, customAttribute, out var obfuscateAssemblyAttributeData) == false)
        {
            return;
        }
        if (obfuscateAssemblyAttributeData.StripAfterObfuscation == false)
        {
            return;
        }

        var attribute = obfuscateAssemblyAttributeData.CustomAttribute;
        if (customAttribute.CustomAttributes.Remove(attribute))
        {
            obfuscateAssemblyAttributesSuccessStrip.Add(attribute);
        }
        else
        {
            obfuscateAssemblyAttributesFailStrip.Add(attribute);
        }
    }
    private void StripObfuscationAttribute(string protectionName, IHasCustomAttribute customAttribute,
        List<CustomAttribute> obfuscationAttributesSuccessStrip, List<CustomAttribute> obfuscationAttributesFailStrip,
        CancellationToken cancellationToken)
    {
        if (_obfuscationAttributeResolver.Resolve(protectionName, customAttribute, out var obfuscationAttributeData) == false)
        {
            return;
        }
        foreach (var attributeData in obfuscationAttributeData)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (attributeData.StripAfterObfuscation == false)
            {
                continue;
            }

            var attribute = attributeData.CustomAttribute;
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
}