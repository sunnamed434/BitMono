namespace BitMono.Obfuscation.Factories;

public class BitMonoContextFactory
{
    private readonly IDependenciesDataResolver m_DependenciesDataResolver;
    private readonly Shared.Models.Obfuscation m_Obfuscation;

    public BitMonoContextFactory(IDependenciesDataResolver dependenciesDataResolver, Shared.Models.Obfuscation obfuscation)
    {
        m_DependenciesDataResolver = dependenciesDataResolver;
        m_Obfuscation = obfuscation;
    }

    public BitMonoContext Create(string outputDirectoryName, string fileName)
    {
        return new BitMonoContext
        {
            OutputDirectoryName = outputDirectoryName,
            ReferencesData = m_DependenciesDataResolver.Resolve(),
            Watermark = m_Obfuscation.Watermark,
            FileName = fileName
        };
    }
}