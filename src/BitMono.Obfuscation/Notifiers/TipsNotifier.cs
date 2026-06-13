namespace BitMono.Obfuscation.Notifiers;

public class TipsNotifier
{
    // Short, actionable hints shown once after a run. Kept in sync with the defaults in
    // obfuscation.json (NoInlining/ObfuscationAttribute excludes are enabled by default).
    private static readonly string[] Tips =
    {
        "Mark a method with [MethodImpl(MethodImplOptions.NoInlining)] to exclude it from obfuscation.",
        "Mark a type or member with [Obfuscation(Feature = \"ProtectionName\", Exclude = true)] to skip a single protection for it.",
        "Set \"Watermark\": false in obfuscation.json (or pass --no-watermark) to disable the BitMono watermark.",
        "Drop your dependencies into the 'libs' directory (or pass -l/--libraries) so references resolve and obfuscation goes deeper.",
        "Choose how aggressive obfuscation is with --preset Minimal|Balanced|Maximum (or set \"Preset\" in obfuscation.json).",
        "See all protections and which runtimes they support: https://bitmono.readthedocs.io",
    };

    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger _logger;

    public TipsNotifier(ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _obfuscationSettings = obfuscationSettings;
        _logger = logger.ForContext<TipsNotifier>();
    }

    public void Notify()
    {
        if (_obfuscationSettings.Tips == false)
        {
            return;
        }
        foreach (var tip in Tips)
        {
            _logger.Information("[Tip]: {0}", tip);
        }
    }
}
