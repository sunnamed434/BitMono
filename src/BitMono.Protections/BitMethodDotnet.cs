namespace BitMono.Protections;

public class BitMethodDotnet : IStageProtection
{
    private readonly RuntimeCriticalAnalyzer m_RuntimeCriticalAnalyzer;
    private readonly Random m_Random;

    public BitMethodDotnet(RuntimeCriticalAnalyzer runtimeCriticalAnalyzer)
    {
        m_RuntimeCriticalAnalyzer = runtimeCriticalAnalyzer;
        m_Random = new Random();
    }

    public PipelineStages Stage => PipelineStages.ModuleWrite;

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body && method.IsConstructor == false
                && m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                var randomMethodBodyIndex = 0;
                if (body.Instructions.Count >= 3)
                {
                    randomMethodBodyIndex = m_Random.Next(0, body.Instructions.Count);
                }

                var randomValue = m_Random.Next(0, 3);
                var randomOpCode = randomValue switch
                {
                    0 => CilOpCodes.Readonly,
                    1 => CilOpCodes.Unaligned,
                    2 => CilOpCodes.Volatile,
                    3 => CilOpCodes.Constrained,
                    _ => throw new ArgumentOutOfRangeException(),
                };

                var label = body.Instructions[randomMethodBodyIndex].CreateLabel();
                body.Instructions.Insert(randomMethodBodyIndex, new CilInstruction(CilOpCodes.Br_S));
                body.Instructions.Insert(randomMethodBodyIndex + 1, new CilInstruction(randomOpCode));
                body.Instructions[randomMethodBodyIndex].Operand = label;
            }
        }
        return Task.CompletedTask;
    }
}