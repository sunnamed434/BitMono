namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class StringsEncryption : Protection
{
    private readonly MscorlibInjector m_Injector;
    private readonly IRenamer m_Renamer;

    public StringsEncryption(RuntimeImplementations runtime, IRenamer renamer, ProtectionContext context) : base(context)
    {
        m_Injector = runtime.MscorlibInjector;
        m_Renamer = renamer;
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        var globalModuleType = Context.Module.GetOrCreateModuleType();
        m_Injector.InjectCompilerGeneratedValueType(Context.Module, globalModuleType, m_Renamer.RenameUnsafely());
        var cryptKeyField = m_Injector.InjectCompilerGeneratedArray(Context.Module, globalModuleType, Data.CryptKeyBytes, m_Renamer.RenameUnsafely());
        var saltBytesField = m_Injector.InjectCompilerGeneratedArray(Context.Module, globalModuleType, Data.SaltBytes, m_Renamer.RenameUnsafely());

        var runtimeDecryptorType = Context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Decryptor));
        var runtimeDecryptMethod = runtimeDecryptorType.Methods.Single(c => c.Name.Equals(nameof(Decryptor.Decrypt)));
        var listener = new ModifyInjectTypeClonerListener(ModifyFlags.All, m_Renamer, Context.Module);
        var memberCloneResult = new MemberCloner(Context.Module, listener)
            .Include(runtimeDecryptorType)
            .Clone();

        var decryptMethod = memberCloneResult.GetClonedMember(runtimeDecryptMethod);

        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                for (var i = 0; i < body.Instructions.Count; i++)
                {
                    if (body.Instructions[i].OpCode.Equals( CilOpCodes.Ldstr) && body.Instructions[i].Operand is string content)
                    {
                        var data = Encryptor.EncryptContent(content, Data.SaltBytes, Data.CryptKeyBytes);
                        var arrayName = m_Renamer.RenameUnsafely();
                        var encryptedDataFieldDef = m_Injector.InjectCompilerGeneratedArray(Context.Module, globalModuleType, data, arrayName);

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