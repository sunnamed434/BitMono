using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Protecting.Analyzing
{
    public class EventDefCriticalAnalyzer : ICriticalAnalyzer<EventDef>
    {
        public bool NotCriticalToMakeChanges(ProtectionContext context, EventDef eventDef)
        {
            return eventDef.IsRuntimeSpecialName == false;
        }
    }
}