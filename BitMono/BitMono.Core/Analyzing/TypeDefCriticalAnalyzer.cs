using BitMono.API.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Analyzing
{
    public class TypeDefCriticalAnalyzer : ICriticalAnalyzer<TypeDef>
    {
        public bool Analyze(TypeDef typeDef)
        {
            return typeDef.IsRuntimeSpecialName == false;
        }
    }
}