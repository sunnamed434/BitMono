using dnlib.DotNet;
using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class DnlibDefReflectionUtility
    {
        [return: AllowNull]
        public static Type GetType(IEnumerable<Type> types, TypeDef typeDef)
        {
            return types.FirstOrDefault(t => t.Name.Equals(typeDef.Name));
        }
        [return: AllowNull]
        public static Type GetType(IEnumerable<Type> types, FieldDef fieldDef)
        {
            return types.FirstOrDefault(t => t.GetFields().FirstOrDefault(f => f.Name.Equals(fieldDef.Name)) != null);
        }
        [return: AllowNull]
        public static Assembly GetAssembly(Assembly[] assemblies, AssemblyDef assemblyDef)
        {
            return assemblies.FirstOrDefault(a => a.GetName().Name.Equals(assemblyDef.Name));
        }
    }
}