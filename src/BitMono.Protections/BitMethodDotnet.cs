namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class BitMethodDotnet : Protection
{
    private readonly RandomNext _randomNext;

    public BitMethodDotnet(RandomNext randomNext, IServiceProvider serviceProvider) : base(serviceProvider)
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

            var randomMethodBodyIndex = 0;
            if (body.Instructions.Count >= 3)
            {
                randomMethodBodyIndex = _randomNext(0, body.Instructions.Count);
            }

            var instruction = GetRandomInstruction();
            var label = body.Instructions[randomMethodBodyIndex].CreateLabel();
            body.Instructions.Insert(randomMethodBodyIndex, new CilInstruction(CilOpCodes.Br_S));
            body.Instructions.Insert(randomMethodBodyIndex + 1, instruction);
            body.Instructions[randomMethodBodyIndex].Operand = label;
        }
        return Task.CompletedTask;
    }

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