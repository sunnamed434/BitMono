namespace BitMono.Protections;

[ConfigureForNativeCode]
[RuntimeMonikerNETCore]
[RuntimeMonikerNETFramework]
public class UnmanagedString : Protection
{
    public UnmanagedString(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        var moduleImporter = Context.ModuleImporter;
        var stringSbytePointerCtor =
            moduleImporter.ImportMethod(typeof(string).GetConstructor([typeof(sbyte*)])!);
        var stringCharPointerCtor =
            moduleImporter.ImportMethod(typeof(string).GetConstructor([typeof(char*)])!);
        var stringSbytePointerWithLengthCtor =
            moduleImporter.ImportMethod(typeof(string).GetConstructor([typeof(sbyte*), typeof(int), typeof(int)])!);
        var stringCharPointerWithLengthCtor =
            moduleImporter.ImportMethod(typeof(string).GetConstructor([typeof(char*), typeof(int), typeof(int)])!);
        var encodedStrings = new Dictionary<string, MethodDefinition>();
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method is { CilMethodBody: { } body })
            {
                var instructions = body.Instructions;
                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    if (instruction.OpCode == CilOpCodes.Ldstr && instruction.Operand is string content && content.Length > 0) // skip empty string
                    {
                        var useUnicode = !CanBeEncodedIn7BitASCII(content);
                        var addNullTerminator = !HasNullCharacter(content);

                        if (encodedStrings.TryGetValue(content, out var nativeMethod) == false) // reuse encoded strings
                        {
                            nativeMethod = CreateNativeMethod(content, Context.Module, Context.X86, useUnicode, addNullTerminator);
                            encodedStrings.Add(content, nativeMethod);
                        }

                        if (nativeMethod != null)
                        {
                            instruction.ReplaceWith(CilOpCodes.Call, nativeMethod);

                            if (addNullTerminator)
                            {
                                method.CilMethodBody.Instructions.Insert(++i,
                                    new CilInstruction(CilOpCodes.Newobj, useUnicode ? stringCharPointerCtor : stringSbytePointerCtor));
                            }
                            else
                            {
                                method.CilMethodBody.Instructions.Insert(++i,
                                    CilInstruction.CreateLdcI4(0));
                                method.CilMethodBody.Instructions.Insert(++i,
                                    CilInstruction.CreateLdcI4(content.Length));
                                method.CilMethodBody.Instructions.Insert(++i,
                                    new CilInstruction(CilOpCodes.Newobj, useUnicode ? stringCharPointerWithLengthCtor : stringSbytePointerWithLengthCtor));
                            }
                        }
                    }
                }

            }
        }
        return Task.CompletedTask;
    }

    private static MethodDefinition CreateNativeMethod(string content, ModuleDefinition module,
        bool x86, bool useUnicode, bool addNullTerminator)
    {
        var methodName = Guid.NewGuid().ToString();
        var factory = module.CorLibTypeFactory;
        var method = new MethodDefinition(methodName, MethodAttributes.Public | MethodAttributes.Static,
            MethodSignature.CreateStatic(factory.SByte.MakePointerType()));

        method.ImplAttributes |= MethodImplAttributes.Native | MethodImplAttributes.Unmanaged |
                                 MethodImplAttributes.PreserveSig;
        method.Attributes |= MethodAttributes.PInvokeImpl;

        module.GetOrCreateModuleType().Methods.Add(method);

        if (addNullTerminator)
        {
            content += "\0"; // not adding on byte level as it has encoding-dependent size
        }

        var stringBytes = useUnicode
            ? Encoding.Unicode.GetBytes(content)
            : Encoding.ASCII.GetBytes(content);

        IEnumerable<byte> code;
        if (x86)
        {
            code = new byte[]
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
            };
        }
        else
        {
            code = new byte[]
            {
                0x48, 0x8D, 0x05, 0x01, 0x00, 0x00, 0x00, // lea rax, [rip + 0x1]
                0xC3 // ret
            };
        }
        code = code.Concat(stringBytes);

        var body = new NativeMethodBody(method)
        {
            Code = code.ToArray()
        };
        method.NativeMethodBody = body;
        return method;
    }
    private static bool CanBeEncodedIn7BitASCII(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] > '\x7f')
            {
                return false;
            }
        }
        return true;
    }
    private static bool HasNullCharacter(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\0')
            {
                return true;
            }
        }
        return false;
    }
}