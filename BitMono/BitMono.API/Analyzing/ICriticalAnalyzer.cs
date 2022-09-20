namespace BitMono.API.Analyzing
{
    public interface ICriticalAnalyzer<TObject>
    {
        bool Analyze(TObject @object);
    }
}