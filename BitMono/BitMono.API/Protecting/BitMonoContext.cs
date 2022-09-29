namespace BitMono.API.Protecting
{
    public class BitMonoContext
    {
        public string ModuleFile { get; set; }
        public string BaseDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string ProtectedModuleFile { get; set; }
        public bool Watermark { get; set; }
    }
}