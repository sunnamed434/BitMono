namespace BitMono.Core;

public abstract class PackerProtection : ProtectionBase, IPacker
{
    protected PackerProtection(ProtectionContext context) : base(context)
    {
    }
}