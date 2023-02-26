namespace BitMono.Core.Configuration;

public static class JsonConfigurationExtensions
{
    public static IConfigurationBuilder AddJsonFileEx(this IConfigurationBuilder builder, Action<JsonConfigurationSourceEx> configure)
    {
        return builder.Add(configure);
    }
}