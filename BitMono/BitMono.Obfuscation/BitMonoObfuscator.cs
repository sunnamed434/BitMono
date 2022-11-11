using Autofac;
using Autofac.Extensions.DependencyInjection;
using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Resolvers;
using BitMono.API.Protecting.Writers;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Obfuscation
{
    public class BitMonoObfuscator
    {
        private readonly IBitMonoObfuscationConfiguration m_ObfuscationConfiguratin;
        private readonly IBitMonoProtectionsConfiguration m_ProtectionsConfiguration;
        private readonly IBitMonoModuleFileResolver m_BitMonoModuleFileResolver;
        private readonly AutofacServiceProvider m_ServiceProvider;
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;
        private readonly IModuleDefMDWriter m_ModuleDefMDWriter;
        private readonly ILogger m_Logger;

        public BitMonoObfuscator(
            IBitMonoObfuscationConfiguration obfuscationConfiguration,
            IBitMonoProtectionsConfiguration protectionsConfiguration,
            IBitMonoModuleFileResolver bitMonoModuleFileResolver,
            AutofacServiceProvider serviceProvider,
            IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver,
            IModuleDefMDWriter moduleDefMDWriter,
            ILogger logger)
        {
            m_ObfuscationConfiguratin = obfuscationConfiguration;
            m_ProtectionsConfiguration = protectionsConfiguration;
            m_BitMonoModuleFileResolver = bitMonoModuleFileResolver;
            m_ServiceProvider = serviceProvider;
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
            m_ModuleDefMDWriter = moduleDefMDWriter;
            m_Logger = logger.ForContext<BitMonoObfuscator>();
        }

        public async Task ObfuscateAsync(BitMonoContext context, ModuleDefMD externalComponentsModuleDefMD, CancellationToken cancellationToken = default)
        {
            try
            {
                context.ModuleFile = await m_BitMonoModuleFileResolver.ResolveAsync(context.BaseDirectory);
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Failed to resolve module file!");
                return;
            }

            var moduleDefMDCreationResult = await new ModuleDefMDCreator().CreateAsync(context.ModuleFile);
            var protectionContext = await new ProtectionContextCreator(moduleDefMDCreationResult, externalComponentsModuleDefMD, context).CreateAsync();
            m_Logger.Information("Loaded Module {0}", moduleDefMDCreationResult.ModuleDefMD.Name);

            var protectionsSortingResult = await new ProtectionsSorter(m_ProtectionsConfiguration, m_ObfuscationAttributeExcludingResolver, moduleDefMDCreationResult.ModuleDefMD.Assembly, m_Logger)
                .SortAsync(m_ServiceProvider.LifetimeScope.Resolve<ICollection<IProtection>>());

            await new ProtectionsExecutionNotifier(m_Logger).NotifyAsync(protectionsSortingResult);

            var stringBuilder = new StringBuilder()
                .Append(Path.GetFileNameWithoutExtension(context.ModuleFile));
            if (context.Watermark)
            {
                stringBuilder.
                    Append("_bitmono");
            }
            stringBuilder.Append(Path.GetExtension(context.ModuleFile));
            var outputFile = Path.Combine(context.OutputDirectory, stringBuilder.ToString());
            context.OutputModuleFile = outputFile;

            m_Logger.Information("Preparing to protect module: {0}", context.ModuleFile);
            await new BitMonoEngine(m_ObfuscationConfiguratin, protectionsSortingResult.Protections, context, protectionContext, m_ModuleDefMDWriter, m_Logger)
                .StartAsync(cancellationToken);
            m_Logger.Information("Protected module`s saved in {0}", context.OutputDirectory);
        }
    }
}