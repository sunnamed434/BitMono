using Microsoft.Extensions.Configuration;

namespace BitMono.API.Configuration
{
    public interface IConfigurationAccessor
    {
        IConfiguration Configuration { get; }
    }
}