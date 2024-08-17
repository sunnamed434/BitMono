namespace BitMono.CLI.Modules;

internal class ObfuscationNeedsFactory
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

    public ObfuscationNeeds? Create(CancellationToken cancellationToken)
    {
        return _args.IsEmpty()
            ? new ReadlineObfuscationNeedsFactory(_args, _obfuscationSettings, _logger).Create(cancellationToken)
            : new OptionsObfuscationNeedsFactory(_args, _obfuscationSettings, _logger).Create(cancellationToken);
    }
}