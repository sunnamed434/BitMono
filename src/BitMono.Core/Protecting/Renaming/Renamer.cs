namespace BitMono.Core.Protecting.Renaming;

public class Renamer : IRenamer
{
    private readonly NameCriticalAnalyzer m_NameCriticalAnalyzer;
    private readonly SpecificNamespaceCriticalAnalyzer m_SpecificNamespaceCriticalAnalyzer;
    private readonly IConfiguration m_Configuration;
    private readonly Random random = new Random();

    public Renamer(
        NameCriticalAnalyzer nameCriticalAnalyzer,
        SpecificNamespaceCriticalAnalyzer specificNamespaceCriticalAnalyzer,
        IBitMonoObfuscationConfiguration configuration)
    {
        m_NameCriticalAnalyzer = nameCriticalAnalyzer;
        m_SpecificNamespaceCriticalAnalyzer = specificNamespaceCriticalAnalyzer;
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
    public void Rename(object @object)
    {
        if (@object is TypeDefinition type)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                type.Name = RenameUnsafely();
            }
        }
        if (@object is MethodDefinition method)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                method.Name = RenameUnsafely();
            }
        }
        if (@object is FieldDefinition field)
        {
            field.Name = RenameUnsafely();
        }
        if (@object is ParameterDefinition parameter)
        {
            parameter.Name = RenameUnsafely();
        }
    }
    public void Rename(params object[] objects)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            Rename(objects[i]);
        }
    }
    public void RemoveNamespace(object @object)
    {
        if (@object is TypeDefinition type)
        {
            if (m_SpecificNamespaceCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                type.Namespace = string.Empty;
            }
        }
        if (@object is MethodDefinition method)
        {
            if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                method.Name = RenameUnsafely();
            }
        }
        if (@object is FieldDefinition field)
        {
            field.Name = RenameUnsafely();
        }
        if (@object is ParameterDefinition parameter)
        {
            parameter.Name = RenameUnsafely();
        }
    }
    public void RemoveNamespace(params object[] objects)
    {
        objects.ForEach(@object => RemoveNamespace(@object));
    }
}