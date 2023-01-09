namespace BitMono.Obfuscation;

internal class LogErrorListener : IErrorListener
{
    private readonly ILogger m_Logger;

    public LogErrorListener(ILogger logger)
    {
        m_Logger = logger;
    }

    public void MarkAsFatal()
    {
        throw new NotImplementedException();
    }
    public void RegisterException(Exception exception)
    {
        m_Logger.Error(exception, "Module raised an error!");
    }
}