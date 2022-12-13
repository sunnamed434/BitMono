using dnlib.DotNet;
using System;
using System.Collections.Generic;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class ModuleDefExtensions
    {
        public static TypeDef ResolveTypeDefOrThrow(this ModuleDefMD source, Type fromType)
        {
            return source.ResolveTypeDef(MDToken.ToRID(fromType.MetadataToken)).ResolveTypeDefThrow();
        }
        public static TypeDef ResolveTypeDefOrThrow<TType>(this ModuleDefMD source)
        {
            return ResolveTypeDefOrThrow(source, typeof(TType));
        }
        public static IEnumerable<IDnlibDef> FindDefinitions(this ModuleDef source)
        {
            yield return source;
            foreach (var typeDef in source.GetTypes())     
            {
                yield return typeDef;
                foreach (var methodDef in typeDef.Methods)
                {
                    yield return methodDef;
                }
                foreach (var fieldDef in typeDef.Fields)
                {
                    yield return fieldDef;
                }
                foreach (var propertyDef in typeDef.Properties)
                {
                    yield return propertyDef;
                }
                foreach (var eventDef in typeDef.Events)
                {
                    yield return eventDef;
                }
            }
        }
    }
}