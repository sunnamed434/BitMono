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

    public List<byte[]> Resolve(ModuleDefinition module)
    {
        var costuraReferencesData = _costuraReferencesDataResolver.Resolve(module);
        costuraReferencesData.AddRange(_referencesData);
        return costuraReferencesData;
    }
}