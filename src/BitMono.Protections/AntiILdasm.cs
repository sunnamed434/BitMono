namespace BitMono.Protections;

[IL2CPPIncompatible("Injects SuppressIldasmAttribute, which only affects ildasm on the managed PE that IL2CPP discards")]
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