using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Resolvers;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet.Emit;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class BitMethodDotnet : IProtection
    {
        private readonly DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<BitMethodDotnet> m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;
        private readonly Random m_Random;

        public BitMethodDotnet(
            DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<BitMethodDotnet> dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer = dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_Logger = logger.ForContext<BitMethodDotnet>();
            m_Random = new Random();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
                {
                    m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                    continue;
                }

                if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
                {
                    m_Logger.Debug("Not able to make changes because of specific namespace was found, skipping.");
                    continue;
                }

                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer.NotCriticalToMakeChanges(methodDef) == false)
                        {
                            m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                            continue;
                        }

                        if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(methodDef) == false)
                        {
                            m_Logger.Debug("Not able to make changes because of specific namespace was found, skipping.");
                            continue;
                        }

                        if (methodDef.HasBody && methodDef.IsConstructor == false
                            && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                        {
                            var randomValueForInsturction = 0;
                            if (methodDef.Body.Instructions.Count >= 3)
                            {
                                randomValueForInsturction = m_Random.Next(0, methodDef.Body.Instructions.Count - 3);
                            }

                            if (methodDef.NotAsync())
                            {
                                var randomValue = m_Random.Next(0, 3);
                                var randomlySelectedInstruction = new Instruction();
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