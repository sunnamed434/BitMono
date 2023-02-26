namespace BitMono.Core.Contexts;

public class BitMonoContext
{
    public string? FileName { get; set; }
    public string? OutputDirectoryName { get; set; }
    public string? OutputFile { get; set; }
    public IEnumerable<byte[]>? ReferencesData { get; set; }
    public bool Watermark { get; set; }
}