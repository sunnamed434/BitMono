namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.MethodBody)]
public class StringsEncryption : Protection
{
    private readonly Renamer _renamer;

    public StringsEncryption(Renamer renamer, IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
        _renamer = renamer;
    }

    public override Task ExecuteAsync()
    {
        var module = Context.Module;
        // Inject into a top-level type, not <Module>: nested types in <Module> crash .NET 9+
        // with BadImageFormatException (dotnet/runtime#111164).
        var containerType = MscorlibInjector.InjectGlobalContainerType(module, _renamer.RenameUnsafely());
        MscorlibInjector.InjectCompilerGeneratedValueType(module, containerType, _renamer.RenameUnsafely());
        var cryptKeyField = MscorlibInjector.InjectCompilerGeneratedArray(module, containerType, Data.CryptKeyBytes, _renamer.RenameUnsafely());
        var saltBytesField = MscorlibInjector.InjectCompilerGeneratedArray(module, containerType, Data.SaltBytes, _renamer.RenameUnsafely());

        var runtimeModule = Context.RuntimeModule;
        var runtimeDecryptorType = runtimeModule.ResolveOrThrow<TypeDefinition>(typeof(Decryptor));
        var runtimeDecryptMethod = runtimeDecryptorType.Methods.Single(x => x.Name!.Equals(nameof(Decryptor.Decrypt)));
        var listener = new ModifyInjectTypeClonerListener(ModifyFlags.All, _renamer, module);
        var memberCloneResult = new MemberCloner(module, listener)
            .Include(runtimeDecryptorType)
            .CloneSafely(module, runtimeModule);

        var decryptMethod = memberCloneResult.GetClonedMember(runtimeDecryptMethod);

        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is not { } body)
            {
                continue;
            }

            var instructions = body.Instructions;
            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (!instruction.OpCode.Equals(CilOpCodes.Ldstr))
                {
                    continue;
                }
                if (instruction.Operand is not string content)
                {
                    continue;
                }

                var data = Encryptor.EncryptContent(content, Data.SaltBytes, Data.CryptKeyBytes);
                var arrayName = _renamer.RenameUnsafely();
                var encryptedDataFieldDef = MscorlibInjector.InjectCompilerGeneratedArray(module, containerType, data, arrayName);

                instruction.ReplaceWith(CilOpCodes.Ldsfld, encryptedDataFieldDef);
                instructions.InsertRange(i + 1,
                [
                    new CilInstruction(CilOpCodes.Ldsfld, saltBytesField),
                    new CilInstruction(CilOpCodes.Ldsfld, cryptKeyField),
                    new CilInstruction(CilOpCodes.Call, decryptMethod)
                ]);
            }
        }
        return Task.CompletedTask;
    }
}