namespace BitMono.Utilities.AsmResolver;

public static class InstructionExtensions
{
    public static int CountWithoutRet(this CilInstructionCollection source)
    {
        const int instructionCount = 1;
        if (source[source.Count - instructionCount].OpCode == CilOpCodes.Ret)
        {
            return source.Count - instructionCount;
        }
        throw new IndexOutOfRangeException();
    }
}