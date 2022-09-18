using dnlib.DotNet;

namespace BitMono.Core.Extensions
{
    public static class PropertyDefExtensions
    {
        public static bool IsVirtual(this PropertyDef source)
        {
            return (source.GetMethod != null && source.GetMethod.IsVirtual
                || source.SetMethod != null && source.SetMethod.IsVirtual) == true;
        }
    }
}