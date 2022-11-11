using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Configuration.Extensions;
using BitMono.Core.Protecting.Resolvers;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Obfuscation
{
    public class ProtectionsSorter
    {
        private readonly IConfiguration m_Configuration;
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;
        private readonly AssemblyDef m_ModuleDefMDAssembly;
        private readonly ILogger m_Logger;

        public ProtectionsSorter(
            IBitMonoProtectionsConfiguration configuration,
            IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver,
            AssemblyDef moduleDefMDAssembly,
            ILogger logger)
        {
            m_Configuration = configuration.Configuration;
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
            m_ModuleDefMDAssembly = moduleDefMDAssembly;
            m_Logger = logger;
        }

        public Task<ProtectionsSortingResult> SortAsync(ICollection<IProtection> protections)
        {
            protections = new DependencyResolver(protections, m_Configuration.GetProtectionSettings(), m_Logger)
                .Sort(out ICollection<string> skipped);
            var stageProtections = protections.Where(p => p is IStageProtection).Cast<IStageProtection>();
            var pipelineProtections = protections.Where(p => p is IPipelineProtection).Cast<IPipelineProtection>();
            var obfuscationAttributeExcludingProtections = protections.Where(p =>
                m_ObfuscationAttributeExcludingResolver.TryResolve(m_ModuleDefMDAssembly, p.GetType().Name,
                out ObfuscationAttribute obfuscationAttribute) && obfuscationAttribute.Exclude);

            protections.Except(stageProtections).Except(obfuscationAttributeExcludingProtections);

            return Task.FromResult(new ProtectionsSortingResult
            {
                Protections = protections,
                Skipped = skipped,
                StageProtections = stageProtections,
                PipelineProtections = pipelineProtections,
                ObfuscationAttributeExcludingProtections = obfuscationAttributeExcludingProtections
            });
        }
    }
}