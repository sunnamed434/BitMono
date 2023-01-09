namespace BitMono.Host.Configurations;

public class BitMonoObfuscationConfiguration : JsonConfigurationAccessor, IBitMonoObfuscationConfiguration
{
    public BitMonoObfuscationConfiguration() : base(file: "obfuscation.json")
    {
    }
}