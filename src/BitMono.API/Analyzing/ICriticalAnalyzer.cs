namespace BitMono.API.Analyzing;

public interface ICriticalAnalyzer<in TObject>
{
    bool NotCriticalToMakeChanges(TObject @object);
}