namespace BitMono.Core.Extensions;

public static class KeyValuePairsExtensions
{
    public static bool TryGetValueOrDefault(this Dictionary<string, object> source, string key, bool defaultValue = false)
    {
        var value = defaultValue;
        if (source.TryGetValue(key, out var valueValue))
        {
            if (valueValue is bool resolveValue)
            {
                value = resolveValue;
            }
        }
        return value;
    }
}