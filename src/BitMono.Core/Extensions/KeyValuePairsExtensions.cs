namespace BitMono.Core.Extensions;

public static class KeyValuePairsExtensions
{
    public static string GetValueOrDefault(this Dictionary<string, object> source, string key, string defaultValue = "")
    {
        var value = defaultValue;
        if (source.TryGetTypedValue(key, out string valueValue))
        {
            value = valueValue;
        }
        return value;
    }
    public static bool GetValueOrDefault(this Dictionary<string, object> source, string key, bool defaultValue = false)
    {
        var value = defaultValue;
        if (source.TryGetTypedValue(key, out bool valueValue))
        {
            value = valueValue;
        }
        return value;
    }
    public static bool TryGetTypedValue<TKey, TValue, TActual>(this IDictionary<TKey, TValue> source, TKey key, [AllowNull] out TActual value)
        where TActual : TValue
    {
        if (source.TryGetValue(key, out TValue tempValue))
        {
            value = (TActual)tempValue;
            return true;
        }
        value = default;
        return false;
    }
}