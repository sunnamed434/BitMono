namespace BitMono.Core.Analyzing;

/// <summary>
/// Helpers over Washi's Echo symbolic data-flow graph for recovering the constant arguments
/// (member-name strings, <c>typeof</c> tokens) that feed a reflection call. Echo already computes
/// the stack and variable dependencies of every instruction, so instead of simulating the stack by
/// hand we just follow its edges back to the producing instructions - seeing through local copies
/// (<c>ldloc</c>/<c>stloc</c>) and <c>dup</c>. Intra-method only, like ConfuserEx.
/// </summary>
internal static class ReflectionDataFlow
{
    /// <summary>
    /// The producer instructions feeding every stack operand of <paramref name="node"/>, flattened.
    /// Order-independent on purpose: a reflection call's name string and type token are recovered by
    /// scanning the whole operand set, so we never depend on Echo's stack-slot ordering.
    /// </summary>
    public static List<DataFlowNode<CilInstruction>> OperandNodes(DataFlowNode<CilInstruction> node)
    {
        var result = new List<DataFlowNode<CilInstruction>>();
        var visited = new HashSet<DataFlowNode<CilInstruction>>();
        foreach (var dependency in node.StackDependencies)
        {
            foreach (var source in dependency)
            {
                Collect(source.Node, visited, result);
            }
        }
        return result;
    }

    private static void Collect(DataFlowNode<CilInstruction>? node,
        HashSet<DataFlowNode<CilInstruction>> visited, List<DataFlowNode<CilInstruction>> result)
    {
        if (node?.Contents == null || !visited.Add(node))
        {
            return;
        }

        var code = node.Contents.OpCode.Code;
        if (IsLoadLocal(code))
        {
            // ldloc: jump to the stloc(s) that defined the variable.
            foreach (var dependency in node.VariableDependencies)
            {
                foreach (var source in dependency)
                {
                    Collect(source.Node, visited, result);
                }
            }
            return;
        }
        if (IsStoreLocal(code) || code == CilCode.Dup)
        {
            // stloc/dup: follow the stored/duplicated value.
            foreach (var dependency in node.StackDependencies)
            {
                foreach (var source in dependency)
                {
                    Collect(source.Node, visited, result);
                }
            }
            return;
        }
        result.Add(node);
    }

    private static bool IsLoadLocal(CilCode code) => code is CilCode.Ldloc or CilCode.Ldloc_S
        or CilCode.Ldloc_0 or CilCode.Ldloc_1 or CilCode.Ldloc_2 or CilCode.Ldloc_3;

    private static bool IsStoreLocal(CilCode code) => code is CilCode.Stloc or CilCode.Stloc_S
        or CilCode.Stloc_0 or CilCode.Stloc_1 or CilCode.Stloc_2 or CilCode.Stloc_3;
}
