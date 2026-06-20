namespace BitMono.Core.Analyzing;

/// <summary>
/// Treats members that take part in serialization as critical, so the renamer leaves their names
/// alone. A serializer maps document/wire elements by member name, so renaming a
/// <c>[DataMember]</c>/<c>[XmlElement]</c>/<c>[JsonProperty]</c> member - or a public field of a type
/// handed to <c>XmlSerializer</c> - silently changes the serialized format and breaks interop with
/// anything reading that data. Detects the well-known serialization attributes by name
/// (System.Runtime.Serialization, System.Xml.Serialization, Newtonsoft.Json, System.Text.Json) plus
/// the implicit <c>XmlSerializer</c>/<c>DataContractSerializer</c> rule. Conservative: a member is
/// kept even when an attribute sets an explicit name that would technically make renaming safe.
/// </summary>
public class SerializationCriticalAnalyzer :
    ICriticalAnalyzer<TypeDefinition>,
    ICriticalAnalyzer<FieldDefinition>,
    ICriticalAnalyzer<PropertyDefinition>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly HashSet<ModuleDefinition> _analyzedModules = [];
    private readonly HashSet<TypeDefinition> _implicitTypes = [];
    private readonly HashSet<FieldDefinition> _implicitFields = [];
    private readonly HashSet<PropertyDefinition> _implicitProperties = [];

    // Presence on a TYPE ties the type name to the serialized form.
    private static readonly (string Namespace, string Name)[] TypeAttributes =
    [
        ("System.Runtime.Serialization", "DataContractAttribute"),
        ("System.Runtime.Serialization", "CollectionDataContractAttribute"),
        ("System.Xml.Serialization", "XmlRootAttribute"),
        ("System.Xml.Serialization", "XmlTypeAttribute"),
        ("Newtonsoft.Json", "JsonObjectAttribute"),
        ("Newtonsoft.Json", "JsonArrayAttribute"),
        ("Newtonsoft.Json", "JsonDictionaryAttribute"),
    ];

    // Presence on a FIELD/PROPERTY ties the member name to the serialized form.
    private static readonly (string Namespace, string Name)[] MemberAttributes =
    [
        ("System.Runtime.Serialization", "DataMemberAttribute"),
        ("System.Runtime.Serialization", "EnumMemberAttribute"),
        ("System.Xml.Serialization", "XmlElementAttribute"),
        ("System.Xml.Serialization", "XmlAttributeAttribute"),
        ("System.Xml.Serialization", "XmlArrayAttribute"),
        ("System.Xml.Serialization", "XmlArrayItemAttribute"),
        ("System.Xml.Serialization", "XmlTextAttribute"),
        ("System.Xml.Serialization", "XmlEnumAttribute"),
        ("Newtonsoft.Json", "JsonPropertyAttribute"),
        ("System.Text.Json.Serialization", "JsonPropertyNameAttribute"),
        ("System.Text.Json.Serialization", "JsonIncludeAttribute"),
    ];

    public SerializationCriticalAnalyzer(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (!_obfuscationSettings.SerializationMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(type.DeclaringModule);
        return !HasAny(type, TypeAttributes) && !_implicitTypes.Contains(type);
    }

    public bool NotCriticalToMakeChanges(FieldDefinition field)
    {
        if (!_obfuscationSettings.SerializationMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(field.DeclaringModule);
        return !HasAny(field, MemberAttributes) && !_implicitFields.Contains(field);
    }

    public bool NotCriticalToMakeChanges(PropertyDefinition property)
    {
        if (!_obfuscationSettings.SerializationMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(property.DeclaringModule);
        return !HasAny(property, MemberAttributes) && !_implicitProperties.Contains(property);
    }

    private static bool HasAny(IHasCustomAttribute member, (string Namespace, string Name)[] attributes)
    {
        foreach (var (ns, name) in attributes)
        {
            if (AttemptAttributeResolver.TryResolve(member, ns, name))
            {
                return true;
            }
        }
        return false;
    }

    // The implicit rule: public fields/properties of any type passed to `new XmlSerializer(typeof(T))`
    // (or DataContractSerializer) are serialized by name. Scanned once per module, up front.
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
                if (method.CilMethodBody is not { } body)
                {
                    continue;
                }
                var instructions = body.Instructions;
                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    if (instruction.OpCode.Code != CilCode.Newobj ||
                        instruction.Operand is not IMethodDefOrRef ctor ||
                        !IsSerializerConstructor(ctor))
                    {
                        continue;
                    }
                    if (NearestPrecedingType(instructions, i, module) is { } serialized)
                    {
                        MarkImplicit(serialized);
                    }
                }
            }
        }
    }

    private static bool IsSerializerConstructor(IMethodDefOrRef ctor)
    {
        if (ctor.Name != ".ctor" || ctor.DeclaringType is not { } declaringType)
        {
            return false;
        }
        var ns = declaringType.Namespace?.Value;
        var name = declaringType.Name?.Value;
        return (ns == "System.Xml.Serialization" && name == "XmlSerializer")
            || (ns == "System.Runtime.Serialization" && name is "DataContractSerializer" or "DataContractJsonSerializer");
    }

    // The serialized root type is the typeof() argument; take the nearest preceding ldtoken of a
    // type we own. Over-reaching (an unrelated typeof earlier in the body) only keeps extra names -
    // safe; it never renames something it shouldn't.
    private static TypeDefinition? NearestPrecedingType(CilInstructionCollection instructions, int index, ModuleDefinition module)
    {
        for (var i = index - 1; i >= 0; i--)
        {
            if (instructions[i].OpCode == CilOpCodes.Ldtoken &&
                instructions[i].Operand is ITypeDefOrRef typeRef &&
                typeRef.ResolveOrNull() is { } type &&
                ReferenceEquals(type.DeclaringModule, module))
            {
                return type;
            }
        }
        return null;
    }

    private void MarkImplicit(TypeDefinition type)
    {
        if (!_implicitTypes.Add(type))
        {
            return;
        }
        foreach (var field in type.Fields)
        {
            if (field.IsPublic && !field.IsStatic)
            {
                _implicitFields.Add(field);
            }
        }
        foreach (var property in type.Properties)
        {
            if (property.GetMethod?.IsPublic == true)
            {
                _implicitProperties.Add(property);
            }
        }
    }
}
