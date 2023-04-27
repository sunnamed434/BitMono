namespace BitMono.GUI.Modules;

internal class GUIDataWriter : IDataWriter
{
    public Task WriteAsync(string outputFile, byte[] outputBuffer)
    {
        File.WriteAllBytes(outputFile, outputBuffer);
        return Task.CompletedTask;
    }
}