using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    public class BitMethodDotnet : IProtection
    {
        private readonly IDnlibDefFeatureObfuscationAttributeHavingResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;
        private readonly Random m_Random;

        public BitMethodDotnet(
            IDnlibDefFeatureObfuscationAttributeHavingResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_Logger = logger.ForContext<BitMethodDotnet>();
            m_Random = new Random();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<BitMethodDotnet>(typeDef) == false)
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
                        if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<BitMethodDotnet>(methodDef) == false)
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
                            var randomMethodDefBodyIndex = 0;
                            if (methodDef.Body.Instructions.Count >= 3)
                            {
                                randomMethodDefBodyIndex = m_Random.Next(0, methodDef.Body.Instructions.Count);
                            }

                            if (methodDef.NotAsync())
                            {
                                var randomValue = m_Random.Next(0, 3);
                                var randomPrefixInstruction = new Instruction();
                                randomPrefixInstruction.OpCode = randomValue switch
                                {
                                    0 => OpCodes.Readonly,
                                    1 => OpCodes.Unaligned,
                                    2 => OpCodes.Volatile,
                                    3 => OpCodes.Constrained,
                                    _ => throw new ArgumentOutOfRangeException(),
                                };

                                methodDef.Body.Instructions.Insert(randomMethodDefBodyIndex, new Instruction(OpCodes.Nop));
                                methodDef.Body.Instructions.Insert(randomMethodDefBodyIndex + 1, randomPrefixInstruction);

                                methodDef.Body.Instructions[randomMethodDefBodyIndex] = new Instruction(OpCodes.Br_S, methodDef.Body.Instructions[randomMethodDefBodyIndex + 2]);
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}