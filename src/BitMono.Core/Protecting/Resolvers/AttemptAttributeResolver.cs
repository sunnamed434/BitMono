namespace BitMono.Core.Protecting.Resolvers;

public class AttemptAttributeResolver : IAttemptAttributeResolver
{
    private readonly ICustomAttributeResolver m_CustomAttributesResolver;

    public AttemptAttributeResolver(ICustomAttributeResolver customAttributesResolver)
    {
        m_CustomAttributesResolver = customAttributesResolver;
    }

    public bool TryResolve(IHasCustomAttribute from, Type attributeType, [AllowNull] out Dictionary<string, CustomAttributesResolve> keyValuePairs)
    {
        keyValuePairs = m_CustomAttributesResolver.Resolve(from, attributeType);
        if (keyValuePairs != null && keyValuePairs.Any())
        {
            return true;
        }
        return false;
    }
}