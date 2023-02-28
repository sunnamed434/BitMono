namespace BitMono.Host.Extensions;

public class PathMaskingOperator : RegexMaskingOperator
{
    private readonly IPathWrapper _pathWrapper;
    private readonly bool _combineMaskWithPath;
    private const string PathPattern =
        @"^(?:[a-zA-Z]\:|\\\\[\w-]+\\[\w-]+\$?|[\/][^\/\0]+)+(\\[^\\/:*?""<>|]*)*(\\?)?$";

    public PathMaskingOperator(IPathWrapper pathWrapper, bool combineMaskWithPath = true) : base(PathPattern)
    {
        _pathWrapper = pathWrapper;
        _combineMaskWithPath = combineMaskWithPath;
    }
    public PathMaskingOperator(bool combineMaskWithPath = true) : this(new PathWrapper(), combineMaskWithPath)
    {
    }
    public PathMaskingOperator() : this(combineMaskWithPath: true)
    {
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    protected override string PreprocessMask(string mask, Match match)
    {
        if (_combineMaskWithPath)
        {
            var value = match.Value;
            return _pathWrapper.IsDirectory(value)
                ? mask + _pathWrapper.GetDirectoryName(value)
                : mask + _pathWrapper.GetFileName(value);
        }
        return mask;
    }
}