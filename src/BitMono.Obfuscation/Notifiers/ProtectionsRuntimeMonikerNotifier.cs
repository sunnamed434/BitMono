namespace BitMono.Obfuscation.Notifiers;

public class ProtectionsRuntimeMonikerNotifier
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger _logger;

    public ProtectionsRuntimeMonikerNotifier(ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _obfuscationSettings = obfuscationSettings;
        _logger = logger.ForContext<ProtectionsRuntimeMonikerNotifier>();
    }

    public void Notify(ProtectionsSort protectionsSort, CancellationToken cancellationToken)
    {
        if (_obfuscationSettings.OutputRuntimeMonikerWarnings == false)
        {
            return;
        }
        var runtimeMonikerProtections = protectionsSort.RuntimeMonikerProtections;
        if (runtimeMonikerProtections.Count == 0)
        {
            return;
        }

        _logger.Warning(
            "Protections marked as \"Intended for ...\" are designed for specific runtimes. Using them with other runtimes may cause crashes or other issues. Proceed with caution.");

        foreach (var (protection, attributes) in runtimeMonikerProtections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var runtimeMonikerAttribute in attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
            }
        }
    }
}