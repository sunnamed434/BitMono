namespace BitMono.Core.Protecting;

public abstract class PackerProtection : ProtectionBase, IPacker
{
    protected PackerProtection(ProtectionContext context) : base(context)
    {
    }
}