namespace BitMono.API.Protecting.Resolvers;

public interface IAttemptAttributeResolver
{
    bool TryResolve<TAttribute>(IHasCustomAttribute from, Func<TAttribute, bool> predicate, out TAttribute attribute)
        where TAttribute : Attribute;
}