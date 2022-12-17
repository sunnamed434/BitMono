namespace BitMono.Protections;

[ProtectionName(nameof(StringsEncryption))]
public class StringsEncryption : IProtection
{
    private readonly IInjector m_Injector;
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
    private readonly IRenamer m_Renamer;

    public StringsEncryption(
        IInjector injector,
        DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
        IRenamer renamer)
    {
        m_Injector = injector;
        m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
        m_Renamer = renamer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var runtimeDecryptorTypeDef = context.RuntimeModuleDefMD.ResolveTypeDefOrThrow<Decryptor>();

        var decryptorTypeDef = m_Injector.InjectInvisibleValueType(context.ModuleDefMD, context.ModuleDefMD.GlobalType, m_Renamer.RenameUnsafely()).ResolveTypeDefThrow();
        var cryptKeyFieldDef = m_Injector.InjectInvisibleArray(context.ModuleDefMD, context.ModuleDefMD.GlobalType, Data.CryptKeyBytes, m_Renamer.RenameUnsafely()).ResolveFieldDefThrow();
        var saltBytesFieldDef = m_Injector.InjectInvisibleArray(context.ModuleDefMD, context.ModuleDefMD.GlobalType, Data.SaltBytes, m_Renamer.RenameUnsafely()).ResolveFieldDefThrow();

        var injectedDecryptorDnlibDefs = InjectHelper.Inject(runtimeDecryptorTypeDef, decryptorTypeDef, context.ModuleDefMD);
        var decryptMethodDef = injectedDecryptorDnlibDefs.FirstOrDefault(i => i.Name.String.Equals(nameof(Decryptor.Decrypt))).ResolveMethodDefOrThrow();
        
        foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
        {
            foreach (var methodDef in typeDef.Methods.ToArray())
            {
                if (methodDef.HasBody && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                {
                    for (var i = 0; i < methodDef.Body.Instructions.Count(); i++)
                    {
                        if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr
                            && methodDef.Body.Instructions[i].Operand is string content)
                        {
                            var encryptedContentBytes = Encryptor.EncryptContent(content, Data.SaltBytes, Data.CryptKeyBytes);
                            var encryptedDataFieldDef = m_Injector.InjectInvisibleArray(context.ModuleDefMD, context.ModuleDefMD.GlobalType, encryptedContentBytes, m_Renamer.RenameUnsafely());

                            methodDef.Body.Instructions[i].ReplaceWith(OpCodes.Ldsfld, encryptedDataFieldDef);
                            methodDef.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldsfld, saltBytesFieldDef));
                            methodDef.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Ldsfld, cryptKeyFieldDef));
                            methodDef.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Call, decryptMethodDef));
                            i += 3;
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}