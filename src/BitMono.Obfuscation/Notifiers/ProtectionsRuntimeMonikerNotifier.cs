#pragma warning disable CS8602
namespace BitMono.Obfuscation.Notifiers;

public class ProtectionsRuntimeMonikerNotifier
{
    private readonly Shared.Models.Obfuscation m_Obfuscation;
    private readonly ILogger m_Logger;

    public ProtectionsRuntimeMonikerNotifier(Shared.Models.Obfuscation obfuscation, ILogger logger)
    {
        m_Obfuscation = obfuscation;
        m_Logger = logger.ForContext<ProtectionsRuntimeMonikerNotifier>();
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public void Notify(ProtectionsSort protectionsSort)
    {
        if (m_Obfuscation.OutputRuntimeMonikerWarnings)
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