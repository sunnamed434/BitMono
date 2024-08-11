namespace BitMono.API.Analyzing;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface ICriticalAnalyzer<in TObject>
{
    bool NotCriticalToMakeChanges(TObject @object);
}