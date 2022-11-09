using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Pipeline;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    internal class DotNetHookPhase : IPhaseProtection
    {
        private readonly IList<(MethodDef, MethodDef, int)> m_InstructionsToBeTokensUpdated;
        private readonly IMethodDefSearcher m_MethodDefSearcher;

        public DotNetHookPhase(IList<(MethodDef, MethodDef, int)> instructionsToBeTokensUpdated, IMethodDefSearcher methodDefSearcher)
        {
            m_InstructionsToBeTokensUpdated = instructionsToBeTokensUpdated;
            m_MethodDefSearcher = methodDefSearcher;
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            var moduleDefMD = ModuleDefMD.Load(context.BitMonoContext.OutputModuleFile, context.ModuleCreationOptions);
            foreach (var tuple in m_InstructionsToBeTokensUpdated)
            {
                var callerMethodDef = m_MethodDefSearcher.Find(tuple.Item1.Name, moduleDefMD);
                var targetMethodDef = m_MethodDefSearcher.Find(tuple.Item2.Name, moduleDefMD);
                if (callerMethodDef != null && targetMethodDef != null)
                {
                    callerMethodDef.Body.Instructions.Insert(tuple.Item3, new Instruction(OpCodes.Ldc_I4, targetMethodDef.MDToken.ToInt32()));
                }
            }

            var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

            using (moduleDefMD)
            using (var fileStream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                moduleDefMD.Write(fileStream, moduleWriterOptions);
            }
            context.ModuleDefMD = moduleDefMD;
            return Task.CompletedTask;
        }
    }
}