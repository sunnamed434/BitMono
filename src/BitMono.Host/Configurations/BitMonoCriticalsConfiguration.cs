namespace BitMono.Host.Configurations;

public class BitMonoCriticalsConfiguration : JsonConfigurationAccessor, IBitMonoCriticalsConfiguration
{
    public BitMonoCriticalsConfiguration() : base(file: "criticals.json")
    {
    }
}