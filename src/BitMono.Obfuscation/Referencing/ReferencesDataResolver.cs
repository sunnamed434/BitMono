namespace BitMono.Obfuscation.Referencing;

public class ReferencesDataResolver : IReferencesDataResolver
{
    private readonly string _referencesDirectoryName;

    public ReferencesDataResolver(string referencesDirectoryName)
    {
        _referencesDirectoryName = referencesDirectoryName;
    }

    public List<byte[]> Resolve(ModuleDefinition module, CancellationToken cancellationToken)
    {
        var result = new List<byte[]>();
        var references = Directory.GetFiles(_referencesDirectoryName);
        foreach (var reference in references)
        {
            result.Add(File.ReadAllBytes(reference));
        }
        return result;
    }
}