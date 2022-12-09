namespace BitMono.API.Protecting.Analyzing
{
    public interface ICriticalAnalyzer<in TObject>
    {
        bool NotCriticalToMakeChanges(TObject @object);
    }
}