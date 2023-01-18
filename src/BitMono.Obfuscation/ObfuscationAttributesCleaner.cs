namespace BitMono.Obfuscation;

public class ObfuscationAttributesCleaner
{
    private readonly Shared.Models.Obfuscation m_Obfuscation;
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver m_ObfuscateAssemblyAttributeResolver;
    private readonly ILogger m_Logger;

    public ObfuscationAttributesCleaner(
        Shared.Models.Obfuscation obfuscation,
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        ObfuscateAssemblyAttributeResolver obfuscateAssemblyAttributeResolver,
        ILogger logger)
    {
        m_Obfuscation = obfuscation;
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_ObfuscateAssemblyAttributeResolver = obfuscateAssemblyAttributeResolver;
        m_Logger = logger;
    }
    
    public void Strip(ProtectionContext context, ProtectionsSort protectionsSort)
    {
        foreach (var customAttribute in context.Module.FindDefinitions().OfType<IHasCustomAttribute>())
        {
            foreach (var protection in protectionsSort.ProtectionsResolve.FoundProtections)
            {
                var protectionName = protection.GetName();
                if (m_Obfuscation.StripObfuscationAttributes)
                {
                    if (m_ObfuscationAttributeResolver.Resolve(protectionName, customAttribute, out ObfuscationAttributeData obfuscationAttributeData))
                    {
                        if (obfuscationAttributeData.StripAfterObfuscation)
                        {
                            if (customAttribute.CustomAttributes.Remove(obfuscationAttributeData.CustomAttribute))
                            {
                                m_Logger.Information("Successfully stripped obfuscation attribute");
                            }
                            else
                            {
                                m_Logger.Warning("Not able to strip obfuscation attribute");
                            }
                        }
                    }
                    if (m_ObfuscateAssemblyAttributeResolver.Resolve(null, customAttribute, out ObfuscateAssemblyAttributeData obfuscateAssemblyAttributeData))
                    {
                        if (customAttribute.CustomAttributes.Remove(obfuscateAssemblyAttributeData.CustomAttribute))
                        {
                            m_Logger.Information("Successfully stripped assembly obfuscation attribute");
                        }
                        else
                        {
                            m_Logger.Warning("Not able to strip assembly obfuscation attribute");
                        }
                    }
                }
            }
        }
    }
}