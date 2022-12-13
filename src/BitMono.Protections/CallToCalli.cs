using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Attributes;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    [ProtectionName(nameof(CallToCalli))]
    public class CallToCalli : IStageProtection
    {
        private readonly IInjector m_Injector;
        private readonly IRenamer m_Renamer;
        private readonly IDnlibDefObfuscationAttributeResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly DnlibDefSpecificNamespaceCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public CallToCalli(
            IInjector injector,
            IRenamer renamer,
            IDnlibDefObfuscationAttributeResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            ILogger logger)
        {
            m_Injector = injector;
            m_Renamer = renamer;
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_Logger = logger.ForContext<CallToCalli>();
        }

        public PipelineStages Stage => PipelineStages.ModuleWritten;

        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            var runtimeMethodHandle = context.Importer.Import(typeof(RuntimeMethodHandle));
            var getTypeFromHandleMethod = context.Importer.Import(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[]
            {
                typeof(RuntimeTypeHandle)
            }));
            var getModuleMethod = context.Importer.Import(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
            var resolveMethodMethod = context.Importer.Import(typeof(Module).GetMethod(nameof(Module.ResolveMethod), new Type[]
            {
                typeof(int)
            }));
            var getMethodHandleMethod = context.Importer.Import(typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle)).GetMethod);
            var getFunctionPointerMethod = context.Importer.Import(typeof(RuntimeMethodHandle).GetMethod(nameof(RuntimeMethodHandle.GetFunctionPointer)));

            foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
            {
                foreach (var methodDef in typeDef.Methods)
                {
                    if (methodDef.HasBody && methodDef.Body.HasInstructions
                        && methodDef.IsConstructor == false
                        && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                    {
                        for (var i = 0; i < methodDef.Body.Instructions.Count; i++)
                        {
                            if (methodDef.Body.Instructions[i].OpCode == OpCodes.Call)
                            {
                                if (methodDef.Body.Instructions[i].Operand is MethodDef callingMethodDef && callingMethodDef.HasBody)
                                {
                                    var runtimeMethodHandleLocal = methodDef.Body.Variables.Add(new Local(new ValueTypeSig(runtimeMethodHandle)));
                                    if (methodDef.Body.HasExceptionHandlers == false)
                                    {
                                        methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;
                                        methodDef.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldtoken, context.ModuleDefMD.GlobalType));
                                        methodDef.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Call, getTypeFromHandleMethod));
                                        methodDef.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Callvirt, getModuleMethod));
                                        methodDef.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Ldc_I4, callingMethodDef.MDToken.ToInt32()));
                                        methodDef.Body.Instructions.Insert(i + 5, new Instruction(OpCodes.Call, resolveMethodMethod));
                                        methodDef.Body.Instructions.Insert(i + 6, new Instruction(OpCodes.Callvirt, getMethodHandleMethod));
                                        methodDef.Body.Instructions.Insert(i + 7, new Instruction(OpCodes.Stloc, runtimeMethodHandleLocal));
                                        methodDef.Body.Instructions.Insert(i + 8, new Instruction(OpCodes.Ldloca, runtimeMethodHandleLocal));
                                        methodDef.Body.Instructions.Insert(i + 9, new Instruction(OpCodes.Call, getFunctionPointerMethod));
                                        methodDef.Body.Instructions.Insert(i + 10, new Instruction(OpCodes.Calli, callingMethodDef.MethodSig));
                                        i += 10;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}