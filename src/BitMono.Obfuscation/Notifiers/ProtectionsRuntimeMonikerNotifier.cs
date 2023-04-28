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

    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public void Notify(ProtectionsSort protectionsSort)
    {
        if (_obfuscationSettings.OutputRuntimeMonikerWarnings)
        {
            foreach (var protection in protectionsSort.SortedProtections)
            {
                var runtimeMonikerAttributes = protection.GetRuntimeMonikerAttributes();
                for (var i = 0; i < runtimeMonikerAttributes.Length; i++)
                {
                    var runtimeMonikerAttribute = runtimeMonikerAttributes[i];
                    _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
                }
            }
            foreach (var protection in protectionsSort.Pipelines)
            {
                var runtimeMonikerAttributes = protection.GetRuntimeMonikerAttributes();
                for (var i = 0; i < runtimeMonikerAttributes.Length; i++)
                {
                    var runtimeMonikerAttribute = runtimeMonikerAttributes[i];
                    _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
                }
            }
            foreach (var protection in protectionsSort.Packers)
            {
                var runtimeMonikerAttributes = protection.GetRuntimeMonikerAttributes();
                for (var i = 0; i < runtimeMonikerAttributes.Length; i++)
                {
                    var runtimeMonikerAttribute = runtimeMonikerAttributes[i];
                    _logger.Warning("[!!!] {Name} - " + runtimeMonikerAttribute.GetMessage(), protection.GetName());
                }
            }
        }
    }
}