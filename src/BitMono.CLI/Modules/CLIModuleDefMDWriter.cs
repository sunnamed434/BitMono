namespace BitMono.CLI.Modules;

internal class CLIModuleDefMDWriter : IDataWriter
{
    public Task WriteAsync(string outputFile, byte[] outputBuffer)
    {
        File.WriteAllBytes(outputFile, outputBuffer);
        return Task.CompletedTask;
    }
}