namespace BitMono.Core.Microsoft.Extensions.Configuration;

public static class JsonConfigurationExtensions
{
    public static IConfigurationBuilder AddJsonFileEx(this IConfigurationBuilder builder, Action<JsonConfigurationSourceEx> configure)
    {
        return builder.Add(configure);
    }
}