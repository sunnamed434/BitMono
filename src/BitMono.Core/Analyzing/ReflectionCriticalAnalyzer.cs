namespace BitMono.Core.Analyzing;

[SuppressMessage("ReSharper", "InvertIf")]
public class ReflectionCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly List<MethodDefinition> _cachedMethods;
    private static readonly string[] ReflectionMethods =
    [
        nameof(Type.GetMethod),
        nameof(Type.GetField),
        nameof(Type.GetProperty),
        nameof(Type.GetEvent),
        nameof(Type.GetMember)
    ];

    public ReflectionCriticalAnalyzer(IOptions<ObfuscationSettings> obfuscation)
    {
        _obfuscationSettings = obfuscation.Value;
        _cachedMethods = [];
    }

    public IReadOnlyList<MethodDefinition> CachedMethods => _cachedMethods.AsReadOnly();

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (_obfuscationSettings.ReflectionMembersObfuscationExclude == false)
        {
            return true;
        }
        if (_cachedMethods.FirstOrDefault(x => x.Name.Equals(method.Name)) != null)
        {
            return false;
        }
        if (method.CilMethodBody is { } body)
        {
            body.ConstructSymbolicFlowGraph(out var dataFlowGraph);
            foreach (var node in dataFlowGraph.Nodes)
            {
                var orderedDependencies =
                    node.GetOrderedDependencies(DependencyCollectionFlags.IncludeStackDependencies);
                foreach (var order in orderedDependencies)
                {
                    var instruction = order.Contents;
                    if (instruction?.OpCode.Code == CilCode.Call && instruction.Operand is IMethodDefOrRef calledMethod)
                    {
                        if (IsReflection(calledMethod))
                        {
                            var traceArgument = TraceLdstrArgument(body, instruction);
                            if (traceArgument?.Operand is string traceMethodName)
                            {
                                foreach (var possibleMethod in method.Module
                                             .FindMembers()
                                             .OfType<MethodDefinition>()
                                             .Where(x => x.Name.Equals(traceMethodName)))
                                {
                                    _cachedMethods.Add(possibleMethod);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }
        return true;
    }

    private static bool IsReflection(IMethodDefOrRef calledMethod)
    {
        return calledMethod.DeclaringType.IsSystemType() &&
               ReflectionMethods.Contains(calledMethod.Name.Value);
    }
    private static CilInstruction? TraceLdstrArgument(CilMethodBody body, CilInstruction instruction)
    {
        for (var i = body.Instructions.IndexOf(instruction); i > 0 && body.Instructions.Count.IsLess(i) == false; i--)
        {
            var previousInstruction = body.Instructions[i];
            if (previousInstruction.OpCode == CilOpCodes.Ldstr)
            {
                return previousInstruction;
            }
        }
        return null;
    }
}