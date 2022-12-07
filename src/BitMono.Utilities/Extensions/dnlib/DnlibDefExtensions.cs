using dnlib.DotNet;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class DnlibDefExtensions
    {
        public static MethodDef ResolveMethodDefOrThrow(this IDnlibDef source) 
        {
            if (source is MethodDef methodDef) 
            {
                return methodDef.ResolveMethodDefThrow();
            }
            throw new MemberRefResolveException($"Could not resolve method: {source}");
        }
        public static FieldDef ResolveFieldDefOrThrow(this IDnlibDef source)
        {
            if (source is FieldDef fieldDef) 
            {
                return fieldDef.ResolveFieldDefThrow();
            }
            throw new MemberRefResolveException($"Could not resolve field: {source}");
        }
    }
}