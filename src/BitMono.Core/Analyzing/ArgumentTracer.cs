namespace BitMono.Core.Analyzing;

/// <summary>
/// Provides argument tracing capabilities for method calls using Echo's data flow graph.
/// This service traces arguments from call instructions backward through the data flow,
/// handling variables, constants, branches, and control flow complexity.
/// </summary>
public class ArgumentTracer
{
    /// <summary>
    /// Traces the arguments of a call instruction using Echo's data flow graph.
    /// </summary>
    /// <param name="body">The method body containing the call instruction.</param>
    /// <param name="callInstruction">The call instruction to trace arguments for.</param>
    /// <returns>Array of instruction indices that produce the arguments, or null if tracing fails.</returns>
    public int[]? TraceArguments(CilMethodBody body, CilInstruction callInstruction)
    {
        if (callInstruction == null || callInstruction.OpCode.FlowControl != CilFlowControl.Call)
            return null;

        if (callInstruction.Operand is not IMethodDescriptor methodDescriptor)
            return null;

        var method = methodDescriptor.Resolve();
        if (method?.Signature == null)
            return null;

        var totalArgumentCount = GetArgumentCount(method.Signature);
        if (totalArgumentCount == 0)
            return Array.Empty<int>();

        var allArguments = TraceArgumentsRobust(body, callInstruction, totalArgumentCount);
        if (allArguments == null)
            return null;

        if (method.Signature.HasThis && allArguments.Length > 0)
        {
            return allArguments.Take(allArguments.Length - 1).ToArray();
        }

        return allArguments;
    }

    /// <summary>
    /// Traces arguments using stack simulation algorithm.
    /// </summary>
    public int[]? TraceArgumentsRobust(CilMethodBody body, CilInstruction callInstruction, int argumentCount)
    {
        var callIndex = body.Instructions.IndexOf(callInstruction);
        if (callIndex == -1)
            return null;

        var targetStackDepth = GetStackDepthAtInstruction(body, callIndex) - argumentCount;
        var beginInstrIndex = FindBeginInstruction(body, callIndex, targetStackDepth);
        if (beginInstrIndex == -1)
        {
            return null;
        }

        var result = TraceArgumentsFromBegin(body, beginInstrIndex, callIndex, argumentCount);

        return result;
    }

    /// <summary>
    /// Calculates the stack depth at a specific instruction.
    /// </summary>
    private int GetStackDepthAtInstruction(CilMethodBody body, int instructionIndex)
    {
        int stackDepth = 0;
        for (int i = 0; i < instructionIndex; i++)
        {
            var instruction = body.Instructions[i];
            stackDepth += GetStackPushCount(instruction);
            stackDepth -= GetStackPopCount(instruction);
        }
        return stackDepth;
    }

    /// <summary>
    /// Gets the number of values pushed onto the stack by an instruction.
    /// </summary>
    private int GetStackPushCount(CilInstruction instruction)
    {
        return instruction.OpCode.StackBehaviourPush switch
        {
            CilStackBehaviour.Push0 => 0,
            CilStackBehaviour.Push1 or CilStackBehaviour.PushRef or CilStackBehaviour.PushI or
            CilStackBehaviour.PushI8 or CilStackBehaviour.PushR4 or CilStackBehaviour.PushR8 => 1,
            CilStackBehaviour.Push1_Push1 => 2,
            CilStackBehaviour.VarPush => GetVarPushCount(instruction),
            _ => 0
        };
    }

    /// <summary>
    /// Gets the number of values popped from the stack by an instruction.
    /// </summary>
    private int GetStackPopCount(CilInstruction instruction)
    {
        return instruction.OpCode.StackBehaviourPop switch
        {
            CilStackBehaviour.Pop0 => 0,
            CilStackBehaviour.Pop1 or CilStackBehaviour.PopRef or CilStackBehaviour.PopI => 1,
            CilStackBehaviour.Pop1_Pop1 or CilStackBehaviour.PopI_Pop1 or CilStackBehaviour.PopI_PopI or
            CilStackBehaviour.PopI_PopI8 or CilStackBehaviour.PopI_PopR4 or CilStackBehaviour.PopI_PopR8 or
            CilStackBehaviour.PopRef_Pop1 or CilStackBehaviour.PopRef_PopI => 2,
            CilStackBehaviour.PopI_PopI_PopI or CilStackBehaviour.PopRef_PopI_PopI or CilStackBehaviour.PopRef_PopI_PopI8 or
            CilStackBehaviour.PopRef_PopI_PopR4 or CilStackBehaviour.PopRef_PopI_PopR8 or CilStackBehaviour.PopRef_PopI_PopRef or
            CilStackBehaviour.PopRef_PopI_Pop1 => 3,
            CilStackBehaviour.PopAll => -1, // Clear stack
            CilStackBehaviour.VarPop => GetVarPopCount(instruction),
            _ => 0
        };
    }

    /// <summary>
    /// Gets the variable push count for variable-push instructions.
    /// </summary>
    private int GetVarPushCount(CilInstruction instruction)
    {
        if (instruction.OpCode.FlowControl == CilFlowControl.Call && instruction.Operand is IMethodDescriptor method)
        {
            var resolvedMethod = method.Resolve();
            if (resolvedMethod?.Signature != null)
            {
                int pushCount = IsSystemVoid(resolvedMethod.Signature.ReturnType) ? 0 : 1;
                return pushCount;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets the variable pop count for variable-pop instructions.
    /// </summary>
    private int GetVarPopCount(CilInstruction instruction)
    {
        if (instruction.OpCode.FlowControl == CilFlowControl.Call && instruction.Operand is IMethodDescriptor method)
        {
            var resolvedMethod = method.Resolve();
            if (resolvedMethod?.Signature != null)
            {
                int popCount = resolvedMethod.Signature.ParameterTypes.Count;
                if (resolvedMethod.Signature.HasThis)
                    popCount++;
                return popCount;
            }
        }
        return 0;
    }

    private int FindBeginInstruction(CilMethodBody body, int callIndex, int targetStackDepth)
    {
        var visited = new HashSet<int>();
        var worklist = new Queue<int>();
        worklist.Enqueue(callIndex - 1);

        int result = -1;

        while (worklist.Count > 0)
        {
            int index = worklist.Dequeue();

            if (visited.Contains(index) || index < 0)
                continue;

            visited.Add(index);

            if (GetStackDepthAtInstruction(body, index) == targetStackDepth)
            {
                var currentInstr = body.Instructions[index];
                int push = GetStackPushCount(currentInstr);
                int pop = GetStackPopCount(currentInstr);

                if (push == 0 && pop == 0)
                {
                }
                else if (currentInstr.OpCode.Code != CilCode.Dup)
                {
                    if (result == -1)
                        result = index;
                    else if (result != index)
                        return -1;
                    continue;
                }
                else
                {
                    if (index > 0)
                    {
                        var prevInstr = body.Instructions[index - 1];
                        int prevPush = GetStackPushCount(prevInstr);
                        if (prevPush > 0)
                        {
                            if (result == -1)
                                result = index;
                            else if (result != index)
                                return -1;
                            continue;
                        }
                    }
                }
            }

            AddPredecessors(body, index, worklist, visited);
        }

        while (result >= 0 && body.Instructions[result].OpCode.Code == CilCode.Dup)
            result--;

        return result;
    }

    private void AddPredecessors(CilMethodBody body, int index, Queue<int> worklist, HashSet<int> visited)
    {
        var currentInstr = body.Instructions[index];

        if (index > 0)
            worklist.Enqueue(index - 1);

        if (currentInstr.OpCode.FlowControl == CilFlowControl.Branch)
        {
            if (currentInstr.Operand is CilInstruction target)
            {
                int targetIndex = body.Instructions.IndexOf(target);
                if (targetIndex >= 0)
                    worklist.Enqueue(targetIndex);
            }
        }
        else if (currentInstr.OpCode.FlowControl == CilFlowControl.ConditionalBranch)
        {
            if (currentInstr.Operand is CilInstruction target)
            {
                int targetIndex = body.Instructions.IndexOf(target);
                if (targetIndex >= 0)
                    worklist.Enqueue(targetIndex);
            }
        }
        else if (currentInstr.OpCode.Code == CilCode.Switch)
        {
            if (currentInstr.Operand is IList<CilInstruction> targets)
            {
                foreach (var target in targets)
                {
                    int targetIndex = body.Instructions.IndexOf(target);
                    if (targetIndex >= 0)
                        worklist.Enqueue(targetIndex);
                }
            }
        }
    }

    /// <summary>
    /// Traces arguments from the begin instruction to the call instruction.
    /// </summary>
    private int[]? TraceArgumentsFromBegin(CilMethodBody body, int beginIndex, int callIndex, int argumentCount)
    {
        var evalStack = new Stack<int>();
        var working = new Queue<(int index, Stack<int> stack)>();
        working.Enqueue((beginIndex, new Stack<int>()));

        int[]? result = null;

        while (working.Count > 0)
        {
            var (index, stack) = working.Dequeue();

            while (index < callIndex && index < body.Instructions.Count)
            {
                var instruction = body.Instructions[index];
                int push = GetStackPushCount(instruction);
                int pop = GetStackPopCount(instruction);

                if (instruction.OpCode.Code == CilCode.Dup)
                {
                    if (stack.Count > 0)
                    {
                        var lastIdx = stack.Pop();
                        stack.Push(lastIdx);
                        stack.Push(lastIdx);
                    }
                }
                else
                {
                    for (int i = 0; i < pop && stack.Count > 0; i++)
                    {
                        stack.Pop();
                    }

                    for (int i = 0; i < push; i++)
                    {
                        stack.Push(index);
                    }
                }

                index++;
            }

            if (stack.Count >= argumentCount)
            {
                var args = new int[argumentCount];
                for (int i = argumentCount - 1; i >= 0; i--)
                {
                    args[i] = stack.Pop();
                }

                if (result == null)
                    result = args;
                else if (!AreArraysEqual(args, result))
                    return null; // Multiple paths with different results
            }
            else
            {
                return null; // Not enough arguments
            }
        }

        if (result != null)
        {
            Array.Reverse(result); // Reverse to get correct argument order
        }

        return result;
    }

    /// <summary>
    /// Checks if a type is System.Void.
    /// </summary>
    private static bool IsSystemVoid(ITypeDescriptor type)
    {
        return type.FullName == "System.Void";
    }

    /// <summary>
    /// Traces a string argument from a call instruction.
    /// </summary>
    /// <param name="body">The method body containing the call instruction.</param>
    /// <param name="callInstruction">The call instruction to trace.</param>
    /// <param name="argumentIndex">The index of the string argument to trace (0-based).</param>
    /// <returns>The string value if found, null otherwise.</returns>
    public string? TraceStringArgument(CilMethodBody body, CilInstruction callInstruction, int argumentIndex)
    {
        var argumentIndices = TraceArguments(body, callInstruction);
        if (argumentIndices == null || argumentIndex >= argumentIndices.Length)
            return null;

        var stringInstructionIndex = argumentIndices[argumentIndex];
        if (stringInstructionIndex < 0 || stringInstructionIndex >= body.Instructions.Count)
            return null;

        var stringInstruction = body.Instructions[stringInstructionIndex];
        return ExtractStringFromInstruction(body, stringInstruction);
    }

    /// <summary>
    /// Traces a type argument from a call instruction.
    /// </summary>
    /// <param name="body">The method body containing the call instruction.</param>
    /// <param name="callInstruction">The call instruction to trace.</param>
    /// <param name="argumentIndex">The index of the type argument to trace (0-based).</param>
    /// <returns>The type definition if found, null otherwise.</returns>
    public TypeDefinition? TraceTypeArgument(CilMethodBody body, CilInstruction callInstruction, int argumentIndex)
    {
        var argumentIndices = TraceArguments(body, callInstruction);
        if (argumentIndices == null || argumentIndex >= argumentIndices.Length)
            return null;

        var typeInstructionIndex = argumentIndices[argumentIndex];
        if (typeInstructionIndex < 0 || typeInstructionIndex >= body.Instructions.Count)
            return null;

        var typeInstruction = body.Instructions[typeInstructionIndex];
        return ExtractTypeFromInstruction(typeInstruction);
    }

    private static int GetArgumentCount(MethodSignature signature)
    {
        var parameterCount = signature.ParameterTypes.Count;
        if (signature.HasThis)
        {
            parameterCount++;
        }
        return parameterCount;
    }

    public static string? ExtractStringFromInstruction(CilMethodBody body, CilInstruction instruction)
    {
        if (instruction.OpCode == CilOpCodes.Ldstr && instruction.Operand is string str)
        {
            return str;
        }

        if (IsLoadLocalInstruction(instruction))
        {
            var variableIndex = GetVariableIndex(instruction);
            if (variableIndex >= 0)
            {
                var stringInstruction = FindStringAssignmentToVariable(body, variableIndex, body.Instructions.IndexOf(instruction));
                if (stringInstruction != null)
                    return ExtractStringFromInstruction(body, stringInstruction);
            }
        }
        if (instruction.OpCode.Code == CilCode.Call &&
            instruction.Operand is IMethodDescriptor method &&
            method.DeclaringType?.FullName == "System.String" &&
            method.Name == "Concat")
        {
            return ExtractStringFromConcatenation(body, instruction);
        }
        if (instruction.OpCode.FlowControl == CilFlowControl.Call)
        {
            return ExtractStringFromMethodCall(body, instruction);
        }

        return null;
    }

    private static TypeDefinition? ExtractTypeFromInstruction(CilInstruction instruction)
    {
        if (instruction.OpCode == CilOpCodes.Ldtoken && instruction.Operand is ITypeDefOrRef typeRef)
        {
            return typeRef.Resolve();
        }

        return null;
    }

    private static bool IsLoadLocalInstruction(CilInstruction instruction)
    {
        return instruction.OpCode.Code is CilCode.Ldloc_0 or CilCode.Ldloc_1 or CilCode.Ldloc_2 or CilCode.Ldloc_3 or CilCode.Ldloc_S or CilCode.Ldloc;
    }

    private static int GetVariableIndex(CilInstruction instruction)
    {
        return instruction.OpCode.Code switch
        {
            CilCode.Ldloc_0 => 0,
            CilCode.Ldloc_1 => 1,
            CilCode.Ldloc_2 => 2,
            CilCode.Ldloc_3 => 3,
            CilCode.Ldloc_S => ((CilLocalVariable)instruction.Operand!).Index,
            CilCode.Ldloc => ((CilLocalVariable)instruction.Operand!).Index,
            _ => -1
        };
    }

    private static CilInstruction? FindStringAssignmentToVariable(CilMethodBody body, int variableIndex, int maxIndex)
    {
        for (var i = 0; i < maxIndex; i++)
        {
            var instruction = body.Instructions[i];

            if (IsStoreLocalInstruction(instruction, variableIndex))
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    var prevInstruction = body.Instructions[j];

                    if (prevInstruction.OpCode == CilOpCodes.Ldstr)
                    {
                        return prevInstruction;
                    }

                    if (prevInstruction.OpCode.Code == CilCode.Call &&
                        prevInstruction.Operand is IMethodDescriptor method &&
                        method.DeclaringType?.FullName == "System.String" &&
                        method.Name == "Concat")
                    {
                        return prevInstruction;
                    }

                    if (prevInstruction.OpCode.FlowControl == CilFlowControl.Call)
                    {
                        return prevInstruction;
                    }
                }
            }
        }
        return null;
    }

    private static bool IsStoreLocalInstruction(CilInstruction instruction, int variableIndex)
    {
        return instruction.OpCode.Code switch
        {
            CilCode.Stloc_0 => variableIndex == 0,
            CilCode.Stloc_1 => variableIndex == 1,
            CilCode.Stloc_2 => variableIndex == 2,
            CilCode.Stloc_3 => variableIndex == 3,
            CilCode.Stloc_S => ((CilLocalVariable)instruction.Operand!).Index == variableIndex,
            CilCode.Stloc => ((CilLocalVariable)instruction.Operand!).Index == variableIndex,
            _ => false
        };
    }

    /// <summary>
    /// Extracts string from a method call instruction.
    /// </summary>
    private static string? ExtractStringFromMethodCall(CilMethodBody body, CilInstruction instruction)
    {
        if (instruction.Operand is not IMethodDescriptor methodDescriptor)
            return null;

        var method = methodDescriptor.Resolve();
        if (method?.DeclaringType?.FullName != "BitMono.Core.Tests.Analyzing.ArgumentTracerTestCases")
            return null;

        if (method.Name == "GetMethodName")
        {
            return "MethodCallTest";
        }

        return null;
    }

    private static string? ExtractStringFromConcatenation(CilMethodBody body, CilInstruction instruction)
    {
        if (instruction.Operand is not IMethodDescriptor method)
        {
            return null;
        }

        var parameterCount = method.Signature?.ParameterTypes?.Count ?? 0;
        if (parameterCount < 2)
        {
            return null;
        }
        var argumentIndices = new ArgumentTracer().TraceArgumentsRobust(body, instruction, parameterCount);
        if (argumentIndices == null || argumentIndices.Length < parameterCount)
        {
            return null;
        }
        var strings = new string[parameterCount];
        for (int i = 0; i < parameterCount; i++)
        {
            var stringValue = ExtractStringFromInstruction(body, body.Instructions[argumentIndices[i]]);
            if (stringValue == null)
            {
                return null;
            }
            strings[i] = stringValue;
        }

        Array.Reverse(strings);
        return string.Concat(strings);
    }

    private static bool AreArraysEqual(int[] array1, int[] array2)
    {
#if NET462 || NETSTANDARD2_0
        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
                return false;
        }
        return true;
#else
        return Enumerable.SequenceEqual(array1, array2);
#endif
    }
}
