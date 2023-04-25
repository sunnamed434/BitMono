namespace BitMono.Obfuscation.Abstractions;

public class ObfuscationArguments
{
    public string FileName { get; set; }
    public string OutputPath { get; set; }
    public byte[] FileData { get; set; }
    public List<byte[]> References { get; set; }
}