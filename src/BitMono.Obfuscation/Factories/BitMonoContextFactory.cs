namespace BitMono.Obfuscation.Factories;

public class BitMonoContextFactory
{
    private readonly ModuleDefinition _module;
    private readonly IReferencesDataResolver _referencesDataResolver;
    private readonly Shared.Models.Obfuscation _obfuscation;

    public BitMonoContextFactory(ModuleDefinition module, IReferencesDataResolver referencesDataResolver, Shared.Models.Obfuscation obfuscation)
    {
        _module = module;
        _referencesDataResolver = referencesDataResolver;
        _obfuscation = obfuscation;
    }

    public BitMonoContext Create(string outputDirectoryName, string fileName)
    {
        var referencesData = _referencesDataResolver.Resolve(_module);
        return new BitMonoContext
        {
            OutputDirectoryName = outputDirectoryName,
            ReferencesData = referencesData,
            Watermark = _obfuscation.Watermark,
            FileName = fileName
        };
    }
}