namespace BitMono.Core.Analyzing;

public class SerializableBitCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly ObfuscationSettings _obfuscationSettings;

    public SerializableBitCriticalAnalyzer(IOptions<ObfuscationSettings> obfuscation)
    {
        _obfuscationSettings = obfuscation.Value;
    }
    
    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (_obfuscationSettings.SerializableBitObfuscationExclude == false)
        {
            return true;
        }
        if (type.IsSerializable)
        {
            return false;
        }
        return true;
    }
}