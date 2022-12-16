namespace BitMono.Core.Protecting.Resolvers;

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
            out ObfuscationAttribute obfuscationAttribute) && obfuscationAttribute.Exclude)
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