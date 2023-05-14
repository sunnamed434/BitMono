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

    void IErrorListener.MarkAsFatal()
    {
        throw new NotImplementedException();
    }
    public void RegisterException(Exception exception)
    {
        if (_obfuscationSettings.OutputPEImageBuildErrors)
        {
            _logger.Error(exception, "Registered error!");
        }
    }
}