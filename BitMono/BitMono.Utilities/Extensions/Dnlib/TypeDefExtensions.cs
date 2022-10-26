using dnlib.DotNet;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class TypeDefExtensions
    {
        public static bool HasBaseType(this TypeDef source)
        {
            return source.BaseType != null;
        }
    }
}