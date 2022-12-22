namespace BitMono.Protections;

public class AntiDebugBreakpoints : IProtection
{
    private readonly CriticalAnalyzer m_DnlibDefCriticalAnalyzer;

    public AntiDebugBreakpoints(CriticalAnalyzer methodDefCriticalAnalyzer)
    {
        m_DnlibDefCriticalAnalyzer = methodDefCriticalAnalyzer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var threadSleepMethods = new List<IMethodDescriptor>
        {
            context.Importer.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new Type[]
            {
                typeof(int)
            })),
            context.Importer.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new Type[]
            {
                typeof(TimeSpan)
            })),
            context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
            {
                typeof(int)
            })),
            context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
            {
                typeof(TimeSpan)
            })),
            context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
            {
                typeof(int),
                typeof(CancellationToken),
            })),
            context.Importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new Type[]
            {
                typeof(TimeSpan),
                typeof(CancellationToken),
            })),
        };
        var dateTimeUtcNowMethod = context.Importer.ImportMethod(typeof(DateTime).GetProperty(nameof(DateTime.UtcNow)).GetMethod);
        var dateTimeSubtractionMethod = context.Importer.ImportMethod(typeof(DateTime).GetMethod("op_Subtraction", new Type[]
        {
            typeof(DateTime),
            typeof(DateTime)
        }));
        var timeSpanTotalMillisecondsMethod = context.Importer.ImportMethod(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds)).GetMethod);
        var environmentFailFast = context.Importer.ImportMethod(typeof(Environment).GetMethod(nameof(Environment.FailFast), new Type[]
        {
            typeof(string)
        }));
        var dateTime = context.Importer.ImportType(typeof(DateTime)).ToTypeSignature(isValueType: true);
        var timeSpan = context.Importer.ImportType(typeof(TimeSpan)).ToTypeSignature(isValueType: true);
        var @int = context.Importer.ImportType(typeof(int)).ToTypeSignature(isValueType: true);

        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(method)
                && method.NotGetterAndSetter()
                && method.IsConstructor == false)
            {
                if (method.CilMethodBody != null
                    && method.CilMethodBody.Instructions.Count >= 5)
                {
                    var startIndex = 0;
                    var endIndex = method.CilMethodBody.Instructions.Count - 1;
                    var methodShouldBeIgnored = false;

                    for (var i = startIndex; i < endIndex; i++)
                    {
                        if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Call
                            && method.CilMethodBody.Instructions[i].Operand is MemberReference member)
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

                    method.CilMethodBody.LocalVariables.Add(dateTimeLocal);
                    method.CilMethodBody.LocalVariables.Add(timeSpanLocal);
                    method.CilMethodBody.LocalVariables.Add(intLocal);

                    method.CilMethodBody.Instructions.InsertRange(startIndex, new CilInstruction[]
                    {
                        new CilInstruction(CilOpCodes.Call, dateTimeUtcNowMethod),
                        new CilInstruction(CilOpCodes.Stloc_S, dateTimeLocal)
                    });

                    var nopInstruction = new CilInstruction(CilOpCodes.Nop);
                    var nopLabel = nopInstruction.CreateLabel();
                    method.CilMethodBody.Instructions.InsertRange(endIndex, new CilInstruction[]
                    {
                        new CilInstruction(CilOpCodes.Call, dateTimeUtcNowMethod),
                        new CilInstruction(CilOpCodes.Ldloc_S, dateTimeLocal),
                        new CilInstruction(CilOpCodes.Call, dateTimeSubtractionMethod),
                        new CilInstruction(CilOpCodes.Stloc_S, timeSpanLocal),
                        new CilInstruction(CilOpCodes.Ldloca_S, timeSpanLocal),
                        new CilInstruction(CilOpCodes.Call, timeSpanTotalMillisecondsMethod),
                        new CilInstruction(CilOpCodes.Ldc_R8, 5000.0),
                        new CilInstruction(CilOpCodes.Ble_Un_S, nopLabel),
                        new CilInstruction(CilOpCodes.Ldc_I4_1),
                        new CilInstruction(CilOpCodes.Ldc_I4_0),
                        new CilInstruction(CilOpCodes.Stloc_S, intLocal),
                        new CilInstruction(CilOpCodes.Ldloc_S, intLocal),
                        new CilInstruction(CilOpCodes.Div),
                        new CilInstruction(CilOpCodes.Pop),
                        nopInstruction
                    });
                }
            }
        }
        return Task.CompletedTask;
    }
}