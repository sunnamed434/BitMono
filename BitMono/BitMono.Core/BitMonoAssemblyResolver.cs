using BitMono.API.Protecting;
using dnlib.DotNet;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = BitMono.Core.Logging.ILogger;

namespace BitMono.Core
{
    public class BitMonoAssemblyResolver : IAsyncDisposable
    {
        private readonly ProtectionContext m_ProtectionContext;
        private readonly ILogger m_Logger;

        public BitMonoAssemblyResolver(ProtectionContext protectionContext, ILogger logger)
        {
            m_ProtectionContext = protectionContext;
            m_Logger = logger;
        }

        public bool ResolvingFailed { get; private set; }


        public Task ResolveAsync(CancellationToken cancellationToken = default)
        {
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
                    m_Logger.Warn("Failed to resolve dependency of " + assemblyRef.FullName);
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