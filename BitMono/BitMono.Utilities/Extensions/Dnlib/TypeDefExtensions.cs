using dnlib.DotNet;

namespace BitMono.Utilities.Extensions.Dnlib
{
    public static class TypeDefExtensions
    {
        public static bool HasBaseType(this TypeDef source)
        {
            return source.BaseType != null;
        }
    }
}