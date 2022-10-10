using Autofac.Util;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using dnlib.DotNet;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace BitMono.Core.Protecting.Analyzing
{
    public class TypeDefModelCriticalAnalyzer : ICriticalAnalyzer<TypeDef>
    {
        private readonly Type[] m_CriticalTypeAttributes = new Type[]
        {
            typeof(SerializableAttribute),
        };
        private readonly Type[] m_CriticalFieldPropertyAttributes = new Type[]
        {
            typeof(XmlAttributeAttribute),
            typeof(XmlArrayItemAttribute),
            typeof(JsonPropertyAttribute),
        };

        public bool NotCriticalToMakeChanges(ProtectionContext context, TypeDef typeDef)
        {
            var assemblyTypes = context.Assembly.GetLoadableTypes();
            var type = assemblyTypes.FirstOrDefault(t => t.Name.Equals(typeDef.ReflectionName));
            if (type != null)
            {
                if (m_CriticalTypeAttributes.Any(c => type.GetCustomAttribute(c, false) != null))
                {
                    return false;
                }

                if (typeDef.HasFields)
                {
                    var field = type.GetFields().FirstOrDefault(f => m_CriticalFieldPropertyAttributes.Any(c => f.GetCustomAttribute(c, false) != null));
                    if (field != null)
                    {
                        return false;
                    }
                }

                if (typeDef.HasProperties)
                {
                    var field = type.GetProperties().FirstOrDefault(f => m_CriticalFieldPropertyAttributes.Any(c => f.GetCustomAttribute(c, false) != null));
                    if (field != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}