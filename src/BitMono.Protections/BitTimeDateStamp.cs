using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Attributes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    [ProtectionName(nameof(BitTimeDateStamp))]
    public class BitTimeDateStamp : IPacker
    {
        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            using (var stream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
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
}