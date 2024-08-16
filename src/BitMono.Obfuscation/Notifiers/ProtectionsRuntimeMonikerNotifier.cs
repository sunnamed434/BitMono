namespace BitMono.Obfuscation.Notifiers;

public class ProtectionsRuntimeMonikerNotifier
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ProtectionsSort _protectionsSort;
    private readonly ILogger _logger;

    public ProtectionsRuntimeMonikerNotifier(ObfuscationSettings obfuscationSettings, ProtectionsSort protectionsSort, ILogger logger)
    {
        _obfuscationSettings = obfuscationSettings;
        _protectionsSort = protectionsSort;
        _logger = logger.ForContext<ProtectionsRuntimeMonikerNotifier>();
    }

    public void Notify(CancellationToken cancellationToken)
    {
        if (_obfuscationSettings.OutputRuntimeMonikerWarnings == false)
        {
            return;
        }

        _logger.Warning(
            "Protections marked as \"Intended for ...\" are designed for specific runtimes. Using them with other runtimes may cause crashes or other issues. Proceed with caution.");

        foreach (var protection in _protectionsSort.SortedProtections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var runtimeMonikerAttribute in protection.GetRuntimeMonikerAttributes())
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
            }
        }
        foreach (var protection in _protectionsSort.Pipelines)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var runtimeMonikerAttribute in protection.GetRuntimeMonikerAttributes())
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
            }
        }
        foreach (var protection in _protectionsSort.Packers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var runtimeMonikerAttribute in protection.GetRuntimeMonikerAttributes())
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
            }
        }
    }
}