namespace BitMono.Obfuscation.Notifiers;

public class ProtectionExecutionNotifier
{
    private readonly ILogger _logger;

    public ProtectionExecutionNotifier(ILogger logger)
    {
        _logger = logger;
    }

    public void Notify(IProtection protection)
    {
        _logger.Information("{0} -> OK", protection.GetName());
    }
}