namespace BitMono.API.Analyzing
{
    public interface ICriticalAnalyzer { }
    public interface ICriticalAnalyzer<TObject>
    {
        bool Analyze(TObject @object);
    }
}