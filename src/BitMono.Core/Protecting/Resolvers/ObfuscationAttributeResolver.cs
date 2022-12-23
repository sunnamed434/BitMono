namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeResolver : IObfuscationAttributeResolver
{
    private readonly IObfuscationAttributeExcludeResolver m_ObfuscationAttributeResolver;

    public ObfuscationAttributeResolver(IObfuscationAttributeExcludeResolver obfuscationAttributeResolver)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
    }

    public bool Resolve(string feature, IHasCustomAttribute from)
    {
        if (m_ObfuscationAttributeResolver.TryResolve(feature, from,
            out ObfuscationAttribute obfuscationAttribute) && obfuscationAttribute.Exclude)
        {
            return true;
        }
        return false;
    }
    public bool Resolve<TFeature>(IHasCustomAttribute from) where TFeature : IProtection
    {
        return Resolve(typeof(TFeature).GetName(), from);
    }
}