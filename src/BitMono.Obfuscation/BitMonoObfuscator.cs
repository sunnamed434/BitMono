using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Writers;
using BitMono.Core.Protecting.Resolvers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Obfuscation
{
    public class BitMonoObfuscator
    {
        private readonly IConfiguration m_Configuration;
        private readonly ICollection<IProtection> m_Protections;
        private readonly BitMonoContext m_BitMonoContext;
        private readonly ProtectionContext m_ProtectionContext;
        private readonly IModuleDefMDWriter m_ModuleDefMDWriter;
        private readonly ILogger m_Logger;

        public BitMonoObfuscator(
            IBitMonoObfuscationConfiguration configuration,
            ICollection<IProtection> protections,
            BitMonoContext bitMonoContext,
            ProtectionContext protectionContext,
            IModuleDefMDWriter moduleWriter,
            ILogger logger)
        {
            m_Configuration = configuration.Configuration;
            m_Protections = protections;
            m_BitMonoContext = bitMonoContext;
            m_ProtectionContext = protectionContext;
            m_ModuleDefMDWriter = moduleWriter;
            m_Logger = logger.ForContext<BitMonoObfuscator>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (m_Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.FailOnNoRequiredDependency)))
            {
                var resolvingSucceed = await new BitMonoAssemblyResolver(m_BitMonoContext.DependenciesData, m_ProtectionContext, m_Logger).ResolveAsync();
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
                        m_Logger.Information("{0} -> Executing..", protection.GetType().FullName);
                        await protection.ExecuteAsync(m_ProtectionContext, cancellationToken);
                        m_Logger.Information("{0} -> Executed!", protection.GetType().FullName);
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
                            m_Logger.Information("{0} -> Executing..", protection.GetType().FullName);
                            await protection.ExecuteAsync(m_ProtectionContext, cancellationToken);
                            m_Logger.Information("{0} -> Executed!", protection.GetType().FullName);
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
                                    m_Logger.Information("Executing.. phase protection!");
                                    await protectionPhase.Item1.ExecuteAsync(m_ProtectionContext, cancellationToken);
                                    m_Logger.Information("Executed phase protection!");
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
                await m_ModuleDefMDWriter.WriteAsync(m_BitMonoContext.OutputModuleFile, m_ProtectionContext.ModuleDefMD, m_ProtectionContext.ModuleWriterOptions);
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
                            m_Logger.Information("{0} -> Executing..", protection.GetType().FullName);
                            await protection.ExecuteAsync(m_ProtectionContext, cancellationToken);
                            m_Logger.Information("{0} -> Executed!", protection.GetType().FullName);
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
                                    m_Logger.Information("Executing.. phase protection!");
                                    await protectionPhase.Item1.ExecuteAsync(m_ProtectionContext, cancellationToken);
                                    m_Logger.Information("Executed phase protection!");
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
        }
    }
}