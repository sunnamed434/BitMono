namespace BitMono.Obfuscation.Referencing;

public class AutomaticReferencesDataResolver : IReferencesDataResolver
{
    private readonly List<byte[]> _referencesData;
    private readonly CosturaReferencesDataResolver _costuraReferencesDataResolver;

    public AutomaticReferencesDataResolver(List<byte[]> referencesData)
    {
        _referencesData = referencesData;
        _costuraReferencesDataResolver = new CosturaReferencesDataResolver();
    }

    public List<byte[]> Resolve(ModuleDefinition module, CancellationToken cancellationToken)
    {
        var costuraReferencesData = _costuraReferencesDataResolver.Resolve(module, cancellationToken);
        costuraReferencesData.AddRange(_referencesData);
        return costuraReferencesData;
    }
}