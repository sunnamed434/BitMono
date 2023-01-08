namespace BitMono.Protections;

public class AntiILdasm : IProtection
{
    private readonly MscorlibInjector m_Injector;

    public AntiILdasm(MscorlibInjector injector)
    {
        m_Injector = injector;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        m_Injector.InjectAttribute(context.Module, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute), context.Module);
        return Task.CompletedTask;
    }
}