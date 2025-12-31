namespace BitMono.Core;

public abstract class PackerProtection : ProtectionBase, IPacker
{
    protected PackerProtection(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}