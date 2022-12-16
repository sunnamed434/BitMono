namespace BitMono.Host.Configuration;

public class BitMonoObfuscationConfiguration : JsonConfigurationAccessor, IBitMonoObfuscationConfiguration
{
    public BitMonoObfuscationConfiguration() : base(file: "obfuscation.json")
    {
    }
}