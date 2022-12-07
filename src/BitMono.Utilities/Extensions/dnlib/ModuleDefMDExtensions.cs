using dnlib.DotNet;
using System;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class ModuleDefMDExtensions
    {
        public static TypeDef ResolveTypeDefOrThrow(this ModuleDefMD source, Type fromType)
        {
            return source.ResolveTypeDef(MDToken.ToRID(fromType.MetadataToken)).ResolveTypeDefThrow();
        }
        public static TypeDef ResolveTypeDefOrThrow<TType>(this ModuleDefMD source)
        {
            return ResolveTypeDefOrThrow(source, typeof(TType));
        }
    }
}