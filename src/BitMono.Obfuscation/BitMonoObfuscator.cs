using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Resolvers;
using BitMono.API.Protecting.Writers;
using BitMono.Core.Protecting.Resolvers;
using BitMono.Utilities.Extensions;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Obfuscation
{
    public class BitMonoObfuscator : IModuleDefMDWriter, IDisposable
    {
        private readonly IConfiguration m_Configuration;
        private readonly ICollection<IProtection> m_Protections;
        private readonly ICollection<IPacker> m_Packers;
        private IEnumerable<IDnlibDefResolver> m_DnlibDefResolvers;
        private readonly ProtectionContext m_ProtectionContext;
        private readonly IModuleDefMDWriter m_ModuleDefMDWriter;
        private DnlibDefsResolver m_DnlibDefsResolver;
        private readonly ILogger m_Logger;

        public BitMonoObfuscator(
            IBitMonoObfuscationConfiguration configuration,
            IEnumerable<IDnlibDefResolver> dnlibDefResolvers,
            ICollection<IProtection> protections,
            ICollection<IPacker> packers,
            ProtectionContext protectionContext,
            IModuleDefMDWriter moduleWriter,
            ILogger logger)
        {
            m_Configuration = configuration.Configuration;
            m_DnlibDefResolvers = dnlibDefResolvers;
            m_Protections = protections;
            m_Packers = packers;
            m_ProtectionContext = protectionContext;
            m_ModuleDefMDWriter = moduleWriter;
            m_DnlibDefsResolver = new DnlibDefsResolver();
            m_Logger = logger.ForContext<BitMonoObfuscator>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (m_Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.FailOnNoRequiredDependency)))
            {
                var resolvingSucceed = await new BitMonoAssemblyResolver(m_ProtectionContext.BitMonoContext.DependenciesData, m_ProtectionContext, m_Logger).ResolveAsync();
                if (resolvingSucceed == false)
                {
                    m_Logger.Fatal("Drop dependencies in 'libs' directory with the same path as your module has, or set in obfuscation.json FailOnNoRequiredDependency to false");
                    Console.ReadLine();
                    return;
                }
            }

            foreach (var protection in m_Protections)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if ((protection is IPipelineStage) == false)
                    {
                        var protectionName = protection.GetName();
                        var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.ModuleDefMD);
                        await protection.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                        m_Logger.Information("{0} -> OK!", protectionName);
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Error(ex, "Error while executing protections!");
                }
            }

            foreach (var protection in m_Protections)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (protection is IPipelineStage stage)
                    {
                        if (stage.Stage == PipelineStages.ModuleWrite)
                        {
                            var protectionName = protection.GetName();
                            var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.ModuleDefMD);
                            await protection.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                            m_Logger.Information("{0} -> OK!", protectionName);
                        }
                    }

                    try
                    {
                        if (protection is IPipelineProtection pipelineProtection)
                        {
                            foreach (var protectionPhase in pipelineProtection.PopulatePipeline())
                            {
                                if (protectionPhase.Item2 == PipelineStages.ModuleWrite)
                                {
                                    var protectionName = protection.GetName();
                                    var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.ModuleDefMD);
                                    await protectionPhase.Item1.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                                    m_Logger.Information("{0} -> OK!", protectionName);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Error(ex, "Error while executing protection pipelines!");
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Error(ex, "Error while executing protections!");
                }
            }

            try
            {
                await WriteAsync(m_ProtectionContext.BitMonoContext.OutputModuleFile, m_ProtectionContext.ModuleDefMD, m_ProtectionContext.ModuleWriterOptions);
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Failed to write module!");
            }

            foreach (var protection in m_Protections)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (protection is IPipelineStage stage)
                    {
                        if (stage.Stage == PipelineStages.ModuleWritten)
                        {
                            var protectionName = protection.GetName();
                            m_ProtectionContext.ReloadModule();
                            var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.ModuleDefMD);
                            await protection.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                            m_Logger.Information("{0} -> OK!", protectionName);
                            await WriteAsync(m_ProtectionContext.BitMonoContext.OutputModuleFile, m_ProtectionContext.ModuleDefMD, m_ProtectionContext.ModuleWriterOptions);
                        }
                    }

                    try
                    {
                        if (protection is IPipelineProtection pipelineProtection)
                        {
                            foreach (var protectionPhase in pipelineProtection.PopulatePipeline())
                            {
                                if (protectionPhase.Item2 == PipelineStages.ModuleWritten)
                                {
                                    var protectionName = protection.GetName();
                                    m_ProtectionContext.ReloadModule();
                                    var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.ModuleDefMD);
                                    await protectionPhase.Item1.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                                    m_Logger.Information("{0} -> OK!", protectionName);
                                    await WriteAsync(m_ProtectionContext.BitMonoContext.OutputModuleFile, m_ProtectionContext.ModuleDefMD, m_ProtectionContext.ModuleWriterOptions);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Error(ex, "Error while executing protection pipelines!");
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Error(ex, "Error while executing protections!");
                }
            }

            try
            {
                await m_ModuleDefMDWriter.WriteAsync(m_ProtectionContext.BitMonoContext.OutputModuleFile, m_ProtectionContext.ModuleDefMD, m_ProtectionContext.ModuleWriterOptions);
            }
            catch (Exception ex)
            {
                m_Logger.Fatal(ex, "Error while writing file!");
                return;
            }

            try
            {
                foreach (var packer in m_Packers)
                {
                    var packerName = packer.GetName();
                    var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(packerName, m_ProtectionContext.ModuleDefMD);
                    await packer.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                    m_Logger.Information("{0} -> OK", packerName);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Error while executing packer!");
            }

            Dispose();
        }

        public Task WriteAsync(string outputFile, ModuleDefMD moduleDefMD, ModuleWriterOptions moduleWriterOptions)
        {
            m_ProtectionContext.ModuleDefMDMemoryStream = new MemoryStream();
            moduleDefMD.Write(m_ProtectionContext.ModuleDefMDMemoryStream, moduleWriterOptions);
            m_ProtectionContext.ModuleDefMDOutput = m_ProtectionContext.ModuleDefMDMemoryStream.ToArray();
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            m_ProtectionContext.ModuleDefMD.Dispose();
            m_ProtectionContext.ModuleDefMDMemoryStream.Dispose();
        }
    }
}