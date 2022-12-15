namespace BitMono.API.Protecting.Writers;

public interface IDataWriter
{
    Task WriteAsync(string outputFile, byte[] outputBuffer);
}