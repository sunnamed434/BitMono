namespace BitMono.Core.Analyzing;

[SuppressMessage("ReSharper", "InvertIf")]
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
            var criticalBaseTypes = _criticalsSettings.CriticalBaseTypes!;
            var typeBaseTypeName = type.BaseType?.Name?.Value.Split('`')[0] ?? string.Empty;
            if (criticalBaseTypes.FirstOrDefault(c => c.StartsWith(typeBaseTypeName)) != null)
            {
                return false;
            }
        }
        return true;
    }
}