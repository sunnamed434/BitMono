namespace BitMono.Protections;

[UsedImplicitly]
public class AntiILdasm : Protection
{
    public AntiILdasm(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        MscorlibInjector.InjectAttribute(Context.Module, typeof(SuppressIldasmAttribute).Namespace!,
            nameof(SuppressIldasmAttribute), Context.Module);
        return Task.CompletedTask;
    }
}