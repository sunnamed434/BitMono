namespace BitMono.Obfuscation.Files;

public class CompleteFileInfo
{
    public CompleteFileInfo(string fileName, byte[] fileData, List<byte[]> fileReferences, string outputDirectoryPath)
    {
        FileName = fileName;
        FileData = fileData;
        FileReferences = fileReferences;
        OutputDirectoryPath = outputDirectoryPath;
    }

    public string FileName { get; set; }
    public byte[] FileData { get; set; }
    public List<byte[]> FileReferences { get; set; }
    public string OutputDirectoryPath { get; set; }
}