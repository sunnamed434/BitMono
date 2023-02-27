namespace BitMono.Obfuscation.Abstractions;

public class CosturaReferencesDataResolver : IReferencesDataResolver
{
    private const string CosturaResourceNameStart = "costura.";
    private const string CosturaResourceNameEnd = ".dll.compressed";
    private const int MinCosturaResourceNameCharactersLenght = 19;

    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public IEnumerable<byte[]> Resolve(ModuleDefinition module)
    {
        for (var i = 0; i < module.Resources.Count; i++)
        {
            var resource = module.Resources[i];
            if (resource.IsEmbedded)
            {
                if (Utf8String.IsNullOrEmpty(resource.Name) == false)
                {
                    var name = resource.Name.Value;
                    if (name.Length > MinCosturaResourceNameCharactersLenght)
                    {
                        if (name.StartsWith(CosturaResourceNameStart) && name.EndsWith(CosturaResourceNameEnd))
                        {
                            var data = resource.GetData();
                            if (data != null)
                            {
                                data = Decompress(data);
                                yield return data;
                            }
                        }
                    }
                }
            }
        }
    }

    private static byte[] Decompress(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var output = new MemoryStream();
        using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
        deflateStream.CopyTo(output);
        return output.ToArray();
    }
}