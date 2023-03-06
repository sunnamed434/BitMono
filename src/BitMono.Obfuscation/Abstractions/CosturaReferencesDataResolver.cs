namespace BitMono.Obfuscation.Abstractions;

public class CosturaReferencesDataResolver : IReferencesDataResolver
{
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public IEnumerable<byte[]> Resolve(ModuleDefinition module)
    {
        var resources = module.Resources;
        for (var i = 0; i < resources.Count; i++)
        {
            var resource = resources[i];
            if (resource.IsEmbeddedCosturaResource())
            {
                var rawData = resource.GetData();
                if (rawData != null)
                {
                    yield return Decompress(rawData);
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