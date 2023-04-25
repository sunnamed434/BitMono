namespace BitMono.Obfuscation.Abstractions;

public class AutomaticReferencesDataResolver : IReferencesDataResolver
{
    private readonly ReferencesDataResolver _referencesDataResolver;
    private readonly CosturaReferencesDataResolver _costuraReferencesDataResolver;

    public AutomaticReferencesDataResolver(string referencesDirectoryName)
    {
        _referencesDataResolver = new ReferencesDataResolver(referencesDirectoryName);
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