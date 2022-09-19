using BitMono.API.Protections;
using BitMono.Core.Analyzing;
using BitMono.Core.Attributes;
using dnlib.DotNet.Emit;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class MethodsBreak : IProtection
    {
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;

        public MethodsBreak(MethodDefCriticalAnalyzer methodDefCriticalAnalyzer)
        {
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
        }


        public Task ExecuteAsync(ProtectionContext context)
        {
            foreach (var typeDef in context.ModuleDefMD.Types)
            {
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.HasBody && methodDef.IsConstructor == false
                            && m_MethodDefCriticalAnalyzer.Analyze(methodDef))
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
