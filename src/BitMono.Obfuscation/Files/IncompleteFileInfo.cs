namespace BitMono.Obfuscation.Files;

public class IncompleteFileInfo
{
    public IncompleteFileInfo(string filePath, string referencesDirectoryPath, string outputDirectoryPath)
        : this(filePath, new[] { referencesDirectoryPath }, outputDirectoryPath)
    {
    }
    public IncompleteFileInfo(string filePath, IReadOnlyList<string> referencesDirectoryPaths, string outputDirectoryPath)
    {
        FilePath = filePath;
        ReferencesDirectoryPaths = referencesDirectoryPaths ?? Array.Empty<string>();
        OutputDirectoryPath = outputDirectoryPath;
    }

    public string FilePath { get; set; }
    public IReadOnlyList<string> ReferencesDirectoryPaths { get; set; }
    /// <summary>
    /// First references directory. Kept for backwards compatibility; prefer
    /// <see cref="ReferencesDirectoryPaths"/> which supports multiple directories.
    /// </summary>
    public string ReferencesDirectoryPath => ReferencesDirectoryPaths.Count > 0 ? ReferencesDirectoryPaths[0] : string.Empty;
    public string OutputDirectoryPath { get; set; }
}
