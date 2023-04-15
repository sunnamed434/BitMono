namespace BitMono.Obfuscation.Abstractions;

public class FileDataWriter : IDataWriter
{
    public Task WriteAsync(string outputFile, byte[] outputBuffer)
    {
        File.WriteAllBytes(outputFile, outputBuffer);
        return Task.CompletedTask;
    }
}