using BitMono.API.Protecting;
using BitMono.API.Protecting.Resolvers;
using BitMono.Utilities.Extensions;
using dnlib.DotNet;
using System.Reflection;

namespace BitMono.Core.Protecting.Resolvers
{
    public class DnlibDefObfuscationAttributeResolver : IDnlibDefObfuscationAttributeResolver
    {
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;

        public DnlibDefObfuscationAttributeResolver(IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver)
        {
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
        }

        public bool Resolve(string feature, IDnlibDef dnlibDef)
        {
            if (m_ObfuscationAttributeExcludingResolver.TryResolve(feature, dnlibDef,
                out ObfuscationAttribute typeDefObfuscationAttribute) && typeDefObfuscationAttribute.Exclude)
            {
                return true;
            }
            return false;
        }
        public bool Resolve<TFeature>(IDnlibDef dnlibDef) where TFeature : IProtection
        {
            return Resolve(typeof(TFeature).GetName(), dnlibDef);
        }
    }
}