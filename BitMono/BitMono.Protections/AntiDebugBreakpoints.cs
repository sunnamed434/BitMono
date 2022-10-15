using BitMono.API.Protecting;
using BitMono.Core.Protecting.Analyzing;
using BitMono.Utilities.Extensions.Dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class AntiDebugBreakpoints : IProtection
    {
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;

        public AntiDebugBreakpoints(MethodDefCriticalAnalyzer methodDefCriticalAnalyzer)
        {
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
        }


        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            var importer = new Importer(context.ModuleDefMD);
            List<IMethod> threadSleepMethods = new List<IMethod>
            {
                importer.Import(typeof(Thread).GetMethod(nameof(Thread.Sleep), new Type[]
                {
                    typeof(int)
                })),
                importer.Import(typeof(Thread).GetMethod(nameof(Thread.Sleep), new Type[]
                {
                    typeof(TimeSpan)
                })),
                importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(int)
                })),
                importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(TimeSpan)
                })),
                importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(int),
                    typeof(CancellationToken),
                })),
                importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(TimeSpan),
                    typeof(CancellationToken),
                })),
            };

            var dateTimeUtcNowMethod = importer.Import(typeof(DateTime).GetMethod("get_UtcNow"));
            var dateTimeSubtractionMethod = importer.Import(typeof(DateTime).GetMethod("op_Subtraction", new Type[]
            {
                typeof(DateTime),
                typeof(DateTime)
            }));
            var timeSpanTotalMillisecondsMethod = importer.Import(typeof(TimeSpan).GetMethod("get_TotalMilliseconds"));
            var environmentFailFast = importer.Import(typeof(Environment).GetMethod(nameof(Environment.FailFast), new Type[]
            {
                typeof(string)
            }));

            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                foreach (var methodDef in typeDef.Methods.ToArray())
                {
                    if (m_MethodDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef)
                        && methodDef.NotGetterAndSetter()
                        && methodDef.IsConstructor == false)
                    {
                        if (methodDef.HasBody
                            && methodDef.Body.Instructions.Count >= 5)
                        {
                            var startIndex = 0;
                            var endIndex = methodDef.Body.Instructions.Count - 1;
                            var methodShouldBeIgnored = false;

                            for (int i = startIndex; i < endIndex; i++)
                            {
                                if (methodDef.Body.Instructions[i].OpCode == OpCodes.Call
                                    && methodDef.Body.Instructions[i].Operand is MemberRef methodRef)
                                {
                                    if (threadSleepMethods.Any(t => new SigComparer().Equals(methodRef, t)))
                                    {
                                        methodShouldBeIgnored = true;
                                        break;
                                    }
                                }
                            }

                            if (methodShouldBeIgnored)
                            {
                                methodShouldBeIgnored = false;
                                continue;
                            }

                            var dateTimeLocal = methodDef.Body.Variables.Add(new Local(importer.ImportAsTypeSig(typeof(DateTime))));
                            var timeSpanLocal = methodDef.Body.Variables.Add(new Local(importer.ImportAsTypeSig(typeof(TimeSpan))));
                            var intValue = methodDef.Body.Variables.Add(new Local(importer.ImportAsTypeSig(typeof(int))));
                            
                            methodDef.Body.Instructions.Insert(startIndex++, new Instruction(OpCodes.Call, dateTimeUtcNowMethod));
                            methodDef.Body.Instructions.Insert(startIndex++, new Instruction(OpCodes.Stloc_S, dateTimeLocal));

                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Call, dateTimeUtcNowMethod));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Ldloc_S, dateTimeLocal));
                            
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Call, dateTimeSubtractionMethod));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Stloc_S, timeSpanLocal));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Ldloca_S, timeSpanLocal));

                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Call, timeSpanTotalMillisecondsMethod));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Ldc_R8, 5000.0));
                            
                            var nopInstruction = new Instruction(OpCodes.Nop);
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Ble_Un_S, nopInstruction));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Ldc_I4_1));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Ldc_I4_0));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Stloc_S, intValue));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Ldloc_S, intValue));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Div));
                            methodDef.Body.Instructions.Insert(endIndex++, new Instruction(OpCodes.Pop));
                            methodDef.Body.Instructions.Insert(endIndex++, nopInstruction);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}