using dnlib.DotNet;

namespace BitMono.Core.Extensions
{
    public static class MethodDefExtensions
    {
        public static bool NotCriticalToMakeChanges(this MethodDef source)
        {
            return source.IsRuntimeSpecialName == false && source.DeclaringType.IsForwarder == false;
        }
    }
}