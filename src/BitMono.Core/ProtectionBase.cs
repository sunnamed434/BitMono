namespace BitMono.Core;

public abstract class ProtectionBase : IProtection
{
    protected ProtectionContext Context { get; }
    protected IBitMonoServiceProvider ServiceProvider { get; }

    protected ProtectionBase(IBitMonoServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Context = ServiceProvider
            .GetRequiredService<ProtectionContextFactory>()
            .Create(this);
    }

    public abstract Task ExecuteAsync();
}