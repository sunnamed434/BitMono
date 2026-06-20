namespace BitMono.CLI.Modules;

// Standalone --inspect-metadata mode: parse a Unity IL2CPP global-metadata.dat and print what survived
// into it (version + the names/literals Il2CppDumper would read back). The encrypt/rewrite half is the
// rest of #276.
internal static class MetadataInspector
{
    private const int SampleSize = 10;

    public static int Run(string path)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"global-metadata.dat not found: {path}");
            return KnownReturnStatuses.Failure;
        }

        try
        {
            var metadata = GlobalMetadataFile.Read(path);
            var reserved = metadata.Strings.Count(ReservedNames.IsReserved);
            var candidates = metadata.Strings.Count - reserved;

            Console.WriteLine($"global-metadata.dat: {path}");
            Console.WriteLine($"  metadata version: {metadata.Version}");
            Console.WriteLine($"  identifier names: {metadata.Strings.Count} ({reserved} reserved, {candidates} rename candidates)");
            Console.WriteLine($"  string literals:  {metadata.StringLiterals.Count}");

            // Candidates that still read like plain English mean renaming didn't reach the metadata.
            PrintSample("rename candidates", metadata.Strings.Where(name => !ReservedNames.IsReserved(name)));
            PrintSample("string literals", metadata.StringLiterals.Where(literal => literal.Length > 0));
            return KnownReturnStatuses.Success;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to parse metadata: {ex.Message}");
            return KnownReturnStatuses.Failure;
        }
    }

    private static void PrintSample(string label, IEnumerable<string> values)
    {
        var sample = values.Take(SampleSize).ToList();
        if (sample.Count == 0)
        {
            return;
        }
        Console.WriteLine($"  first {sample.Count} {label}:");
        foreach (var value in sample)
        {
            Console.WriteLine($"    {Truncate(value)}");
        }
    }

    private static string Truncate(string value)
    {
        var singleLine = value.Replace('\r', ' ').Replace('\n', ' ');
        return singleLine.Length <= 80 ? singleLine : singleLine.Substring(0, 77) + "...";
    }
}
