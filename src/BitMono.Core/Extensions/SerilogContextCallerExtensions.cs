namespace BitMono.Core.Extensions;

public static class SerilogContextCallerExtensions
{
    private const string CsharpFileExtension = ".cs";
    public static ILogger ForContextFile(this ILogger source, [CallerFilePath] string caller = "")
    {
        caller = caller
            .SplitToDirectorySeparators()
            .Last()
            .Replace(CsharpFileExtension, string.Empty);
        return source.ForContext(Constants.SourceContextPropertyName, caller);
    }
}