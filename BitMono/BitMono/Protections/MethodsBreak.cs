using BitMono.API.Protections;
using BitMono.Core.Attributes;
using BitMono.Core.Extensions;
using BitMono.Core.Protections;
using dnlib.DotNet.Emit;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    [ExceptRegisterService]
    public class MethodsBreak : IProtection
    {
        private readonly ProtectionContext m_Context;

        public MethodsBreak(ProtectionContext context)
        {
            m_Context = context;
        }


        public Task ExecuteAsync()
        {
            foreach (var typeDef in m_Context.ModuleDefMD.Types)
            {
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.HasBody && methodDef.IsConstructor == false
                            && methodDef.NotCriticalToMakeChanges())
                        {
                            var exceptionHandler = new ExceptionHandler();
                            exceptionHandler.TryStart = new Instruction(OpCodes.Nop);
                            exceptionHandler.TryEnd = new Instruction(OpCodes.Nop);
                            exceptionHandler.FilterStart = null;
                            exceptionHandler.HandlerStart = new Instruction(OpCodes.Nop);
                            exceptionHandler.HandlerEnd = new Instruction(OpCodes.Nop);
                            exceptionHandler.HandlerType = ExceptionHandlerType.Catch;
                            exceptionHandler.CatchType = null;
                            methodDef.Body.ExceptionHandlers.Add(exceptionHandler);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
