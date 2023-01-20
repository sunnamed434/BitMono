namespace BitMono.Protections;

public class BitMono : IPacker
{
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        using (var stream = File.Open(context.BitMonoContext.OutputFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
        using (var reader = new BinaryReader(stream))
        using (var writer = new BinaryWriter(stream))
        {
            stream.Position = 0x3C;
            var peHeader = reader.ReadUInt32();
            stream.Position = peHeader;

            stream.Position += 0x18;

            stream.Position += 0x5C;
            writer.Write(0x00013);

            stream.Position += 0x8;
            writer.Write(0);

            stream.Position += 0x24;
            writer.Write(0);

            stream.Position += 64;
            writer.Write(0);
        }
        return Task.CompletedTask;
    }
}