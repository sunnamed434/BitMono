using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using System.Collections.Generic;
using System.Linq;

namespace BitMono.Obfuscation
{
    public class ProtectionParametersCreator
    {
        private readonly DnlibDefsResolver m_DnlibDefsResolver;
        private readonly IEnumerable<IDnlibDefResolver> m_Resolvers;

        public ProtectionParametersCreator(DnlibDefsResolver dnlibDefsResolver, IEnumerable<IDnlibDefResolver> resolvers)
        {
            m_DnlibDefsResolver = dnlibDefsResolver;
            m_Resolvers = resolvers;
        }

        public ProtectionParameters Create(string feature, ModuleDefMD moduleDefMD)
        {
            var definitions = moduleDefMD.FindDefinitions();
            var targets = m_DnlibDefsResolver.Resolve(feature, definitions, m_Resolvers).ToList();
            return new ProtectionParameters(targets);
        }
    }
}