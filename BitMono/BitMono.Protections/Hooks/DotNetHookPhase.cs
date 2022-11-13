using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Pipeline;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections.Hooks
{
    internal class DotNetHookPhase : IPhaseProtection
    {
        private readonly IList<InstructionTokensUpdate> m_InstructionsToBeTokensUpdated;
        private readonly IMethodDefSearcher m_MethodDefSearcher;

        public DotNetHookPhase(IList<InstructionTokensUpdate> instructionsToBeTokensUpdated, IMethodDefSearcher methodDefSearcher)
        {
            m_InstructionsToBeTokensUpdated = instructionsToBeTokensUpdated;
            m_MethodDefSearcher = methodDefSearcher;
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            var moduleDefMD = ModuleDefMD.Load(context.BitMonoContext.OutputModuleFile, context.ModuleCreationOptions);
            foreach (var instructionTokensUpdate in m_InstructionsToBeTokensUpdated)
            {
                var initializatorMethodDef = m_MethodDefSearcher.Find(instructionTokensUpdate.InitializatorMethodDef.Name, moduleDefMD);
                var fromMethodDef = m_MethodDefSearcher.Find(instructionTokensUpdate.FromMethodDef.Name, moduleDefMD);
                var toMethodDef = m_MethodDefSearcher.Find(instructionTokensUpdate.ToMethodDef.Name, moduleDefMD);

                if (initializatorMethodDef != null && fromMethodDef != null && toMethodDef != null)
                {
                    initializatorMethodDef.Body.Instructions.Insert(instructionTokensUpdate.Index, new Instruction(OpCodes.Ldc_I4, fromMethodDef.MDToken.ToInt32()));
                    initializatorMethodDef.Body.Instructions.Insert(instructionTokensUpdate.Index, new Instruction(OpCodes.Ldc_I4, toMethodDef.MDToken.ToInt32()));
                }
            }

            var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;

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