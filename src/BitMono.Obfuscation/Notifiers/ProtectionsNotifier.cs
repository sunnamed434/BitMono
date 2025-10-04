namespace BitMono.Obfuscation.Notifiers;

public class ProtectionsNotifier
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger _logger;

    public ProtectionsNotifier(ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _obfuscationSettings = obfuscationSettings;
        _logger = logger.ForContext<ProtectionsNotifier>();
    }

    public void Notify(ProtectionsSort protectionsSort, CancellationToken cancellationToken)
    {
        if (_obfuscationSettings.NotifyProtections == false)
        {
            return;
        }
        if (!protectionsSort.HasProtections)
        {
            return;
        }

        var stringBuilder = new StringBuilder();
        List<string> allProtections = [];
        allProtections.AddRange(protectionsSort.SortedProtections.Select(x => x.GetName()));
        allProtections.AddRange(protectionsSort.Pipelines.Select(x => x.GetName()));
        allProtections.AddRange(protectionsSort.Packers.Select(x => x.GetName()));
        stringBuilder.Append(string.Join(", ", allProtections));
        var enabledProtectionsCount = protectionsSort.SortedProtections.Count + protectionsSort.Pipelines.Count + protectionsSort.Packers.Count;
        _logger.Information("({0}) Enabled protection(s): {1}", enabledProtectionsCount, stringBuilder.ToString());
        var runtimeMonikerNotifier = new ProtectionsRuntimeMonikerNotifier(_obfuscationSettings, _logger);
        runtimeMonikerNotifier.Notify(protectionsSort, cancellationToken);
        if (protectionsSort.DeprecatedProtections.Any())
        {
            _logger.Warning("Skip deprecated protection(s): {0}", string.Join(", ", protectionsSort.DeprecatedProtections.Select(p => p?.GetName())));
        }
        if (protectionsSort.ProtectionsResolve.DisabledProtections.Any())
        {
            var disabledProtectionsCount = protectionsSort.ProtectionsResolve.DisabledProtections.Count;
            _logger.Information("({0}) Disabled protection(s): {1}", disabledProtectionsCount, string.Join(", ", protectionsSort.ProtectionsResolve.DisabledProtections.Select(p => p ?? "Unnamed Protection")));
        }
        if (protectionsSort.ProtectionsResolve.UnknownProtections.Any())
        {
            _logger.Warning("Unknown protection(s): {0}", string.Join(", ", protectionsSort.ProtectionsResolve.UnknownProtections.Select(p => p ?? "Unnamed Protection")));
        }
        if (protectionsSort.ObfuscationAttributeExcludeProtections.Any())
        {
            _logger.Information("Skip protection(s) with obfuscation attribute: {0}", string.Join(", ", protectionsSort.ObfuscationAttributeExcludeProtections.Select(p => p?.GetName())));
        }
    }
}