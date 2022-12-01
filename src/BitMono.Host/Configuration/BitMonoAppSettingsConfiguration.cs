using BitMono.API.Configuration;

namespace BitMono.Host.Configuration
{
    public class BitMonoAppSettingsConfiguration : JsonConfigurationAccessor, IBitMonoAppSettingsConfiguration
    {
        public BitMonoAppSettingsConfiguration() : base(file: "appsettings.json")
        {
        }
    }
}