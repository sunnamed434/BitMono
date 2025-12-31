namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class AntiDebugBreakpoints : Protection
{
    public AntiDebugBreakpoints(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync()
    {
        var importer = Context.ModuleImporter;
        var threadSleepMethods = new List<IMethodDescriptor>
        {
            importer.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new[]
            {
                typeof(int)
            })),
            importer.ImportMethod(typeof(Thread).GetMethod(nameof(Thread.Sleep), new[]
            {
                typeof(TimeSpan)
            })),
            importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(int)
            })),
            importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(TimeSpan)
            })),
            importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(int),
                typeof(CancellationToken),
            })),
            importer.ImportMethod(typeof(Task).GetMethod(nameof(Task.Delay), new[]
            {
                typeof(TimeSpan),
                typeof(CancellationToken),
            })),
        };
        var dateTimeUtcNowMethod =
            importer.ImportMethod(typeof(DateTime).GetProperty(nameof(DateTime.UtcNow)).GetMethod);
        var dateTimeSubtractionMethod = importer.ImportMethod(typeof(DateTime).GetMethod("op_Subtraction", new[]
        {
            typeof(DateTime),
            typeof(DateTime)
        }));
        var timeSpanTotalMillisecondsMethod =
            importer.ImportMethod(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds))
                .GetMethod);
        var dateTime = importer.ImportType(typeof(DateTime)).ToTypeSignature(isValueType: true);
        var timeSpan = importer.ImportType(typeof(TimeSpan)).ToTypeSignature(isValueType: true);
        var @int = importer.ImportType(typeof(int)).ToTypeSignature(isValueType: true);

        var signatureComparer = new SignatureComparer();
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.IsConstructor)
            {
                continue;
            }
            if (method.IsGetMethod || method.IsSetMethod)
            {
                continue;
            }
            if (method.CilMethodBody is not { } body)
            {
                continue;
            }
            if (body.Instructions.Count < 5)
            {
                continue;
            }

            var startIndex = 0;
            var endIndex = body.Instructions.Count - 1;
            var methodShouldBeIgnored = false;

            for (var i = startIndex; i < endIndex; i++)
            {
                var instruction = body.Instructions[i];
                if (instruction.OpCode != CilOpCodes.Call)
                {
                    continue;
                }
                if (instruction.Operand is not MemberReference member)
                {
                    continue;
                }
                if (threadSleepMethods.Any(x => signatureComparer.Equals(member, x)) == false)
                {
                    continue;
                }

                methodShouldBeIgnored = true;
                break;
            }

            if (methodShouldBeIgnored)
            {
                continue;
            }

            var dateTimeLocal = new CilLocalVariable(dateTime);
            var timeSpanLocal = new CilLocalVariable(timeSpan);
            var intLocal = new CilLocalVariable(@int);

            body.LocalVariables.Add(dateTimeLocal);
            body.LocalVariables.Add(timeSpanLocal);
            body.LocalVariables.Add(intLocal);

            body.Instructions.InsertRange(startIndex,
            [
                new CilInstruction(CilOpCodes.Call, dateTimeUtcNowMethod),
                new CilInstruction(CilOpCodes.Stloc_S, dateTimeLocal)
            ]);

            var nopInstruction = new CilInstruction(CilOpCodes.Nop);
            var nopLabel = nopInstruction.CreateLabel();
            body.Instructions.InsertRange(endIndex,
            [
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
            ]);
        }
        return Task.CompletedTask;
    }
}