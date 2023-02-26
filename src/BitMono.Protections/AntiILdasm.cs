namespace BitMono.Protections;

public class AntiILdasm : Protection
{
    private readonly MscorlibInjector _injector;

    public AntiILdasm(MscorlibInjector injector, ProtectionContext context) : base(context)
    {
        _injector = injector;
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        _injector.InjectAttribute(Context.Module, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute), Context.Module);
        return Task.CompletedTask;
    }
}