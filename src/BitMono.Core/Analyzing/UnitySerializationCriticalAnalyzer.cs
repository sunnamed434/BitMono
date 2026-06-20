namespace BitMono.Core.Analyzing;

/// <summary>
/// Treats Unity-serialized fields as critical, so the renamer leaves their names alone. Unity stores
/// scene/prefab values keyed by field name, so renaming a serialized field on a MonoBehaviour or
/// ScriptableObject silently unbinds it - the saved value resets to default when the asset loads.
/// Covers what the static <c>[SerializeField]</c> config rule cannot: PUBLIC instance fields, which
/// Unity serializes by default with no attribute, while still honoring <c>[SerializeField]</c> on
/// private fields. Static, const, readonly and <c>[NonSerialized]</c> fields are never serialized,
/// so they stay renamable.
/// </summary>
public class UnitySerializationCriticalAnalyzer : ICriticalAnalyzer<FieldDefinition>
{
    private static readonly string[] UnityContainerTypes =
    [
        "UnityEngine.MonoBehaviour",
        "UnityEngine.ScriptableObject",
    ];

    private readonly ObfuscationSettings _obfuscationSettings;

    public UnitySerializationCriticalAnalyzer(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
    }

    public bool NotCriticalToMakeChanges(FieldDefinition field)
    {
        if (!_obfuscationSettings.UnitySerializedFieldsObfuscationExclude)
        {
            return true;
        }
        // Unity never serializes static/const/readonly/[NonSerialized] fields - they stay renamable.
        if (field.IsStatic || field.IsLiteral || field.IsInitOnly || field.IsNotSerialized)
        {
            return true;
        }
        if (!DerivesFromUnityContainer(field.DeclaringType))
        {
            return true;
        }
        // Public fields are serialized by default; private/protected fields only with [SerializeField].
        if (field.IsPublic)
        {
            return false;
        }
        return !AttemptAttributeResolver.TryResolve(field, "UnityEngine", "SerializeFieldAttribute");
    }

    private static bool DerivesFromUnityContainer(TypeDefinition? type)
    {
        // Match the base type by full name: UnityEngine usually isn't resolvable at obfuscation time,
        // so we read the name off the reference and only resolve to keep walking the chain.
        var baseType = type?.BaseType;
        while (baseType != null)
        {
            if (Array.IndexOf(UnityContainerTypes, baseType.FullName) != -1)
            {
                return true;
            }
            baseType = baseType.ResolveOrNull()?.BaseType;
        }
        return false;
    }
}
