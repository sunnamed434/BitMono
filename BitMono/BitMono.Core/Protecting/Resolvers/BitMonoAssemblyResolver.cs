using BitMono.API.Protecting.Context;
using dnlib.DotNet;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Core.Protecting.Resolvers
{
    public class BitMonoAssemblyResolver
    {
        private readonly string[] m_DependencyFiles;
        private readonly ProtectionContext m_ProtectionContext;
        private readonly ILogger m_Logger;

        public BitMonoAssemblyResolver(string[] dependencyFiles, ProtectionContext protectionContext, ILogger logger)
        {
            m_DependencyFiles = dependencyFiles;
            m_ProtectionContext = protectionContext;
            m_Logger = logger.ForContext<BitMonoAssemblyResolver>();
        }
        public BitMonoAssemblyResolver(ProtectionContext protectionContext, ILogger logger) : this(Array.Empty<string>(), protectionContext, logger)
        {
        }

        public Task<bool> ResolveAsync(CancellationToken cancellationToken = default)
        {
            var resolvingSucceed = true;
            for (int i = 0; i < m_DependencyFiles.Length; i++)
            {
                m_ProtectionContext.AssemblyResolver.AddToCache(AssemblyDef.Load(m_DependencyFiles[i]));
            }

            foreach (AssemblyRef assemblyRef in m_ProtectionContext.ModuleDefMD.GetAssemblyRefs())
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    m_ProtectionContext.ModuleCreationOptions.Context.AssemblyResolver.ResolveThrow(assemblyRef, m_ProtectionContext.ModuleDefMD);
                }
                catch (AssemblyResolveException)
                {
                    resolvingSucceed = false;
                    m_Logger.Error("Failed to resolve dependency {0}", assemblyRef.FullName);
                }
            }
            return Task.FromResult(resolvingSucceed);
        }
    }
}