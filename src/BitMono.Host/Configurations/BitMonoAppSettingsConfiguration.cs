namespace BitMono.Host.Configurations;

public class BitMonoAppSettingsConfiguration : JsonConfigurationAccessor, IBitMonoAppSettingsConfiguration
{
    public BitMonoAppSettingsConfiguration() : base(file: "appsettings.json")
    {
    }
}