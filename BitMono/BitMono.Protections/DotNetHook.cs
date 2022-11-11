using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Renaming;
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

namespace BitMono.Protections
{
    public class DotNetHook : IPipelineProtection, IPipelineStage
    {
        private readonly DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<DotNetHook> m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly IMethodDefSearcher m_MethodDefSearcher;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;
        private readonly IList<(MethodDef, MethodDef, int)> m_InstructionsToBeTokensUpdated;

        public DotNetHook(
            DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<DotNetHook> dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            IMethodDefSearcher methodDefSearcher,
            IRenamer renamer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer = dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_MethodDefSearcher = methodDefSearcher;
            m_Renamer = renamer;
            m_Logger = logger.ForContext<DotNetHook>();
            m_InstructionsToBeTokensUpdated = new List<(MethodDef, MethodDef, int)>();
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
            context.Importer = new Importer(moduleDefMD);

            var virtualProtectMethodDef = context.ExternalComponentsImporter.Import(typeof(Hooking).GetMethod(nameof(Hooking.VirtualProtect), BindingFlags.Public | BindingFlags.Static)).ResolveMethodDefThrow();
            virtualProtectMethodDef.Access = MethodAttributes.Assembly;

            var managedHookTypeDef = new TypeDefUser("ManagedHook", moduleDefMD.CorLibTypes.Object.TypeDefOrRef);

            var redirectStubMethodDef = context.ExternalComponentsImporter.Import(typeof(Hooking).GetMethod(nameof(Hooking.RedirectStub), BindingFlags.Public | BindingFlags.Static)).ResolveMethodDefThrow();
            redirectStubMethodDef.Access = MethodAttributes.Assembly;
            virtualProtectMethodDef.DeclaringType = null;
            redirectStubMethodDef.Body.Instructions[0].Operand = context.ModuleDefMD.GlobalType;
            redirectStubMethodDef.Body.Instructions[7].Operand = context.ModuleDefMD.GlobalType;

            managedHookTypeDef.Methods.Add(virtualProtectMethodDef);
            redirectStubMethodDef.DeclaringType = null;
            managedHookTypeDef.Methods.Add(redirectStubMethodDef);
            moduleDefMD.Types.Add(managedHookTypeDef);

            var writeLineMethod = context.Importer.Import(typeof(Console).GetMethod(nameof(Console.WriteLine), new Type[0]));

            foreach (var typeDef in moduleDefMD.GetTypes().ToArray())
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

                foreach (var methodDef in typeDef.Methods.ToArray())
                {
                    if (methodDef.HasBody)
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

                        for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                        {
                            if (methodDef.Body.Instructions[i].OpCode == OpCodes.Call
                                && methodDef.Body.Instructions[i].Operand is MethodDef callingMethodDef
                                && callingMethodDef.ParamDefs.Any(p => p.IsIn || p.IsOut) == false)
                            {
                                var dummyMethod = new MethodDefUser(m_Renamer.RenameUnsafely(), callingMethodDef.MethodSig, callingMethodDef.ImplAttributes, callingMethodDef.Attributes);
                                dummyMethod.Body = new CilBody();
                                foreach (var paramDef in callingMethodDef.ParamDefs)
                                {
                                    dummyMethod.ParamDefs.Add(new ParamDefUser(paramDef.Name, paramDef.Sequence, paramDef.Attributes));
                                }
                                dummyMethod.Body.MaxStack = 4;
                                
                                var initializatorMethodDef = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                    MethodSig.CreateStatic(moduleDefMD.CorLibTypes.Void), MethodAttributes.Assembly | MethodAttributes.Static);

                                initializatorMethodDef.Body = new CilBody();
                                var index = initializatorMethodDef.Body.Instructions.Count;
                                m_InstructionsToBeTokensUpdated.Insert(0, (initializatorMethodDef, callingMethodDef, index));
                                m_InstructionsToBeTokensUpdated.Insert(1, (initializatorMethodDef, dummyMethod, index));
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Call, redirectStubMethodDef));
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                                moduleDefMD.GlobalType.Methods.Add(initializatorMethodDef);

                                dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Call, writeLineMethod));
                                if (callingMethodDef.HasReturnType)
                                {
                                    dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldnull));
                                }
                                dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                                typeDef.Methods.Add(dummyMethod);

                                methodDef.Body.Instructions.Insert(0, new Instruction(OpCodes.Call, initializatorMethodDef));
                                methodDef.Body.Instructions[i + 1].Operand = dummyMethod;
                                i += 1;
                            }
                        }
                    }
                }
            }

            var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

            using (moduleDefMD)
            using (var fileStream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                moduleDefMD.Write(fileStream, moduleWriterOptions);
            }
            return Task.CompletedTask;
        }
    }
}