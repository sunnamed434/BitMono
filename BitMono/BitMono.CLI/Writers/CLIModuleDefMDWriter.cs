using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Writers;
using dnlib.DotNet;
using System.IO;
using System.Threading.Tasks;

namespace BitMono.CLI.Writers
{
    public class CLIModuleDefMDWriter : IModuleDefMDWriter
    {
        private readonly ProtectionContext m_Context;

        public CLIModuleDefMDWriter(ProtectionContext context)
        {
            m_Context = context;
        }

        public Task WriteAsync(ModuleDefMD moduleDefMD)
        {
            using (moduleDefMD)
            using (var fileStream = File.Create(m_Context.BitMonoContext.OutputModuleFile))
            {
                m_Context.ModuleDefMD.Write(fileStream, m_Context.ModuleWriterOptions);
            }
            return Task.CompletedTask;
        }
    }
}