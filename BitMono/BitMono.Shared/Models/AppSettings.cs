namespace BitMono.Core.Models
{
    public class AppSettings
    {
        public bool Watermark { get; set; }
        public bool NoInliningMethodObfuscationExcluding { get; set; }
        public bool ObfuscationAttributeObfuscationExcluding { get; set; }
        public bool FailOnNoRequiredDependency { get; set; }
    }
}