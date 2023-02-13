namespace BitMono.Host.Configurations;

public class BitMonoProtectionsConfiguration : JsonConfigurationAccessor, IBitMonoProtectionsConfiguration
{
    public BitMonoProtectionsConfiguration(string? file = null) : base(file ?? "protections.json")
    {
    }
}