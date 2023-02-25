namespace BitMono.Protections;

public class AntiILdasm : Protection
{
    private readonly MscorlibInjector m_Injector;

    public AntiILdasm(MscorlibInjector injector, ProtectionContext context) : base(context)
    {
        m_Injector = injector;
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        m_Injector.InjectAttribute(Context.Module, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute), Context.Module);
        return Task.CompletedTask;
    }
}