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
}