namespace BitMono.Host.Configurations;

public class BitMonoProtectionsConfiguration : JsonConfigurationAccessor, IBitMonoProtectionsConfiguration
{
    public BitMonoProtectionsConfiguration() : base(file: "protections.json")
    {
    }
}