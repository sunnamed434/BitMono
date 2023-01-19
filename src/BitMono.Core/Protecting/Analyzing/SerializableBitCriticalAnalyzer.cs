namespace BitMono.Core.Protecting.Analyzing;

public class SerializableBitCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly Obfuscation m_Obfuscation;

    public SerializableBitCriticalAnalyzer(IOptions<Obfuscation> obfuscation)
    {
        m_Obfuscation = obfuscation.Value;
    }
    
    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (m_Obfuscation.SerializableBitObfuscationExclude == false)
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