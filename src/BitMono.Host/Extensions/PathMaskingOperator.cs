namespace BitMono.Host.Extensions;

public class PathMaskingOperator : RegexMaskingOperator
{
    private const string PathPattern = @"^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$";

    public PathMaskingOperator() : base(PathPattern)
    {
    }

    protected override string PreprocessMask(string mask, Match match)
    {
        var value = match.Value;
        var attributes = File.GetAttributes(value);
        return attributes.HasFlag(FileAttributes.Directory)
            ? mask + Path.GetFileName(Path.GetDirectoryName(value))
            : mask + Path.GetFileName(value);
    }
}