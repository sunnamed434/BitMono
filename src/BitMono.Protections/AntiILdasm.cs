namespace BitMono.Protections;

[UsedImplicitly]
public class AntiILdasm : Protection
{
    public AntiILdasm(ProtectionContext context) : base(context)
    {
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        MscorlibInjector.InjectAttribute(Context.Module, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute), Context.Module);
        return Task.CompletedTask;
    }
}