#nullable enable
namespace BitMono.Host.Configurations;

public class BitMonoProtectionsConfiguration : JsonConfigurationAccessor, IBitMonoProtectionsConfiguration
{
    public BitMonoProtectionsConfiguration() : base("protections.json")
    {
    }
}