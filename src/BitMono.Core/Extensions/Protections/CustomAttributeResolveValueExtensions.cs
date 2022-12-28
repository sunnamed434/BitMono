namespace BitMono.Core.Extensions.Protections;

public static class CustomAttributeResolveValueExtensions
{
    public static bool TryGetValueOrDefault(this Dictionary<string, CustomAttributeResolve> source, string key, out CustomAttributeResolve attributeResolve, bool defaultValue = false)
    {
        var value = defaultValue;
        if (source.TryGetValue(key, out attributeResolve))
        {
            if (attributeResolve.Value is bool resolveValue)
            {
                value = resolveValue;
            }
        }
        return value;
    }
    public static bool TryGetValueOrDefault(this Dictionary<string, CustomAttributeResolve> source, string key, bool defaultValue = false)
    {
        return TryGetValueOrDefault(source, key, out _, defaultValue);
    }
}