namespace BitMono.Obfuscation.Factories;

public class BitMonoContextFactory
{
    private readonly ModuleDefinition _module;
    private readonly IReferencesDataResolver _referencesDataResolver;
    private readonly ObfuscationSettings _obfuscationSettings;

    public BitMonoContextFactory(ModuleDefinition module, IReferencesDataResolver referencesDataResolver, ObfuscationSettings obfuscationSettings)
    {
        _module = module;
        _referencesDataResolver = referencesDataResolver;
        _obfuscationSettings = obfuscationSettings;
    }

    public BitMonoContext Create(string outputDirectoryName, string fileName)
    {
        var referencesData = _referencesDataResolver.Resolve(_module);
        return new BitMonoContext
        {
            OutputDirectoryName = outputDirectoryName,
            ReferencesData = referencesData,
            Watermark = _obfuscationSettings.Watermark,
            FileName = fileName
        };
    }
}