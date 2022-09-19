using BitMono.API.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Analyzing
{
    public class EventDefCriticalAnalyzer : ICriticalAnalyzer<EventDef>
    {
        public bool Analyze(EventDef eventDef)
        {
            return eventDef.IsRuntimeSpecialName == false;
        }
    }
}