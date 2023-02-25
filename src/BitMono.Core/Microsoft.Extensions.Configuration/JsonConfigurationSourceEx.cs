namespace BitMono.Core.Configuration;

public class JsonConfigurationSourceEx : FileConfigurationSource
{
    public IDictionary<string, string>? Variables { get; set; }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new JsonConfigurationProviderEx(this);
    }
}