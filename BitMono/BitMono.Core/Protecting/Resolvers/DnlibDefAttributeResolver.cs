using Autofac.Util;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Resolvers;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using NullGuard;
using System;
using System.Linq;
using System.Reflection;

namespace BitMono.Core.Protecting.Resolvers
{
    public class DnlibDefAttributeResolver : IDnlibDefAttributeResolver
    {
        [return: AllowNull]
        public TAttribute ResolveOrDefault<TAttribute>(ProtectionContext context, IDnlibDef dnlibDef, [AllowNull] Func<TAttribute, bool> predicate = default, bool inherit = false)
            where TAttribute : Attribute
        {
            var types = context.Assembly.GetLoadableTypes();
            if (dnlibDef is TypeDef typeDef)
            {
                var type = DnlibDefReflectionUtility.GetType(types, typeDef);
                if (type != null)
                {
                    if (predicate == null)
                    {
                        return type.GetCustomAttribute<TAttribute>(inherit);
                    }
                    else
                    {
                        return type.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault(predicate);
                    }
                }
            }
            if (dnlibDef is FieldDef fieldDef)
            {
                var field = types.FirstOrDefault(t => t.GetFields().FirstOrDefault(f => f.Name.Equals(fieldDef.Name)) != null);
                if (field != null)
                {
                    if (predicate == null)
                    {
                        return field.GetCustomAttribute<TAttribute>(inherit);
                    }
                    else
                    {
                        return field.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault(predicate);
                    }
                }
            }
            if (dnlibDef is AssemblyDef assemblyDef)
            {
                var assembly = DnlibDefReflectionUtility.GetAssembly(AppDomain.CurrentDomain.GetAssemblies(), assemblyDef);
                if (assembly != null)
                {
                    if (predicate == null)
                    {
                        return assembly.GetCustomAttribute<TAttribute>();
                    }
                    else
                    {
                        return assembly.GetCustomAttributes<TAttribute>().FirstOrDefault(predicate);
                    }
                }
            }
            return default(TAttribute);
        }
    }
}