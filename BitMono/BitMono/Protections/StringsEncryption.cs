using Autofac;
using BitMono.API.Injection;
using BitMono.API.Injection.Fields;
using BitMono.API.Injection.Methods;
using BitMono.API.Injection.Types;
using BitMono.API.Naming;
using BitMono.API.Protections;
using BitMono.Core.Extensions;
using BitMono.Core.Protections;
using BitMono.Encryption;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Packers
{
    public class StringsEncryption : IProtection
    {
        private readonly ProtectionContext m_Context;
        private readonly IInjector m_Injector;
        private readonly IRenamer m_Renamer;
        private readonly ITypeSearcher m_TypeSearcher;
        private readonly ITypeRemover m_TypeRemover;
        private readonly IFieldSearcher m_FieldSearcher;
        private readonly IMethodSearcher m_MethodSearcher;
        private readonly IMethodRemover m_MethodRemover;

        public StringsEncryption(
            ProtectionContext context, 
            IInjector injector,
            IRenamer renamer,
            ITypeSearcher typeSearcher, 
            ITypeRemover typeRemover, 
            IFieldSearcher fieldSearcher,
            IMethodSearcher methodSearcher,
            IMethodRemover methodRemover)
        {
            m_Context = context;
            m_Injector = injector;
            m_Renamer = renamer;
            m_TypeSearcher = typeSearcher;
            m_TypeRemover = typeRemover;
            m_FieldSearcher = fieldSearcher;
            m_MethodSearcher = methodSearcher;
            m_MethodRemover = methodRemover;
        }


        public Task ExecuteAsync()
        {
            m_Context.ModuleDefMD.GlobalType.FindOrCreateStaticConstructor();
            TypeDef encryptorType = m_TypeSearcher.Find("Encryptor", m_Context.EncryptionModuleDefMD);
            TypeDef encryptorPrivateImplementationType = m_TypeSearcher.Find("<PrivateImplementationDetails>", m_Context.EncryptionModuleDefMD);

            encryptorType.DeclaringType = null;
            encryptorPrivateImplementationType.DeclaringType = null;
            m_TypeRemover.Remove("Encryptor", m_Context.EncryptionModuleDefMD);
            m_TypeRemover.Remove("<PrivateImplementationDetails>", m_Context.EncryptionModuleDefMD);

            m_Context.ModuleDefMD.GlobalType.NestedTypes.Add(encryptorPrivateImplementationType);
            m_Context.ModuleDefMD.GlobalType.NestedTypes.Add(encryptorType);
            m_MethodRemover.Remove("EncryptContent", m_Context.ModuleDefMD);
            encryptorType = m_TypeSearcher.FindInGlobalNestedTypes("Encryptor", m_Context.ModuleDefMD);
            encryptorType.Namespace = string.Empty;

            m_MethodRemover.Remove(encryptorType, "EncryptContent");

            MethodDef decryptorMethod = m_MethodSearcher.FindInGlobalNestedMethods("Decrypt", m_Context.ModuleDefMD);
            encryptorType.Name = "a";
            decryptorMethod.Name = "c";
            foreach (var decryptorMethodParameter in decryptorMethod.Parameters)
            {
                decryptorMethodParameter.Name = string.Empty;
            }
            var saltBytesField = m_FieldSearcher.FindInGlobalNestedTypes("saltBytes", m_Context.ModuleDefMD);
            var cryptKeyBytesField = m_FieldSearcher.FindInGlobalNestedTypes("cryptKeyBytes", m_Context.ModuleDefMD);
            m_Renamer.Rename(saltBytesField, cryptKeyBytesField);

            foreach (TypeDef type in m_Context.ModuleDefMD.Types)
            {
                if (type.HasNestedTypes)
                {
                    foreach (TypeDef childType in type.NestedTypes)
                    {
                        if (childType.HasMethods)
                        {
                            foreach (MethodDef childTypeMethod in childType.Methods)
                            {
                                if (childTypeMethod.HasBody && childTypeMethod.NotCriticalToMakeChanges())
                                {
                                    for (int i = 0; i < childTypeMethod.Body.Instructions.Count(); i++)
                                    {
                                        if (childTypeMethod.Body.Instructions[i].OpCode == OpCodes.Ldstr
                                            && childTypeMethod.Body.Instructions[i].Operand is string stringContent)
                                        {
                                            byte[] encryptedContentBytes = Encryptor.EncryptContent(stringContent);

                                            FieldDef injectedEncryptedArrayBytes = m_Injector.InjectArrayInGlobalType(m_Context.ModuleDefMD, encryptedContentBytes, m_Renamer.Rename());
                                            childTypeMethod.Body.Instructions[i].OpCode = OpCodes.Nop;
                                            childTypeMethod.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldsfld, injectedEncryptedArrayBytes));
                                            childTypeMethod.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Callvirt, decryptorMethod));
                                            i += 2;
                                            childTypeMethod.Body.SimplifyAndOptimizeBranches();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (type.HasMethods)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count(); i++)
                            {
                                if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr
                                    && method.Body.Instructions[i].Operand is string stringContent)
                                {
                                    byte[] encryptedContentBytes = Encryptor.EncryptContent(stringContent);
                                    FieldDef injectedEncryptedArrayBytes = m_Injector.InjectArrayInGlobalType(m_Context.ModuleDefMD, encryptedContentBytes, m_Renamer.Rename());

                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                    method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldsfld, injectedEncryptedArrayBytes));
                                    method.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Callvirt, decryptorMethod));
                                    i += 2;
                                    method.Body.SimplifyAndOptimizeBranches();
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