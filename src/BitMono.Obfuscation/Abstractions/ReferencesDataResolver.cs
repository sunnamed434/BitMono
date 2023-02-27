namespace BitMono.Obfuscation.Abstractions;

public class ReferencesDataResolver : IReferencesDataResolver
{
    private readonly string _referencesDirectoryName;

    public ReferencesDataResolver(string referencesDirectoryName)
    {
        _referencesDirectoryName = referencesDirectoryName;
    }

    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public IEnumerable<byte[]> Resolve(ModuleDefinition module)
    {
        var dependencies = Directory.GetFiles(_referencesDirectoryName);
        for (var i = 0; i < dependencies.Length; i++)
        {
            yield return File.ReadAllBytes(dependencies[i]);
        }
    }
}