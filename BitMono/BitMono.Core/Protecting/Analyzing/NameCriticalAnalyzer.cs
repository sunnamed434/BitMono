using Autofac.Util;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.Core.Configuration.Extensions;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace BitMono.Core.Protecting.Analyzing
{
    public class NameCriticalAnalyzer :
        ICriticalAnalyzer<string>, 
        ICriticalAnalyzer<TypeDef>, 
        ICriticalAnalyzer<MethodDef>
    {
        private readonly IConfiguration m_Configuration;

        public NameCriticalAnalyzer(IConfiguration configuration)
        {
            m_Configuration = configuration;
        }


        public bool NotCriticalToMakeChanges(ProtectionContext context, string text)
        {
            var criticalMethodNames = m_Configuration.GetCriticalMethods();
            if (criticalMethodNames.Any(c => c.Equals(text)))
            {
                return false;
            }
            return true;
        }
        public bool NotCriticalToMakeChanges(ProtectionContext context, TypeDef typeDef)
        {
            var assemblyTypes = context.TargetAssembly.GetLoadableTypes();
            var type = assemblyTypes.FirstOrDefault(t => t.Name.Equals(typeDef.Name));

            if (type != null && type.GetCustomAttribute<SerializableAttribute>() != null)
            {
                return false;
            }

            var criticalInterfaces = m_Configuration.GetCriticalInterfaces();
            var criticalBaseTypes = m_Configuration.GetCriticalBaseTypes();
            if (typeDef.Interfaces.Any(i => criticalInterfaces.FirstOrDefault(c => c.Equals(i.Interface.Name)) != null)
                || criticalBaseTypes.FirstOrDefault(c => c.StartsWith(typeDef.BaseType.TypeName.Split('`')[0])) != null)
            {
                return false;
            }
            return true;
        }
        public bool NotCriticalToMakeChanges(ProtectionContext context, MethodDef methodDef)
        {
            var criticalMethodNames = m_Configuration.GetCriticalMethods();
            if (criticalMethodNames.Any(c => c.Equals(methodDef.Name)))
            {
                return false;
            }
            return true;
        }
    }
}