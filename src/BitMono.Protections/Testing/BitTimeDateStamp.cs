using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Pipeline;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections.Testing
{
    public class BitTimeDateStamp : IStageProtection
    {
        public PipelineStages Stage => PipelineStages.ModuleWritten;

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
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