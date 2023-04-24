#pragma warning disable CS8602
namespace BitMono.Core.Analyzing;

public class CriticalBaseTypesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly CriticalsSettings _criticalsSettings;

    public CriticalBaseTypesCriticalAnalyzer(IOptions<CriticalsSettings> criticals)
    {
        _criticalsSettings = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (_criticalsSettings.UseCriticalBaseTypes == false)
        {
            return true;
        }
        if (type.HasBaseType())
        {
            var criticalBaseTypes = _criticalsSettings.CriticalBaseTypes;
            if (criticalBaseTypes.FirstOrDefault(c => c.StartsWith(type.BaseType.Name.Value.Split('`')[0])) != null)
            {
                return false;
            }
        }
        return true;
    }
}