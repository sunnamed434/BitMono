namespace BitMono.Core;

public abstract class PhaseProtection : ProtectionBase, IPhaseProtection
{
    protected PhaseProtection(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}