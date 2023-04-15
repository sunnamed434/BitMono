#pragma warning disable CS8602
namespace BitMono.Obfuscation.Notifiers;

public class ProtectionsRuntimeMonikerNotifier
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger m_Logger;

    public ProtectionsRuntimeMonikerNotifier(ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _obfuscationSettings = obfuscationSettings;
        m_Logger = logger.ForContext<ProtectionsRuntimeMonikerNotifier>();
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public void Notify(ProtectionsSort protectionsSort)
    {
        if (_obfuscationSettings.OutputRuntimeMonikerWarnings)
        {
            foreach (var protection in protectionsSort.SortedProtections)
            {
                if (protection.TryGetRuntimeMonikerAttribute(out var attribute))
                {
                    m_Logger.Warning("[!!!] {Name} - " + attribute.GetMessage(), protection.GetName());
                }
            }
            foreach (var protection in protectionsSort.Pipelines)
            {
                if (protection.TryGetRuntimeMonikerAttribute(out var attribute))
                {
                    m_Logger.Warning("[!!!] {Name} - " + attribute.GetMessage(), protection.GetName());
                }
            }
            foreach (var protection in protectionsSort.Packers)
            {
                if (protection.TryGetRuntimeMonikerAttribute(out var attribute))
                {
                    m_Logger.Warning("[!!!] {Name} - " + attribute.GetMessage(), protection.GetName());
                }
            }
        }
    }
}