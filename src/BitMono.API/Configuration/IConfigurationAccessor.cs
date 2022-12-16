namespace BitMono.API.Configuration;

public interface IConfigurationAccessor
{
    IConfiguration Configuration { get; }
}