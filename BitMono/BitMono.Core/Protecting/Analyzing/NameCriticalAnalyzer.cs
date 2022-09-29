using Autofac.Util;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.Core.Configuration.Extensions;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace BitMono.Core.Protecting.Analyzing
{
    public class NameCriticalAnalyzer :
        ICriticalAnalyzer<string>, 
        ICriticalAnalyzer<TypeDef>, 
        ICriticalAnalyzer<MethodDef>,
        ICriticalAnalyzer<FieldDef>
    {
        private readonly TypeDefModelCriticalAnalyzer m_TypeDefModelCriticalAnalyzer;
        private readonly TypeDefCriticalInterfacesCriticalAnalyzer m_TypeDefCriticalInterfacesCriticalAnalyzer;
        private readonly TypeDefCriticalBaseTypesCriticalAnalyzer m_TypeDefCriticalBaseTypesCriticalAnalyzer;
        private readonly IConfiguration m_Configuration;

        public NameCriticalAnalyzer(
            TypeDefModelCriticalAnalyzer typeDefModelCriticalAnalyzer, 
            TypeDefCriticalInterfacesCriticalAnalyzer typeDefCriticalInterfacesCriticalAnalyzer,
            TypeDefCriticalBaseTypesCriticalAnalyzer typeDefCriticalBaseTypesCriticalAnalyzer,
            IConfiguration configuration)
        {
            m_TypeDefModelCriticalAnalyzer = typeDefModelCriticalAnalyzer;
            m_TypeDefCriticalInterfacesCriticalAnalyzer = typeDefCriticalInterfacesCriticalAnalyzer;
            m_TypeDefCriticalBaseTypesCriticalAnalyzer = typeDefCriticalBaseTypesCriticalAnalyzer;
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
            if (m_TypeDefModelCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef) == false)
            {
                return false;
            }
            if (m_TypeDefCriticalInterfacesCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef) == false)
            {
                return false;
            }
            if (m_TypeDefCriticalBaseTypesCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef) == false)
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
        public bool NotCriticalToMakeChanges(ProtectionContext context, FieldDef fieldDef)
        {
            var assemblyTypes = context.TargetAssembly.GetLoadableTypes();
            var type = assemblyTypes.FirstOrDefault(t => t.Name.Equals(fieldDef.DeclaringType.Name));

            if (type != null)
            {
                var fields = type.GetFields().Where(f =>
                    f.GetCustomAttribute<JsonPropertyAttribute>(false) != null
                    || f.GetCustomAttribute<XmlAttributeAttribute>(false) != null);
                if (fields.Any(f => f.Name == fieldDef.Name))
                {
                    return false;
                }
            }
            return true;
        }
    }
}