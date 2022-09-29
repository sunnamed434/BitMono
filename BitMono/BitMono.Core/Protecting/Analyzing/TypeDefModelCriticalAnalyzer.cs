using Autofac.Util;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using dnlib.DotNet;
using System;
using System.Linq;
using System.Reflection;

namespace BitMono.Core.Protecting.Analyzing
{
    public class TypeDefModelCriticalAnalyzer : ICriticalAnalyzer<TypeDef>
    {
        public bool NotCriticalToMakeChanges(ProtectionContext context, TypeDef typeDef)
        {
            var assemblyTypes = context.TargetAssembly.GetLoadableTypes();
            var type = assemblyTypes.FirstOrDefault(t => t.Name.Equals(typeDef.Name));
            if (type != null)
            {
                if (type.GetCustomAttribute<SerializableAttribute>(false) != null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}