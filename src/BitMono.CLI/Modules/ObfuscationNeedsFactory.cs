namespace BitMono.CLI.Modules;

public class ObfuscationNeedsFactory
{
    private readonly string[] _args;
    private readonly ILogger _logger;

    public ObfuscationNeedsFactory(string[] args, ILogger logger)
    {
        _args = args;
        _logger = logger;
    }

    public ObfuscationNeeds? Create()
    {
        return _args.IsEmpty()
            ? new CLIObfuscationNeedsFactory(_args, _logger).Create()
            : new CLIOptionsObfuscationNeedsFactory(_args, _logger).Create();
    }
}