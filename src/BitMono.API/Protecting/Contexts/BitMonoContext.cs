using System.Collections.Generic;

namespace BitMono.API.Protecting.Contexts
{
    public class BitMonoContext
    {
        public string ModuleFileName { get; set; }
        public string OutputPath { get; set; }
        public string OutputModuleFile { get; set; }
        public IEnumerable<byte[]> DependenciesData { get; set; }
        public bool Watermark { get; set; }
    }
}