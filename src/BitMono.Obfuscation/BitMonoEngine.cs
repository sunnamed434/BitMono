using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Resolvers;
using BitMono.API.Protecting.Writers;
using BitMono.Core.Models;
using BitMono.Obfuscation.API;
using dnlib.DotNet;
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
        private readonly IModuleDefMDWriter m_ModuleDefMDWriter;
        private readonly IModuleDefMDCreator m_ModuleDefMDCreator;
        private readonly IDnlibDefObfuscationAttributeResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly IBitMonoObfuscationConfiguration m_ObfuscationConfiguratin;
        private readonly List<IDnlibDefResolver> m_DnlibDefResolvers;
        private readonly List<IProtection> m_Protections;
        private readonly List<ProtectionSettings> m_ProtectionSettings;
        private readonly ILogger m_Logger;

        public BitMonoEngine(
            IModuleDefMDWriter moduleDefMDWriter,
            IModuleDefMDCreator moduleDefMDCreator,
            IDnlibDefObfuscationAttributeResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            IBitMonoObfuscationConfiguration obfuscationConfiguration,
            List<IDnlibDefResolver> dnlibDefResolvers,
            List<IProtection> protections,
            List<ProtectionSettings> protectionSettings,
            ILogger logger)
        {
            m_ModuleDefMDWriter = moduleDefMDWriter;
            m_ModuleDefMDCreator = moduleDefMDCreator;
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_ObfuscationConfiguratin = obfuscationConfiguration;
            m_DnlibDefResolvers = dnlibDefResolvers;
            m_Protections = protections;
            m_ProtectionSettings = protectionSettings;
            m_Logger = logger.ForContext<BitMonoEngine>();
        }

        public async Task ObfuscateAsync(BitMonoContext context, ModuleDefMD runtimeModuleDefMD, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var moduleDefMDCreationResult = m_ModuleDefMDCreator.Create();
            var protectionContext = new ProtectionContextCreator(moduleDefMDCreationResult, runtimeModuleDefMD, context).Create();
            m_Logger.Information("Loaded Module {0}", moduleDefMDCreationResult.ModuleDefMD.Name);

            var protectionsSortingResult = new ProtectionsSorter(m_DnlibDefFeatureObfuscationAttributeHavingResolver, moduleDefMDCreationResult.ModuleDefMD.Assembly, m_Logger)
                .Sort(m_Protections, m_ProtectionSettings);

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
            await new BitMonoObfuscator(
                m_ObfuscationConfiguratin, 
                m_DnlibDefResolvers, 
                protectionsSortingResult.Protections, 
                protectionsSortingResult.Packers,
                protectionContext, 
                m_ModuleDefMDWriter, 
                m_Logger)
                .StartAsync(cancellationToken);
            m_Logger.Information("Protected module`s saved in {0}", context.OutputPath);
        }
    }
}