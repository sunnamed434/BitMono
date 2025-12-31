namespace BitMono.Protections;

public class BitTimeDateStamp : PackerProtection
{
    public BitTimeDateStamp(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        using (var stream = File.Open(Context.BitMonoContext.OutputFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
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