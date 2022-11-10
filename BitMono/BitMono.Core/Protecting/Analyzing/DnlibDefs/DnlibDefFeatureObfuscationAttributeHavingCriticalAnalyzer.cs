using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.API.Protecting.Resolvers;
using dnlib.DotNet;
using System.Reflection;

namespace BitMono.Core.Protecting.Analyzing.DnlibDefs
{
    public class DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<TFeature> : ICriticalAnalyzer<IDnlibDef>
        where TFeature : IProtection
    {
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;

        public DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer(IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver)
        {
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
        }

        public bool NotCriticalToMakeChanges(IDnlibDef dnlibDef)
        {
            if (m_ObfuscationAttributeExcludingResolver.TryResolve(dnlibDef, feature: typeof(TFeature).Name,
                out ObfuscationAttribute typeDefObfuscationAttribute))
            {
                if (typeDefObfuscationAttribute.Exclude)
                {
                    return false;
                }
            }
            return true;
        }
    }
}