using BitMono.API.Configuration;
using BitMono.API.Protecting.Contexts;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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

        public Task<BitMonoContext> CreateAsync(string outputDirectoryName, IEnumerable<byte[]> dependeciesData)
        {
            var bitMonoContext = new BitMonoContext
            {
                OutputPath = outputDirectoryName,
                DependenciesData = dependeciesData,
                Watermark = m_Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.Watermark)),
            };
            return Task.FromResult(bitMonoContext);
        }
    }
}