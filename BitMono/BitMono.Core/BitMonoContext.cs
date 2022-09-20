namespace BitMono.Core
{
    public class BitMonoContext
    {
        public BitMonoContext(string moduleFile, string baseDirectory, string outputDirectory, bool fileWatermark = true)
        {
            ModuleFile = moduleFile;
            BaseDirectory = baseDirectory;
            OutputDirectory = outputDirectory;
            FileWatermark = fileWatermark;
        }
        public BitMonoContext(string baseDirectory, string outputDirectory, bool fileWatermark = true) 
            : this(null, baseDirectory, outputDirectory, fileWatermark)
        {
        }
        public BitMonoContext()
        {
        }

        public string ModuleFile { get; set; }
        public string BaseDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public bool FileWatermark { get; set; }
    }
}