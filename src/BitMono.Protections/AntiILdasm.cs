namespace BitMono.Protections;

public class AntiILdasm : IProtection
{
    private readonly IInjector m_Injector;

    public AntiILdasm(IInjector injector)
    {
        m_Injector = injector;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        m_Injector.InjectAttribute(context.Module, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute));
        return Task.CompletedTask;
    }
}