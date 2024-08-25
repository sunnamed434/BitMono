namespace BitMono.Obfuscation.Starter;

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

    public BitMonoContext Create(string filePath, string outputDirectoryName, CancellationToken cancellationToken)
    {
        var referencesData = _referencesDataResolver.Resolve(_module, cancellationToken);
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