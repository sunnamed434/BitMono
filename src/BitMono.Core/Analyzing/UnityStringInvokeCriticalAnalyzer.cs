namespace BitMono.Core.Analyzing;

/// <summary>
/// Treats methods that Unity invokes by string name as critical, so the renamer leaves their names
/// alone. <c>Invoke</c>/<c>InvokeRepeating</c>/<c>CancelInvoke</c>/<c>StartCoroutine</c>/
/// <c>StopCoroutine</c> and the <c>SendMessage</c>/<c>SendMessageUpwards</c>/<c>BroadcastMessage</c>
/// family take the target method NAME as a string, so renaming that method turns the call into a
/// silent no-op. The module is scanned once: every string literal flowing into one of those calls
/// (recovered through Echo's data-flow graph via <see cref="ReflectionDataFlow"/>, so locals and dups
/// are seen through) freezes every method of that name. Module-wide by name on purpose - SendMessage
/// can hit any component, and over-freezing a same-named method is safe.
/// </summary>
public class UnityStringInvokeCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private static readonly HashSet<string> InvokeApis =
    [
        "Invoke", "InvokeRepeating", "CancelInvoke",
        "StartCoroutine", "StopCoroutine",
        "SendMessage", "SendMessageUpwards", "BroadcastMessage",
    ];

    private static readonly HashSet<string> Receivers = ["MonoBehaviour", "Component", "GameObject"];

    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly HashSet<ModuleDefinition> _analyzedModules = [];
    private readonly HashSet<string> _invokedNames = [];

    public UnityStringInvokeCriticalAnalyzer(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
    }

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (!_obfuscationSettings.UnityStringInvokeMethodsObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(method.DeclaringModule);
        var name = method.Name?.Value;
        return name == null || !_invokedNames.Contains(name);
    }

    private void EnsureAnalyzed(ModuleDefinition? module)
    {
        if (module == null || !_analyzedModules.Add(module))
        {
            return;
        }
        foreach (var type in module.GetAllTypes())
        {
            foreach (var method in type.Methods)
            {
                if (method.CilMethodBody is not { } body || !MayInvokeByString(body))
                {
                    continue;
                }
                try
                {
                    AnalyzeBody(body);
                }
                catch
                {
                    // ponytail: skip a body Echo can't model rather than abort the whole-module pass.
                }
            }
        }
    }

    private void AnalyzeBody(CilMethodBody body)
    {
        body.ConstructSymbolicFlowGraph(out var dataFlowGraph);
        foreach (var node in dataFlowGraph.Nodes)
        {
            if (node.Contents is not { } instruction ||
                (instruction.OpCode.Code != CilCode.Call && instruction.OpCode.Code != CilCode.Callvirt) ||
                instruction.Operand is not IMethodDefOrRef called ||
                !IsUnityInvokeApi(called))
            {
                continue;
            }
            foreach (var operand in ReflectionDataFlow.OperandNodes(node))
            {
                if (operand.Contents?.OpCode == CilOpCodes.Ldstr && operand.Contents.Operand is string name)
                {
                    _invokedNames.Add(name);
                }
            }
        }
    }

    // Cheap pre-filter so we only build a data-flow graph for bodies that actually call one of the APIs.
    private static bool MayInvokeByString(CilMethodBody body)
    {
        foreach (var instruction in body.Instructions)
        {
            if (instruction.OpCode.Code is CilCode.Call or CilCode.Callvirt &&
                instruction.Operand is IMethodDefOrRef called &&
                InvokeApis.Contains(called.Name?.Value ?? string.Empty))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsUnityInvokeApi(IMethodDefOrRef called)
    {
        if (!InvokeApis.Contains(called.Name?.Value ?? string.Empty))
        {
            return false;
        }
        var declaringType = called.DeclaringType;
        return declaringType?.Namespace?.Value == "UnityEngine"
            && Receivers.Contains(declaringType.Name?.Value ?? string.Empty);
    }
}
