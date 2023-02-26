namespace BitMono.Obfuscation.Abstractions;

public class DependenciesDataResolver : IDependenciesDataResolver
{
    private readonly string m_DependenciesDirectoryName;

    public DependenciesDataResolver(string dependenciesDirectoryName)
    {
        m_DependenciesDirectoryName = dependenciesDirectoryName;
    }

    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public IEnumerable<byte[]> Resolve()
    {
        var dependencies = Directory.GetFiles(m_DependenciesDirectoryName);
        for (var i = 0; i < dependencies.Length; i++)
        {
            yield return File.ReadAllBytes(dependencies[i]);
        }
    }
}