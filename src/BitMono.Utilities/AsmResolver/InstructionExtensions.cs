namespace BitMono.Utilities.AsmResolver;

public static class InstructionExtensions
{
    public static int CountWithoutRet(this CilInstructionCollection source)
    {
        const int InstructionCount = 1;
        if (source[source.Count - InstructionCount].OpCode == CilOpCodes.Ret) 
        {
            return source.Count - InstructionCount;
        }
        throw new IndexOutOfRangeException();
    }
}