namespace BitMono.Core;

public abstract class Protection : ProtectionBase
{
    protected Protection(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}