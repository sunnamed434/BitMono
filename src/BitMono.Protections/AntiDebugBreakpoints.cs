namespace BitMono.Protections;

[UsedImplicitly]
[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class AntiDebugBreakpoints : Protection
{
    public AntiDebugBreakpoints(ProtectionContext context) : base(context)
    {
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        var threadSleepMethods = new List<IMethodDescriptor>
        {
            Context.Importer.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new[]
            {
                typeof(int)
            })),
            Context.Importer.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new[]
            {
                typeof(TimeSpan)
            })),
            Context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(int)
            })),
            Context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(TimeSpan)
            })),
            Context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(int),
                typeof(CancellationToken),
            })),
            Context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(TimeSpan),
                typeof(CancellationToken),
            })),
        };
        var dateTimeUtcNowMethod = Context.Importer.ImportMethod(typeof(DateTime).GetProperty(nameof(DateTime.UtcNow)).GetMethod);
        var dateTimeSubtractionMethod = Context.Importer.ImportMethod(typeof(DateTime).GetMethod("op_Subtraction", new[]
        {
            typeof(DateTime),
            typeof(DateTime)
        }));
        var timeSpanTotalMillisecondsMethod = Context.Importer.ImportMethod(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds)).GetMethod);
        var environmentFailFast = Context.Importer.ImportMethod(typeof(Environment).GetMethod(nameof(Environment.FailFast), new[]
        {
            typeof(string)
        }));
        var dateTime = Context.Importer.ImportType(typeof(DateTime)).ToTypeSignature(isValueType: true);
        var timeSpan = Context.Importer.ImportType(typeof(TimeSpan)).ToTypeSignature(isValueType: true);
        var @int = Context.Importer.ImportType(typeof(int)).ToTypeSignature(isValueType: true);

        foreach (var method in parameters.Members.OfType<MethodDefinition>())
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
                            if (threadSleepMethods.Any(t => new SignatureComparer().Equals(member, t)))
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