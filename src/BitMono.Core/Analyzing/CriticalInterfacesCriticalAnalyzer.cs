namespace BitMono.Core.Analyzing;

public class CriticalInterfacesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly Criticals m_Criticals;

    public CriticalInterfacesCriticalAnalyzer(IOptions<Criticals> criticals)
    {
        m_Criticals = criticals.Value;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (m_Criticals.UseCriticalInterfaces == false)
        {
            return true;
        }
        var criticalInterfaces = m_Criticals.CriticalInterfaces;
        if (type.Interfaces.Any(i => criticalInterfaces.FirstOrDefault(c => c.Equals(i.Interface.Name)) != null))
        {
            return false;
        }
        return true;
    }
}