namespace BitMono.Utilities.Paths;

public static class PathFormatterUtility
{
    private const string QuotesValue = "\"";

    [return: NullGuard.AllowNull]
    public static string? Format(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }
        return path.Replace(QuotesValue, string.Empty);
    }
}