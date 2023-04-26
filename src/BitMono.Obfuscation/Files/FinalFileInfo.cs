namespace BitMono.Obfuscation.Files;

public class FinalFileInfo
{
    public FinalFileInfo(string filePath, string outputDirectoryPath)
    {
        FilePath = filePath;
        OutputDirectoryPath = outputDirectoryPath;
    }

    public string FilePath { get; set; }
    public string OutputDirectoryPath { get; set; }
}