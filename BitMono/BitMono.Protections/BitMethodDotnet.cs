using BitMono.API.Protecting;
using BitMono.Core.Protecting.Analyzing;
using BitMono.Utilities.Extensions.Dnlib;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class BitMethodDotnet : IProtection
    {
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;
        private readonly Random m_Random;

        public BitMethodDotnet(MethodDefCriticalAnalyzer methodDefCriticalAnalyzer)
        {
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_Random = new Random();
        }


        public Task ExecuteAsync(ProtectionContext context)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.HasBody && methodDef.IsConstructor == false
                            && m_MethodDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef))
                        {
                            int randomValueForInsturction = 0;
                            if (methodDef.Body.Instructions.Count >= 3)
                            {
                                randomValueForInsturction = m_Random.Next(0, methodDef.Body.Instructions.Count - 3);
                            }

                            if (methodDef.NotAsync())
                            {
                                int randomValue = m_Random.Next(0, 3);
                                Instruction randomlySelectedInstruction = new Instruction();
                                randomlySelectedInstruction.OpCode = randomValue switch
                                {
                                    0 => OpCodes.Readonly,
                                    1 => OpCodes.Unaligned,
                                    2 => OpCodes.Volatile,
                                    3 => OpCodes.Constrained,
                                    _ => throw new ArgumentOutOfRangeException(),
                                };

                                var jump = Instruction.Create(OpCodes.Br_S, methodDef.Body.Instructions[randomValueForInsturction]);
                                methodDef.Body.Instructions.Insert(randomValueForInsturction, jump);
                                methodDef.Body.Instructions.Insert(randomValueForInsturction + 1, randomlySelectedInstruction);
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}