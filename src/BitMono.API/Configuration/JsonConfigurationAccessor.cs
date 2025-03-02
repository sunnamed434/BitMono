namespace BitMono.API.Configuration;

public class JsonConfigurationAccessor : IConfigurationAccessor
{
    protected JsonConfigurationAccessor(string file)
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile(file, false, true)
            .Build();
    }

    public IConfiguration Configuration { get; }
}