namespace BitMono.Obfuscation.Notifiers;

public class ObfuscationAttributesStripNotifier
{
    private readonly ILogger _logger;

    public ObfuscationAttributesStripNotifier(ILogger logger)
    {
        _logger = logger.ForContext<ObfuscationAttributesStripNotifier>();
    }

    public void Notify(ObfuscationAttributesStrip obfuscationAttributesStrip)
    {
        if (obfuscationAttributesStrip.ObfuscationAttributesSuccessStrip.IsEmpty() == false)
        {
            _logger.Information("Successfully stripped {0} obfuscation attribute(s)",
                obfuscationAttributesStrip.ObfuscationAttributesSuccessStrip.Count);
        }
        if (obfuscationAttributesStrip.ObfuscateAssemblyAttributesSuccessStrip.IsEmpty() == false)
        {
            _logger.Information("Successfully stripped {0} assembly obfuscation attribute(s)",
                obfuscationAttributesStrip.ObfuscateAssemblyAttributesSuccessStrip.Count);
        }
        if (obfuscationAttributesStrip.ObfuscationAttributesFailStrip.IsEmpty() == false)
        {
            _logger.Information("Failed to strip {0} assembly obfuscation attribute(s)",
                obfuscationAttributesStrip.ObfuscationAttributesFailStrip.Count);
        }
        if (obfuscationAttributesStrip.ObfuscateAssemblyAttributesFailStrip.IsEmpty() == false)
        {
            _logger.Information("Failed to strip {0} obfuscation attribute(s)",
                obfuscationAttributesStrip.ObfuscateAssemblyAttributesFailStrip.Count);
        }
    }
}