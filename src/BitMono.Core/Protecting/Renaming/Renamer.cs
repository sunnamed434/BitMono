namespace BitMono.Core.Protecting.Renaming;

public class Renamer : IRenamer
{
    private readonly NameCriticalAnalyzer m_NameCriticalAnalyzer;
    private readonly IConfiguration m_Configuration;
    private readonly Random random = new Random();

    public Renamer(NameCriticalAnalyzer nameCriticalAnalyzer, IBitMonoObfuscationConfiguration configuration)
    {
        m_NameCriticalAnalyzer = nameCriticalAnalyzer;
        m_Configuration = configuration.Configuration;
    }

    public string RenameUnsafely()
    {
        var strings = m_Configuration.GetRandomStrings();
        var randomStringOne = strings[random.Next(0, strings.Length - 1)] + " " + strings[random.Next(0, strings.Length - 1)];
        var randomStringTwo = strings[random.Next(0, strings.Length - 1)];
        var randomStringThree = strings[random.Next(0, strings.Length - 1)];
        return $"{randomStringTwo} {randomStringOne}.{randomStringThree}";
    }
    public void Rename(IMetadataMember metadataMember)
    {
        if (metadataMember is TypeDefinition type)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                type.Name = RenameUnsafely();
            }
        }
        if (metadataMember is MethodDefinition method)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                method.Name = RenameUnsafely();
            }
        }
        if (metadataMember is FieldDefinition field)
        {
            field.Name = RenameUnsafely();
        }
        if (metadataMember is ParameterDefinition parameter)
        {
            parameter.Name = RenameUnsafely();
        }
    }
    public void Rename(params IMetadataMember[] metadataMembers)
    {
        for (int i = 0; i < metadataMembers.Length; i++)
        {
            Rename(metadataMembers[i]);
        }
    }
}