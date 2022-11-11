using BitMono.API.Configuration;
using BitMono.API.Protecting.Context;
using Microsoft.Extensions.Configuration;
using NullGuard;
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

        public Task<BitMonoContext> CreateAsync(string outputDirectoryName, [AllowNull] string dependenciesDirectoryName = null)
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