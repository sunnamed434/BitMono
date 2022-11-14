using BitMono.API.Protecting.Resolvers;
using NullGuard;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.CLI.Modules
{
    internal class CLIBitMonoModuleFileResolver : IBitMonoModuleFileResolver
    {
        private readonly string[] m_Args;

        public CLIBitMonoModuleFileResolver(string[] args)
        {
            m_Args = args;
        }

        [return: AllowNull]
        public Task<string> ResolveAsync()
        {
            string file = null;
            if (m_Args?.Any() == true)
            {
                file = m_Args[0];
            }
            return Task.FromResult(file);
        }
    }
}