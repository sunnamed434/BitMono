namespace BitMono.Obfuscation.Notifiers;

public class ObfuscationAttributesStripNotifier
{
    private readonly ILogger m_Logger;

    public ObfuscationAttributesStripNotifier(ILogger logger)
    {
        m_Logger = logger.ForContext<ObfuscationAttributesStripNotifier>();
    }

    public void Notify(ObfuscationAttributesStrip obfuscationAttributesStrip)
    {
        if (obfuscationAttributesStrip.ObfuscationAttributesSuccessStrip.IsEmpty() == false)
        {
            m_Logger.Information("Successfully stripped {0} obfuscation attribute(s)!",
                obfuscationAttributesStrip.ObfuscationAttributesSuccessStrip.Count);
        }
        if (obfuscationAttributesStrip.ObfuscateAssemblyAttributesSuccessStrip.IsEmpty() == false)
        {
            m_Logger.Information("Successfully stripped {0} assembly obfuscation attribute(s)!",
                obfuscationAttributesStrip.ObfuscateAssemblyAttributesSuccessStrip.Count);
        }
        if (obfuscationAttributesStrip.ObfuscationAttributesFailStrip.IsEmpty() == false)
        {
            m_Logger.Information("Failed to strip {0} assembly obfuscation attribute(s)!",
                obfuscationAttributesStrip.ObfuscationAttributesFailStrip.Count);
        }
        if (obfuscationAttributesStrip.ObfuscateAssemblyAttributesFailStrip.IsEmpty() == false)
        {
            m_Logger.Information("Failed to strip {0} obfuscation attribute(s)!",
                obfuscationAttributesStrip.ObfuscateAssemblyAttributesFailStrip.Count);
        }
    }
}