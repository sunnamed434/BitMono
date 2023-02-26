namespace BitMono.Obfuscation.Interfaces;

public interface IDataWriter
{
    Task WriteAsync(string outputFile, byte[] outputBuffer);
}