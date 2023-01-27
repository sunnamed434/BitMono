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
            var x64PEOptionsHeader = reader.ReadUInt16() == 0x20B;

            stream.Position += x64PEOptionsHeader ? 0x6A : 0x5A;
            writer.Write(0x00013); // NumberOfRvaAndSizes
            
            stream.Position += 0xC;
            writer.Write(0); // Import.Size
            Console.WriteLine(stream.Position.ToHexString());

            stream.Position += 0x20;
            writer.Write(0); // Debug.VirtualAddress
            writer.Write(0); // Debug.Size

            stream.Position += 0x3C;
            writer.Write(0); // .NET.Size
        }
        return Task.CompletedTask;
    }
}