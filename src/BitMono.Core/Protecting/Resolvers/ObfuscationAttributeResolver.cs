namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeResolver : IObfuscationAttributeResolver
{
    private readonly IObfuscationAttributeExcludeResolver m_ObfuscationAttributeExcludingResolver;

    public ObfuscationAttributeResolver(IObfuscationAttributeExcludeResolver obfuscationAttributeExcludingResolver)
    {
        m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
    }

    public bool Resolve(string feature, IHasCustomAttribute from)
    {
        if (m_ObfuscationAttributeExcludingResolver.TryResolve(feature, from,
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