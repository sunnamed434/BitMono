using BitMono.API.Configuration;
using BitMono.API.Protecting.Context;
using Microsoft.Extensions.Configuration;
using System.IO;
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

        public Task<BitMonoContext> CreateAsync(string currentAssemblyDirectory, string baseDirectoryName, string outputDirectoryName)
        {
            Directory.CreateDirectory(baseDirectoryName);
            Directory.CreateDirectory(outputDirectoryName);
            var baseDirectoryPath = Path.Combine(currentAssemblyDirectory, baseDirectoryName);
            var outputDirectoryPath = Path.Combine(currentAssemblyDirectory, outputDirectoryName);
            var bitMonoContext = new BitMonoContext
            {
                BaseDirectory = baseDirectoryPath,
                OutputDirectory = outputDirectoryPath,
                Watermark = m_Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.Watermark)),
            };
            return Task.FromResult(bitMonoContext);
        }
    }
}