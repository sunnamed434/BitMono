using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Pipeline;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class BitDotNet : IStageProtection
    {
        public PipelineStages Stage => PipelineStages.ModuleWritten;

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            using (var stream = File.Open(context.BitMonoContext.OutputModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var reader = new BinaryReader(stream))
            using (var writer = new BinaryWriter(stream))
            {
                var dotnetSize = 0x16C;
                stream.Position = dotnetSize;
                writer.Write(0);

                var debugVirtualAddress = 0x128;
                stream.Position = debugVirtualAddress;
                writer.Write(0);

                var debugSize = 0x12C;
                stream.Position = debugSize;
                writer.Write(0);

                var importSize = 0x104;
                stream.Position = importSize;
                writer.Write(0);

                stream.Position = 0x3C;
                var peHeader = reader.ReadUInt32();
                stream.Position = peHeader;

                const int bittenPEHeaderWithExtraByte = 0x00014550;
                writer.Write(bittenPEHeaderWithExtraByte);

                stream.Position += 0x2;
                var numberOfSections = reader.ReadUInt16();

                stream.Position += 0x10;
                var is64PEOptionsHeader = reader.ReadUInt16() == 0x20B;

                stream.Position += is64PEOptionsHeader ? 0x38 : 0x28 + 0xA6;
                var dotNetVirtualAddress = reader.ReadUInt32();

                uint dotNetPointerRaw = 0;
                stream.Position += 0xC;
                for (int i = 0; i < numberOfSections; i++)
                {
                    stream.Position += 0xC;
                    var virtualAddress = reader.ReadUInt32();
                    var sizeOfRawData = reader.ReadUInt32();
                    var pointerToRawData = reader.ReadUInt32();
                    stream.Position += 0x10;

                    if (dotNetVirtualAddress >= virtualAddress && dotNetVirtualAddress < virtualAddress + sizeOfRawData && dotNetPointerRaw == 0)
                    {
                        dotNetPointerRaw = dotNetVirtualAddress + pointerToRawData - virtualAddress;
                    }
                }

                stream.Position = dotNetPointerRaw;
                writer.Write(0);
                stream.Position += 0x8;
                writer.Write(0);
            }
            return Task.CompletedTask;
        }
    }
}