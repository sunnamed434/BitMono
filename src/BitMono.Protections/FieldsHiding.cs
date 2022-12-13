using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Attributes;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    [Obsolete]
    [ProtectionName(nameof(FieldsHiding))]
    public class FieldsHiding : IStageProtection
    {
        private readonly IDnlibDefObfuscationAttributeResolver m_DnlibDefFeatureObfuscationAttributeResolver;
        private readonly DnlibDefSpecificNamespaceCriticalAnalyzer m_DnlibDefSpecificNamespaceCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public FieldsHiding(
            IDnlibDefObfuscationAttributeResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefSpecificNamespaceCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_Logger = logger.ForContext<FieldsHiding>();
        }

        public PipelineStages Stage => PipelineStages.ModuleWritten;

        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            var moduleDefMD = ModuleDefMD.Load(context.BitMonoContext.OutputModuleFile, context.ModuleCreationOptions);
            context.ModuleDefMD = moduleDefMD;
            var importer = new Importer(context.ModuleDefMD);

            var moduleTypeDef = context.Importer.Import(typeof(Module));

            var initializeArrayMethod = importer.Import(typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.InitializeArray), new Type[]
            {
                typeof(Array),
                typeof(RuntimeFieldHandle)
            }));
            var getTypeFromHandleMethod = importer.Import(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[]
            {
                typeof(RuntimeTypeHandle)
            }));
            var getModuleMethod = importer.Import(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
            var resolveFieldMethod = importer.Import(typeof(Module).GetMethod(nameof(Module.ResolveField), new Type[]
            {
                typeof(int)
            }));
            var getFieldHandleMethod = importer.Import(typeof(FieldInfo).GetProperty(nameof(FieldInfo.FieldHandle)).GetMethod);

            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_DnlibDefFeatureObfuscationAttributeResolver.Resolve<FieldsHiding>(typeDef))
                {
                    m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                    continue;
                }

                if (m_DnlibDefSpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
                {
                    m_Logger.Information("Not able to make changes because of specific namespace was found, skipping.");
                    continue;
                }

                if (typeDef.HasFields)
                {
                    foreach (var fieldDef in typeDef.Fields.ToArray())
                    {
                        if (m_DnlibDefFeatureObfuscationAttributeResolver.Resolve<FieldsHiding>(fieldDef))
                        {
                            m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                            continue;
                        }

                        if (m_DnlibDefSpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(fieldDef) == false)
                        {
                            m_Logger.Information("Not able to make changes because of specific namespace was found, skipping.");
                            continue;
                        }

                        if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(fieldDef)
                            && fieldDef.HasFieldRVA)
                        {
                            var cctor = fieldDef.DeclaringType.FindOrCreateStaticConstructor();
                            for (int i = 0; i < cctor.Body.Instructions.Count; i++)
                            {
                                if (cctor.Body.Instructions[i].OpCode == OpCodes.Call)
                                {
                                    cctor.Body.Instructions[i].OpCode = OpCodes.Nop;

                                    cctor.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldtoken, moduleTypeDef));
                                    cctor.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Call, getTypeFromHandleMethod));
                                    cctor.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Callvirt, getModuleMethod));

                                    cctor.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Ldc_I4, fieldDef.MDToken.ToInt32()));
                                    cctor.Body.Instructions.Insert(i + 5, new Instruction(OpCodes.Callvirt, resolveFieldMethod));
                                    cctor.Body.Instructions.Insert(i + 6, new Instruction(OpCodes.Callvirt, getFieldHandleMethod));

                                    cctor.Body.Instructions.Insert(i + 7, new Instruction(OpCodes.Call, initializeArrayMethod));
                                    i += 7;
                                }
                            }
                        }
                    }
                }
            }
            using (context.ModuleDefMD)
            using (var fileStream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                context.ModuleDefMD.Write(fileStream, context.ModuleWriterOptions);
            }
            return Task.CompletedTask;
        }
    }
}