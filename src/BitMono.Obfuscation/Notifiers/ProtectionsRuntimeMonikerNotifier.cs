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

        var protectionsWithAttributes = _protectionsSort.SortedProtections
            .Concat(_protectionsSort.Pipelines)
            .Concat(_protectionsSort.Packers)
            .Select(x => new { Protection = x, Attributes = x.GetRuntimeMonikerAttributes() })
            .Where(x => x.Attributes.Length > 0)
            .ToList();

        if (protectionsWithAttributes.Count == 0)
        {
            return;
        }

        _logger.Warning(
            "Protections marked as \"Intended for ...\" are designed for specific runtimes. Using them with other runtimes may cause crashes or other issues. Proceed with caution.");

        foreach (var item in protectionsWithAttributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var protection = item.Protection;
            var attributes = item.Attributes;

            foreach (var runtimeMonikerAttribute in attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
            }
        }
    }
}