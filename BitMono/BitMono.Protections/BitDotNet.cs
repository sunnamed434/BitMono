using BitMono.API.Protecting;
using dnlib.DotNet.Writer;
using System.IO;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class BitDotNet : IProtection
    {
        private ProtectionContext _context;

        public Task ExecuteAsync(ProtectionContext context)
        {
            _context = context;
            _context.ModuleWriterOptions.WriterEvent += writerEvent;
            return Task.CompletedTask;
        }


        private void writerEvent(object sender, ModuleWriterEventArgs e)
        {
            if (e.Event == ModuleWriterEvent.End)
            {
                using (var stream = File.Open(_context.BitMonoContext.ProtectedModuleFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (var reader = new BinaryReader(stream))
                using (var writer = new BinaryWriter(stream))
                {
                    var dotnetSize = 0x16C;
                    stream.Position = dotnetSize;
                    writer.Write(0); // breaks ILspy (didnt affects to mono)

                    var debugVirtualAddress = 0x128;
                    stream.Position = debugVirtualAddress;
                    writer.Write(0); // didnt affect to mono

                    var debugSize = 0x12C;
                    stream.Position = debugSize;
                    writer.Write(0); //didnt affect to mono

                    var importSize = 0x104;
                    stream.Position = importSize;
                    writer.Write(0); // didnt affect to mono, but may to some decompilers

                    stream.Position = 0x3C;
                    var peHeader = reader.ReadUInt32();
                    stream.Position = peHeader;

                    writer.Write(0x00014550); // BIT PE SIGNATURE

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
                    writer.Write(0); // BIT CB Bytes (breaks dnlib example: Dnspy)
                    stream.Position += 0x8;
                    writer.Write(0); // BIT Metadata size (breaks Mono.Cecil and maybe more :) examples: ILSpy, dotPeek)
                }

                _context.ModuleWriterOptions.WriterEvent -= writerEvent;
            }
        }
    }
}