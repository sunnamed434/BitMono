namespace BitMono.Core;

public abstract class PhaseProtection : ProtectionBase, IPhaseProtection
{
    protected PhaseProtection(ProtectionContext context) : base(context)
    {
    }
}