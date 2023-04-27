namespace BitMono.Obfuscation.Files;

public interface IDataWriter
{
    Task WriteAsync(string outputFile, byte[] outputBuffer);
}