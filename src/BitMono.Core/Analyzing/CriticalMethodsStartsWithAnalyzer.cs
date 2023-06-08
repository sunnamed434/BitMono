namespace BitMono.Core.Analyzing;

[UsedImplicitly]
public class CriticalMethodsStartsWithAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalMethodsStartsWithAnalyzer(IOptions<CriticalsSettings> criticals)
    {
        _criticalsSettings = criticals.Value;
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