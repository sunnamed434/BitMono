namespace BitMono.Core;

public abstract class ProtectionBase : IProtection
{
    protected ProtectionContext Context { get; }

    protected ProtectionBase(ProtectionContext context)
    {
        Context = context;
    }

    public abstract Task ExecuteAsync(ProtectionParameters parameters);
}