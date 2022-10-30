using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
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
    public class FieldsHiding : IStageProtection
    {
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public FieldsHiding(
            IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            ILogger logger)
        {
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_Logger = logger;
        }

        public PipelineStages Stage => PipelineStages.ModuleWritten;

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            if (m_ObfuscationAttributeExcludingResolver.TryResolve(context, context.ModuleDefMD.Assembly, nameof(AntiDe4dot), out ObfuscationAttribute obfuscationAttribute))
            {
                if (obfuscationAttribute.Exclude)
                {
                    m_Logger.Debug("Skip protection because {0} declared in Assembly.", nameof(ObfuscationAttribute));
                    return Task.CompletedTask;
                }
            }

            var moduleDefMD = ModuleDefMD.Load(context.BitMonoContext.OutputModuleFile, context.ModuleCreationOptions);

            var importer = new Importer(moduleDefMD);
            var initializeArrayMethod = importer.Import(typeof(RuntimeHelpers).GetMethod("InitializeArray", new Type[]
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

            foreach (var typeDef in moduleDefMD.GetTypes().ToArray())
            {
                if (typeDef.HasFields)
                {
                    foreach (var fieldDef in typeDef.Fields.ToArray())
                    {
                        if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(context, fieldDef))
                        {
                            if (fieldDef.HasFieldRVA)
                            {
                                var cctor = fieldDef.DeclaringType.FindOrCreateStaticConstructor();
                                for (int i = 0; i < cctor.Body.Instructions.Count; i++)
                                {
                                    if (cctor.Body.Instructions[i].OpCode == OpCodes.Call)
                                    {
                                        cctor.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                        cctor.Body.Instructions[i].OpCode = OpCodes.Nop;

                                        cctor.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldtoken, fieldDef.DeclaringType));
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
            }
            using (moduleDefMD)
            using (var fileStream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                moduleDefMD.Write(fileStream, context.ModuleWriterOptions);
            }
            context.ModuleDefMD = moduleDefMD;
            return Task.CompletedTask;
        }
    }
}