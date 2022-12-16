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
    public void Rename(IDnlibDef dnlibDef)
    {
        if (dnlibDef is TypeDef typeDef)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(typeDef))
            {
                typeDef.Name = RenameUnsafely();
            }
        }
        if (dnlibDef is MethodDef methodDef)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
            {
                methodDef.Name = RenameUnsafely();
            }
        }
        if (dnlibDef is FieldDef fieldDef)
        {
            fieldDef.Name = RenameUnsafely();
        }
    }
    public void Rename(IFullName fullName)
    {
        fullName.Name = RenameUnsafely();
    }
    public void Rename(params IFullName[] fullNames)
    {
        for (int i = 0; i < fullNames.Length; i++)
        {
            Rename(fullNames[i]);
        }
    }
    public void Rename(params IDnlibDef[] dnlibDefs)
    {
        for (int i = 0; i < dnlibDefs.Length; i++)
        {
            Rename(dnlibDefs[i]);
        }
    }
    public void Rename(IVariable variable)
    {
        variable.Name = RenameUnsafely();
    }
}