namespace BitMono.Core.Analyzing;

[SuppressMessage("ReSharper", "InvertIf")]
public class ReflectionCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly List<MethodDefinition> _cachedMethods;
    private readonly List<FieldDefinition> _cachedFields;
    private readonly List<PropertyDefinition> _cachedProperties;
    private readonly List<EventDefinition> _cachedEvents;
    private readonly List<TypeDefinition> _cachedTypes;
    private static readonly string[] ReflectionMethods =
    [
        nameof(Type.GetMethod),
        nameof(Type.GetField),
        nameof(Type.GetProperty),
        nameof(Type.GetEvent),
        nameof(Type.GetMember),
        nameof(Type.GetTypeFromHandle)
    ];

    public ReflectionCriticalAnalyzer(IOptions<ObfuscationSettings> obfuscation)
    {
        _obfuscationSettings = obfuscation.Value;
        _cachedMethods = [];
        _cachedFields = [];
        _cachedProperties = [];
        _cachedEvents = [];
        _cachedTypes = [];
    }

    public IReadOnlyList<MethodDefinition> CachedMethods => _cachedMethods.AsReadOnly();
    public IReadOnlyList<FieldDefinition> CachedFields => _cachedFields.AsReadOnly();
    public IReadOnlyList<PropertyDefinition> CachedProperties => _cachedProperties.AsReadOnly();
    public IReadOnlyList<EventDefinition> CachedEvents => _cachedEvents.AsReadOnly();
    public IReadOnlyList<TypeDefinition> CachedTypes => _cachedTypes.AsReadOnly();

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        if (_cachedMethods.FirstOrDefault(x => x.Name.Equals(method.Name)) != null)
        {
            return false;
        }

        bool foundReflection = false;

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
                            var traceArgument = TraceStringArgument(body, instruction);
                            if (traceArgument?.Operand is string memberName)
                            {
                                var module = method.DeclaringModule;
                                var allMembers = module.FindMembers();

                                switch (calledMethod.Name.Value)
                                {
                                    case nameof(Type.GetMethod):
                                        foreach (var possibleMethod in allMembers.OfType<MethodDefinition>()
                                                     .Where(x => x.Name.Equals(memberName)))
                                        {
                                            if (possibleMethod == method && !_cachedMethods.Contains(possibleMethod))
                                            {
                                                _cachedMethods.Add(possibleMethod);
                                                foundReflection = true;
                                            }
                                        }
                                        break;

                                    case nameof(Type.GetField):
                                        foreach (var possibleField in allMembers.OfType<FieldDefinition>()
                                                     .Where(x => x.Name.Equals(memberName)))
                                        {
                                            _cachedFields.Add(possibleField);
                                            foundReflection = true;
                                        }
                                        break;

                                    case nameof(Type.GetProperty):
                                        foreach (var possibleProperty in allMembers.OfType<PropertyDefinition>()
                                                     .Where(x => x.Name.Equals(memberName)))
                                        {
                                            _cachedProperties.Add(possibleProperty);
                                            foundReflection = true;
                                        }
                                        break;

                                    case nameof(Type.GetEvent):
                                        foreach (var possibleEvent in allMembers.OfType<EventDefinition>()
                                                     .Where(x => x.Name.Equals(memberName)))
                                        {
                                            _cachedEvents.Add(possibleEvent);
                                            foundReflection = true;
                                        }
                                        break;

                                    case nameof(Type.GetMember):
                                        foreach (var possibleMember in allMembers)
                                        {
                                            string? memberNameToCheck = possibleMember switch
                                            {
                                                MethodDefinition m => m.Name,
                                                FieldDefinition f => f.Name,
                                                PropertyDefinition p => p.Name,
                                                EventDefinition e => e.Name,
                                                TypeDefinition t => t.Name,
                                                _ => null
                                            };

                                            if (memberNameToCheck != null && memberNameToCheck.Equals(memberName))
                                            {
                                                switch (possibleMember)
                                                {
                                                    case MethodDefinition m:
                                                        _cachedMethods.Add(m);
                                                        break;
                                                    case FieldDefinition f:
                                                        _cachedFields.Add(f);
                                                        break;
                                                    case PropertyDefinition p:
                                                        _cachedProperties.Add(p);
                                                        break;
                                                    case EventDefinition e:
                                                        _cachedEvents.Add(e);
                                                        break;
                                                    case TypeDefinition t:
                                                        _cachedTypes.Add(t);
                                                        break;
                                                }
                                                foundReflection = true;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        else if (IsTypeGetTypeFromHandle(calledMethod))
                        {
                            var typeFromHandle = TraceTypeFromHandle(body, instruction);
                            if (typeFromHandle != null)
                            {
                                _cachedTypes.Add(typeFromHandle);
                                foundReflection = true;
                            }
                        }
                    }
                    else if (instruction?.OpCode == CilOpCodes.Ldtoken && instruction.Operand is ITypeDefOrRef typeRef)
                    {
                        if (typeRef.Resolve() is TypeDefinition typeDef)
                        {
                            _cachedTypes.Add(typeDef);
                            foundReflection = true;
                        }
                    }
                }
            }
        }

        return !foundReflection;
    }

    public bool NotCriticalToMakeChanges(FieldDefinition field)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        return _cachedFields.FirstOrDefault(x => x.Name.Equals(field.Name)) != null;
    }

    public bool NotCriticalToMakeChanges(PropertyDefinition property)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        return _cachedProperties.FirstOrDefault(x => x.Name.Equals(property.Name)) != null;
    }

    public bool NotCriticalToMakeChanges(EventDefinition eventDef)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        return _cachedEvents.FirstOrDefault(x => x.Name.Equals(eventDef.Name)) != null;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        return _cachedTypes.FirstOrDefault(x => x.Name.Equals(type.Name)) != null;
    }

    private static bool IsReflection(IMethodDefOrRef calledMethod)
    {
        return calledMethod.DeclaringType.IsSystemType() &&
               ReflectionMethods.Contains(calledMethod.Name.Value);
    }

    private static bool IsTypeGetTypeFromHandle(IMethodDefOrRef calledMethod)
    {
        return calledMethod.DeclaringType.IsSystemType() &&
               calledMethod.Name.Value == nameof(Type.GetTypeFromHandle);
    }

    private static TypeDefinition? TraceTypeFromHandle(CilMethodBody body, CilInstruction instruction)
    {
        var callIndex = body.Instructions.IndexOf(instruction);
        if (callIndex <= 0) return null;

        for (var i = callIndex - 1; i >= 0; i--)
        {
            var prevInstruction = body.Instructions[i];
            if (prevInstruction.OpCode == CilOpCodes.Ldtoken && prevInstruction.Operand is ITypeDefOrRef typeRef)
            {
                return typeRef.Resolve();
            }
        }
        return null;
    }
    private static CilInstruction? TraceStringArgument(CilMethodBody body, CilInstruction instruction)
    {
        return TraceStringArgumentSimple(body, instruction);
    }

    private static CilInstruction? TraceStringArgumentSimple(CilMethodBody body, CilInstruction instruction)
    {
        var callIndex = body.Instructions.IndexOf(instruction);
        if (callIndex <= 0) return null;

        for (var i = callIndex - 1; i >= 0; i--)
        {
            var previousInstruction = body.Instructions[i];

            if (previousInstruction.OpCode == CilOpCodes.Ldstr)
            {
                return previousInstruction;
            }

            if (previousInstruction.OpCode == CilOpCodes.Ldloc_0 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_1 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_2 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_3 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_S ||
                previousInstruction.OpCode == CilOpCodes.Ldloc)
            {
                var variableIndex = GetVariableIndex(previousInstruction);
                if (variableIndex >= 0)
                {
                    var stringInstruction = FindStringAssignmentToVariable(body, variableIndex, i);
                    if (stringInstruction != null)
                    {
                        return stringInstruction;
                    }
                }
            }
        }
        return null;
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

            if ((instruction.OpCode == CilOpCodes.Stloc_0 && variableIndex == 0) ||
                (instruction.OpCode == CilOpCodes.Stloc_1 && variableIndex == 1) ||
                (instruction.OpCode == CilOpCodes.Stloc_2 && variableIndex == 2) ||
                (instruction.OpCode == CilOpCodes.Stloc_3 && variableIndex == 3) ||
                (instruction.OpCode == CilOpCodes.Stloc_S && ((CilLocalVariable)instruction.Operand!).Index == variableIndex) ||
                (instruction.OpCode == CilOpCodes.Stloc && ((CilLocalVariable)instruction.Operand!).Index == variableIndex))
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    var prevInstruction = body.Instructions[j];
                    if (prevInstruction.OpCode == CilOpCodes.Ldstr)
                    {
                        return prevInstruction;
                    }
                }
            }
        }
        return null;
    }
}