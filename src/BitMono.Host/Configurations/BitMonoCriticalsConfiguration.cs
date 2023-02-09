namespace BitMono.Host.Configurations;

public class BitMonoCriticalsConfiguration : JsonConfigurationAccessor, IBitMonoCriticalsConfiguration
{
    public BitMonoCriticalsConfiguration(string? file = null) : base(file ?? "criticals.json")
    {
    }
}