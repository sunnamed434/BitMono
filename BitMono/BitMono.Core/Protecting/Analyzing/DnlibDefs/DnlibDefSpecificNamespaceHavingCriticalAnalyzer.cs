using BitMono.API.Configuration;
using BitMono.API.Protecting.Analyzing;
using BitMono.Core.Extensions.Configuration;
using BitMono.Shared.Models;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace BitMono.Core.Protecting.Analyzing.DnlibDefs
{
    public class DnlibDefSpecificNamespaceHavingCriticalAnalyzer : ICriticalAnalyzer<IDnlibDef>
    {
        private readonly IConfiguration m_Configuration;

        public DnlibDefSpecificNamespaceHavingCriticalAnalyzer(IBitMonoObfuscationConfiguration configuration)
        {
            m_Configuration = configuration.Configuration;
        }

        public bool NotCriticalToMakeChanges(IDnlibDef dnlibDef)
        {
            if (m_Configuration.GetValue<bool>(nameof(Obfuscation.SpecificNamespacesObfuscationOnly)) == false)
            {
                return true;
            }

            var specificNamespaces = m_Configuration.GetSpecificNamespaces();
            if (dnlibDef is TypeDef typeDef && typeDef.HasNamespace()) 
            {
                if (specificNamespaces.Any(n => n.Equals(typeDef.Namespace.String)) == false)
                {
                    return false;
                }
            }
            if (dnlibDef is MethodDef methodDef && methodDef.DeclaringType.HasNamespace())
            {
                if (specificNamespaces.Any(n => n.Equals(methodDef.DeclaringType.Namespace.String)) == false)
                {
                    return false;
                }
            }
            if (dnlibDef is FieldDef fieldDef && fieldDef.DeclaringType.HasNamespace())
            {
                if (specificNamespaces.Any(n => n.Equals(fieldDef.DeclaringType.Namespace.String)) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}