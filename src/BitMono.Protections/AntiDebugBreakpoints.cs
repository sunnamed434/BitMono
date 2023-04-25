namespace BitMono.Protections;

[UsedImplicitly]
[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class AntiDebugBreakpoints : Protection
{
    public AntiDebugBreakpoints(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync()
    {
        var threadSleepMethods = new List<IMethodDescriptor>
        {
            Context.ModuleImporter.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new[]
            {
                typeof(int)
            })),
            Context.ModuleImporter.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new[]
            {
                typeof(TimeSpan)
            })),
            Context.ModuleImporter.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(int)
            })),
            Context.ModuleImporter.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(TimeSpan)
            })),
            Context.ModuleImporter.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(int),
                typeof(CancellationToken),
            })),
            Context.ModuleImporter.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(TimeSpan),
                typeof(CancellationToken),
            })),
        };
        var dateTimeUtcNowMethod =
            Context.ModuleImporter.ImportMethod(typeof(DateTime).GetProperty(nameof(DateTime.UtcNow)).GetMethod);
        var dateTimeSubtractionMethod = Context.ModuleImporter.ImportMethod(typeof(DateTime).GetMethod("op_Subtraction", new[]
        {
            typeof(DateTime),
            typeof(DateTime)
        }));
        var timeSpanTotalMillisecondsMethod =
            Context.ModuleImporter.ImportMethod(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds))
                .GetMethod);
        var dateTime = Context.ModuleImporter.ImportType(typeof(DateTime)).ToTypeSignature(isValueType: true);
        var timeSpan = Context.ModuleImporter.ImportType(typeof(TimeSpan)).ToTypeSignature(isValueType: true);
        var @int = Context.ModuleImporter.ImportType(typeof(int)).ToTypeSignature(isValueType: true);

        var signatureComparer = new SignatureComparer();
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.NotGetterAndSetter() && method.IsConstructor == false)
            {
                if (method.CilMethodBody is { Instructions.Count: >= 5 } body)
                {
                    var startIndex = 0;
                    var endIndex = body.Instructions.Count - 1;
                    var methodShouldBeIgnored = false;

                    for (var i = startIndex; i < endIndex; i++)
                    {
                        var instruction = body.Instructions[i];
                        if (instruction.OpCode == CilOpCodes.Call && instruction.Operand is MemberReference member)
                        {
                            if (threadSleepMethods.Any(t => signatureComparer.Equals(member, t)))
                            {
                                methodShouldBeIgnored = true;
                                break;
                            }
                        }
                    }

                    if (methodShouldBeIgnored)
                    {
                        methodShouldBeIgnored = false;
                        continue;
                    }

                    var dateTimeLocal = new CilLocalVariable(dateTime);
                    var timeSpanLocal = new CilLocalVariable(timeSpan);
                    var intLocal = new CilLocalVariable(@int);

                    body.LocalVariables.Add(dateTimeLocal);
                    body.LocalVariables.Add(timeSpanLocal);
                    body.LocalVariables.Add(intLocal);

                    body.Instructions.InsertRange(startIndex, new[]
                    {
                        new CilInstruction(CilOpCodes.Call, dateTimeUtcNowMethod),
                        new CilInstruction(CilOpCodes.Stloc_S, dateTimeLocal)
                    });

                    var nopInstruction = new CilInstruction(CilOpCodes.Nop);
                    var nopLabel = nopInstruction.CreateLabel();
                    body.Instructions.InsertRange(endIndex, new[]
                    {
                        new(CilOpCodes.Call, dateTimeUtcNowMethod),
                        new(CilOpCodes.Ldloc_S, dateTimeLocal),
                        new(CilOpCodes.Call, dateTimeSubtractionMethod),
                        new(CilOpCodes.Stloc_S, timeSpanLocal),
                        new(CilOpCodes.Ldloca_S, timeSpanLocal),
                        new(CilOpCodes.Call, timeSpanTotalMillisecondsMethod),
                        new(CilOpCodes.Ldc_R8, 5000.0),
                        new(CilOpCodes.Ble_Un_S, nopLabel),
                        new(CilOpCodes.Ldc_I4_1),
                        new(CilOpCodes.Ldc_I4_0),
                        new(CilOpCodes.Stloc_S, intLocal),
                        new(CilOpCodes.Ldloc_S, intLocal),
                        new(CilOpCodes.Div),
                        new(CilOpCodes.Pop),
                        nopInstruction
                    });
                }
            }
        }
        return Task.CompletedTask;
    }
}