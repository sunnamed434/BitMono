namespace BitMono.Core.Analyzing;

public class CriticalMethodsCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly Criticals m_Criticals;

    public CriticalMethodsCriticalAnalyzer(IOptions<Criticals> criticals)
    {
        m_Criticals = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (m_Criticals.UseCriticalMethods == false)
        {
            return true;
        }
        var criticalMethodNames = m_Criticals.CriticalMethods;
        if (criticalMethodNames.Any(c => c.Equals(method.Name)))
        {
            return false;
        }
        return true;
    }
}