using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Resolvers;
using BitMono.API.Protecting.Writers;
using BitMono.Core.Models;
using BitMono.Obfuscation.API;
using dnlib.DotNet;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Obfuscation
{
    public class BitMonoEngine
    {
        private readonly IServiceProvider m_ServiceProvider;
        private readonly IModuleDefMDWriter m_ModuleDefMDWriter;
        private readonly IModuleDefMDCreator m_ModuleDefMDCreator;
        private readonly IDnlibDefFeatureObfuscationAttributeHavingResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly IBitMonoObfuscationConfiguration m_ObfuscationConfiguratin;
        private readonly ILogger m_Logger;

        public BitMonoEngine(
            IServiceProvider serviceProvider,
            IModuleDefMDWriter moduleDefMDWriter,
            IModuleDefMDCreator moduleDefMDCreator,
            ILogger logger)
        {
            m_ServiceProvider = serviceProvider;
            m_ModuleDefMDWriter = moduleDefMDWriter;
            m_ModuleDefMDCreator = moduleDefMDCreator;
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = m_ServiceProvider.GetRequiredService<IDnlibDefFeatureObfuscationAttributeHavingResolver>();
            m_ObfuscationConfiguratin = m_ServiceProvider.GetRequiredService<IBitMonoObfuscationConfiguration>();
            m_Logger = logger.ForContext<BitMonoEngine>();
        }

        public async Task ObfuscateAsync(BitMonoContext context, ModuleDefMD externalComponentsModuleDefMD, ICollection<IProtection> protections, List<ProtectionSettings> protectionSettings, CancellationToken cancellationToken = default)
        {
            var moduleDefMDCreationResult = await m_ModuleDefMDCreator.CreateAsync();
            var protectionContext = await new ProtectionContextCreator(moduleDefMDCreationResult, externalComponentsModuleDefMD, context).CreateAsync();
            m_Logger.Information("Loaded Module {0}", moduleDefMDCreationResult.ModuleDefMD.Name);

            var protectionsSortingResult = await new ProtectionsSorter(m_DnlibDefFeatureObfuscationAttributeHavingResolver, moduleDefMDCreationResult.ModuleDefMD.Assembly, m_Logger)
                .SortAsync(protections, protectionSettings);

            await new ProtectionsExecutionNotifier(m_Logger).NotifyAsync(protectionsSortingResult);

            var stringBuilder = new StringBuilder()
                .Append(Path.GetFileNameWithoutExtension(context.ModuleFileName));
            if (context.Watermark)
            {
                stringBuilder.
                    Append("_bitmono");
            }
            stringBuilder.Append(Path.GetExtension(context.ModuleFileName));
            var outputFile = Path.Combine(context.OutputPath, stringBuilder.ToString());
            context.OutputModuleFile = outputFile;

            m_Logger.Information("Preparing to protect module: {0}", context.ModuleFileName);
            await new BitMonoObfuscator(m_ObfuscationConfiguratin, protectionsSortingResult.Protections, context, protectionContext, m_ModuleDefMDWriter, m_Logger)
                .StartAsync(cancellationToken);
            m_Logger.Information("Protected module`s saved in {0}", context.OutputPath);
        }
    }
}