using NullGuard;

namespace BitMono.API.Protecting.Context
{
    public class BitMonoContext
    {
        [AllowNull]
        public string ModuleFileName { get; set; }
        [AllowNull]
        public string OutputPath { get; set; }
        [AllowNull]
        public string OutputModuleFile { get; set; }
        [AllowNull]
        public string DependenciesDirectoryName { get; set; }
        public bool Watermark { get; set; }
    }
}