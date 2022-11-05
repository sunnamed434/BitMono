using BitMono.API.Protecting.Analyzing;
using BitMono.API.Protecting.Context;
using dnlib.DotNet;

namespace BitMono.Core.Protecting.Analyzing.DnlibDefs
{
    public class DnlibDefCriticalAnalyzer : ICriticalAnalyzer<IDnlibDef>
    {
        public bool NotCriticalToMakeChanges(ProtectionContext context, IDnlibDef dnlibDef)
        {
            if (dnlibDef is TypeDef typeDef)
            {
                return typeDef.IsRuntimeSpecialName == false;
            }
            if (dnlibDef is FieldDef fieldDef)
            {
                return fieldDef.IsRuntimeSpecialName == false
                    && fieldDef.IsLiteral == false
                    && fieldDef.DeclaringType.IsEnum == false;
            }
            if (dnlibDef is MethodDef methodDef)
            {
                return methodDef.IsRuntimeSpecialName && methodDef.DeclaringType.IsForwarder
                    ? false
                    : true;
            }
            if (dnlibDef is EventDef eventDef)
            {
                return eventDef.IsRuntimeSpecialName == false;
            }
            return true;
        }
    }
}