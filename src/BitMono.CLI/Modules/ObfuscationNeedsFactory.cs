namespace BitMono.CLI.Modules;

public class ObfuscationNeedsFactory : IObfuscationNeedsFactory
{
    private readonly string[] m_Args;
    private readonly ILogger m_Logger;

    public ObfuscationNeedsFactory(string[] args, ILogger logger)
    {
        m_Args = args;
        m_Logger = logger;
    }

    public ObfuscationNeeds? Create()
    {
        return m_Args.IsEmpty()
            ? new CLIObfuscationNeedsFactory(m_Args, m_Logger).Create()
            : new CLIOptionsObfuscationNeedsFactory(m_Args, m_Logger).Create();
    }
}