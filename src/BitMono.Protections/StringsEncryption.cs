namespace BitMono.Protections;

[DoNotResolve(Members.SpecialRuntime)]
public class StringsEncryption : IProtection
{
    private readonly IInjector m_Injector;
    private readonly IRenamer m_Renamer;

    public StringsEncryption(IInjector injector, IRenamer renamer)
    {
        m_Injector = injector;
        m_Renamer = renamer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        var globalModuleType = context.Module.GetOrCreateModuleType();
        var decryptorType = m_Injector.InjectInvisibleValueType(context.Module, globalModuleType, m_Renamer.RenameUnsafely());
        var cryptKeyField = m_Injector.InjectInvisibleArray(context.Module, globalModuleType, Data.CryptKeyBytes, m_Renamer.RenameUnsafely());
        var saltBytesField = m_Injector.InjectInvisibleArray(context.Module, globalModuleType, Data.SaltBytes, m_Renamer.RenameUnsafely());

        var runtimeDecryptorType = context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Decryptor));
        var runtimeDecryptMethod = runtimeDecryptorType.Methods.Single(c => c.Name.Equals(nameof(Decryptor.Decrypt)));
        var listener = new ModifyInjectTypeClonerListener(Modifies.RenameAndRemoveNamespace, m_Renamer, context.Module);
        var memberCloneResult = new MemberCloner(context.Module, listener)
            .Include(runtimeDecryptorType)
            .Clone();

        var decryptMethod = memberCloneResult.GetClonedMember(runtimeDecryptMethod);

        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                for (var i = 0; i < body.Instructions.Count(); i++)
                {
                    if (body.Instructions[i].OpCode == CilOpCodes.Ldstr
                        && body.Instructions[i].Operand is string content)
                    {
                        var data = Encryptor.EncryptContent(content, Data.SaltBytes, Data.CryptKeyBytes);
                        var encryptedDataFieldDef = m_Injector.InjectInvisibleArray(context.Module, globalModuleType, data, m_Renamer.RenameUnsafely());

                        body.Instructions[i].ReplaceWith(CilOpCodes.Ldsfld, encryptedDataFieldDef);
                        body.Instructions.InsertRange(i + 1, new CilInstruction[]
                        {
                            new CilInstruction(CilOpCodes.Ldsfld, saltBytesField),
                            new CilInstruction(CilOpCodes.Ldsfld, cryptKeyField),
                            new CilInstruction(CilOpCodes.Call, decryptMethod),
                        });
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}