using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.ExternalComponents;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;
using MethodAttributes = dnlib.DotNet.MethodAttributes;

namespace BitMono.Protections.Hooks
{
    public class DotNetHook : IPipelineProtection, IPipelineStage
    {
        private readonly IDnlibDefFeatureObfuscationAttributeHavingResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly IMethodDefSearcher m_MethodDefSearcher;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;
        private readonly IList<InstructionTokensUpdate> m_InstructionsToBeTokensUpdated;
        private readonly Random m_Random;

        public DotNetHook(
            IDnlibDefFeatureObfuscationAttributeHavingResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            IMethodDefSearcher methodDefSearcher,
            IRenamer renamer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_MethodDefSearcher = methodDefSearcher;
            m_Renamer = renamer;
            m_Logger = logger.ForContext<DotNetHook>();
            m_InstructionsToBeTokensUpdated = new List<InstructionTokensUpdate>();
            m_Random = new Random();
        }

        public PipelineStages Stage => PipelineStages.ModuleWritten;

        public IEnumerable<(IPhaseProtection, PipelineStages)> PopulatePipeline()
        {
            yield return (new DotNetHookPhase(m_InstructionsToBeTokensUpdated, m_MethodDefSearcher), PipelineStages.ModuleWritten);
        }
        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            var moduleDefMD = ModuleDefMD.Load(context.BitMonoContext.OutputModuleFile);
            context.ModuleDefMD = moduleDefMD;
            context.Importer = new Importer(context.ModuleDefMD);

            var virtualProtectMethodDef = context.ExternalComponentsImporter.Import(typeof(Hooking).GetMethod(nameof(Hooking.VirtualProtect), BindingFlags.Public | BindingFlags.Static)).ResolveMethodDefThrow();
            virtualProtectMethodDef.Access = MethodAttributes.Assembly;

            var managedHookTypeDef = new TypeDefUser(m_Renamer.RenameUnsafely(), context.ModuleDefMD.CorLibTypes.Object.TypeDefOrRef);

            var redirectStubMethodDef = context.ExternalComponentsImporter.Import(typeof(Hooking).GetMethod(nameof(Hooking.RedirectStub), BindingFlags.Public | BindingFlags.Static)).ResolveMethodDefThrow();
            redirectStubMethodDef.Name = m_Renamer.RenameUnsafely();
            redirectStubMethodDef.Access = MethodAttributes.Assembly;
            virtualProtectMethodDef.DeclaringType = null;
            redirectStubMethodDef.Body.Instructions[0].Operand = context.ModuleDefMD.GlobalType;
            redirectStubMethodDef.Body.Instructions[7].Operand = context.ModuleDefMD.GlobalType;

            managedHookTypeDef.Methods.Add(virtualProtectMethodDef);
            redirectStubMethodDef.DeclaringType = null;
            managedHookTypeDef.Methods.Add(redirectStubMethodDef);
            context.ModuleDefMD.Types.Add(managedHookTypeDef);

            var writeLineMethod = context.Importer.Import(typeof(Console).GetMethod(nameof(Console.WriteLine), new Type[0]));

            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<DotNetHook>(typeDef))
                {
                    m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                    continue;
                }

                if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
                {
                    m_Logger.Information("Not able to make changes because of specific namespace was found, skipping.");
                    continue;
                }

                foreach (var methodDef in typeDef.Methods.ToArray())
                {
                    if (methodDef.HasBody)
                    {
                        if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<DotNetHook>(methodDef))
                        {
                            m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                            continue;
                        }

                        if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(methodDef) == false)
                        {
                            m_Logger.Information("Not able to make changes because of specific namespace was found, skipping.");
                            continue;
                        }

                        for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                        {
                            if (methodDef.Body.Instructions[i].OpCode == OpCodes.Call
                                && methodDef.Body.Instructions[i].Operand is MethodDef callingMethodDef
                                && callingMethodDef.ParamDefs.Any(p => p.IsIn || p.IsOut) == false)
                            {
                                var dummyMethod = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                    callingMethodDef.MethodSig, callingMethodDef.ImplAttributes, callingMethodDef.Attributes);
                                dummyMethod.IsStatic = true;
                                dummyMethod.Access = MethodAttributes.Assembly;
                                dummyMethod.Body = new CilBody();
                                foreach (var paramDef in callingMethodDef.ParamDefs)
                                {
                                    dummyMethod.ParamDefs.Add(new ParamDefUser(paramDef.Name, paramDef.Sequence, paramDef.Attributes));
                                }

                                var initializatorMethodDef = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                    MethodSig.CreateStatic(context.ModuleDefMD.CorLibTypes.Void), MethodAttributes.Assembly | MethodAttributes.Static);

                                initializatorMethodDef.Body = new CilBody();

                                var index = initializatorMethodDef.Body.Instructions.Count;
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Nop));
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Call, redirectStubMethodDef));
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                                context.ModuleDefMD.GlobalType.Methods.Add(initializatorMethodDef);

                                dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Call, writeLineMethod));
                                if (callingMethodDef.HasReturnType)
                                {
                                    dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldnull));
                                }
                                dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                                context.ModuleDefMD.GlobalType.Methods.Add(dummyMethod);

                                m_InstructionsToBeTokensUpdated.Add(new InstructionTokensUpdate
                                {
                                    InitializatorMethodDef = initializatorMethodDef,
                                    FromMethodDef = callingMethodDef,
                                    ToMethodDef = dummyMethod,
                                    Index = index,
                                });

                                methodDef.Body.Instructions[i].Operand = dummyMethod;
                                var globalTypeCctor = context.ModuleDefMD.GlobalType.FindOrCreateStaticConstructor();
                                var randomIndex = m_Random.Next(0, globalTypeCctor.Body.Instructions.Count - 1);
                                globalTypeCctor.Body.Instructions.Insert(randomIndex, new Instruction(OpCodes.Call, initializatorMethodDef));
                            }
                        }
                    }
                }
            }

            var moduleWriterOptions = new ModuleWriterOptions(context.ModuleDefMD);
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

            using (context.ModuleDefMD)
            using (var fileStream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                context.ModuleDefMD.Write(fileStream, moduleWriterOptions);
            }
            return Task.CompletedTask;
        }
    }
}