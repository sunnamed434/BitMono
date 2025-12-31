namespace BitMono.Core.Analyzing;

public class CriticalInterfacesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalInterfacesCriticalAnalyzer(CriticalsSettings criticalsSettings)
    {
        _criticalsSettings = criticalsSettings;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (_criticalsSettings.UseCriticalInterfaces == false)
        {
            return true;
        }
        var criticalInterfaces = _criticalsSettings.CriticalInterfaces;
        if (type.Interfaces.Any(x => criticalInterfaces.FirstOrDefault(xx => xx.Equals(x.Interface?.Name)) != null))
        {
            return false;
        }
        return true;
    }
}