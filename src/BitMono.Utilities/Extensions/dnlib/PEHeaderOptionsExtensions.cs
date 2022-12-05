using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace BitMono.Utilities.Extensions.dnlib
{
    public static class PEHeaderOptionsExtensions
    {
        public static PEHeadersOptions CopyPEHeaders(this PEHeadersOptions source, ModuleDefMD moduleDefMD)
        {
            var image = moduleDefMD.Metadata.PEImage;
            source.MajorImageVersion = image.ImageNTHeaders.OptionalHeader.MajorImageVersion;
            source.MajorLinkerVersion = image.ImageNTHeaders.OptionalHeader.MajorLinkerVersion;
            source.MajorOperatingSystemVersion = image.ImageNTHeaders.OptionalHeader.MajorOperatingSystemVersion;
            source.MajorSubsystemVersion = image.ImageNTHeaders.OptionalHeader.MajorSubsystemVersion;
            source.MinorImageVersion = image.ImageNTHeaders.OptionalHeader.MinorImageVersion;
            source.MinorLinkerVersion = image.ImageNTHeaders.OptionalHeader.MinorLinkerVersion;
            source.MinorOperatingSystemVersion = image.ImageNTHeaders.OptionalHeader.MinorOperatingSystemVersion;
            source.MinorSubsystemVersion = image.ImageNTHeaders.OptionalHeader.MinorSubsystemVersion;
            return source;
        }
    }
}