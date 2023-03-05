namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class StringsEncryption : Protection
{
    private readonly Renamer _renamer;

    public StringsEncryption(Renamer renamer, ProtectionContext context) : base(context)
    {
        _renamer = renamer;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        var globalModuleType = Context.Module.GetOrCreateModuleType();
        MscorlibInjector.InjectCompilerGeneratedValueType(Context.Module, globalModuleType, _renamer.RenameUnsafely());
        var cryptKeyField = MscorlibInjector.InjectCompilerGeneratedArray(Context.Module, globalModuleType, Data.CryptKeyBytes, _renamer.RenameUnsafely());
        var saltBytesField = MscorlibInjector.InjectCompilerGeneratedArray(Context.Module, globalModuleType, Data.SaltBytes, _renamer.RenameUnsafely());

        var runtimeDecryptorType = Context.RuntimeModule.ResolveOrThrow<TypeDefinition>(typeof(Decryptor));
        var runtimeDecryptMethod = runtimeDecryptorType.Methods.Single(c => c.Name.Equals(nameof(Decryptor.Decrypt)));
        var listener = new ModifyInjectTypeClonerListener(ModifyFlags.All, _renamer, Context.Module);
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
                        var arrayName = _renamer.RenameUnsafely();
                        var encryptedDataFieldDef = MscorlibInjector.InjectCompilerGeneratedArray(Context.Module, globalModuleType, data, arrayName);

                        body.Instructions[i].ReplaceWith(CilOpCodes.Ldsfld, encryptedDataFieldDef);
                        body.Instructions.InsertRange(i + 1, new CilInstruction[]
                        {
                            new(CilOpCodes.Ldsfld, saltBytesField),
                            new(CilOpCodes.Ldsfld, cryptKeyField),
                            new(CilOpCodes.Call, decryptMethod),
                        });
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}