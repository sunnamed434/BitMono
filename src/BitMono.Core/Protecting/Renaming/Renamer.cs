namespace BitMono.Core.Protecting.Renaming;

public class Renamer : IRenamer
{
    private readonly NameCriticalAnalyzer m_NameCriticalAnalyzer;
    private readonly SpecificNamespaceCriticalAnalyzer m_SpecificNamespaceCriticalAnalyzer;
    private readonly Obfuscation m_Obfuscation;
    private readonly Random m_Random;

    public Renamer(
        NameCriticalAnalyzer nameCriticalAnalyzer,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer,
        IOptions<Obfuscation> configuration,
        RuntimeImplementations runtime)
    {
        m_NameCriticalAnalyzer = nameCriticalAnalyzer;
        m_SpecificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
        m_Obfuscation = configuration.Value;
        m_Random = runtime.Random;
    }

    public string RenameUnsafely()
    {
        var strings = m_Obfuscation.RandomStrings;
        var randomStringOne = strings[m_Random.Next(0, strings.Length - 1)] + " " + strings[m_Random.Next(0, strings.Length - 1)];
        var randomStringTwo = strings[m_Random.Next(0, strings.Length - 1)];
        var randomStringThree = strings[m_Random.Next(0, strings.Length - 1)];
        return $"{randomStringTwo} {randomStringOne}.{randomStringThree}";
    }
    public void Rename(IMetadataMember member)
    {
        if (member is TypeDefinition type)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                type.Name = RenameUnsafely();
            }
        }
        if (member is MethodDefinition method)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                method.Name = RenameUnsafely();
            }
        }
        if (member is FieldDefinition field)
        {
            field.Name = RenameUnsafely();
        }
        if (member is ParameterDefinition parameter)
        {
            parameter.Name = RenameUnsafely();
        }
    }
    public void Rename(params IMetadataMember[] members)
    {
        for (int i = 0; i < members.Length; i++)
        {
            Rename(members[i]);
        }
    }
    public void RemoveNamespace(IMetadataMember member)
    {
        if (member is TypeDefinition type)
        {
            if (m_SpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                type.Namespace = string.Empty;
            }
        }
        if (member is MethodDefinition method)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                method.DeclaringType.Namespace = string.Empty;
            }
        }
        if (member is FieldDefinition field)
        {
            field.DeclaringType.Namespace = string.Empty;
        }
    }
    public void RemoveNamespace(params IMetadataMember[] members)
    {
        members.ForEach(member => RemoveNamespace(member));
    }
}