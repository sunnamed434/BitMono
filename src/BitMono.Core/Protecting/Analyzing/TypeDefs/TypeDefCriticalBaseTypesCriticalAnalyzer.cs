namespace BitMono.Core.Protecting.Analyzing.TypeDefs;

public class TypeDefCriticalBaseTypesCriticalAnalyzer : ICriticalAnalyzer<TypeDefinition>
{
    private readonly IConfiguration m_Configuration;

    public TypeDefCriticalBaseTypesCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition typeDefinition)
    {
        if (typeDefinition.HasBaseType())
        {
            var criticalBaseTypes = m_Configuration.GetCriticalBaseTypes();
            Console.WriteLine("Base types: " + typeDefinition.BaseType.Name.Value);
            if (criticalBaseTypes.FirstOrDefault(c => c.StartsWith(typeDefinition.BaseType.Name.Value.Split('`')[0])) != null)
            {
                return false;
            }
        }
        return true;
    }
}