using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Attributes;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    [ProtectionName(nameof(AntiDebugBreakpoints))]
    public class AntiDebugBreakpoints : IProtection
    {
        private readonly IDnlibDefObfuscationAttributeResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly DnlibDefSpecificNamespaceCriticalAnalyzer m_DnlibDefSpecificNamespaceCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public AntiDebugBreakpoints(
            IDnlibDefObfuscationAttributeResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer methodDefCriticalAnalyzer, 
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_DnlibDefSpecificNamespaceCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_Logger = logger.ForContext<AntiDebugBreakpoints>();
        }

        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            var threadSleepMethods = new List<IMethod>
            {
                context.Importer.Import(typeof(Thread).GetMethod(nameof(Thread.Sleep), new Type[]
                {
                    typeof(int)
                })),
                context.Importer.Import(typeof(Thread).GetMethod(nameof(Thread.Sleep), new Type[]
                {
                    typeof(TimeSpan)
                })),
                context.Importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(int)
                })),
                context.Importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(TimeSpan)
                })),
                context.Importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(int),
                    typeof(CancellationToken),
                })),
                context.Importer.Import(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
                {
                    typeof(TimeSpan),
                    typeof(CancellationToken),
                })),
            };

            var dateTimeUtcNowMethod = context.Importer.Import(typeof(DateTime).GetMethod("get_UtcNow"));
            var dateTimeSubtractionMethod = context.Importer.Import(typeof(DateTime).GetMethod("op_Subtraction", new Type[]
            {
                typeof(DateTime),
                typeof(DateTime)
            }));
            var timeSpanTotalMillisecondsMethod = context.Importer.Import(typeof(TimeSpan).GetMethod("get_TotalMilliseconds"));
            var environmentFailFast = context.Importer.Import(typeof(Environment).GetMethod(nameof(Environment.FailFast), new Type[]
            {
                typeof(string)
            }));

            foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
            {
                foreach (var methodDef in typeDef.Methods.ToArray())
                {
                    if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef)
                        && methodDef.NotGetterAndSetter()
                        && methodDef.IsConstructor == false)
                    {
                        if (methodDef.HasBody
                            && methodDef.Body.Instructions.Count >= 5)
                        {
                            var startIndex = 0;
                            var endIndex = methodDef.Body.Instructions.Count - 1;
                            var methodShouldBeIgnored = false;

                            for (var i = startIndex; i < endIndex; i++)
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

                            var dateTimeLocal = methodDef.Body.Variables.Add(new Local(context.Importer.ImportAsTypeSig(typeof(DateTime))));
                            var timeSpanLocal = methodDef.Body.Variables.Add(new Local(context.Importer.ImportAsTypeSig(typeof(TimeSpan))));
                            var intValue = methodDef.Body.Variables.Add(new Local(context.Importer.ImportAsTypeSig(typeof(int))));

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