namespace BitMono.Core.Tests.Shared;

public class TestBitMonoCriticalsConfiguration : IBitMonoCriticalsConfiguration
{
    public TestBitMonoCriticalsConfiguration(string json)
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
            .Build();
    }

    public IConfiguration Configuration { get; }
}