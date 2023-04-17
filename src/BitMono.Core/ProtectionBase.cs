namespace BitMono.Core;

public abstract class ProtectionBase : IProtection
{
    protected ProtectionContext Context { get; }
    protected IServiceProvider ServiceProvider { get; }

    protected ProtectionBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Context = ServiceProvider
            .GetRequiredService<ProtectionContextFactory>()
            .Create(this);
    }

    public abstract Task ExecuteAsync();
}