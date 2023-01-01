namespace BitMono.Core.Protecting.Resolvers;

public class AttemptAttributeResolver : IAttemptAttributeResolver
{
    private readonly ICustomAttributeResolver m_CustomAttributesResolver;

    public AttemptAttributeResolver(ICustomAttributeResolver customAttributesResolver)
    {
        m_CustomAttributesResolver = customAttributesResolver;
    }

    public bool TryResolve(IHasCustomAttribute from, string @namespace, string name, [AllowNull] out Dictionary<string, CustomAttributeResolve> keyValuePairs)
    {
        keyValuePairs = m_CustomAttributesResolver.Resolve(from, @namespace, name);
        if (keyValuePairs != null && keyValuePairs.Any())
        {
            return true;
        }
        return false;
    }
}