using BitMono.API.Protecting;
using BitMono.API.Protecting.Resolvers;
using dnlib.DotNet;
using System.Reflection;

namespace BitMono.Core.Protecting.Resolvers
{
    public class DnlibDefFeatureObfuscationAttributeHavingResolver : IDnlibDefFeatureObfuscationAttributeHavingResolver
    {
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;

        public DnlibDefFeatureObfuscationAttributeHavingResolver(IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver)
        {
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
        }

        public bool Resolve(IDnlibDef dnlibDef, string feature)
        {
            if (m_ObfuscationAttributeExcludingResolver.TryResolve(dnlibDef, feature: feature,
                out ObfuscationAttribute typeDefObfuscationAttribute))
            {
                if (typeDefObfuscationAttribute.Exclude)
                {
                    return false;
                }
            }
            return true;
        }
        public bool Resolve<TFeature>(IDnlibDef dnlibDef) where TFeature : IProtection
        {
            return Resolve(dnlibDef, typeof(TFeature).Name);
        }
    }
}