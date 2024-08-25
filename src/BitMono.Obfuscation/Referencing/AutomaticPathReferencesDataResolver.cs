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

    public List<byte[]> Resolve(ModuleDefinition module, CancellationToken cancellationToken)
    {
        var referencesData = _referencesDataResolver.Resolve(module, cancellationToken);
        var costuraReferencesData = _costuraReferencesDataResolver.Resolve(module, cancellationToken);
        if (costuraReferencesData.IsEmpty() == false)
        {
            referencesData.AddRange(costuraReferencesData);
        }
        return referencesData;
    }
}