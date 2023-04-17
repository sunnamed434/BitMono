namespace BitMono.Core.Analyzing;

[UsedImplicitly]
[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
public class NameCriticalAnalyzer :
    ICriticalAnalyzer<TypeDefinition>,
    ICriticalAnalyzer<MethodDefinition>
{
    private readonly CriticalInterfacesCriticalAnalyzer _criticalInterfacesCriticalAnalyzer;
    private readonly CriticalBaseTypesCriticalAnalyzer _criticalBaseTypesCriticalAnalyzer;
    private readonly CriticalMethodsCriticalAnalyzer _criticalMethodsCriticalAnalyzer;
    private readonly CriticalMethodsStartsWithAnalyzer _criticalMethodsStartsWithAnalyzer;

    public NameCriticalAnalyzer(
        CriticalInterfacesCriticalAnalyzer criticalInterfacesCriticalAnalyzer,
        CriticalBaseTypesCriticalAnalyzer criticalBaseTypesCriticalAnalyzer,
        CriticalMethodsCriticalAnalyzer criticalMethodsCriticalAnalyzer,
        CriticalMethodsStartsWithAnalyzer criticalMethodsStartsWithAnalyzer)
    {
        _criticalInterfacesCriticalAnalyzer = criticalInterfacesCriticalAnalyzer;
        _criticalBaseTypesCriticalAnalyzer = criticalBaseTypesCriticalAnalyzer;
        _criticalMethodsCriticalAnalyzer = criticalMethodsCriticalAnalyzer;
        _criticalMethodsStartsWithAnalyzer = criticalMethodsStartsWithAnalyzer;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (_criticalInterfacesCriticalAnalyzer.NotCriticalToMakeChanges(type) == false)
        {
            return false;
        }
        if (_criticalBaseTypesCriticalAnalyzer.NotCriticalToMakeChanges(type) == false)
        {
            return false;
        }
        return true;
    }
    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (_criticalMethodsCriticalAnalyzer.NotCriticalToMakeChanges(method) == false)
        {
            return false;
        }
        if (_criticalMethodsStartsWithAnalyzer.NotCriticalToMakeChanges(method) == false)
        {
            return false;
        }
        return true;
    }
}