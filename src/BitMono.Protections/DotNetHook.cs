using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Attributes;
using BitMono.Core.Protecting.Helpers;
using BitMono.Runtime;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;
using MethodAttributes = dnlib.DotNet.MethodAttributes;

namespace BitMono.Protections
{
    [ProtectionName(nameof(DotNetHook))]
    public class DotNetHook : IPipelineProtection, IPipelineStage
    {
        private readonly IDnlibDefObfuscationAttributeResolver m_DnlibDefObfuscationAttributeResolver;
        private readonly DnlibDefSpecificNamespaceCriticalAnalyzer m_DnlibDefSpecificNamespaceCriticalAnalyzer;
        private readonly IInjector m_Injector;
        private readonly IMethodDefSearcher m_MethodDefSearcher;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;
        private readonly List<InstructionTokensUpdate> m_InstructionsToBeTokensUpdated;
        private readonly Random m_Random;

        public DotNetHook(
            IDnlibDefObfuscationAttributeResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            IInjector injector,
            IMethodDefSearcher methodDefSearcher,
            IRenamer renamer,
            ILogger logger)
        {
            m_DnlibDefObfuscationAttributeResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefSpecificNamespaceCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
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
        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            var runtimeHookingTypeDef = context.RuntimeModuleDefMD.ResolveTypeDefOrThrow<Hooking>();
            var injectedHookingDnlibDefs = InjectHelper.Inject(runtimeHookingTypeDef, context.ModuleDefMD.GlobalType, context.ModuleDefMD);
            var redirectStubMethodDef = injectedHookingDnlibDefs.FirstOrDefault(i => i.Name.String.Equals(nameof(Hooking.RedirectStub))).ResolveMethodDefOrThrow();

            foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
            {
                foreach (var methodDef in typeDef.Methods.ToArray())
                {
                    if (methodDef.HasBody)
                    {
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
                                    dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                                }
                                dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                                var initializatorMethodDef = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                    MethodSig.CreateStatic(context.ModuleDefMD.CorLibTypes.Void), MethodAttributes.Assembly | MethodAttributes.Static);
                                initializatorMethodDef.Body = new CilBody();
                                initializatorMethodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                                initializatorMethodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));  
                                initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Call, redirectStubMethodDef));
                                initializatorMethodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                                context.ModuleDefMD.GlobalType.Methods.Add(initializatorMethodDef);

                                const int StartInitializatiorMethodDefBodyIndex = 1;
                                m_InstructionsToBeTokensUpdated.Add(new InstructionTokensUpdate
                                {
                                    InitializatorMethodDef = initializatorMethodDef,
                                    FromMethodDef = dummyMethod,
                                    ToMethodDef = callingMethodDef,
                                    Index = StartInitializatiorMethodDefBodyIndex,
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
            return Task.CompletedTask;
        }
    }
}