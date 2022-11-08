using BitMono.API.Protecting.Analyzing;
using BitMono.Core.Configuration.Extensions;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace BitMono.Core.Protecting.Analyzing.TypeDefs
{
    public class TypeDefCriticalInterfacesCriticalAnalyzer : ICriticalAnalyzer<TypeDef>
    {
        private readonly IConfiguration m_Configuration;

        public TypeDefCriticalInterfacesCriticalAnalyzer(IConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        public bool NotCriticalToMakeChanges(TypeDef typeDef)
        {
            var criticalInterfaces = m_Configuration.GetCriticalInterfaces();
            if (typeDef.Interfaces.Any(i => criticalInterfaces.FirstOrDefault(c => c.Equals(i.Interface.Name)) != null))
            {
                return false;
            }
            return true;
        }
    }
}