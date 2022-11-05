using dnlib.DotNet;
using System;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IAttemptAttributeResolver
    {
        bool TryResolve<TAttribute>(IHasCustomAttribute from, Predicate<TAttribute> review, Func<TAttribute, bool> predicate, out TAttribute attribute)
            where TAttribute : Attribute;
    }
}