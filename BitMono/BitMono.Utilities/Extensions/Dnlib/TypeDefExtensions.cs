using dnlib.DotNet;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class TypeDefExtensions
    {
        public static bool HasBaseType(this TypeDef source)
        {
            return source.BaseType != null;
        }
        public static bool HasNamespace(this TypeDef source)
        {
            return UTF8String.IsNullOrEmpty(source.Namespace) == false;
        }
    }
}