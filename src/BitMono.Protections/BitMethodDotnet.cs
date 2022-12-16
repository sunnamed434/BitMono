namespace BitMono.Protections;

[ProtectionName(nameof(BitMethodDotnet))]
public class BitMethodDotnet : IStageProtection
{
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
    private readonly ILogger m_Logger;
    private readonly Random m_Random;

    public BitMethodDotnet(
        DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
        ILogger logger)
    {
        m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
        m_Logger = logger.ForContext<BitMethodDotnet>();
        m_Random = new Random();
    }

    public PipelineStages Stage => PipelineStages.ModuleWritten;

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
        {
            foreach (var methodDef in typeDef.Methods)
            {
                if (methodDef.HasBody && methodDef.IsConstructor == false
                    && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                {
                    var randomMethodDefBodyIndex = 0;
                    if (methodDef.Body.Instructions.Count >= 3)
                    {
                        randomMethodDefBodyIndex = m_Random.Next(0, methodDef.Body.Instructions.Count);
                    }

                    var randomValue = m_Random.Next(0, 3);
                    var randomPrefixInstruction = new Instruction();
                    randomPrefixInstruction.OpCode = randomValue switch
                    {
                        0 => OpCodes.Readonly,
                        1 => OpCodes.Unaligned,
                        2 => OpCodes.Volatile,
                        3 => OpCodes.Constrained,
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    methodDef.Body.Instructions.Insert(randomMethodDefBodyIndex, new Instruction(OpCodes.Nop));
                    methodDef.Body.Instructions.Insert(randomMethodDefBodyIndex + 1, randomPrefixInstruction);
                    methodDef.Body.Instructions[randomMethodDefBodyIndex] = new Instruction(OpCodes.Br_S, methodDef.Body.Instructions[randomMethodDefBodyIndex + 2]);
                }
            }
        }
        return Task.CompletedTask;
    }
}