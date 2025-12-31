namespace BitMono.Protections;

public class AntiILdasm : Protection
{
    public AntiILdasm(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        MscorlibInjector.InjectAttribute(Context.Module, typeof(SuppressIldasmAttribute).Namespace!,
            nameof(SuppressIldasmAttribute), Context.Module);
        return Task.CompletedTask;
    }
}