namespace BitMono.Core.Analyzing;

public class CriticalMethodsCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalMethodsCriticalAnalyzer(CriticalsSettings criticalsSettings)
    {
        _criticalsSettings = criticalsSettings;
    }

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (_criticalsSettings.UseCriticalMethods == false)
        {
            return true;
        }
        var criticalMethodNames = _criticalsSettings.CriticalMethods;
        return criticalMethodNames.Any(x => x.Equals(method.Name)) == false;
    }
}