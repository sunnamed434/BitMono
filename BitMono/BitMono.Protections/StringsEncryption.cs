using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.FieldDefs;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Encryption;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;
using MethodAttributes = dnlib.DotNet.MethodAttributes;

namespace BitMono.Protections
{
    public class StringsEncryption : IProtection
    {
        private readonly IInjector m_Injector;
        private readonly IMethodDefSearcher m_MethodSearcher;
        private readonly IFieldSearcher m_FieldSearcher;
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;

        public StringsEncryption(
            IInjector injector,
            IFieldSearcher fieldSearcher,
            IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver,
            IMethodDefSearcher methodSearcher,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            IRenamer renamer,
            ILogger logger)
        {
            m_Injector = injector;
            m_MethodSearcher = methodSearcher;
            m_FieldSearcher = fieldSearcher;
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_Renamer = renamer;
            m_Logger = logger.ForContext<StringsEncryption>();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            context.ModuleDefMD.GlobalType.FindOrCreateStaticConstructor();

            var encryptorTypeDef = m_Injector.CreateInvisibleValueType(context.ModuleDefMD, "Encryptor");

            var saltBytes = new byte[] { 0x1, 0x3, 0x2, 0x3, 0x3, 0x4, 0x5, 0x10, 0x10 };
            var cryptKeyBytes = new byte[] { 0x1, 0x3, 0x10, 0x15, 0x20, 0x50, 0x5, 0x10, 0x10 };
            var saltBytesFieldDef = m_Injector.InjectArrayInGlobalNestedTypes(context.ModuleDefMD, saltBytes, "saltBytes");
            var cryptKeyBytesFieldDef = m_Injector.InjectArrayInGlobalNestedTypes(context.ModuleDefMD, cryptKeyBytes, "cryptKeyBytes");

            var decryptMethodDefFromEncryptionModule = m_MethodSearcher.Find("Decrypt", context.ExternalComponentsModuleDefMD);
            var decryptorMethodDef = new MethodDefUser("Decrypt", decryptMethodDefFromEncryptionModule.MethodSig, MethodAttributes.Static | MethodAttributes.Assembly);
            decryptorMethodDef.Body = decryptMethodDefFromEncryptionModule.Body;
            var saltBytesInjected = false;
            var cryptKeyBytesInjected = false;
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
            m_Renamer.Rename(saltBytesField, cryptKeyBytesField);

            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_ObfuscationAttributeExcludingResolver.TryResolve(typeDef, feature: nameof(StringsEncryption),
                    out ObfuscationAttribute typeDefObfuscationAttribute))
                {
                    if (typeDefObfuscationAttribute.Exclude)
                    {
                        m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                        continue;
                    }
                }

                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods.ToArray())
                    {
                        if (m_ObfuscationAttributeExcludingResolver.TryResolve(methodDef, feature: nameof(StringsEncryption),
                            out ObfuscationAttribute methodDefObfuscationAttribute))
                        {
                            if (methodDefObfuscationAttribute.Exclude)
                            {
                                m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                                continue;
                            }
                        }

                        if (methodDef.HasBody && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                        {
                            for (int i = 0; i < methodDef.Body.Instructions.Count(); i++)
                            {
                                if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr
                                    && methodDef.Body.Instructions[i].Operand is string content)
                                {
                                    var encryptedContentBytes = Encryptor.EncryptContent(content, saltBytes, cryptKeyBytes);
                                    var injectedEncryptedArrayBytes = m_Injector.InjectArrayInGlobalNestedTypes(context.ModuleDefMD, encryptedContentBytes, m_Renamer.RenameUnsafely());

                                    methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;
                                    methodDef.Body.Instructions.Insert(i++, new Instruction(OpCodes.Ldsfld, injectedEncryptedArrayBytes));
                                    methodDef.Body.Instructions.Insert(i++, new Instruction(OpCodes.Call, decryptorMethodDef));
                                    methodDef.Body.SimplifyAndOptimizeBranches();
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