using BitMono.API.Configuration;
using BitMono.API.Protecting.Contexts;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BitMono.Obfuscation
{
    public class BitMonoContextCreator
    {
        private readonly IConfiguration m_Configuration;

        public BitMonoContextCreator(IBitMonoObfuscationConfiguration configuration)
        {
            m_Configuration = configuration.Configuration;
        }

        public Task<BitMonoContext> CreateAsync(string outputDirectoryName, string dependenciesDirectoryName)
        {
            var bitMonoContext = new BitMonoContext
            {
                OutputPath = outputDirectoryName,
                DependenciesDirectoryName = dependenciesDirectoryName,
                Watermark = m_Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.Watermark)),
            };
            return Task.FromResult(bitMonoContext);
        }
    }
}