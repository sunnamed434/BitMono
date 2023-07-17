namespace BitMono.Obfuscation.Logging;

internal class LogErrorListener : IErrorListener
{
    private readonly ILogger _logger;
    private readonly ObfuscationSettings _obfuscationSettings;

    public LogErrorListener(ILogger logger, ObfuscationSettings obfuscationSettings)
    {
        _logger = logger;
        _obfuscationSettings = obfuscationSettings;
    }

    public void MarkAsFatal()
    {
        if (_obfuscationSettings.OutputPEImageBuildErrors)
        {
            _logger.Fatal("An fatal error just occured!");
        }
    }
    public void RegisterException(Exception exception)
    {
        if (_obfuscationSettings.OutputPEImageBuildErrors)
        {
            _logger.Error(exception, "Registered error!");
        }
    }
}