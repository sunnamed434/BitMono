namespace BitMono.Core.Analyzing;

[UsedImplicitly]
[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
public class CriticalInterfacesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalInterfacesCriticalAnalyzer(IOptions<CriticalsSettings> criticals)
    {
        _criticalsSettings = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (_criticalsSettings.UseCriticalInterfaces == false)
        {
            return true;
        }
        var criticalInterfaces = _criticalsSettings.CriticalInterfaces;
        if (type.Interfaces.Any(i => criticalInterfaces.FirstOrDefault(c => c.Equals(i.Interface?.Name)) != null))
        {
            return false;
        }
        return true;
    }
}