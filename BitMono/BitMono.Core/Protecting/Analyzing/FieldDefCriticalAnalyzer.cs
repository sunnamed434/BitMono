using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Protecting.Analyzing
{
    public class FieldDefCriticalAnalyzer : ICriticalAnalyzer<FieldDef>
    {
        public bool NotCriticalToMakeChanges(ProtectionContext context, FieldDef fieldDef)
        {
            return fieldDef.IsRuntimeSpecialName == false
                    && fieldDef.IsLiteral == false
                    && fieldDef.DeclaringType.IsEnum == false;
        }
    }
}