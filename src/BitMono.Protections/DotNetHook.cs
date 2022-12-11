using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Helpers;
using BitMono.Runtime;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
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
        private readonly IDnlibDefFeatureObfuscationAttributeHavingResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly IInjector m_Injector;
        private readonly IMethodDefSearcher m_MethodDefSearcher;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;
        private readonly IList<InstructionTokensUpdate> m_InstructionsToBeTokensUpdated;
        private readonly Random m_Random;

        public DotNetHook(
            IDnlibDefFeatureObfuscationAttributeHavingResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            IInjector injector,
            IMethodDefSearcher methodDefSearcher,
            IRenamer renamer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_Injector = injector;
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
            var moduleDefMD = ModuleDefMD.Load(context.BitMonoContext.OutputModuleFile, context.ModuleCreationOptions);
            context.ModuleDefMD = moduleDefMD;
            context.Importer = new Importer(context.ModuleDefMD);

            var runtimeHookingTypeDef = context.RuntimeModuleDefMD.ResolveTypeDefOrThrow<Hooking>();
            var hookingTypeDef = m_Injector.InjectInvisibleValueType(context.ModuleDefMD, context.ModuleDefMD.GlobalType, m_Renamer.RenameUnsafely());
            var injectedHookingDnlibDefs = InjectHelper.Inject(runtimeHookingTypeDef, hookingTypeDef, context.ModuleDefMD);
            var redirectStubMethodDef = injectedHookingDnlibDefs.FirstOrDefault(i => i.Name.String.Equals(nameof(Hooking.RedirectStub))).ResolveMethodDefOrThrow();

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
                                && callingMethodDef.HasBody
                                && callingMethodDef.ParamDefs.Any(p => p.IsIn || p.IsOut) == false)
                            {
                                var dummyMethod = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                    callingMethodDef.MethodSig, callingMethodDef.ImplAttributes, callingMethodDef.Attributes);
                                dummyMethod.IsStatic = true;
                                dummyMethod.Access = MethodAttributes.Assembly;
                                context.ModuleDefMD.GlobalType.Methods.Add(dummyMethod);
                                foreach (var paramDef in callingMethodDef.ParamDefs)
                                {
                                    dummyMethod.ParamDefs.Add(new ParamDefUser(paramDef.Name, paramDef.Sequence, paramDef.Attributes));
                                }
                                dummyMethod.Body = new CilBody();
                                if (callingMethodDef.HasReturnType)
                                {
                                    dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldnull));
                                }
                                dummyMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                                var initializatorMethodDef = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                    MethodSig.CreateStatic(context.ModuleDefMD.CorLibTypes.Void), MethodAttributes.Assembly | MethodAttributes.Static);
                                initializatorMethodDef.Body = new CilBody();
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Nop));
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Call, redirectStubMethodDef));
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                                context.ModuleDefMD.GlobalType.Methods.Add(initializatorMethodDef);

                                const int StartMethodDefIndex = 0;
                                m_InstructionsToBeTokensUpdated.Add(new InstructionTokensUpdate
                                {
                                    InitializatorMethodDef = initializatorMethodDef,
                                    FromMethodDef = callingMethodDef,
                                    ToMethodDef = dummyMethod,
                                    Index = StartMethodDefIndex,
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
            using (context.ModuleDefMD)
            using (var fileStream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                context.ModuleDefMD.Write(fileStream, context.ModuleWriterOptions);
            }
            return Task.CompletedTask;
        }
    }
}