namespace BitMono.Core.Resolvers;

public class AttemptAttributeResolver
{
    private readonly CustomAttributeResolver m_CustomAttributesResolver;

    public AttemptAttributeResolver(CustomAttributeResolver customAttributesResolver)
    {
        m_CustomAttributesResolver = customAttributesResolver;
    }

    public bool TryResolve(IHasCustomAttribute from, string @namespace, string name, out IEnumerable<CustomAttributeResolve>? attributesResolve)
    {
        attributesResolve = m_CustomAttributesResolver.Resolve(from, @namespace, name);
        return attributesResolve.IsNullOrEmpty() == false;
    }
    public bool TryResolve(IHasCustomAttribute from, string @namespace, string name)
    {
        return TryResolve(from, @namespace, name, out _);
    }
}