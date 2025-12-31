namespace BitMono.Core.Analyzing;

public class CriticalMethodsStartsWithAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalMethodsStartsWithAnalyzer(CriticalsSettings criticalsSettings)
    {
        _criticalsSettings = criticalsSettings;
    }

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (_criticalsSettings.UseCriticalMethodsStartsWith == false)
        {
            return true;
        }

        var criticalMethodsStartWith = _criticalsSettings.CriticalMethodsStartsWith;
        return criticalMethodsStartWith.Any(c => c.StartsWith(method.Name)) == false;
    }
}