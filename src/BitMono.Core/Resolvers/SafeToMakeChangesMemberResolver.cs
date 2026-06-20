namespace BitMono.Core.Resolvers;

public class SafeToMakeChangesMemberResolver : IMemberResolver
{
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver _obfuscateAssemblyAttributeResolver;
    private readonly CriticalAttributeResolver _criticalAttributeResolver;
    private readonly SerializableBitCriticalAnalyzer _serializableBitCriticalAnalyzer;
    private readonly SerializationCriticalAnalyzer _serializationCriticalAnalyzer;
    private readonly UnitySerializationCriticalAnalyzer _unitySerializationCriticalAnalyzer;
    private readonly UnityStringInvokeCriticalAnalyzer _unityStringInvokeCriticalAnalyzer;
    private readonly SpecificNamespaceCriticalAnalyzer _specificNamespaceCriticalAnalyzer;

    public SafeToMakeChangesMemberResolver(
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        ObfuscateAssemblyAttributeResolver obfuscateAssemblyAttributeResolver,
        CriticalAttributeResolver criticalAttributeResolver,
        SerializableBitCriticalAnalyzer serializableBitCriticalAnalyzer,
        SerializationCriticalAnalyzer serializationCriticalAnalyzer,
        UnitySerializationCriticalAnalyzer unitySerializationCriticalAnalyzer,
        UnityStringInvokeCriticalAnalyzer unityStringInvokeCriticalAnalyzer,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer)
    {
        _obfuscationAttributeResolver = obfuscationAttributeResolver;
        _obfuscateAssemblyAttributeResolver = obfuscateAssemblyAttributeResolver;
        _criticalAttributeResolver = criticalAttributeResolver;
        _serializableBitCriticalAnalyzer = serializableBitCriticalAnalyzer;
        _serializationCriticalAnalyzer = serializationCriticalAnalyzer;
        _unitySerializationCriticalAnalyzer = unitySerializationCriticalAnalyzer;
        _unityStringInvokeCriticalAnalyzer = unityStringInvokeCriticalAnalyzer;
        _specificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        if (member is IHasCustomAttribute customAttribute)
        {
            var feature = protection.GetName();
            if (_obfuscationAttributeResolver.Resolve(feature, customAttribute, out var obfuscationAttributes)
                && obfuscationAttributes.Any(x => x.Exclude))
            {
                return false;
            }
            if (_obfuscateAssemblyAttributeResolver.Resolve(null, customAttribute, out var obfuscateAssemblyAttributeData)
                && obfuscateAssemblyAttributeData!.AssemblyIsPrivate)
            {
                return false;
            }
            if (_criticalAttributeResolver.Resolve(feature, customAttribute))
            {
                return false;
            }
        }
        if (member is TypeDefinition type)
        {
            if (!_serializableBitCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                return false;
            }
            if (!_serializationCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                return false;
            }
        }
        if (member is FieldDefinition field)
        {
            if (!_serializationCriticalAnalyzer.NotCriticalToMakeChanges(field))
            {
                return false;
            }
            if (!_unitySerializationCriticalAnalyzer.NotCriticalToMakeChanges(field))
            {
                return false;
            }
        }
        if (member is PropertyDefinition property && !_serializationCriticalAnalyzer.NotCriticalToMakeChanges(property))
        {
            return false;
        }
        if (member is MethodDefinition method && !_unityStringInvokeCriticalAnalyzer.NotCriticalToMakeChanges(method))
        {
            return false;
        }
        if (!_specificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(member))
        {
            return false;
        }
        return true;
    }
}