namespace BitMono.Obfuscation.Logging;

internal class LogErrorListener : IErrorListener
{
    private readonly ILogger m_Logger;

    public LogErrorListener(ILogger logger)
    {
        m_Logger = logger;
    }

    void IErrorListener.MarkAsFatal()
    {
        throw new NotImplementedException();
    }
    public void RegisterException(Exception exception)
    {
        m_Logger.Error(exception, "Registered error!");
    }
}