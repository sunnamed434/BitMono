using BitMono.API.Configuration;

namespace BitMono.Host.Configuration
{
    public class BitMonoProtectionsConfiguration : JsonConfigurationAccessor, IBitMonoProtectionsConfiguration
    {
        public BitMonoProtectionsConfiguration() : base(file: "protections.json")
        {
        }
    }
}