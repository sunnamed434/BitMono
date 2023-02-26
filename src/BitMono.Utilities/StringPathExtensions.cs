namespace BitMono.Utilities;

public static class StringPathExtensions
{
    public static string ReplaceDirectorySeparatorToAlt(this string source)
    {
        return source.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
    public static string[] SplitToDirectorySeparators(this string source)
    {
        return source.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}