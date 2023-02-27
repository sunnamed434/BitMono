namespace BitMono.Obfuscation.Abstractions;

public class AutomaticReferencesDataResolver : IReferencesDataResolver
{
    private readonly string _referencesDirectoryName;

    public AutomaticReferencesDataResolver(string referencesDirectoryName)
    {
        _referencesDirectoryName = referencesDirectoryName;
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public IEnumerable<byte[]> Resolve(ModuleDefinition module)
    {
        var referencesData = new ReferencesDataResolver(_referencesDirectoryName).Resolve(module);
        var costuraReferencesData = new CosturaReferencesDataResolver().Resolve(module);
        if (costuraReferencesData.IsEmpty() == false)
        {
            return referencesData.Concat(costuraReferencesData);
        }
        return referencesData;
    }
}