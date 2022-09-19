using BitMono.API.Analyzing;
using dnlib.DotNet;

namespace BitMono.Core.Analyzing
{
    public class FieldDefCriticalAnalyzerr : ICriticalAnalyzer<FieldDef>
    {
        public bool Analyze(FieldDef fieldDef)
        {
            return fieldDef.IsRuntimeSpecialName == false && fieldDef.IsLiteral == false && fieldDef.DeclaringType.IsEnum == false;
        }
    }
}