namespace BitMono.Core.Protecting.Resolvers;

public class AttemptAttributeResolver
{
    private readonly CustomAttributeResolver m_CustomAttributesResolver;

    public AttemptAttributeResolver(CustomAttributeResolver customAttributesResolver)
    {
        m_CustomAttributesResolver = customAttributesResolver;
    }

    public bool TryResolve(IHasCustomAttribute from, string @namespace, string name, [AllowNull] out CustomAttributeResolve attributeResolve)
    {
        return (attributeResolve = m_CustomAttributesResolver.Resolve(from, @namespace, name)) != null;
    }
    public bool TryResolve(IHasCustomAttribute from, string @namespace, string name)
    {
        return TryResolve(from, @namespace, name, out _);
    }
}