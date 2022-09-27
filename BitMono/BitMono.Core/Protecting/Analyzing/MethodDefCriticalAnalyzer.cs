using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Protecting.Analyzing
{
    public class MethodDefCriticalAnalyzer : ICriticalAnalyzer<MethodDef>
    {
        public bool NotCriticalToMakeChanges(ProtectionContext context, MethodDef methodDef)
        {
            if (methodDef.IsNoInlining)
            {
                return false;
            }

            if (methodDef.IsRuntimeSpecialName
                && methodDef.DeclaringType.IsForwarder)
            {
                return false;
            }
            return true;
        }
    }
}