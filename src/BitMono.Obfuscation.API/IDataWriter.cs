namespace BitMono.Obfuscation.API;

public interface IDataWriter
{
    Task WriteAsync(string outputFile, byte[] outputBuffer);
}