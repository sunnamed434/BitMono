using BitMono.API.Protecting;
using BitMono.Core.Protecting.Analyzing;
using BitMono.Utilities.Extensions.Dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = BitMono.Core.Logging.ILogger;

namespace BitMono.Protections
{
    public class CallToCalli : IProtection
    {
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;
        private readonly IDictionary<string, MethodDef> m_CalliMethods;
        private readonly ILogger m_Logger;

        public CallToCalli(MethodDefCriticalAnalyzer methodDefCriticalAnalyzer, ILogger logger)
        {
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_CalliMethods = new Dictionary<string, MethodDef>();
            m_Logger = logger;
        }


        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            var importer = new Importer(context.ModuleDefMD);
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                foreach (var methodDef in typeDef.Methods.ToArray())
                {
                    if (methodDef.HasBody && methodDef.Body.HasInstructions 
                        && methodDef.DeclaringType.IsGlobalModuleType == false
                        && methodDef.IsConstructor == false
                        && methodDef.NotGetterAndSetter()
                        && m_MethodDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef))
                    {
                        for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                        {
                            if (methodDef.Body.Instructions[i].OpCode == OpCodes.Call)
                            {
                                if (methodDef.DeclaredInSameAssemblyAs(context.TargetAssembly))
                                {
                                    if (methodDef.Body.Instructions[i].Operand is MemberRef memberRef && memberRef.Signature != null)
                                    {
                                        
                                        m_Logger.Info("methodDef.HasGenericParameters: " + methodDef.HasGenericParameters);
                                        m_Logger.Info("memberRef.Signature.ContainsGenericParameter: " + memberRef.Signature.ContainsGenericParameter);
                                        if (methodDef.Body.HasExceptionHandlers == false)
                                        {
                                            var locals = methodDef.Body.Variables;
                                            var addrLocal = locals.Add(new Local(new ValueTypeSig(importer.Import(typeof(RuntimeMethodHandle)))));

                                            methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;

                                            var getTypeFromHandleMethod = importer.Import(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[]
                                            {
                                                typeof(RuntimeTypeHandle)
                                            }));
                                            methodDef.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldtoken, methodDef.DeclaringType));
                                            methodDef.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Call, getTypeFromHandleMethod));

                                            var getModuleMethod = importer.Import(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
                                            methodDef.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Callvirt, getModuleMethod));

                                            var resolveMethodMethod = importer.Import(typeof(Module).GetMethod(nameof(Module.ResolveMethod), new Type[]
                                            {
                                                typeof(int)
                                            }));

                                            methodDef.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Ldc_I4, memberRef.MDToken.ToInt32()));
                                            methodDef.Body.Instructions.Insert(i + 5, new Instruction(OpCodes.Call, resolveMethodMethod));

                                            var getMethodHandleMethod = importer.Import(typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle)).GetMethod);
                                            var getFunctionPointerMethod = importer.Import(typeof(RuntimeMethodHandle).GetMethod(nameof(RuntimeMethodHandle.GetFunctionPointer)));

                                            methodDef.Body.Instructions.Insert(i + 6, new Instruction(OpCodes.Callvirt, getMethodHandleMethod));

                                            methodDef.Body.Instructions.Insert(i + 7, new Instruction(OpCodes.Stloc, addrLocal));
                                            methodDef.Body.Instructions.Insert(i + 8, new Instruction(OpCodes.Ldloca, addrLocal));

                                            methodDef.Body.Instructions.Insert(i + 9, new Instruction(OpCodes.Call, getFunctionPointerMethod));
                                            methodDef.Body.Instructions.Insert(i + 10, new Instruction(OpCodes.Calli, memberRef.MethodSig));
                                            i += 10;
                                        }
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