namespace BitMono.Protections;

public class BitTimeDateStamp : IPacker
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
            var timeDateStamp = stream.Position + 0x8;
            stream.Position = timeDateStamp;
            writer.Write(0);
        }
        return Task.CompletedTask;
    }
}