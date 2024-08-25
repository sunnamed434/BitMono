namespace BitMono.Obfuscation.Notifiers;

public class ProtectionsConfigureForNativeCodeNotifier
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger _logger;

    public ProtectionsConfigureForNativeCodeNotifier(ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _obfuscationSettings = obfuscationSettings;
        _logger = logger.ForContext<ProtectionsConfigureForNativeCodeNotifier>();
    }

    public void Notify(ProtectionsSort protectionsSort, CancellationToken cancellationToken)
    {
        if (_obfuscationSettings.OutputConfigureForNativeCodeWarnings == false)
        {
            return;
        }
        var configureForNativeCodeProtections = protectionsSort.ConfigureForNativeCodeProtections;
        if (configureForNativeCodeProtections.Any() == false)
        {
            return;
        }

        _logger.Warning(
            "Enabled protections may create native code configurations, which can sometimes break the app. Proceed with caution. If issues arise, disable the following protections.");
        foreach (var protection in configureForNativeCodeProtections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.Warning("{Name} - is using Native Code.", protection.GetName());
        }
    }
}