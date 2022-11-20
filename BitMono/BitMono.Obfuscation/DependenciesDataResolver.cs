using BitMono.Obfuscation.API;
using System.Collections.Generic;
using System.IO;

namespace BitMono.Obfuscation
{
    public class DependenciesDataResolver : IDependenciesDataResolver
    {
        private readonly string m_DependenciesDirectoryName;

        public DependenciesDataResolver(string dependenciesDirectoryName)
        {
            m_DependenciesDirectoryName = dependenciesDirectoryName;
        }

        public IEnumerable<byte[]> Resolve()
        {
            var dependencies = Directory.GetFiles(m_DependenciesDirectoryName);
            for (int i = 0; i < dependencies.Length; i++)
            {
                yield return File.ReadAllBytes(dependencies[i]);
            }
        }
    }
}