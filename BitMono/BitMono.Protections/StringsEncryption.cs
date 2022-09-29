using BitMono.API.Protecting;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.Fields;
using BitMono.API.Protecting.Injection.Methods;
using BitMono.API.Protecting.Renaming;
using BitMono.Core.Protecting.Analyzing;
using BitMono.Encryption;
using BitMono.Utilities.Extensions.Dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class StringsEncryption : IProtection
    {
        private readonly IInjector m_Injector;
        private readonly IRenamer m_Renamer;
        private readonly IFieldSearcher m_FieldSearcher;
        private readonly IMethodSearcher m_MethodSearcher;
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;

        public StringsEncryption(
            IInjector injector,
            IRenamer renamer,
            IFieldSearcher fieldSearcher,
            IMethodSearcher methodSearcher,
            MethodDefCriticalAnalyzer methodDefCriticalAnalyzer)
        {
            m_Injector = injector;
            m_Renamer = renamer;
            m_FieldSearcher = fieldSearcher;
            m_MethodSearcher = methodSearcher;
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
        }


        public Task ExecuteAsync(ProtectionContext context)
        {
            context.ModuleDefMD.GlobalType.FindOrCreateStaticConstructor();

            var encryptorTypeDef = m_Injector.CreateInvisibleValueType(context.ModuleDefMD, "Encryptor");
            //m_Injector.InjectArray(context.ModuleDefMD, encryptorTypeDef, new byte[10], m_Renamer.RenameUnsafely());
            var saltBytes = new byte[] { 0x1, 0x3, 0x2, 0x3, 0x3, 0x4, 0x5, 0x10, 0x10 };
            var cryptKeyBytes = new byte[] { 0x1, 0x3, 0x10, 0x15, 0x20, 0x50, 0x5, 0x10, 0x10 };
            var saltBytesFieldDef = m_Injector.InjectArrayInGlobalNestedTypes(context.ModuleDefMD, saltBytes, "saltBytes");
            var cryptKeyBytesFieldDef = m_Injector.InjectArrayInGlobalNestedTypes(context.ModuleDefMD, cryptKeyBytes, "cryptKeyBytes");
            
            var decryptMethodDefFromEncryptionModule = m_MethodSearcher.Find("Decrypt", context.EncryptionModuleDefMD);
            var decryptorMethodDef = new MethodDefUser("Decrypt", decryptMethodDefFromEncryptionModule.MethodSig, MethodAttributes.Static | MethodAttributes.Assembly);
            decryptorMethodDef.Body = decryptMethodDefFromEncryptionModule.Body;
            bool saltBytesInjected = false;
            bool cryptKeyBytesInjected = false;
            for (int i = 0; i < decryptorMethodDef.Body.Instructions.Count; i++)
            {
                if (decryptorMethodDef.Body.Instructions[i].OpCode == OpCodes.Ldsfld)
                {
                    if (saltBytesInjected && cryptKeyBytesInjected)
                    {
                        break;
                    }

                    if (saltBytesInjected == false)
                    {
                        decryptorMethodDef.Body.Instructions[i] = new Instruction(OpCodes.Ldsfld, saltBytesFieldDef);
                        saltBytesInjected = true;
                        continue;
                    }

                    if (cryptKeyBytesInjected == false)
                    {
                        decryptorMethodDef.Body.Instructions[i] = new Instruction(OpCodes.Ldsfld, cryptKeyBytesFieldDef);
                        cryptKeyBytesInjected = true;
                        continue;
                    }
                }
            }
            encryptorTypeDef.Methods.Add(decryptorMethodDef);
            context.ModuleDefMD.GlobalType.NestedTypes.Add(encryptorTypeDef);

            var saltBytesField = m_FieldSearcher.FindInGlobalNestedTypes("saltBytes", context.ModuleDefMD);
            var cryptKeyBytesField = m_FieldSearcher.FindInGlobalNestedTypes("cryptKeyBytes", context.ModuleDefMD);
            m_Renamer.Rename(context, saltBytesField, cryptKeyBytesField);

            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods.ToArray())
                    {
                        if (methodDef.HasBody && m_MethodDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef))
                        {
                            for (int i = 0; i < methodDef.Body.Instructions.Count(); i++)
                            {
                                if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr
                                    && methodDef.Body.Instructions[i].Operand is string stringContent)
                                {
                                    byte[] encryptedContentBytes = Encryptor.EncryptContent(stringContent, saltBytes, cryptKeyBytes);
                                    FieldDef injectedEncryptedArrayBytes = m_Injector.InjectArrayInGlobalNestedTypes(context.ModuleDefMD, encryptedContentBytes, m_Renamer.RenameUnsafely());

                                    methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;
                                    methodDef.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldsfld, injectedEncryptedArrayBytes));
                                    methodDef.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Callvirt, decryptorMethodDef));
                                    methodDef.Body.SimplifyAndOptimizeBranches();
                                    i += 2;
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