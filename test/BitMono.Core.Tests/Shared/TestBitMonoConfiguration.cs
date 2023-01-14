namespace BitMono.Core.Tests.Shared;

public abstract class TestBitMonoConfiguration : IConfigurationAccessor
{
    public TestBitMonoConfiguration(string json)
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
            .Build();
    }

    public IConfiguration Configuration { get; }
}