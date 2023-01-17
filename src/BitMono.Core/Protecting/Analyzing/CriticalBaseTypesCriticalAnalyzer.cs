namespace BitMono.Core.Protecting.Analyzing;

public class CriticalBaseTypesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly Criticals m_Criticals;

    public CriticalBaseTypesCriticalAnalyzer(IOptions<Criticals> criticals)
    {
        m_Criticals = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition typeDefinition)
    {
        if (m_Criticals.UseCriticalBaseTypes == false)
        {
            return true;
        }
        if (typeDefinition.HasBaseType())
        {
            var criticalBaseTypes = m_Criticals.CriticalBaseTypes;
            if (criticalBaseTypes.FirstOrDefault(c => c.StartsWith(typeDefinition.BaseType.Name.Value.Split('`')[0])) != null)
            {
                return false;
            }
        }
        return true;
    }
}