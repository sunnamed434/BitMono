namespace BitMono.Protections;

[UsedImplicitly]
[SuppressMessage("ReSharper", "InvertIf")]
[RuntimeMonikerNETCore]
[RuntimeMonikerNETFramework]
public class UnmanagedString : Protection
{
    private const int Windows1252Encoding = 1252;

    public UnmanagedString(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        var moduleImporter = Context.ModuleImporter;
        var stringSbytePointerCtor =
            moduleImporter.ImportMethod(typeof(string).GetConstructor(new[] { typeof(sbyte*) })!);
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method is { CilMethodBody: { } body })
            {
                var instructions = body.Instructions;
                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    if (instruction.OpCode == CilOpCodes.Ldstr && instruction.Operand is string content)
                    {
                        var nativeMethod = CreateNativeMethod(content, Context.Module, Context.X86);
                        if (nativeMethod != null)
                        {
                            instruction.ReplaceWith(CilOpCodes.Call, nativeMethod);
                            method.CilMethodBody.Instructions.Insert(++i,
                                new CilInstruction(CilOpCodes.Newobj, stringSbytePointerCtor));
                        }
                    }
                }

            }
        }
        return Task.CompletedTask;
    }

    private static MethodDefinition? CreateNativeMethod(string content, ModuleDefinition module,
        bool x86)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var windows1252Encoding = Encoding.GetEncoding(Windows1252Encoding);
        if (CanBeEncodedInWindows1252(content) == false)
        {
            return null;
        }

        var factory = module.CorLibTypeFactory;

        var methodName = Guid.NewGuid().ToString();
        var method = new MethodDefinition(methodName, MethodAttributes.Public | MethodAttributes.Static,
            MethodSignature.CreateStatic(factory.SByte.MakePointerType()));

        method.ImplAttributes |= MethodImplAttributes.Native | MethodImplAttributes.Unmanaged |
                                 MethodImplAttributes.PreserveSig;
        method.Attributes |= MethodAttributes.PInvokeImpl;

        module.GetOrCreateModuleType().Methods.Add(method);

        var stringBytes = windows1252Encoding.GetBytes(content);

        NativeMethodBody body;
        if (x86)
        {
            body = new NativeMethodBody(method)
            {
                Code = new byte[]
                {
                    0x55, // push ebp
                    0x89, 0xE5, // mov ebp, esp
                    0xE8, 0x05, 0x00, 0x00, 0x00, // call <jump1>
                    0x83, 0xC0, 0x01, // add eax, 1
                    // <jump2>:
                    0x5D, // pop ebp
                    0xC3, // ret
                    // <jump1>:
                    0x58, // pop eax
                    0x83, 0xC0, 0x0B, // add eax, 0xb
                    0xEB, 0xF8 // jmp <jump2>
                }.Concat(stringBytes).Concat(new byte[] { 0x00 }).ToArray()
            };
        }
        else
        {
            body = new NativeMethodBody(method)
            {
                Code = new byte[]
                {
                    0x48, 0x8D, 0x05, 0x01, 0x00, 0x00, 0x00, // lea rax, [rip + 0x1]
                    0xC3 // ret
                }.Concat(stringBytes).Concat(new byte[] { 0x00 }).ToArray()
            };
        }

        method.NativeMethodBody = body;
        return method;
    }
    private static bool CanBeEncodedInWindows1252(string text)
    {
        try
        {
            _ = Encoding
                .GetEncoding(Windows1252Encoding, EncoderFallback.ExceptionFallback, DecoderFallback.ReplacementFallback)
                .GetBytes(text);
            return true;
        }
        catch
        {
            return false;
        }
    }
}