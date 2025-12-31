namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class BitMethodDotnet : Protection
{
    private readonly RandomNext _randomNext;

    public BitMethodDotnet(RandomNext randomNext, IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
        _randomNext = randomNext;
    }

    public override Task ExecuteAsync()
    {
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is not { } body)
            {
                continue;
            }
            if (method.IsConstructor)
            {
                continue;
            }

            const int firstInstruction = 0;
            var instruction = GetRandomInstruction();
            var label = body.Instructions[firstInstruction].CreateLabel();
            body.Instructions.Insert(firstInstruction, new CilInstruction(CilOpCodes.Br_S));
            body.Instructions.Insert(firstInstruction + 1, instruction);
            body.Instructions[firstInstruction].Operand = label;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get the random instruction that breaks the decompiler method.
    /// </summary>
    private CilInstruction GetRandomInstruction()
    {
        var randomValue = _randomNext(0, 3);
        var randomOpCode = randomValue switch
        {
            0 => CilOpCodes.Readonly,
            1 => CilOpCodes.Unaligned,
            2 => CilOpCodes.Volatile,
            3 => CilOpCodes.Constrained,
            _ => throw new ArgumentOutOfRangeException($"Random value {randomValue} cannot be selected as Random CilOpCode"),
        };
        return randomOpCode == CilOpCodes.Unaligned
            ? new CilInstruction(randomOpCode, (sbyte)0)
            : new CilInstruction(randomOpCode);
    }
}