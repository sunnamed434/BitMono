namespace BitMono.Obfuscation.Engine;

public class BitMonoContextFactory
{
    private readonly ModuleDefinition _module;
    private readonly IReferencesDataResolver _referencesDataResolver;
    private readonly ObfuscationSettings _obfuscationSettings;

    public BitMonoContextFactory(ModuleDefinition module, IReferencesDataResolver referencesDataResolver,
        ObfuscationSettings obfuscationSettings)
    {
        _module = module;
        _referencesDataResolver = referencesDataResolver;
        _obfuscationSettings = obfuscationSettings;
    }

    public BitMonoContext Create(string filePath, string outputDirectoryName)
    {
        var referencesData = _referencesDataResolver.Resolve(_module);
        var fileName = Path.GetFileName(filePath);
        return new BitMonoContext
        {
            OutputDirectoryName = outputDirectoryName,
            ReferencesData = referencesData,
            Watermark = _obfuscationSettings.Watermark,
            FileName = fileName
        };
    }
}