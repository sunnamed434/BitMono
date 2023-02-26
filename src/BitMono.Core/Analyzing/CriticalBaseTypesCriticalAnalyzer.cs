#pragma warning disable CS8602
namespace BitMono.Core.Analyzing;

public class CriticalBaseTypesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly Criticals m_Criticals;

    public CriticalBaseTypesCriticalAnalyzer(IOptions<Criticals> criticals)
    {
        m_Criticals = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (m_Criticals.UseCriticalBaseTypes == false)
        {
            return true;
        }
        if (type.HasBaseType())
        {
            var criticalBaseTypes = m_Criticals.CriticalBaseTypes;
            if (criticalBaseTypes.FirstOrDefault(c => c.StartsWith(type.BaseType.Name.Value.Split('`')[0])) != null)
            {
                return false;
            }
        }
        return true;
    }
}