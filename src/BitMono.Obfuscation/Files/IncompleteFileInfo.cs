namespace BitMono.Obfuscation.Files;

public class IncompleteFileInfo
{
    public IncompleteFileInfo(string filePath, string referencesDirectoryPath, string outputDirectoryPath)
    {
        FilePath = filePath;
        ReferencesDirectoryPath = referencesDirectoryPath;
        OutputDirectoryPath = outputDirectoryPath;
    }

    public string FilePath { get; set; }
    public string ReferencesDirectoryPath { get; set; }
    public string OutputDirectoryPath { get; set; }
}