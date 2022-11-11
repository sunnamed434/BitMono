using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Modules;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Writers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Core.Protecting.Modules
{
    public class BitMonoEngine : IEngine
    {
        private readonly ICollection<IProtection> m_Protections;
        private readonly ProtectionContext m_Context;
        private readonly IModuleDefMDWriter m_ModuleWriter;
        private readonly ILogger m_Logger;

        public BitMonoEngine(
            ICollection<IProtection> protections,
            ProtectionContext context,
            IModuleDefMDWriter moduleWriter,
            ILogger logger)
        {
            m_Protections = protections;
            m_Context = context;
            m_ModuleWriter = moduleWriter;
            m_Logger = logger.ForContext<BitMonoEngine>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            foreach (var protection in m_Protections)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (protection is IPipelineStage stage)
                    {
                        if (stage.Stage == PipelineStages.Begin)
                        {
                            m_Logger.Information("{0} -> Executing..", protection.GetType().FullName);
                            await protection.ExecuteAsync(m_Context, cancellationToken);
                            m_Logger.Information("{0} -> Executed!", protection.GetType().FullName);
                        }
                    }
                    else
                    {
                        m_Logger.Information("{0} -> Executing..", protection.GetType().FullName);
                        await protection.ExecuteAsync(m_Context, cancellationToken);
                        m_Logger.Information("{0} -> Executed!", protection.GetType().FullName);
                    }

                    try
                    {
                        if (protection is IPipelineProtection pipelineProtection)
                        {
                            foreach (var protectionPhase in pipelineProtection.PopulatePipeline())
                            {
                                if (protectionPhase.Item2 == PipelineStages.Begin)
                                {
                                    m_Logger.Information("Executing.. phase protection!");
                                    await protectionPhase.Item1.ExecuteAsync(m_Context, cancellationToken);
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
                            await protection.ExecuteAsync(m_Context, cancellationToken);
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
                                    await protectionPhase.Item1.ExecuteAsync(m_Context, cancellationToken);
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
                await m_ModuleWriter.WriteAsync(m_Context.ModuleDefMD);
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
                            await protection.ExecuteAsync(m_Context, cancellationToken);
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
                                    await protectionPhase.Item1.ExecuteAsync(m_Context, cancellationToken);
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