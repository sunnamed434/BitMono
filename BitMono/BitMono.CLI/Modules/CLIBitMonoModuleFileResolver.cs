using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Resolvers;
using NullGuard;
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

        [return: AllowNull]
        public Task<string> ResolveAsync(string baseDirectory)
        {
            string file;
            if (m_Args?.Any() == true)
            {
                file = m_Args[0];
            }
            else
            {
                var baseDirectoryFiles = Directory.GetFiles(baseDirectory);
                if (baseDirectoryFiles.Any() == false)
                {
                    throw new InvalidOperationException("No one file were found in base directory!");
                }
                file = baseDirectoryFiles[0];
            }
            return Task.FromResult(file);
        }
    }
}