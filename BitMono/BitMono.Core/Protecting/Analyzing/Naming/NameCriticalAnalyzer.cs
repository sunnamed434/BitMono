using BitMono.API.Protecting.Analyzing;
using BitMono.API.Protecting.Contexts;
using BitMono.Core.Configuration.Extensions;
using BitMono.Core.Protecting.Analyzing.TypeDefs;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace BitMono.Core.Protecting.Analyzing.Naming
{
    public class NameCriticalAnalyzer :
        ICriticalAnalyzer<string>,
        ICriticalAnalyzer<TypeDef>,
        ICriticalAnalyzer<MethodDef>
    {
        private readonly TypeDefCriticalInterfacesCriticalAnalyzer m_TypeDefCriticalInterfacesCriticalAnalyzer;
        private readonly TypeDefCriticalBaseTypesCriticalAnalyzer m_TypeDefCriticalBaseTypesCriticalAnalyzer;
        private readonly IConfiguration m_Configuration;

        public NameCriticalAnalyzer(
            TypeDefCriticalInterfacesCriticalAnalyzer typeDefCriticalInterfacesCriticalAnalyzer,
            TypeDefCriticalBaseTypesCriticalAnalyzer typeDefCriticalBaseTypesCriticalAnalyzer,
            IConfiguration configuration)
        {
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
    }
}