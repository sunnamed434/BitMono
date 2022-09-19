using BitMono.API.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Analyzing
{
    public class MethodDefCriticalAnalyzer : ICriticalAnalyzer<MethodDef>
    {
        public bool Analyze(MethodDef methodDef)
        {
            return methodDef.IsRuntimeSpecialName == false && methodDef.DeclaringType.IsForwarder == false;
        }
    }
}