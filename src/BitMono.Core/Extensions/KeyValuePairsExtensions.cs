namespace BitMono.Core.Extensions;

public static class KeyValuePairsExtensions
{
    public static bool GetValueOrDefault(this Dictionary<string, object> source, string key, bool defaultValue = false)
    {
        var value = defaultValue;
        if (source.TryGetTypedValue(key, out bool valueValue))
        {
            value = valueValue;
        }
        return value;
    }
    public static bool TryGetTypedValue<TKey, TValue, TActual>(this IDictionary<TKey, TValue> source, TKey key, out TActual? value)
        where TActual : TValue
    {
        if (source.TryGetValue(key, out var tempValue))
        {
            value = (TActual?)tempValue;
            return true;
        }
        value = default;
        return false;
    }
}