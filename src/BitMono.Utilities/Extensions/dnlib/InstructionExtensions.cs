using dnlib.DotNet.Emit;

namespace BitMono.Utilities.Extensions.dnlib;

public static class InstructionExtensions
{
    public static Instruction ReplaceWith(this Instruction source, OpCode opCode, object operand)
    {
        source.OpCode = opCode;
        source.Operand = operand;
        return source;
    }
    public static int CountWithoutRet(this IList<Instruction> source)
    {
        const int InstructionCount = 1;
        if (source[source.Count - InstructionCount].OpCode == OpCodes.Ret) 
        {
            return source.Count - InstructionCount;
        }
        throw new IndexOutOfRangeException();
    }
}