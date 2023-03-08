namespace BitMono.Utilities.AsmResolver;

[SuppressMessage("ReSharper", "InvertIf")]
public static class ManifestResourceExtensions
{
    private const string CosturaResourceNameStart = "costura.";
    private const string CosturaResourceNameEnd = ".dll.compressed";
    private const int MinCosturaResourceNameCharactersLenght = 19;

    public static bool IsEmbeddedCosturaResource(this ManifestResource source)
    {
        if (Utf8String.IsNullOrEmpty(source.Name) == false)
        {
            if (source.IsEmbedded)
            {
                var name = source.Name.Value;
                if (name.Length > MinCosturaResourceNameCharactersLenght
                    && name.StartsWith(CosturaResourceNameStart)
                    && name.EndsWith(CosturaResourceNameEnd))
                {
                    return true;
                }
            }
        }
        return false;
    }
}