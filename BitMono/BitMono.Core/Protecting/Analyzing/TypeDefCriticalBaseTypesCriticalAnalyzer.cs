using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.Core.Configuration.Extensions;
using BitMono.Utilities.Extensions.Dnlib;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace BitMono.Core.Protecting.Analyzing
{
    public class TypeDefCriticalBaseTypesCriticalAnalyzer : ICriticalAnalyzer<TypeDef>
    {
        private readonly IConfiguration m_Configuration;

        public TypeDefCriticalBaseTypesCriticalAnalyzer(IConfiguration configuration)
        {
            m_Configuration = configuration;
        }


        public bool NotCriticalToMakeChanges(ProtectionContext context, TypeDef typeDef)
        {
            if (typeDef.HasBaseType())
            {
                var criticalBaseTypes = m_Configuration.GetCriticalBaseTypes();
                if (criticalBaseTypes.FirstOrDefault(c => c.StartsWith(typeDef.BaseType.TypeName.Split('`')[0])) != null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}