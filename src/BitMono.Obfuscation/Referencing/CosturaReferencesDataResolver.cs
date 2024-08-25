namespace BitMono.Obfuscation.Referencing;

public class CosturaReferencesDataResolver : IReferencesDataResolver
{
    public List<byte[]> Resolve(ModuleDefinition module, CancellationToken cancellationToken)
    {
        var result = new List<byte[]>();
        var resources = module.Resources;

        foreach (var resource in resources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (resource.IsEmbeddedCosturaResource() == false)
            {
                continue;
            }
            var rawData = resource.GetData();
            if (rawData == null)
            {
                continue;
            }

            result.Add(Decompress(rawData));
        }
        return result;
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