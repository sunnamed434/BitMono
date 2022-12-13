using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Pipeline;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Attributes;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    [ProtectionName(nameof(DotNetHookPhase))]
    internal class DotNetHookPhase : IPhaseProtection
    {
        private readonly IEnumerable<InstructionTokensUpdate> m_InstructionsToBeTokensUpdated;
        private readonly IMethodDefSearcher m_MethodDefSearcher;

        public DotNetHookPhase(IEnumerable<InstructionTokensUpdate> instructionsToBeTokensUpdated, IMethodDefSearcher methodDefSearcher)
        {
            m_InstructionsToBeTokensUpdated = instructionsToBeTokensUpdated;
            m_MethodDefSearcher = methodDefSearcher;
        }

        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            foreach (var instructionTokensUpdate in m_InstructionsToBeTokensUpdated)
            {
                var initializatorMethodDef = m_MethodDefSearcher.Find(instructionTokensUpdate.InitializatorMethodDef.Name, context.ModuleDefMD);
                var fromMethodDef = m_MethodDefSearcher.Find(instructionTokensUpdate.FromMethodDef.Name, context.ModuleDefMD);
                var toMethodDef = m_MethodDefSearcher.Find(instructionTokensUpdate.ToMethodDef.Name, context.ModuleDefMD);
                if (initializatorMethodDef != null && fromMethodDef != null && toMethodDef != null)
                {
                    initializatorMethodDef.Body.Instructions.Insert(instructionTokensUpdate.Index + 1, new Instruction(OpCodes.Ldc_I4, fromMethodDef.MDToken.ToInt32()));
                    initializatorMethodDef.Body.Instructions.Insert(instructionTokensUpdate.Index + 2, new Instruction(OpCodes.Ldc_I4, toMethodDef.MDToken.ToInt32()));
                }
            }
            return Task.CompletedTask;
        }
    }
}