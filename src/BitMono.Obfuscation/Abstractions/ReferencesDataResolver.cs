namespace BitMono.Obfuscation.Abstractions;

public class ReferencesDataResolver : IReferencesDataResolver
{
    private readonly string _referencesDirectoryName;

    public ReferencesDataResolver(string referencesDirectoryName)
    {
        _referencesDirectoryName = referencesDirectoryName;
    }

    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public List<byte[]> Resolve(ModuleDefinition module)
    {
        var result = new List<byte[]>();
        var references = Directory.GetFiles(_referencesDirectoryName);
        for (var i = 0; i < references.Length; i++)
        {
            var reference = references[i];
            result.Add(File.ReadAllBytes(reference));
        }
        return result;
    }
}