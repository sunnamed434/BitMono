using BitMono.Obfuscation.API;

namespace BitMono.CLI.Modules;

internal class CLIDataWriter : IDataWriter
{
    public Task WriteAsync(string outputFile, byte[] outputBuffer)
    {
        File.WriteAllBytes(outputFile, outputBuffer);
        return Task.CompletedTask;
    }
}