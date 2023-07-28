namespace BitMono.CLI.Modules;

public class ObfuscationNeedsFactory
{
    private readonly string[] _args;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger _logger;

    public ObfuscationNeedsFactory(string[] args,
        ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _args = args;
        _obfuscationSettings = obfuscationSettings;
        _logger = logger;
    }

    public ObfuscationNeeds? Create()
    {
        return _args.IsEmpty()
            ? new CLIObfuscationNeedsFactory(_args, _obfuscationSettings, _logger).Create()
            : new CLIOptionsObfuscationNeedsFactory(_args, _obfuscationSettings, _logger).Create();
    }
}