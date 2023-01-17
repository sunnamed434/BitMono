namespace BitMono.Obfuscation;

internal class FileDataWriter : IDataWriter
{
    public Task WriteAsync(string outputFile, byte[] outputBuffer)
    {
        File.WriteAllBytes(outputFile, outputBuffer);
        return Task.CompletedTask;
    }
}