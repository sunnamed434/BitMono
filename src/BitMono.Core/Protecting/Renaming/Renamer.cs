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
    public void Rename(IMemberDefinition memberDefinition)
    {
        if (memberDefinition is TypeDefinition typeDefinition)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(typeDefinition))
            {
                typeDefinition.Name = RenameUnsafely();
            }
        }
        if (memberDefinition is MethodDefinition methodDefinition)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(methodDefinition))
            {
                methodDefinition.Name = RenameUnsafely();
            }
        }
        if (memberDefinition is FieldDefinition fieldDefinition)
        {
            fieldDefinition.Name = RenameUnsafely();
        }
    }
    public void Rename(params IMemberDefinition[] memberDefinitions)
    {
        for (int i = 0; i < memberDefinitions.Length; i++)
        {
            Rename(memberDefinitions[i]);
        }
    }
}