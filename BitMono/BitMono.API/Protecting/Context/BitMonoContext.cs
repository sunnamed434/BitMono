namespace BitMono.API.Protecting.Contexts
{
    public class BitMonoContext
    {
        public string ModuleFile { get; set; }
        public string BaseDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string OutputModuleFile { get; set; }
        public bool Watermark { get; set; }
    }
}