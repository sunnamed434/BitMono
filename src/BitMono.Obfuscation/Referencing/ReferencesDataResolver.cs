namespace BitMono.Obfuscation.Referencing;

public class ReferencesDataResolver : IReferencesDataResolver
{
    private readonly IReadOnlyList<string> _referencesDirectories;

    public ReferencesDataResolver(string referencesDirectoryName)
        : this(new[] { referencesDirectoryName })
    {
    }
    public ReferencesDataResolver(IEnumerable<string> referencesDirectories)
    {
        _referencesDirectories = referencesDirectories?.ToList() ?? new List<string>();
    }

    public List<byte[]> Resolve(ModuleDefinition module, CancellationToken cancellationToken)
    {
        var result = new List<byte[]>();
        // Resolve from every provided directory. Non-existent ones are skipped (a user may pass
        // several -l paths, not all of which exist), and files are de-duplicated by name across
        // directories so the same dependency isn't added twice.
        var seenFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in _referencesDirectories)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                continue;
            }
            foreach (var reference in Directory.GetFiles(directory))
            {
                if (!seenFileNames.Add(Path.GetFileName(reference)))
                {
                    continue;
                }
                result.Add(File.ReadAllBytes(reference));
            }
        }
        return result;
    }
}