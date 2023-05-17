namespace BitMono.Protections;

[UsedImplicitly]
[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class BitMethodDotnet : Protection
{
    private readonly RandomNext _randomNext;

    public BitMethodDotnet(RandomNext randomNext, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _randomNext = randomNext;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    public override Task ExecuteAsync()
    {
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method is { CilMethodBody: { } body, IsConstructor: false })
            {
                var randomMethodBodyIndex = 0;
                if (body.Instructions.Count >= 3)
                {
                    randomMethodBodyIndex = _randomNext(0, body.Instructions.Count);
                }

                var randomValue = _randomNext(0, 3);
                var randomOpCode = randomValue switch
                {
                    0 => CilOpCodes.Readonly,
                    1 => CilOpCodes.Unaligned,
                    2 => CilOpCodes.Volatile,
                    3 => CilOpCodes.Constrained,
                    _ => throw new ArgumentOutOfRangeException($"Random value {randomValue} cannot be selected as Random CilOpCode"),
                };

                var label = body.Instructions[randomMethodBodyIndex].CreateLabel();
                body.Instructions.Insert(randomMethodBodyIndex, new CilInstruction(CilOpCodes.Br_S));
                if (randomOpCode == CilOpCodes.Unaligned)
                {
                    body.Instructions.Insert(randomMethodBodyIndex + 1, new CilInstruction(randomOpCode, (sbyte)0));
                }
                else
                {
                    body.Instructions.Insert(randomMethodBodyIndex + 1, new CilInstruction(randomOpCode));
                }
                body.Instructions[randomMethodBodyIndex].Operand = label;
            }
        }
        return Task.CompletedTask;
    }
}