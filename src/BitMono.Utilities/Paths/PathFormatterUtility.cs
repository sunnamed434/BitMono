namespace BitMono.Utilities.Paths;

public static class PathFormatterUtility
{
    private const string QuotesValue = "\"";

    public static string Format(string path)
    {
        return path.Replace(QuotesValue,string.Empty);
    }
}