namespace BitMono.Core.Analyzing;

[UsedImplicitly]
public class CriticalMethodsCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalMethodsCriticalAnalyzer(IOptions<CriticalsSettings> criticals)
    {
        _criticalsSettings = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (_criticalsSettings.UseCriticalMethods == false)
        {
            return true;
        }
        var criticalMethodNames = _criticalsSettings.CriticalMethods;
        if (criticalMethodNames.Any(c => c.Equals(method.Name)))
        {
            return false;
        }
        return true;
    }
}