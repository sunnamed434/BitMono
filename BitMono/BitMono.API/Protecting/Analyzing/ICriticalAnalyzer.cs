using BitMono.API.Protecting.Context;

namespace BitMono.API.Protecting.Analyzing
{
    public interface ICriticalAnalyzer<in TObject>
    {
        bool NotCriticalToMakeChanges(ProtectionContext context, TObject @object);
    }
}