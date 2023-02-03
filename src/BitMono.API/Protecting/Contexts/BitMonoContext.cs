namespace BitMono.API.Protecting.Contexts;

public class BitMonoContext
{
    public string? FileName { get; set; }
    public string? OutputDirectoryName { get; set; }
    public string? OutputFile { get; set; }
    public IEnumerable<byte[]>? DependenciesData { get; set; }
    public bool Watermark { get; set; }
}