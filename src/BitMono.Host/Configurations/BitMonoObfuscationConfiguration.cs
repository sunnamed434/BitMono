namespace BitMono.Host.Configurations;

public class BitMonoObfuscationConfiguration : JsonConfigurationAccessor, IBitMonoObfuscationConfiguration
{
    public BitMonoObfuscationConfiguration(string? file = null) : base(file ?? "obfuscation.json")
    {
    }
}