using BitMono.API.Protecting;
using dnlib.DotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Core.Protecting
{
    public class BitMonoAssemblyResolver : IAsyncDisposable
    {
        private readonly string[] m_DependencyFiles;
        private readonly ProtectionContext m_ProtectionContext;
        private readonly ILogger m_Logger;

        public BitMonoAssemblyResolver(string[] dependencyFiles, ProtectionContext protectionContext, ILogger logger)
        {
            m_DependencyFiles = dependencyFiles;
            m_ProtectionContext = protectionContext;
            m_Logger = logger;
        }
        public BitMonoAssemblyResolver(ProtectionContext protectionContext, ILogger logger) : this(Array.Empty<string>(), protectionContext, logger)
        {
        }


        public bool ResolvingFailed { get; private set; }


        public Task ResolveAsync(CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < m_DependencyFiles.Length; i++)
            {
                m_ProtectionContext.AssemblyResolver.AddToCache(AssemblyDef.Load(m_DependencyFiles[i]));
            }

            foreach (AssemblyRef assemblyRef in m_ProtectionContext.ModuleDefMD.GetAssemblyRefs())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                try
                {
                    m_ProtectionContext.ModuleCreationOptions.Context.AssemblyResolver.ResolveThrow(assemblyRef, m_ProtectionContext.ModuleDefMD);
                }
                catch (AssemblyResolveException)
                {
                    ResolvingFailed = true;
                    m_Logger.Error("Failed to resolve dependency {0}", assemblyRef.FullName);
                }
            }
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            ResolvingFailed = false;
            return new ValueTask();
        }
    }
}