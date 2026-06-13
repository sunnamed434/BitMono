namespace BitMono.Obfuscation.Notifiers;

public class ProtectionsIL2CPPNotifier
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger _logger;

    public ProtectionsIL2CPPNotifier(ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _obfuscationSettings = obfuscationSettings;
        _logger = logger.ForContext<ProtectionsIL2CPPNotifier>();
    }

    public void Notify(ProtectionsSort protectionsSort, CancellationToken cancellationToken)
    {
        if (!_obfuscationSettings.IL2CPP)
        {
            return;
        }

        _logger.Information(
            "IL2CPP mode is on: the managed assembly is obfuscated before il2cpp.exe converts it to C++, " +
            "so renamed names land cloaked in global-metadata.dat. Only IL2CPP-compatible protections run.");

        var excluded = protectionsSort.IL2CPPIncompatibleProtections;
        if (excluded.Count == 0)
        {
            return;
        }

        _logger.Warning("({0}) protection(s) skipped because they don't work on IL2CPP builds:", excluded.Count);
        foreach (var (protection, reason) in excluded)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.Warning("[IL2CPP] {Name} - {Reason}", protection.GetName(), reason);
        }
    }
}
