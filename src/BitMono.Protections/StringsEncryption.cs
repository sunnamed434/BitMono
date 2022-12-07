using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.FieldDefs;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Helpers;
using BitMono.Runtime;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    public class StringsEncryption : IProtection
    {
        private readonly IInjector m_Injector;
        private readonly IFieldSearcher m_FieldSearcher;
        private readonly IMethodDefSearcher m_MethodSearcher;
        private readonly IDnlibDefFeatureObfuscationAttributeHavingResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;

        public StringsEncryption(
            IInjector injector,
            IFieldSearcher fieldSearcher,
            IMethodDefSearcher methodSearcher,
            IDnlibDefFeatureObfuscationAttributeHavingResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            IRenamer renamer,
            ILogger logger)
        {
            m_Injector = injector;
            m_FieldSearcher = fieldSearcher;
            m_MethodSearcher = methodSearcher;
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_Renamer = renamer;
            m_Logger = logger.ForContext<StringsEncryption>();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            var runtimeDecryptorTypeDef = context.RuntimeModuleDefMD.ResolveTypeDefOrThrow<Decryptor>();

            var decryptorTypeDef = m_Injector.InjectInvisibleValueType(context.ModuleDefMD, context.ModuleDefMD.GlobalType, m_Renamer.RenameUnsafely()).ResolveTypeDefThrow();
            var cryptKeyFieldDef = m_Injector.InjectInvisibleArray(context.ModuleDefMD, context.ModuleDefMD.GlobalType, Data.CryptKeyBytes, m_Renamer.RenameUnsafely()).ResolveFieldDefThrow();
            var saltBytesFieldDef = m_Injector.InjectInvisibleArray(context.ModuleDefMD, context.ModuleDefMD.GlobalType, Data.SaltBytes, m_Renamer.RenameUnsafely()).ResolveFieldDefThrow();

            var injectedDecryptorDnlibDefs = InjectHelper.Inject(runtimeDecryptorTypeDef, decryptorTypeDef, context.ModuleDefMD);
            var decryptMethodDef = injectedDecryptorDnlibDefs.FirstOrDefault(i => i.Name.String.Equals(nameof(Decryptor.Decrypt))).ResolveMethodDefOrThrow();
            
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<StringsEncryption>(typeDef))
                {
                    m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                    continue;
                }
                if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
                {
                    m_Logger.Information("Not able to make changes because of specific namespace was found, skipping.");
                    continue;
                }

                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods.ToArray())
                    {
                        if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<StringsEncryption>(methodDef))
                        {
                            m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                            continue;
                        }
                        if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(methodDef) == false)
                        {
                            m_Logger.Information("Not able to make changes because of specific namespace was found, skipping.");
                            continue;
                        }

                        if (methodDef.HasBody && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                        {
                            for (int i = 0; i < methodDef.Body.Instructions.Count(); i++)
                            {
                                if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr
                                    && methodDef.Body.Instructions[i].Operand is string content)
                                {
                                    var encryptedContentBytes = Encryptor.EncryptContent(content, Data.SaltBytes, Data.CryptKeyBytes);
                                    var encryptedDataFieldDef = m_Injector.InjectInvisibleArray(context.ModuleDefMD, context.ModuleDefMD.GlobalType, encryptedContentBytes, m_Renamer.RenameUnsafely());

                                    methodDef.Body.Instructions[i] = new Instruction(OpCodes.Nop);
                                    methodDef.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldsfld, encryptedDataFieldDef));
                                    methodDef.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Ldsfld, saltBytesFieldDef));
                                    methodDef.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Ldsfld, cryptKeyFieldDef));
                                    methodDef.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Call, decryptMethodDef));
                                    i += 4;
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