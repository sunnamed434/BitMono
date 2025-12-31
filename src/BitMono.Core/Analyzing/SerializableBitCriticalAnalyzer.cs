namespace BitMono.Core.Analyzing;

[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
public class SerializableBitCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly ObfuscationSettings _obfuscationSettings;

    public SerializableBitCriticalAnalyzer(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
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