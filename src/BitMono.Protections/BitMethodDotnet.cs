using BitMono.Core.Protecting.Analyzing;

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
            if (method.CilMethodBody != null && method.IsConstructor == false
                && m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                var randomMethodBodyIndex = 0;
                if (method.CilMethodBody.Instructions.Count >= 3)
                {
                    randomMethodBodyIndex = m_Random.Next(0, method.CilMethodBody.Instructions.Count);
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

                var label = method.CilMethodBody.Instructions[randomMethodBodyIndex].CreateLabel();
                method.CilMethodBody.Instructions.Insert(randomMethodBodyIndex, new CilInstruction(CilOpCodes.Br_S));
                method.CilMethodBody.Instructions.Insert(randomMethodBodyIndex + 1, new CilInstruction(randomOpCode));
                method.CilMethodBody.Instructions[randomMethodBodyIndex].Operand = label;
            }
        }
        return Task.CompletedTask;
    }
}