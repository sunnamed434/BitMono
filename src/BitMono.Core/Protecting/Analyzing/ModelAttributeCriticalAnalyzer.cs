namespace BitMono.Core.Protecting.Analyzing;

public class ModelAttributeCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly IConfiguration m_Configuration;
    private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;
    private readonly Dictionary<string, string> m_Attributes = new Dictionary<string, string>
    {
        { nameof(SerializableAttribute), typeof(SerializableAttribute).Namespace },
        { nameof(XmlAttributeAttribute), typeof(XmlAttributeAttribute).Namespace },
        { nameof(XmlArrayItemAttribute), typeof(XmlArrayItemAttribute).Namespace },
        { nameof(JsonPropertyAttribute), typeof(JsonPropertyAttribute).Namespace },
    };

    public ModelAttributeCriticalAnalyzer(IBitMonoObfuscationConfiguration configuration, IAttemptAttributeResolver attemptAttributeResolver)
    {
        m_Configuration = configuration.Configuration;
        m_AttemptAttributeResolver = attemptAttributeResolver;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute customAttribute)
    {
        if (m_Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.ModelAttributeObfuscationExclude)) == false)
        {
            return true;
        }
        foreach (var attribute in m_Attributes)
        {
            if (m_AttemptAttributeResolver.TryResolve(customAttribute, attribute.Value, attribute.Key, out _))
            {
                return false;
            }
        }
        return false;
    }
}