using BitMono.API.Configuration;

namespace BitMono.Host.Configuration
{
    public class BitMonoCriticalsConfiguration : JsonConfigurationAccessor, IBitMonoCriticalsConfiguration
    {
        public BitMonoCriticalsConfiguration() : base(file: "criticals.json")
        {
        }
    }
}