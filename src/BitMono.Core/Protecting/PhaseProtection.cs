namespace BitMono.Core.Protecting;

public abstract class PhaseProtection : ProtectionBase, IPhaseProtection
{
    protected PhaseProtection(ProtectionContext context) : base(context)
    {
    }
}