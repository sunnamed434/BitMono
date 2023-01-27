namespace BitMono.Core.Protecting.Analyzing;

[SuppressMessage("ReSharper", "IdentifierTypo")]
public class ReflectionCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly Obfuscation m_Obfuscation;
    private List<MethodDefinition> m_CachedMethods;
    private static readonly string[] ReflectionMethods = new string[]
    {
        nameof(Type.GetMethod),
        nameof(Type.GetField),
        nameof(Type.GetProperty),
        nameof(Type.GetEvent),
        nameof(Type.GetMember),
    };

    public ReflectionCriticalAnalyzer(IOptions<Obfuscation> obfuscation)
    {
        m_Obfuscation = obfuscation.Value;
        m_CachedMethods = new List<MethodDefinition>();
    }

    public IReadOnlyList<MethodDefinition> CachedMethods => m_CachedMethods.AsReadOnly();

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (m_Obfuscation.ReflectionMembersObfuscationExclude == false)
        {
            return true;
        }
        if (m_CachedMethods.FirstOrDefault(r => r.Name.Equals(method.Name)) != null)
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
                    if (instruction.OpCode.Code == CilCode.Call && instruction.Operand is IMethodDefOrRef calledMethod)
                    {
                        if (IsReflection(calledMethod))
                        {
                            var traceArgument = TraceLdstrArgument(body, instruction);
                            if (traceArgument != null)
                            {
                                if (traceArgument.Operand is string traceMethodName)
                                {
                                    foreach (var possibleMember in method.Module
                                                 .FindMembers()
                                                 .OfType<MethodDefinition>()
                                                 .Where(m => m.Name.Equals(traceMethodName)))
                                    {
                                        m_CachedMethods.Add(possibleMember);
                                        return false;
                                    }
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
        return calledMethod.DeclaringType.IsTypeOf(typeof(Type).Namespace, nameof(Type)) &&
               ReflectionMethods.Contains(calledMethod.Name.Value);
    }
    [return: AllowNull]
    private static CilInstruction TraceLdstrArgument(CilMethodBody body, CilInstruction instruction)
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