namespace BitMono.Obfuscation;

public class ProtectionExecutionNotifier
{
    private readonly ILogger m_Logger;

    public ProtectionExecutionNotifier(ILogger logger)
    {
        m_Logger = logger;
    }

    public void Notify(IProtection protection)
    {
        m_Logger.Information("{0} -> OK", protection.GetName());
    }
}