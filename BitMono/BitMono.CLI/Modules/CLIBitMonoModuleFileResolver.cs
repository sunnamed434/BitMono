using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Resolvers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.CLI.Modules
{
    public class CLIBitMonoModuleFileResolver : IBitMonoModuleFileResolver
    {
        private readonly string[] m_Args;

        public CLIBitMonoModuleFileResolver(string[] args)
        {
            m_Args = args;
        }

        public Task ResolveAsync(BitMonoContext context)
        {
            if (m_Args?.Any() == true)
            {
                context.ModuleFile = m_Args[0];
            }
            else
            {
                var baseDirectoryFiles = Directory.GetFiles(context.BaseDirectory);
                if (baseDirectoryFiles.Any() == false)
                {
                    throw new InvalidOperationException("No one file were found in base directory!");
                }
                context.ModuleFile = baseDirectoryFiles[0];
            }
            return Task.CompletedTask;
        }
    }
}