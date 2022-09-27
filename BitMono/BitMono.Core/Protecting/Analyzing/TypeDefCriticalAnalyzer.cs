using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Protecting.Analyzing
{
    public class TypeDefCriticalAnalyzer : ICriticalAnalyzer<TypeDef>
    {
        public bool NotCriticalToMakeChanges(ProtectionContext context, TypeDef typeDef)
        {
            return typeDef.IsRuntimeSpecialName == false;
        }
    }
}