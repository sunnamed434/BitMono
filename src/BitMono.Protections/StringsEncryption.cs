namespace BitMono.Protections;

public class StringsEncryption : IProtection
{
    private readonly IInjector m_Injector;
    private readonly CriticalAnalyzer m_CriticalAnalyzer;
    private readonly IRenamer m_Renamer;

    public StringsEncryption(
        IInjector injector,
        CriticalAnalyzer criticalAnalyzer,
        IRenamer renamer)
    {
        m_Injector = injector;
        m_CriticalAnalyzer = criticalAnalyzer;
        m_Renamer = renamer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var globalModuleType = context.Module.GetOrCreateModuleType();
        var decryptorType = m_Injector.InjectInvisibleValueType(context.Module, globalModuleType, m_Renamer.RenameUnsafely());
        var cryptKeyField = m_Injector.InjectInvisibleArray(context.Module, globalModuleType, Data.CryptKeyBytes, m_Renamer.RenameUnsafely());
        var saltBytesField = m_Injector.InjectInvisibleArray(context.Module, globalModuleType, Data.SaltBytes, m_Renamer.RenameUnsafely());

        var runtimeDecryptorType = context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Decryptor));
        var memberCloneResult = new MemberCloner(context.Module, new InjectTypeClonerListener(context.Module))
            .Include(runtimeDecryptorType)
            .Clone();

        var decryptMethod = memberCloneResult.ClonedMembers.Single(c => c.Name.Equals(nameof(Decryptor.Decrypt)));

        memberCloneResult
            .RenameClonedMembers(m_Renamer)
            .RemoveNamespaceOfClonedMembers(m_Renamer);

        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (method.HasMethodBody && m_CriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                for (var i = 0; i < method.CilMethodBody.Instructions.Count(); i++)
                {
                    if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Ldstr
                        && method.CilMethodBody.Instructions[i].Operand is string content)
                    {
                        var data = Encryptor.EncryptContent(content, Data.SaltBytes, Data.CryptKeyBytes);
                        var encryptedDataFieldDef = m_Injector.InjectInvisibleArray(context.Module, globalModuleType, data, m_Renamer.RenameUnsafely());

                        method.CilMethodBody.Instructions[i].ReplaceWith(CilOpCodes.Ldsfld, encryptedDataFieldDef);
                        method.CilMethodBody.Instructions.Insert(i + 1, new CilInstruction(CilOpCodes.Ldsfld, saltBytesField));
                        method.CilMethodBody.Instructions.Insert(i + 2, new CilInstruction(CilOpCodes.Ldsfld, cryptKeyField));
                        method.CilMethodBody.Instructions.Insert(i + 3, new CilInstruction(CilOpCodes.Call, decryptMethod));
                        i += 3;
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}