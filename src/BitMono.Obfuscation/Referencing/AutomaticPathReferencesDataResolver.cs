namespace BitMono.Obfuscation.Referencing;

public class AutomaticPathReferencesDataResolver : IReferencesDataResolver
{
    private readonly ReferencesDataResolver _referencesDataResolver;
    private readonly CosturaReferencesDataResolver _costuraReferencesDataResolver;

    public AutomaticPathReferencesDataResolver(string referencesDirectoryPath)
    {
        _referencesDataResolver = new ReferencesDataResolver(referencesDirectoryPath);
        _costuraReferencesDataResolver = new CosturaReferencesDataResolver();
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public List<byte[]> Resolve(ModuleDefinition module)
    {
        var referencesData = _referencesDataResolver.Resolve(module);
        var costuraReferencesData = _costuraReferencesDataResolver.Resolve(module);
        if (costuraReferencesData.IsEmpty() == false)
        {
            referencesData.AddRange(costuraReferencesData);
        }
        return referencesData;
    }
}