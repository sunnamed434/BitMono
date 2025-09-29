namespace BitMono.CLI.Modules;

internal class ObfuscationNeedsFactory
{
    private readonly string[] _args;

    public ObfuscationNeedsFactory(string[] args)
    {
        _args = args;
    }

    public ObfuscationNeeds? Create(CancellationToken cancellationToken)
    {
        return _args.IsEmpty()
            ? new ReadlineObfuscationNeedsFactory(_args).Create(cancellationToken)
            : new OptionsObfuscationNeedsFactory(_args).Create(cancellationToken);
    }
}