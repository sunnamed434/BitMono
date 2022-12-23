namespace BitMono.Core.Protecting.Resolvers;

public class AttemptAttributeResolver : IAttemptAttributeResolver
{
    private readonly ICustomAttributesResolver m_CustomAttributesResolver;

    public AttemptAttributeResolver(ICustomAttributesResolver customAttributesResolver)
    {
        m_CustomAttributesResolver = customAttributesResolver;
    }

    public bool TryResolve<TAttribute>(IHasCustomAttribute from, [AllowNull] Func<TAttribute, bool> predicate, [AllowNull] out TAttribute attribute)
        where TAttribute : Attribute
    {
        attribute = predicate != null
            ? m_CustomAttributesResolver.Resolve<TAttribute>(from).Where(a => a != null)?.FirstOrDefault(predicate)
            : m_CustomAttributesResolver.Resolve<TAttribute>(from).FirstOrDefault();
        if (attribute != null)
        {
            return true;
        }
        return false;
    }
}